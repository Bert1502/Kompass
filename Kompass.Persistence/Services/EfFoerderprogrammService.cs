using Kompass.Application.Funding;
using Kompass.Domain.Common;
using Kompass.Domain.Funding;
using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Persistence.Services;

public sealed class EfFoerderprogrammService : IFoerderprogrammService
{
    private readonly KompassDbContext _dbContext;

    public EfFoerderprogrammService(
        KompassDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Foerderprogramm>> ListenAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Foerderprogramme
            .Include(f => f.Foerderquoten)
            .Include(f => f.Hoechstbetraege)
            .Include(f => f.Kumulierbarkeitsregeln)
            .Include(f => f.Pflichtnachweisregeln)
            .Include(f => f.Gueltigkeitsregeln)
            .OrderBy(f => f.Programmkennung)
            .ThenBy(f => f.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<Foerderprogramm> AnlegenAsync(
        Foerderprogramm foerderprogramm,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(foerderprogramm);

        var vorhanden =
            await _dbContext.Foerderprogramme.AnyAsync(
                vorhandenes =>
                    vorhandenes.Programmkennung == foerderprogramm.Programmkennung &&
                    vorhandenes.Version == foerderprogramm.Version,
                cancellationToken);

        if (vorhanden)
        {
            throw new DomainException(
                $"Für das Förderprogramm '{foerderprogramm.Programmkennung}' existiert bereits Version {foerderprogramm.Version}.");
        }

        _dbContext.Foerderprogramme.Add(
            foerderprogramm);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return foerderprogramm;
    }
}
