using Kompass.Application.Verbrauch;
using Kompass.Domain.Verbrauch;
using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Persistence.Services;

public sealed class EfVerbrauchsDatenService : IVerbrauchsDatenService
{
    private readonly KompassDbContext _dbContext;

    public EfVerbrauchsDatenService(
        KompassDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<VerbrauchsDaten>> ListenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.VerbrauchsDaten
            .Where(v => v.ProjektId == projektId)
            .OrderBy(v => v.PeriodeVon)
            .ThenBy(v => v.Energietraeger)
            .ToListAsync(cancellationToken);
    }

    public async Task<VerbrauchsDaten?> AbrufenAsync(
        Guid projektId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.VerbrauchsDaten
            .FirstOrDefaultAsync(
                v => v.ProjektId == projektId && v.Id == id,
                cancellationToken);
    }

    public async Task<VerbrauchsDaten?> AnlegenAsync(
        VerbrauchsDaten verbrauchsDaten,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(verbrauchsDaten);

        var projektVorhanden = await _dbContext.Projekte
            .AnyAsync(
                p => p.Id == verbrauchsDaten.ProjektId,
                cancellationToken);

        if (!projektVorhanden)
        {
            return null;
        }

        _dbContext.VerbrauchsDaten.Add(verbrauchsDaten);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return verbrauchsDaten;
    }

    public async Task<bool> AktualisierenAsync(
        VerbrauchsDaten verbrauchsDaten,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(verbrauchsDaten);

        var vorhanden = await _dbContext.VerbrauchsDaten
            .AnyAsync(
                v => v.ProjektId == verbrauchsDaten.ProjektId &&
                     v.Id == verbrauchsDaten.Id,
                cancellationToken);

        if (!vorhanden)
        {
            return false;
        }

        _dbContext.VerbrauchsDaten.Update(verbrauchsDaten);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> LoeschenAsync(
        Guid projektId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var datensatz = await _dbContext.VerbrauchsDaten
            .FirstOrDefaultAsync(
                v => v.ProjektId == projektId && v.Id == id,
                cancellationToken);

        if (datensatz is null)
        {
            return false;
        }

        _dbContext.VerbrauchsDaten.Remove(datensatz);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<VerbrauchsZusammenfassungJeEnergietraeger>?> ZusammenfassenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default)
    {
        var projektVorhanden = await _dbContext.Projekte
            .AnyAsync(p => p.Id == projektId, cancellationToken);

        if (!projektVorhanden)
        {
            return null;
        }

        var datensaetze = await _dbContext.VerbrauchsDaten
            .AsNoTracking()
            .Where(v => v.ProjektId == projektId)
            .ToListAsync(cancellationToken);

        var zusammenfassungen = datensaetze
            .GroupBy(v => v.Energietraeger)
            .Select(gruppe =>
            {
                var gesamtTage = gruppe.Sum(
                    v => (v.PeriodeBis.ToDateTime(TimeOnly.MinValue) -
                          v.PeriodeVon.ToDateTime(TimeOnly.MinValue)).TotalDays);

                var gesamtMenge = gruppe.Sum(v => v.Menge);

                var jaehrlicheMenge = gesamtTage > 0
                    ? gesamtMenge / (decimal)gesamtTage * 365m
                    : 0m;

                return new VerbrauchsZusammenfassungJeEnergietraeger(
                    gruppe.Key,
                    gruppe.Count(),
                    gesamtMenge,
                    gruppe.Sum(v => v.WitterungsbereinigteMenge),
                    Math.Round(jaehrlicheMenge, 2),
                    gruppe.Sum(v => v.Kosten));
            })
            .OrderBy(z => z.Energietraeger)
            .ToList();

        return zusammenfassungen;
    }
}
