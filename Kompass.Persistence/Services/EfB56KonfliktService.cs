using Kompass.Application.B56Import;
using Kompass.Persistence.Data;
using Kompass.Persistence.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kompass.Persistence.Services;

public sealed class EfB56KonfliktService : IB56KonfliktService
{
    private static readonly JsonSerializerOptions JsonOptionen =
        new(JsonSerializerDefaults.Web);

    private readonly KompassDbContext _dbContext;

    public EfB56KonfliktService(
        KompassDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<B56KonfliktEintrag>> ListenOderErzeugenAsync(
        Guid projektId,
        Guid vorgaengerImportId,
        Guid nachfolgerImportId,
        CancellationToken cancellationToken = default)
    {
        var vorhandene = await _dbContext.B56KonfliktEintraege
            .Where(k =>
                k.ProjektId == projektId &&
                k.VorgaengerImportId == vorgaengerImportId &&
                k.NachfolgerImportId == nachfolgerImportId)
            .ToListAsync(cancellationToken);

        if (vorhandene.Count > 0)
        {
            return vorhandene
                .OrderBy(k => k.Bereich)
                .ThenBy(k => k.Schluessel)
                .Select(ZuModell)
                .ToList();
        }

        var vergleichEntity =
            await _dbContext.B56SnapshotVergleiche
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    v =>
                        v.ProjektId == projektId &&
                        v.VorgaengerSnapshotId == vorgaengerImportId &&
                        v.NachfolgerSnapshotId == nachfolgerImportId,
                    cancellationToken);

        if (vergleichEntity is null ||
            string.IsNullOrWhiteSpace(vergleichEntity.VergleichJson))
        {
            return Array.Empty<B56KonfliktEintrag>();
        }

        B56SnapshotVergleich vergleich;

        try
        {
            vergleich =
                JsonSerializer.Deserialize<B56SnapshotVergleich>(
                    vergleichEntity.VergleichJson,
                    JsonOptionen)
                ?? throw new InvalidOperationException(
                    "Das persistierte Vergleichsergebnis ist leer.");
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "Das persistierte Vergleichsergebnis ist beschädigt.",
                exception);
        }

        var jetzt = DateTimeOffset.UtcNow;

        var neueEintraege = vergleich.Konflikte
            .Select(k => new B56KonfliktEintragEntity
            {
                Id = Guid.NewGuid(),
                ProjektId = projektId,
                VorgaengerImportId = vorgaengerImportId,
                NachfolgerImportId = nachfolgerImportId,
                Bereich = k.Bereich,
                Schluessel = k.Schluessel,
                Feld = k.Feld,
                Aenderung = k.Aenderung,
                Entscheidung = B56KonfliktEntscheidungsTyp.Offen,
                ErstelltAm = jetzt
            })
            .ToList();

        if (neueEintraege.Count > 0)
        {
            _dbContext.B56KonfliktEintraege.AddRange(neueEintraege);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return neueEintraege
            .OrderBy(k => k.Bereich)
            .ThenBy(k => k.Schluessel)
            .Select(ZuModell)
            .ToList();
    }

    public async Task<bool> EntscheidungSetzenAsync(
        Guid projektId,
        Guid nachfolgerImportId,
        Guid id,
        B56KonfliktEntscheidungsTyp entscheidung,
        CancellationToken cancellationToken = default)
    {
        var entity =
            await _dbContext.B56KonfliktEintraege
                .SingleOrDefaultAsync(
                    k =>
                        k.Id == id &&
                        k.ProjektId == projektId &&
                        k.NachfolgerImportId == nachfolgerImportId,
                    cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.Entscheidung = entscheidung;
        entity.EntschiedenAm = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static B56KonfliktEintrag ZuModell(
        B56KonfliktEintragEntity entity)
    {
        return new B56KonfliktEintrag
        {
            Id = entity.Id,
            ProjektId = entity.ProjektId,
            VorgaengerImportId = entity.VorgaengerImportId,
            NachfolgerImportId = entity.NachfolgerImportId,
            Bereich = entity.Bereich,
            Schluessel = entity.Schluessel,
            Feld = entity.Feld,
            Aenderung = entity.Aenderung,
            Entscheidung = entity.Entscheidung,
            EntschiedenAm = entity.EntschiedenAm,
            ErstelltAm = entity.ErstelltAm
        };
    }
}
