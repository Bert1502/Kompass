using Kompass.Application.Projects;
using Kompass.Domain.Economics;
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
                projekt.ProjektmodellVersion,
                projekt.InterneBezeichnung,
                projekt.Bearbeitungsstatus,
                projekt.Auftraggeber,
                projekt.Ansprechpartner,
                projekt.Strasse,
                projekt.Ort,
                projekt.Postleitzahl,
                projekt.Gebaeudeart,
                projekt.Freigabestatus,
                projekt.FreigegebenAm,
                projekt.Notizen))
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
                projekt.ProjektmodellVersion,
                projekt.InterneBezeichnung,
                projekt.Bearbeitungsstatus,
                projekt.Auftraggeber,
                projekt.Ansprechpartner,
                projekt.Strasse,
                projekt.Ort,
                projekt.Postleitzahl,
                projekt.Gebaeudeart,
                projekt.Freigabestatus,
                projekt.FreigegebenAm,
                projekt.Notizen))
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

    public async Task<ProjektUebersicht?> StammdatenAktualisierenAsync(
        Guid id,
        string? auftraggeber,
        string? ansprechpartner,
        string? strasse,
        string? ort,
        string? postleitzahl,
        string? gebaeudeart,
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

        projekt.StammdatenAktualisieren(
            auftraggeber,
            ansprechpartner,
            strasse,
            ort,
            postleitzahl,
            gebaeudeart);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ErzeugeUebersicht(projekt);
    }

    public async Task<ProjektUebersicht?> FreigabestatusAktualisierenAsync(
        Guid id,
        Freigabestatus status,
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

        projekt.FreigabestatusAktualisieren(status);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ErzeugeUebersicht(projekt);
    }

    public async Task<ProjektUebersicht?> NotizenAktualisierenAsync(
        Guid id,
        string? notizen,
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

        projekt.NotizenAktualisieren(notizen);

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

    public async Task<AlternativeKurzinfo?> AlternativeNachIdAbrufenAsync(
        Guid projektId,
        Guid alternativeId,
        CancellationToken cancellationToken = default)
    {
        if (projektId == Guid.Empty ||
            alternativeId == Guid.Empty)
        {
            return null;
        }

        var alternative = await _dbContext
            .Set<Modernisierungsalternative>()
            .AsNoTracking()
            .Include(a => a.Kostenpositionen)
            .Where(
                a =>
                    a.Id == alternativeId &&
                    EF.Property<Guid?>(a, "ProjektId") == projektId)
            .SingleOrDefaultAsync(cancellationToken);

        return alternative is null
            ? null
            : new AlternativeKurzinfo(
                alternative.Id,
                alternative.Bezeichnung,
                projektId,
                alternative.Gesamtkosten);
    }

    public async Task<IReadOnlyList<AlternativeKurzinfo>> AlternativenAbrufenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default)
    {
        if (projektId == Guid.Empty)
        {
            return Array.Empty<AlternativeKurzinfo>();
        }

        var alternativen = await _dbContext
            .Set<Modernisierungsalternative>()
            .AsNoTracking()
            .Include(a => a.Kostenpositionen)
            .Where(a => EF.Property<Guid?>(a, "ProjektId") == projektId)
            .OrderBy(a => a.B56Position)
            .ThenBy(a => a.Bezeichnung)
            .ToListAsync(cancellationToken);

        return alternativen
            .Select(a => new AlternativeKurzinfo(
                a.Id,
                a.Bezeichnung,
                projektId,
                a.Gesamtkosten))
            .ToList();
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
            projekt.Bearbeitungsstatus,
            projekt.Auftraggeber,
            projekt.Ansprechpartner,
            projekt.Strasse,
            projekt.Ort,
            projekt.Postleitzahl,
            projekt.Gebaeudeart,
            projekt.Freigabestatus,
            projekt.FreigegebenAm,
            projekt.Notizen);
    }
}
