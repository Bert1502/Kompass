using Kompass.Application.Funding;
using Kompass.Domain.Funding;
using Kompass.Domain.Projects;
using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Persistence.Services;

public sealed class EfAlternativeFoerderungService : IAlternativeFoerderungService
{
    private readonly KompassDbContext _dbContext;

    public EfAlternativeFoerderungService(
        KompassDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Foerderprogramm>> ZugeordneteProgrammeListenAsync(
        Guid projektId,
        Guid alternativeId,
        CancellationToken cancellationToken = default)
    {
        if (!await AlternativeGehoertZuProjektAsync(
                projektId,
                alternativeId,
                cancellationToken))
        {
            return [];
        }

        var zugeordneteProgrammIds = await _dbContext.FoerderungZuordnungen
            .Where(z => z.ModernisierungsalternativeId == alternativeId)
            .Select(z => z.FoerderprogrammId)
            .ToListAsync(cancellationToken);

        return await _dbContext.Foerderprogramme
            .Include(f => f.Foerderquoten)
            .Include(f => f.Hoechstbetraege)
            .Include(f => f.Kumulierbarkeitsregeln)
            .Include(f => f.Pflichtnachweisregeln)
            .Include(f => f.Gueltigkeitsregeln)
            .Where(f => zugeordneteProgrammIds.Contains(f.Id))
            .OrderBy(f => f.Programmkennung)
            .ThenBy(f => f.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ProgrammZuordnenAsync(
        Guid projektId,
        Guid alternativeId,
        Guid foerderprogrammId,
        CancellationToken cancellationToken = default)
    {
        if (!await AlternativeGehoertZuProjektAsync(
                projektId,
                alternativeId,
                cancellationToken))
        {
            return false;
        }

        var programmVorhanden = await _dbContext.Foerderprogramme
            .AnyAsync(
                f => f.Id == foerderprogrammId,
                cancellationToken);

        if (!programmVorhanden)
        {
            return false;
        }

        var bereitsZugeordnet = await _dbContext.FoerderungZuordnungen
            .AnyAsync(
                z =>
                    z.ModernisierungsalternativeId == alternativeId &&
                    z.FoerderprogrammId == foerderprogrammId,
                cancellationToken);

        if (bereitsZugeordnet)
        {
            return false;
        }

        _dbContext.FoerderungZuordnungen.Add(
            new FoerderungZuordnung(
                Guid.NewGuid(),
                alternativeId,
                foerderprogrammId));

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ProgrammEntfernenAsync(
        Guid projektId,
        Guid alternativeId,
        Guid foerderprogrammId,
        CancellationToken cancellationToken = default)
    {
        if (!await AlternativeGehoertZuProjektAsync(
                projektId,
                alternativeId,
                cancellationToken))
        {
            return false;
        }

        var zuordnung = await _dbContext.FoerderungZuordnungen
            .SingleOrDefaultAsync(
                z =>
                    z.ModernisierungsalternativeId == alternativeId &&
                    z.FoerderprogrammId == foerderprogrammId,
                cancellationToken);

        if (zuordnung is null)
        {
            return false;
        }

        _dbContext.FoerderungZuordnungen.Remove(zuordnung);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<Foerderberechnungsergebnis?> FoerderungBerechnenAsync(
        Guid projektId,
        Guid alternativeId,
        DateOnly stichtag,
        CancellationToken cancellationToken = default)
    {
        var alternative = await _dbContext.Set<Modernisierungsalternative>()
            .Include(a => a.Kostenpositionen)
            .Where(
                a =>
                    _dbContext.Projekte
                        .Where(p => p.Id == projektId)
                        .SelectMany(p => p.Alternativen)
                        .Select(pa => pa.Id)
                        .Contains(a.Id) &&
                    a.Id == alternativeId)
            .SingleOrDefaultAsync(cancellationToken);

        if (alternative is null)
        {
            return null;
        }

        var programme =
            await ZugeordneteProgrammeListenAsync(
                projektId,
                alternativeId,
                cancellationToken);

        if (programme.Count == 0)
        {
            programme = await _dbContext.Foerderprogramme
                .Include(f => f.Foerderquoten)
                .Include(f => f.Hoechstbetraege)
                .Include(f => f.Kumulierbarkeitsregeln)
                .Include(f => f.Pflichtnachweisregeln)
                .Include(f => f.Gueltigkeitsregeln)
                .OrderBy(f => f.Programmkennung)
                .ThenBy(f => f.Version)
                .ToListAsync(cancellationToken);
        }

        var voraussetzungen = await _dbContext.Foerdervoraussetzungen
            .SingleOrDefaultAsync(x => x.ProjektId == projektId, cancellationToken);

        var investitionskosten = alternative.Gesamtkosten;

        var anteile = new List<ProgrammFoerderungsanteil>();

        foreach (var programm in programme)
        {
            var anteil = BerechneProgrammAnteil(
                programm,
                investitionskosten,
                stichtag,
                voraussetzungen);

            anteile.Add(anteil);
        }

        var gesamtFoerderung = anteile.Sum(a => a.Foerderbetrag);
        var eigenanteil = Math.Max(0m, investitionskosten - gesamtFoerderung);

        return new Foerderberechnungsergebnis(
            stichtag,
            investitionskosten,
            anteile,
            gesamtFoerderung,
            eigenanteil);
    }

    private static ProgrammFoerderungsanteil BerechneProgrammAnteil(
        Foerderprogramm programm,
        decimal investitionskosten,
        DateOnly stichtag,
        Foerdervoraussetzungen? voraussetzungen)
    {
        var fehlend = new List<string>();
        var ausschluss = new List<string>();

        if (stichtag < programm.GueltigAb || programm.GueltigBis is not null && stichtag > programm.GueltigBis)
            ausschluss.Add("Das Förderprogramm ist am gewählten Stichtag nicht gültig.");
        if (voraussetzungen is null)
            fehlend.Add("Fördervoraussetzungen");
        else
        {
            if (!voraussetzungen.Baujahr.HasValue) fehlend.Add("Baujahr");
            if (!voraussetzungen.Erstnutzung.HasValue) fehlend.Add("Erstnutzung");
            if (!voraussetzungen.Gebaeudeart.HasValue) fehlend.Add("Gebäudeart");
            if (!voraussetzungen.Nutzung.HasValue) fehlend.Add("Nutzung");
            if (!voraussetzungen.Eigentuemart.HasValue) fehlend.Add("Eigentümer-/Antragstellerart");
            if (voraussetzungen.Gebaeudeart == FoerderGebaeudeart.Wohngebaeude && !voraussetzungen.Wohneinheiten.HasValue)
                fehlend.Add("Wohneinheiten");
            if (voraussetzungen.Gebaeudeart == FoerderGebaeudeart.Nichtwohngebaeude && !voraussetzungen.Nettogrundflaeche.HasValue)
                fehlend.Add("Nettogrundfläche aus B56");

            var ziel = programm.Zielgruppe;
            if (ziel.Contains("Kommune", StringComparison.OrdinalIgnoreCase) && voraussetzungen.Eigentuemart != Antragstellerart.Kommune)
                ausschluss.Add("Die Eigentümer-/Antragstellerart entspricht nicht der Zielgruppe des Programms.");
            if (ziel.Contains("gemeinn", StringComparison.OrdinalIgnoreCase) && voraussetzungen.Gemeinnuetzigkeit != true)
                ausschluss.Add("Die erforderliche Gemeinnützigkeit ist nicht bestätigt.");
            if (programm.Foerdergegenstand.Contains("Nichtwohn", StringComparison.OrdinalIgnoreCase) && voraussetzungen.Gebaeudeart != FoerderGebaeudeart.Nichtwohngebaeude)
                ausschluss.Add("Das Programm gilt nur für Nichtwohngebäude.");
            if (programm.Foerdergegenstand.Contains("Wohngeb", StringComparison.OrdinalIgnoreCase) && !programm.Foerdergegenstand.Contains("Nichtwohn", StringComparison.OrdinalIgnoreCase) && voraussetzungen.Gebaeudeart != FoerderGebaeudeart.Wohngebaeude)
                ausschluss.Add("Das Programm gilt nur für Wohngebäude.");
        }

        var aktiveQuote = programm.Foerderquoten
            .Where(
                r =>
                    r.GueltigAb <= stichtag &&
                    (r.GueltigBis is null || r.GueltigBis >= stichtag))
            .OrderByDescending(r => r.GueltigAb)
            .FirstOrDefault();

        var grundquote = aktiveQuote?.Quote ?? 0m;

        var aktiverHoechstbetrag = programm.Hoechstbetraege
            .Where(
                r =>
                    r.GueltigAb <= stichtag &&
                    (r.GueltigBis is null || r.GueltigBis >= stichtag))
            .OrderByDescending(r => r.GueltigAb)
            .FirstOrDefault();

        var istBegEm = programm.Programmkennung.Contains("BEG", StringComparison.OrdinalIgnoreCase) &&
            programm.Programmkennung.Contains("EM", StringComparison.OrdinalIgnoreCase);
        var begEmObergrenze = istBegEm
            ? BerechneBegEmObergrenze(voraussetzungen, fehlend)
            : (decimal?)null;
        var hoechstbetrag = aktiverHoechstbetrag is null
            ? begEmObergrenze ?? investitionskosten
            : BerechneObergrenze(aktiverHoechstbetrag, voraussetzungen, fehlend);
        var istKostenobergrenze = begEmObergrenze.HasValue || aktiverHoechstbetrag is not null &&
            (aktiverHoechstbetrag.Bezugsbasis.Contains("Wohneinheit", StringComparison.OrdinalIgnoreCase) ||
             aktiverHoechstbetrag.Bezugsbasis.Contains("NGF", StringComparison.OrdinalIgnoreCase) ||
             aktiverHoechstbetrag.Bezugsbasis.Contains("m²", StringComparison.OrdinalIgnoreCase));
        var foerderfaehigeKosten = istKostenobergrenze
            ? Math.Min(investitionskosten, hoechstbetrag)
            : investitionskosten;

        var iSfpBonusquote = voraussetzungen?.ISfp == true ? 0.05m : 0m;
        var wpbBonusquote = voraussetzungen?.WpbFachlichBestaetigt == true ? 0.10m : 0m;
        var grundfoerderung = Math.Round(grundquote * foerderfaehigeKosten, 2);
        var iSfpBonus = Math.Round(iSfpBonusquote * foerderfaehigeKosten, 2);
        var wpbBonus = Math.Round(wpbBonusquote * foerderfaehigeKosten, 2);
        var foerderbetrag = grundfoerderung + iSfpBonus + wpbBonus;
        if (aktiverHoechstbetrag is not null && !istKostenobergrenze)
            foerderbetrag = Math.Min(foerderbetrag, hoechstbetrag);

        var aktiveKumulierbarkeit = programm.Kumulierbarkeitsregeln
            .Where(
                r =>
                    r.GueltigAb <= stichtag &&
                    (r.GueltigBis is null || r.GueltigBis >= stichtag))
            .OrderByDescending(r => r.GueltigAb)
            .FirstOrDefault();

        var kumulierbarkeit = aktiveKumulierbarkeit?.Status
            ?? KumulierbarkeitStatus.Unbestimmt;

        if (kumulierbarkeit == KumulierbarkeitStatus.Unbestimmt)
            fehlend.Add("Kumulierbarkeit ist fachlich zu prüfen.");

        foreach (var nachweis in programm.Pflichtnachweisregeln.Where(r => r.IstPflicht && r.GueltigAb <= stichtag && (r.GueltigBis is null || r.GueltigBis >= stichtag)))
        {
            if (voraussetzungen is null || !voraussetzungen.Nachweise.Contains(nachweis.Bezeichnung, StringComparison.OrdinalIgnoreCase))
                fehlend.Add($"Pflichtnachweis: {nachweis.Bezeichnung}");
        }

        var status = ausschluss.Count > 0 ? Foerderpruefstatus.NichtFoerderfaehig
            : fehlend.Count > 0 ? Foerderpruefstatus.AngabenFehlen
            : Foerderpruefstatus.VoraussichtlichFoerderfaehig;
        if (status == Foerderpruefstatus.NichtFoerderfaehig) foerderbetrag = 0m;

        return new ProgrammFoerderungsanteil(
            programm.Id,
            programm.Programmkennung,
            programm.Version,
            foerderbetrag,
            kumulierbarkeit,
            status,
            foerderfaehigeKosten,
            hoechstbetrag,
            grundquote,
            iSfpBonusquote,
            wpbBonusquote,
            grundfoerderung,
            iSfpBonus,
            wpbBonus,
            Math.Max(0m, investitionskosten - foerderbetrag),
            fehlend.Distinct().ToArray(),
            ausschluss.Distinct().ToArray());
    }

    private static decimal BerechneObergrenze(HoechstbetragRegel regel, Foerdervoraussetzungen? v, ICollection<string> fehlend)
    {
        if (regel.Bezugsbasis.Contains("Wohneinheit", StringComparison.OrdinalIgnoreCase))
        {
            if (v?.Wohneinheiten is not > 0) { fehlend.Add("Wohneinheiten für die Kostenobergrenze"); return 0m; }
            return regel.Betrag * v.Wohneinheiten.Value;
        }
        if (regel.Bezugsbasis.Contains("NGF", StringComparison.OrdinalIgnoreCase) || regel.Bezugsbasis.Contains("m²", StringComparison.OrdinalIgnoreCase))
        {
            if (v?.Nettogrundflaeche is not > 0) { fehlend.Add("Nettogrundfläche für die Kostenobergrenze"); return 0m; }
            return regel.Betrag * v.Nettogrundflaeche.Value;
        }
        return regel.Betrag;
    }

    private static decimal? BerechneBegEmObergrenze(
        Foerdervoraussetzungen? voraussetzungen,
        ICollection<string> fehlend)
    {
        if (voraussetzungen?.Gebaeudeart == FoerderGebaeudeart.Nichtwohngebaeude)
        {
            if (voraussetzungen.Nettogrundflaeche is not > 0)
            {
                fehlend.Add("Nettogrundfläche für die BEG-EM-Kostenobergrenze");
                return null;
            }

            return 500m * voraussetzungen.Nettogrundflaeche.Value;
        }

        if (voraussetzungen?.Gebaeudeart == FoerderGebaeudeart.Wohngebaeude)
        {
            if (voraussetzungen.Wohneinheiten is not > 0)
            {
                fehlend.Add("Wohneinheiten für die BEG-EM-Kostenobergrenze");
                return null;
            }

            var betragJeWohneinheit = voraussetzungen.ISfp == true ? 60_000m : 30_000m;
            return betragJeWohneinheit * voraussetzungen.Wohneinheiten.Value;
        }

        fehlend.Add("Gebäudeart für die BEG-EM-Kostenobergrenze");
        return null;
    }

    private async Task<bool> AlternativeGehoertZuProjektAsync(
        Guid projektId,
        Guid alternativeId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Projekte
            .Where(p => p.Id == projektId)
            .SelectMany(p => p.Alternativen)
            .AnyAsync(
                a => a.Id == alternativeId,
                cancellationToken);
    }
}
