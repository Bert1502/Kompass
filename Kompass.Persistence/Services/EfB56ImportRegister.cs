using Kompass.Application.B56Import;
using Kompass.Persistence.Data;
using Kompass.Persistence.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kompass.Persistence.Services;

public sealed class EfB56ImportRegister : IB56ImportRegister
{
    private static readonly JsonSerializerOptions JsonOptionen =
        new(JsonSerializerDefaults.Web);

    private readonly KompassDbContext _dbContext;

    public EfB56ImportRegister(
        KompassDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<B56ImportEintrag?> NachHashSuchenAsync(
        Guid projektId,
        string sha256,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sha256);

        var entities = await _dbContext.B56ImportEintraege
            .AsNoTracking()
            .Where(x =>
                x.ProjektId == projektId &&
                x.Sha256 == sha256)
            .ToListAsync(cancellationToken);

        var entity = entities
            .OrderByDescending(x => x.ImportiertAm)
            .FirstOrDefault();

        return entity is null
            ? null
            : ZuModell(entity);
    }

    public async Task<IReadOnlyList<B56ImportEintrag>> AlleFuerProjektAbrufenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.B56ImportEintraege
            .AsNoTracking()
            .Where(x => x.ProjektId == projektId)
            .ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.ImportiertAm)
            .Select(ZuModell)
            .ToList();
    }

    public async Task<B56ImportEintrag?> NachIdSuchenAsync(
        Guid projektId,
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        var entity =
            await _dbContext.B56ImportEintraege
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    eintrag =>
                        eintrag.ProjektId == projektId &&
                        eintrag.ImportId == importId,
                    cancellationToken);

        return entity is null
            ? null
            : ZuModell(entity);
    }

    public async Task EintragSpeichernAsync(
        B56ImportEintrag eintrag,
        CancellationToken cancellationToken = default)
    {
        await EintragSpeichernAsync(
            eintrag,
            fachdaten: null,
            cancellationToken);
    }

    public async Task EintragMitFachdatenSpeichernAsync(
        B56ImportEintrag eintrag,
        B56ImportPipelineErgebnis fachdaten,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fachdaten);

        await EintragSpeichernAsync(
            eintrag,
            fachdaten,
            cancellationToken);
    }

    public async Task<B56ImportPipelineErgebnis?>
        FachdatenAbrufenAsync(
            Guid projektId,
            Guid importId,
            CancellationToken cancellationToken = default)
    {
        var entity =
            await _dbContext.B56ImportEintraege
                .AsNoTracking()
                .Where(
                    eintrag =>
                        eintrag.ProjektId == projektId &&
                        eintrag.ImportId == importId)
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (entity is null ||
            string.IsNullOrWhiteSpace(
                entity.FachdatenJson))
        {
            return null;
        }

        if (entity.SnapshotSchemaVersion !=
            B56SnapshotVersionen.AktuelleSchemaVersion)
        {
            throw new B56SnapshotFormatException(
                importId,
                $"Die B56-Snapshot-Schemaversion '{entity.SnapshotSchemaVersion}' wird nicht unterstützt.");
        }

        try
        {
            var fachdaten =
                JsonSerializer.Deserialize<
                    B56ImportPipelineErgebnis>(
                        entity.FachdatenJson,
                        JsonOptionen)
                ?? throw new B56SnapshotFormatException(
                    importId,
                    "Der B56-Snapshot enthält keine lesbaren Fachdaten.");

            for (var index = 0;
                 index < fachdaten.Modernisierungsalternativen.Count;
                 index++)
            {
                var alternative =
                    fachdaten.Modernisierungsalternativen[index];

                if (alternative.Position == 0)
                {
                    alternative.Position =
                        index + 1;
                }
            }

            return fachdaten;
        }
        catch (JsonException exception)
        {
            throw new B56SnapshotFormatException(
                importId,
                "Der B56-Snapshot enthält beschädigte Fachdaten.",
                exception);
        }
    }

    public async Task<B56SnapshotVergleich?> VergleichAbrufenAsync(
        Guid projektId,
        Guid vorgaengerSnapshotId,
        Guid nachfolgerSnapshotId,
        CancellationToken cancellationToken = default)
    {
        var entity =
            await _dbContext.B56SnapshotVergleiche
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    vergleich =>
                        vergleich.ProjektId == projektId &&
                        vergleich.VorgaengerSnapshotId ==
                            vorgaengerSnapshotId &&
                        vergleich.NachfolgerSnapshotId ==
                            nachfolgerSnapshotId,
                    cancellationToken);

        if (entity is null ||
            string.IsNullOrWhiteSpace(entity.VergleichJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<
                       B56SnapshotVergleich>(
                       entity.VergleichJson,
                       JsonOptionen);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "Das persistierte Vergleichsergebnis ist beschädigt.",
                exception);
        }
    }

    public async Task VergleichSpeichernAsync(
        B56SnapshotVergleich vergleich,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(vergleich);

        var entity =
            await _dbContext.B56SnapshotVergleiche
                .SingleOrDefaultAsync(
                    vorhandenerVergleich =>
                        vorhandenerVergleich.ProjektId ==
                            vergleich.ProjektId &&
                        vorhandenerVergleich.VorgaengerSnapshotId ==
                            vergleich.VorgaengerSnapshotId &&
                        vorhandenerVergleich.NachfolgerSnapshotId ==
                            vergleich.NachfolgerSnapshotId,
                    cancellationToken);

        if (entity is null)
        {
            _dbContext.B56SnapshotVergleiche.Add(
                new B56SnapshotVergleichEntity
                {
                    VergleichId = Guid.NewGuid(),
                    ProjektId = vergleich.ProjektId,
                    VorgaengerSnapshotId =
                        vergleich.VorgaengerSnapshotId,
                    NachfolgerSnapshotId =
                        vergleich.NachfolgerSnapshotId,
                    HatAenderungen =
                        vergleich.HatAenderungen,
                    VergleichJson =
                        JsonSerializer.Serialize(
                            vergleich,
                            JsonOptionen),
                    ErstelltAm = DateTimeOffset.UtcNow
                });
        }
        else
        {
            entity.HatAenderungen =
                vergleich.HatAenderungen;
            entity.VergleichJson =
                JsonSerializer.Serialize(
                    vergleich,
                    JsonOptionen);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task LebenszyklusSpeichernAsync(
        B56ImportEintrag eintrag,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eintrag);

        var entity =
            await _dbContext.B56ImportEintraege
                .SingleOrDefaultAsync(
                    vorhandenerEintrag =>
                        vorhandenerEintrag.ProjektId ==
                            eintrag.ProjektId &&
                        vorhandenerEintrag.ImportId ==
                            eintrag.ImportId,
                    cancellationToken)
            ?? throw new InvalidOperationException(
                "Der zu aktualisierende B56-Snapshot wurde nicht gefunden.");

        entity.SnapshotStatus =
            eintrag.SnapshotStatus;
        entity.BestaetigtAm =
            eintrag.BestaetigtAm;
        entity.VerworfenAm =
            eintrag.VerworfenAm;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private async Task EintragSpeichernAsync(
        B56ImportEintrag eintrag,
        B56ImportPipelineErgebnis? fachdaten,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(eintrag);

        _dbContext.B56ImportEintraege.Add(
            ZuEntity(
                eintrag,
                fachdaten));

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static B56ImportEintragEntity ZuEntity(
        B56ImportEintrag eintrag,
        B56ImportPipelineErgebnis? fachdaten)
    {
        return new B56ImportEintragEntity
        {
            ImportId = eintrag.ImportId,
            ProjektId = eintrag.ProjektId,
            Projektname = eintrag.Projektname,
            Originaldateiname = eintrag.Originaldateiname,
            Archivdateipfad = eintrag.Archivdateipfad,
            Sha256 = eintrag.Sha256,
            DateigroesseBytes = eintrag.DateigroesseBytes,
            ImportiertAm = eintrag.ImportiertAm,
            Dateiendung = eintrag.Dateiendung,
            FachdatenJson =
                fachdaten is null
                    ? null
                    : JsonSerializer.Serialize(
                        fachdaten,
                        JsonOptionen),
            SnapshotSchemaVersion =
                eintrag.SnapshotSchemaVersion,
            ParserVersion =
                eintrag.ParserVersion,
            SnapshotStatus =
                eintrag.SnapshotStatus,
            BestaetigtAm =
                eintrag.BestaetigtAm,
            VerworfenAm =
                eintrag.VerworfenAm
        };
    }

    private static B56ImportEintrag ZuModell(
        B56ImportEintragEntity entity)
    {
        return new B56ImportEintrag
        {
            ImportId = entity.ImportId,
            ProjektId = entity.ProjektId,
            Projektname = entity.Projektname,
            Originaldateiname = entity.Originaldateiname,
            Archivdateipfad = entity.Archivdateipfad,
            Sha256 = entity.Sha256,
            DateigroesseBytes = entity.DateigroesseBytes,
            ImportiertAm = entity.ImportiertAm,
            Dateiendung = entity.Dateiendung,
            SnapshotSchemaVersion =
                entity.SnapshotSchemaVersion,
            ParserVersion =
                entity.ParserVersion,
            SnapshotStatus =
                entity.SnapshotStatus,
            BestaetigtAm =
                entity.BestaetigtAm,
            VerworfenAm =
                entity.VerworfenAm
        };
    }
}
