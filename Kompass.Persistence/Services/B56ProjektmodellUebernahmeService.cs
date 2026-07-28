using Kompass.Application.B56Import;
using Kompass.Domain.B56;
using Kompass.Domain.Common;
using Kompass.Domain.Projects;
using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Persistence.Services;

public sealed class B56ProjektmodellUebernahmeService
    : IB56ProjektmodellUebernahmeService
{
    private readonly KompassDbContext _dbContext;
    private readonly IB56ImportRegister _importRegister;

    public B56ProjektmodellUebernahmeService(
        KompassDbContext dbContext,
        IB56ImportRegister importRegister)
    {
        _dbContext = dbContext;
        _importRegister = importRegister;
    }

    public async Task<B56ProjektmodellUebernahmeErgebnis> UebernehmenAsync(
        Guid projektId,
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        var projekt =
            await _dbContext.Projekte
                .Include(eintrag => eintrag.Alternativen)
                .SingleOrDefaultAsync(
                    eintrag => eintrag.Id == projektId,
                    cancellationToken);

        var snapshot =
            await _importRegister.NachIdSuchenAsync(
                projektId,
                importId,
                cancellationToken);

        if (projekt is null ||
            snapshot is null)
        {
            return ErzeugeErgebnis(
                B56ProjektmodellUebernahmeStatus.NichtGefunden,
                projektId,
                importId,
                0,
                0,
                "Projekt oder B56-Snapshot wurde nicht gefunden.");
        }

        if (projekt.QuellSnapshotId == importId &&
            snapshot.SnapshotStatus ==
                B56SnapshotStatus.InProjektmodellUebernommen)
        {
            return ErzeugeErgebnis(
                B56ProjektmodellUebernahmeStatus.Erfolgreich,
                projektId,
                importId,
                projekt.ProjektmodellVersion,
                projekt.Alternativen.Count,
                "Der B56-Snapshot wurde bereits in das Projektmodell übernommen.");
        }

        if (snapshot.SnapshotStatus !=
            B56SnapshotStatus.FachlichBestaetigt)
        {
            return ErzeugeErgebnis(
                B56ProjektmodellUebernahmeStatus.NichtZulaessig,
                projektId,
                importId,
                projekt.ProjektmodellVersion,
                0,
                "Nur ein fachlich bestätigter B56-Snapshot kann übernommen werden.");
        }

        var fachdaten =
            await _importRegister.FachdatenAbrufenAsync(
                projektId,
                importId,
                cancellationToken);

        if (fachdaten is null)
        {
            return ErzeugeErgebnis(
                B56ProjektmodellUebernahmeStatus.NichtZulaessig,
                projektId,
                importId,
                projekt.ProjektmodellVersion,
                0,
                "Der B56-Snapshot enthält keine übernehmbaren Fachdaten.");
        }

        var bauteilcodes =
            await _dbContext.Set<Bauteilcode>()
                .ToDictionaryAsync(
                    eintrag => eintrag.Code,
                    StringComparer.OrdinalIgnoreCase,
                    cancellationToken);

        var alternativen =
            fachdaten.Modernisierungsalternativen
                .Select(
                    alternative =>
                        ErzeugeAlternative(
                            alternative,
                            importId,
                            bauteilcodes))
                .ToList();

        try
        {
            var hinzugefuegteAlternativen =
                projekt.AusSnapshotErzeugen(
                importId,
                alternativen);

            _dbContext.Set<Modernisierungsalternative>().AddRange(
                hinzugefuegteAlternativen);
        }
        catch (DomainException exception)
        {
            return ErzeugeErgebnis(
                B56ProjektmodellUebernahmeStatus.NichtZulaessig,
                projektId,
                importId,
                projekt.ProjektmodellVersion,
                0,
                exception.Message);
        }

        await _importRegister.LebenszyklusSpeichernAsync(
            snapshot with
            {
                SnapshotStatus =
                    B56SnapshotStatus.InProjektmodellUebernommen
            },
            cancellationToken);

        return ErzeugeErgebnis(
            B56ProjektmodellUebernahmeStatus.Erfolgreich,
            projektId,
            importId,
            projekt.ProjektmodellVersion,
            alternativen.Count,
            "Der B56-Snapshot wurde in das Projektmodell übernommen.");
    }

    private Modernisierungsalternative ErzeugeAlternative(
        B56Modernisierungsalternative quelle,
        Guid snapshotId,
        IDictionary<string, Bauteilcode> bauteilcodes)
    {
        var alternative =
            new Modernisierungsalternative(
                Guid.NewGuid(),
                quelle.Bezeichnung,
                quelle.Beschreibung,
                snapshotId,
                quelle.Position);

        foreach (var quellBauteil in quelle.Bauteile)
        {
            if (!bauteilcodes.TryGetValue(
                    quellBauteil.Bauteilcode,
                    out var bauteilcode))
            {
                bauteilcode =
                    new Bauteilcode(
                        Guid.NewGuid(),
                        quellBauteil.Bauteilcode,
                        quellBauteil.Bezeichnung);

                _dbContext.Set<Bauteilcode>().Add(
                    bauteilcode);

                bauteilcodes.Add(
                    quellBauteil.Bauteilcode,
                    bauteilcode);
            }

            alternative.BauteilHinzufuegen(
                new AlternativeBauteil(
                    Guid.NewGuid(),
                    bauteilcode));
        }

        return alternative;
    }

    private static B56ProjektmodellUebernahmeErgebnis ErzeugeErgebnis(
        B56ProjektmodellUebernahmeStatus status,
        Guid projektId,
        Guid importId,
        int projektmodellVersion,
        int uebernommeneAlternativen,
        string nachricht)
    {
        return new B56ProjektmodellUebernahmeErgebnis(
            status,
            projektId,
            importId,
            projektmodellVersion,
            uebernommeneAlternativen,
            nachricht);
    }
}
