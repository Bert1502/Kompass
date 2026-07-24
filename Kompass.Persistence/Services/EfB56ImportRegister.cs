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
        var fachdatenJson =
            await _dbContext.B56ImportEintraege
                .AsNoTracking()
                .Where(
                    eintrag =>
                        eintrag.ProjektId == projektId &&
                        eintrag.ImportId == importId)
                .Select(
                    eintrag =>
                        eintrag.FachdatenJson)
                .SingleOrDefaultAsync(
                    cancellationToken);

        return string.IsNullOrWhiteSpace(
                fachdatenJson)
            ? null
            : JsonSerializer.Deserialize<
                B56ImportPipelineErgebnis>(
                    fachdatenJson,
                    JsonOptionen);
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
                        JsonOptionen)
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
            Dateiendung = entity.Dateiendung
        };
    }
}
