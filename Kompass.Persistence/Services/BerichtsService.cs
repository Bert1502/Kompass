using Kompass.Application.Reports;
using Kompass.Domain.Reports;
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
}
