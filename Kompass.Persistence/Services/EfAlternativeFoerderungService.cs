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

        var investitionskosten = alternative.Gesamtkosten;

        var anteile = new List<ProgrammFoerderungsanteil>();

        foreach (var programm in programme)
        {
            var anteil = BerechneProgrammAnteil(
                programm,
                investitionskosten,
                stichtag);

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
        DateOnly stichtag)
    {
        var aktiveQuote = programm.Foerderquoten
            .Where(
                r =>
                    r.GueltigAb <= stichtag &&
                    (r.GueltigBis is null || r.GueltigBis >= stichtag))
            .OrderByDescending(r => r.GueltigAb)
            .FirstOrDefault();

        var foerderbetrag = aktiveQuote is not null
            ? Math.Round(aktiveQuote.Quote * investitionskosten, 2)
            : 0m;

        var aktiverHoechstbetrag = programm.Hoechstbetraege
            .Where(
                r =>
                    r.GueltigAb <= stichtag &&
                    (r.GueltigBis is null || r.GueltigBis >= stichtag))
            .OrderByDescending(r => r.GueltigAb)
            .FirstOrDefault();

        if (aktiverHoechstbetrag is not null)
        {
            foerderbetrag = Math.Min(foerderbetrag, aktiverHoechstbetrag.Betrag);
        }

        var aktiveKumulierbarkeit = programm.Kumulierbarkeitsregeln
            .Where(
                r =>
                    r.GueltigAb <= stichtag &&
                    (r.GueltigBis is null || r.GueltigBis >= stichtag))
            .OrderByDescending(r => r.GueltigAb)
            .FirstOrDefault();

        var kumulierbarkeit = aktiveKumulierbarkeit?.Status
            ?? KumulierbarkeitStatus.Unbestimmt;

        return new ProgrammFoerderungsanteil(
            programm.Id,
            programm.Programmkennung,
            programm.Version,
            foerderbetrag,
            kumulierbarkeit);
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
