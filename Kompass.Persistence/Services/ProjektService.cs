using Kompass.Application.Projects;
using Kompass.Domain.Projects;
using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Persistence.Services;

public sealed class ProjektService : IProjektService
{
    private readonly KompassDbContext _dbContext;

    public ProjektService(KompassDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProjektUebersicht>> AlleAbrufenAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projekte
            .AsNoTracking()
            .OrderBy(projekt => projekt.Name)
            .Select(projekt => new ProjektUebersicht(
                projekt.Id,
                projekt.Name,
                projekt.Alternativen.Count,
                projekt.QuellSnapshotId,
                projekt.ProjektmodellVersion))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjektUebersicht?> NachIdAbrufenAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        return await _dbContext.Projekte
            .AsNoTracking()
            .Where(projekt => projekt.Id == id)
            .Select(projekt => new ProjektUebersicht(
                projekt.Id,
                projekt.Name,
                projekt.Alternativen.Count,
                projekt.QuellSnapshotId,
                projekt.ProjektmodellVersion))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ProjektUebersicht> ErstellenAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var bereinigterName = name.Trim();

        var namensgleichesProjektVorhanden =
            await _dbContext.Projekte
                .AsNoTracking()
                .AnyAsync(
                    projekt => projekt.Name == bereinigterName,
                    cancellationToken);

        if (namensgleichesProjektVorhanden)
        {
            throw new InvalidOperationException(
                $"Ein Projekt mit dem Namen '{bereinigterName}' ist bereits vorhanden.");
        }

        var projekt = new Projekt(
            Guid.NewGuid(),
            bereinigterName);

        _dbContext.Projekte.Add(projekt);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ErzeugeUebersicht(projekt);
    }

    public async Task<ProjektUebersicht?> AktualisierenAsync(
        Guid id,
        string name,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var bereinigterName = name.Trim();

        var projekt = await _dbContext.Projekte
            .Include(eintrag => eintrag.Alternativen)
            .SingleOrDefaultAsync(
                eintrag => eintrag.Id == id,
                cancellationToken);

        if (projekt is null)
        {
            return null;
        }

        var namensgleichesProjektVorhanden =
            await _dbContext.Projekte
                .AsNoTracking()
                .AnyAsync(
                    eintrag =>
                        eintrag.Id != id &&
                        eintrag.Name == bereinigterName,
                    cancellationToken);

        if (namensgleichesProjektVorhanden)
        {
            throw new InvalidOperationException(
                $"Ein anderes Projekt mit dem Namen '{bereinigterName}' ist bereits vorhanden.");
        }

        projekt.Umbenennen(bereinigterName);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ErzeugeUebersicht(projekt);
    }

    public async Task<ProjektUebersicht?> ProjektdatenAktualisierenAsync(
        Guid id,
        string? interneBezeichnung,
        Bearbeitungsstatus bearbeitungsstatus,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var projekt = await _dbContext.Projekte
            .Include(eintrag => eintrag.Alternativen)
            .SingleOrDefaultAsync(
                eintrag => eintrag.Id == id,
                cancellationToken);

        if (projekt is null)
        {
            return null;
        }

        projekt.ProjektdatenAktualisieren(
            interneBezeichnung,
            bearbeitungsstatus);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ErzeugeUebersicht(projekt);
    }

    public async Task<bool> LoeschenAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var projekt = await _dbContext.Projekte
            .SingleOrDefaultAsync(
                eintrag => eintrag.Id == id,
                cancellationToken);

        if (projekt is null)
        {
            return false;
        }

        _dbContext.Projekte.Remove(projekt);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static ProjektUebersicht ErzeugeUebersicht(
        Projekt projekt)
    {
        return new ProjektUebersicht(
            projekt.Id,
            projekt.Name,
            projekt.Alternativen.Count,
            projekt.QuellSnapshotId,
            projekt.ProjektmodellVersion,
            projekt.InterneBezeichnung,
            projekt.Bearbeitungsstatus);
    }
}
