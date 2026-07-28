using Kompass.Application.Economics;
using Kompass.Domain.Economics;
using Kompass.Domain.Projects;
using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Persistence.Services;

public sealed class EfWirtschaftlichkeitsService : IWirtschaftlichkeitsService
{
    private readonly KompassDbContext _dbContext;

    public EfWirtschaftlichkeitsService(
        KompassDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Wirtschaftlichkeitsannahmen?> AnnahmenAbrufenAsync(
        Guid projektId,
        Guid alternativeId,
        WirtschaftlichkeitsBasis basis,
        CancellationToken cancellationToken = default)
    {
        if (!await AlternativeGehoertZuProjektAsync(
                projektId,
                alternativeId,
                cancellationToken))
        {
            return null;
        }

        return await _dbContext.Wirtschaftlichkeitsannahmen
            .Include(
                annahmen => annahmen.EnergietraegerAnnahmen)
            .SingleOrDefaultAsync(
                annahmen =>
                    annahmen.ModernisierungsalternativeId == alternativeId &&
                    annahmen.Basis == basis,
                cancellationToken);
    }

    public async Task<Wirtschaftlichkeitsannahmen> AnnahmenSpeichernAsync(
        Wirtschaftlichkeitsannahmen annahmen,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(annahmen);

        var vorhandene = await _dbContext.Wirtschaftlichkeitsannahmen
            .SingleOrDefaultAsync(
                a =>
                    a.ModernisierungsalternativeId ==
                        annahmen.ModernisierungsalternativeId &&
                    a.Basis == annahmen.Basis,
                cancellationToken);

        if (vorhandene is null)
        {
            _dbContext.Wirtschaftlichkeitsannahmen.Add(annahmen);
        }
        else if (vorhandene.Id != annahmen.Id)
        {
            _dbContext.Wirtschaftlichkeitsannahmen.Remove(vorhandene);
            _dbContext.Wirtschaftlichkeitsannahmen.Add(annahmen);
        }
        else
        {
            _dbContext.Wirtschaftlichkeitsannahmen.Update(annahmen);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return annahmen;
    }

    public async Task<Wirtschaftlichkeitsergebnis?> BerechnenAsync(
        Guid projektId,
        Guid alternativeId,
        WirtschaftlichkeitsBasis basis,
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

        var annahmen = await _dbContext.Wirtschaftlichkeitsannahmen
            .Include(a => a.EnergietraegerAnnahmen)
            .SingleOrDefaultAsync(
                a =>
                    a.ModernisierungsalternativeId == alternativeId &&
                    a.Basis == basis,
                cancellationToken);

        if (annahmen is null)
        {
            return null;
        }

        return annahmen.Berechnen(
            alternative.Gesamtkosten);
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
