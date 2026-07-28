using Kompass.Application.Economics;
using Kompass.Domain.Economics;
using Kompass.Domain.Projects;
using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Persistence.Services;

public sealed class EfKostenpositionService : IKostenpositionService
{
    private readonly KompassDbContext _dbContext;

    public EfKostenpositionService(
        KompassDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Kostenposition>> ListenAsync(
        Guid projektId,
        Guid alternativeId,
        CancellationToken cancellationToken = default)
    {
        var alternative =
            await LadeAlternativeAsync(
                projektId,
                alternativeId,
                cancellationToken);

        if (alternative is null)
        {
            return [];
        }

        return alternative.Kostenpositionen.ToList();
    }

    public async Task<Kostenposition?> HinzufuegenAsync(
        Guid projektId,
        Guid alternativeId,
        Kostenposition kostenposition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kostenposition);

        var alternative =
            await LadeAlternativeAsync(
                projektId,
                alternativeId,
                cancellationToken);

        if (alternative is null)
        {
            return null;
        }

        alternative.KostenpositionHinzufuegen(kostenposition);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return kostenposition;
    }

    public async Task<bool> EntfernenAsync(
        Guid projektId,
        Guid alternativeId,
        Guid kostenpositionId,
        CancellationToken cancellationToken = default)
    {
        var alternative =
            await LadeAlternativeAsync(
                projektId,
                alternativeId,
                cancellationToken);

        if (alternative is null)
        {
            return false;
        }

        var kostenposition =
            alternative.Kostenpositionen
                .SingleOrDefault(k => k.Id == kostenpositionId);

        if (kostenposition is null)
        {
            return false;
        }

        _dbContext.Set<Kostenposition>().Remove(kostenposition);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<Modernisierungsalternative?> LadeAlternativeAsync(
        Guid projektId,
        Guid alternativeId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Set<Modernisierungsalternative>()
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
    }
}
