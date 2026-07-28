using Kompass.Application.Economics;
using Kompass.Domain.Economics;
using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Persistence.Services;

/// <summary>
/// EF-Core-Implementierung des <see cref="IWirtschaftlichkeitsannahmenRepository"/>.
/// </summary>
public sealed class EfWirtschaftlichkeitsannahmenRepository
    : IWirtschaftlichkeitsannahmenRepository
{
    private readonly KompassDbContext _dbContext;

    public EfWirtschaftlichkeitsannahmenRepository(
        KompassDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Wirtschaftlichkeitsannahmen?> NachAlternativeIdAbrufenAsync(
        Guid alternativeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext
            .Set<Wirtschaftlichkeitsannahmen>()
            .Include(a => a.Energietraeger)
            .Where(
                a =>
                    EF.Property<Guid>(a, "ModernisierungsalternativeId") ==
                    alternativeId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Wirtschaftlichkeitsannahmen> SpeichernAsync(
        Guid alternativeId,
        Wirtschaftlichkeitsannahmen annahmen,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(annahmen);

        var vorhandene =
            await _dbContext
                .Set<Wirtschaftlichkeitsannahmen>()
                .Include(a => a.Energietraeger)
                .Where(
                    a =>
                        EF.Property<Guid>(a, "ModernisierungsalternativeId") ==
                        alternativeId)
                .SingleOrDefaultAsync(cancellationToken);

        if (vorhandene is not null)
        {
            _dbContext.Set<Wirtschaftlichkeitsannahmen>().Remove(vorhandene);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        _dbContext.Set<Wirtschaftlichkeitsannahmen>().Add(annahmen);

        _dbContext.Entry(annahmen)
            .Property("ModernisierungsalternativeId")
            .CurrentValue = alternativeId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return annahmen;
    }
}
