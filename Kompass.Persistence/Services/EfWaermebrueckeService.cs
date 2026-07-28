using Kompass.Application.Waermebruecken;
using Kompass.Domain.Common;
using Kompass.Domain.Waermebruecken;
using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Persistence.Services;

public sealed class EfWaermebrueckeService : IWaermebrueckeService
{
    private readonly KompassDbContext _dbContext;

    public EfWaermebrueckeService(
        KompassDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Waermebruecke>> ListenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Waermebruecken
            .Where(w => w.ProjektId == projektId)
            .OrderBy(w => w.InterneNummer)
            .ToListAsync(cancellationToken);
    }

    public async Task<Waermebruecke?> AbrufenAsync(
        Guid projektId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Waermebruecken
            .FirstOrDefaultAsync(
                w => w.ProjektId == projektId && w.Id == id,
                cancellationToken);
    }

    public async Task<Waermebruecke?> AnlegenAsync(
        Waermebruecke waermebruecke,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(waermebruecke);

        var projektVorhanden = await _dbContext.Projekte
            .AnyAsync(
                p => p.Id == waermebruecke.ProjektId,
                cancellationToken);

        if (!projektVorhanden)
        {
            return null;
        }

        var nummerVergeben = await _dbContext.Waermebruecken
            .AnyAsync(
                w =>
                    w.ProjektId == waermebruecke.ProjektId &&
                    w.InterneNummer == waermebruecke.InterneNummer,
                cancellationToken);

        if (nummerVergeben)
        {
            throw new DomainException(
                $"Die interne Nummer '{waermebruecke.InterneNummer}' ist in diesem Projekt bereits vergeben.");
        }

        _dbContext.Waermebruecken.Add(waermebruecke);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return waermebruecke;
    }

    public async Task<bool> AktualisierenAsync(
        Waermebruecke waermebruecke,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(waermebruecke);

        var vorhanden = await _dbContext.Waermebruecken
            .AnyAsync(
                w => w.ProjektId == waermebruecke.ProjektId && w.Id == waermebruecke.Id,
                cancellationToken);

        if (!vorhanden)
        {
            return false;
        }

        _dbContext.Waermebruecken.Update(waermebruecke);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> LoeschenAsync(
        Guid projektId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var waermebruecke = await _dbContext.Waermebruecken
            .FirstOrDefaultAsync(
                w => w.ProjektId == projektId && w.Id == id,
                cancellationToken);

        if (waermebruecke is null)
        {
            return false;
        }

        _dbContext.Waermebruecken.Remove(waermebruecke);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
