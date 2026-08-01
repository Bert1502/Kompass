using Kompass.Application.Reports;
using Kompass.Domain.Economics;
using Kompass.Domain.Funding;
using Kompass.Domain.Projects;
using Kompass.Domain.Reports;
using Kompass.Domain.Verbrauch;
using Kompass.Domain.Waermebruecken;
using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Persistence.Services;

/// <summary>
/// Erzeugt KOMPASS-Berichte durch Aggregation vorhandener Domänendaten.
/// Gemäß ADR-0007 werden keine separaten Berichtsdaten gespeichert.
/// </summary>
public sealed class BerichtsService : IBerichtsService
{
    private readonly KompassDbContext _dbContext;

    public BerichtsService(KompassDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AlternativenvergleichBericht?> AlternativenvergleichErzeugenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default)
    {
        var projekt = await _dbContext.Projekte
            .AsNoTracking()
            .Include(p => p.Alternativen)
            .ThenInclude(a => a.Kostenpositionen)
            .FirstOrDefaultAsync(
                p => p.Id == projektId,
                cancellationToken);

        if (projekt is null)
        {
            return null;
        }

        var kopf = new Berichtskopf(
            projekt.Id,
            projekt.Name,
            projekt.InterneBezeichnung,
            projekt.Bearbeitungsstatus,
            projekt.QuellSnapshotId,
            DateTimeOffset.UtcNow,
            Berichtstyp.Alternativenvergleich);

        var zeilen = projekt.Alternativen
            .OrderBy(a => a.B56Position ?? int.MaxValue)
            .ThenBy(a => a.Bezeichnung)
            .Select(a => new AlternativenvergleichZeile(
                a.Id,
                a.B56Position,
                a.Bezeichnung,
                a.Kurztext,
                a.Gesamtkosten,
                a.Kostenpositionen.Count,
                a.IstImAktuellenB56SnapshotVorhanden))
            .ToList();

        return new AlternativenvergleichBericht(kopf, zeilen);
    }

    public async Task<WaermebrueckenuebersichtBericht?> WaermebrueckenuebersichtErzeugenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default)
    {
        var projekt = await _dbContext.Projekte
            .AsNoTracking()
            .Where(p => p.Id == projektId)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.InterneBezeichnung,
                p.Bearbeitungsstatus,
                p.QuellSnapshotId,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (projekt is null)
        {
            return null;
        }

        var waermebruecken = await _dbContext.Waermebruecken
            .AsNoTracking()
            .Where(w => w.ProjektId == projektId)
            .OrderBy(w => w.InterneNummer)
            .ToListAsync(cancellationToken);

        var kopf = new Berichtskopf(
            projekt.Id,
            projekt.Name,
            projekt.InterneBezeichnung,
            projekt.Bearbeitungsstatus,
            projekt.QuellSnapshotId,
            DateTimeOffset.UtcNow,
            Berichtstyp.Waermebrueckenuebersicht);

        return new WaermebrueckenuebersichtBericht(
            kopf,
            waermebruecken);
    }

    public async Task<WirtschaftlichkeitsberichtBericht?> WirtschaftlichkeitsberichtErzeugenAsync(
        Guid projektId,
        WirtschaftlichkeitsBasis basis,
        CancellationToken cancellationToken = default)
    {
        var projekt = await _dbContext.Projekte
            .AsNoTracking()
            .Include(p => p.Alternativen)
            .ThenInclude(a => a.Kostenpositionen)
            .FirstOrDefaultAsync(
                p => p.Id == projektId,
                cancellationToken);

        if (projekt is null)
        {
            return null;
        }

        var alternativeIds = projekt.Alternativen.Select(a => a.Id).ToList();

        var annahmenListe = await _dbContext.Wirtschaftlichkeitsannahmen
            .AsNoTracking()
            .Include(a => a.EnergietraegerAnnahmen)
            .Where(
                a =>
                    alternativeIds.Contains(a.ModernisierungsalternativeId) &&
                    a.Basis == basis)
            .ToListAsync(cancellationToken);

        var annahmenIndex = annahmenListe.ToDictionary(
            a => a.ModernisierungsalternativeId);

        var kopf = new Berichtskopf(
            projekt.Id,
            projekt.Name,
            projekt.InterneBezeichnung,
            projekt.Bearbeitungsstatus,
            projekt.QuellSnapshotId,
            DateTimeOffset.UtcNow,
            Berichtstyp.Wirtschaftlichkeitsbericht);

        var zeilen = projekt.Alternativen
            .OrderBy(a => a.B56Position ?? int.MaxValue)
            .ThenBy(a => a.Bezeichnung)
            .Where(a => annahmenIndex.ContainsKey(a.Id))
            .Select(a =>
            {
                var annahmen = annahmenIndex[a.Id];
                var ergebnis = annahmen.Berechnen(a.Gesamtkosten);

                return new WirtschaftlichkeitsberichtZeile(
                    a.Id,
                    a.B56Position,
                    a.Bezeichnung,
                    basis,
                    a.Gesamtkosten,
                    annahmen.Foerderung,
                    annahmen.Betrachtungszeitraum,
                    annahmen.Diskontsatz,
                    annahmen.Inflationsrate,
                    ergebnis);
            })
            .ToList();

        return new WirtschaftlichkeitsberichtBericht(kopf, zeilen);
    }

    public async Task<FoerderuebersichtBericht?> FoerderuebersichtErzeugenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default)
    {
        var projekt = await _dbContext.Projekte
            .AsNoTracking()
            .Include(p => p.Alternativen)
            .FirstOrDefaultAsync(
                p => p.Id == projektId,
                cancellationToken);

        if (projekt is null)
        {
            return null;
        }

        var alternativeIds = projekt.Alternativen.Select(a => a.Id).ToList();

        var zuordnungen = await _dbContext.FoerderungZuordnungen
            .AsNoTracking()
            .Where(z => alternativeIds.Contains(z.ModernisierungsalternativeId))
            .ToListAsync(cancellationToken);

        var programmIds = zuordnungen.Select(z => z.FoerderprogrammId).Distinct().ToList();

        var programme = await _dbContext.Foerderprogramme
            .AsNoTracking()
            .Include(f => f.Foerderquoten)
            .Include(f => f.Hoechstbetraege)
            .Include(f => f.Kumulierbarkeitsregeln)
            .Include(f => f.Pflichtnachweisregeln)
            .Include(f => f.Gueltigkeitsregeln)
            .Where(f => programmIds.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, cancellationToken);

        var kopf = new Berichtskopf(
            projekt.Id,
            projekt.Name,
            projekt.InterneBezeichnung,
            projekt.Bearbeitungsstatus,
            projekt.QuellSnapshotId,
            DateTimeOffset.UtcNow,
            Berichtstyp.Foerderuebersicht);

        var zuordnungenNachAlternative = zuordnungen
            .GroupBy(z => z.ModernisierungsalternativeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var alternativen = projekt.Alternativen
            .OrderBy(a => a.B56Position ?? int.MaxValue)
            .ThenBy(a => a.Bezeichnung)
            .Select(a =>
            {
                var zugeordneteProgramme = zuordnungenNachAlternative.TryGetValue(
                        a.Id,
                        out var altZuordnungen)
                    ? altZuordnungen
                        .Where(z => programme.ContainsKey(z.FoerderprogrammId))
                        .Select(z => programme[z.FoerderprogrammId])
                        .OrderBy(p => p.Programmkennung)
                        .ThenBy(p => p.Version)
                        .ToList<Foerderprogramm>()
                    : new List<Foerderprogramm>();

                return new FoerderuebersichtAlternative(
                    a.Id,
                    a.B56Position,
                    a.Bezeichnung,
                    a.Gesamtkosten,
                    zugeordneteProgramme);
            })
            .ToList();

        return new FoerderuebersichtBericht(kopf, alternativen);
    }

    public async Task<VerbrauchsvergleichBericht?> VerbrauchsvergleichErzeugenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default)
    {
        var projekt = await _dbContext.Projekte
            .AsNoTracking()
            .Where(p => p.Id == projektId)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.InterneBezeichnung,
                p.Bearbeitungsstatus,
                p.QuellSnapshotId,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (projekt is null)
        {
            return null;
        }

        var verbrauchsDaten = await _dbContext.VerbrauchsDaten
            .AsNoTracking()
            .Where(v => v.ProjektId == projektId)
            .OrderBy(v => v.PeriodeVon)
            .ThenBy(v => v.Energietraeger)
            .ToListAsync(cancellationToken);

        var kopf = new Berichtskopf(
            projekt.Id,
            projekt.Name,
            projekt.InterneBezeichnung,
            projekt.Bearbeitungsstatus,
            projekt.QuellSnapshotId,
            DateTimeOffset.UtcNow,
            Berichtstyp.Verbrauchsvergleich);

        var zeilen = verbrauchsDaten
            .Select(v =>
            {
                decimal? abweichung = v.B56VergleichsWert.HasValue
                    ? v.WitterungsbereinigteMenge - v.B56VergleichsWert.Value
                    : null;

                decimal? abweichungProzent =
                    abweichung.HasValue && v.B56VergleichsWert!.Value != 0
                        ? abweichung.Value / v.B56VergleichsWert.Value * 100m
                        : null;

                return new VerbrauchsvergleichZeile(
                    v.Id,
                    v.PeriodeVon,
                    v.PeriodeBis,
                    v.Energietraeger,
                    v.Menge,
                    v.WitterungsbereinigteMenge,
                    v.B56VergleichsWert,
                    abweichung,
                    abweichungProzent);
            })
            .ToList();

        return new VerbrauchsvergleichBericht(kopf, zeilen);
    }
}
