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

    public async Task<IReadOnlyList<B56KonfliktEintrag>> ListenAsync(
        Guid projektId,
        Guid vorgaengerSnapshotId,
        Guid nachfolgerSnapshotId,
        CancellationToken cancellationToken = default)
    {
        var vorhandene =
            await _dbContext.B56KonfliktEintraege
                .Where(e =>
                    e.ProjektId == projektId &&
                    e.VorgaengerSnapshotId == vorgaengerSnapshotId &&
                    e.NachfolgerSnapshotId == nachfolgerSnapshotId)
                .ToListAsync(cancellationToken);

        if (vorhandene.Count > 0)
        {
            return vorhandene
                .Select(ZuModell)
                .ToList();
        }

        var vergleich =
            await LadeVergleichAsync(
                projektId,
                vorgaengerSnapshotId,
                nachfolgerSnapshotId,
                cancellationToken);

        if (vergleich is null ||
            vergleich.Konflikte.Count == 0)
        {
            return [];
        }

        var neueEintraege =
            ErstelleEintraege(
                projektId,
                vorgaengerSnapshotId,
                nachfolgerSnapshotId,
                vergleich);

        _dbContext.B56KonfliktEintraege.AddRange(neueEintraege);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return neueEintraege
            .Select(ZuModell)
            .ToList();
    }

    public async Task<B56KonfliktEintrag?> EntscheidenAsync(
        Guid projektId,
        Guid vorgaengerSnapshotId,
        Guid nachfolgerSnapshotId,
        Guid konfliktId,
        B56KonfliktEntscheidungsTyp entscheidung,
        CancellationToken cancellationToken = default)
    {
        if (entscheidung == B56KonfliktEntscheidungsTyp.Ausstehend)
        {
            throw new ArgumentException(
                "Eine Entscheidung muss entweder 'Akzeptiert' oder 'Abgelehnt' sein.",
                nameof(entscheidung));
        }

        var entity =
            await _dbContext.B56KonfliktEintraege
                .SingleOrDefaultAsync(
                    e =>
                        e.KonfliktId == konfliktId &&
                        e.ProjektId == projektId &&
                        e.VorgaengerSnapshotId == vorgaengerSnapshotId &&
                        e.NachfolgerSnapshotId == nachfolgerSnapshotId,
                    cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Entscheidung = (int)entscheidung;
        entity.EntschiedenAm = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ZuModell(entity);
    }

    public async Task<int> AlleAusstehendAkzeptierenAsync(
        Guid projektId,
        Guid vorgaengerSnapshotId,
        Guid nachfolgerSnapshotId,
        CancellationToken cancellationToken = default)
    {
        var ausstehende =
            await _dbContext.B56KonfliktEintraege
                .Where(e =>
                    e.ProjektId == projektId &&
                    e.VorgaengerSnapshotId == vorgaengerSnapshotId &&
                    e.NachfolgerSnapshotId == nachfolgerSnapshotId &&
                    e.Entscheidung ==
                        (int)B56KonfliktEntscheidungsTyp.Ausstehend)
                .ToListAsync(cancellationToken);

        if (ausstehende.Count == 0)
        {
            return 0;
        }

        var jetzt = DateTimeOffset.UtcNow;

        foreach (var entity in ausstehende)
        {
            entity.Entscheidung =
                (int)B56KonfliktEntscheidungsTyp.Akzeptiert;
            entity.EntschiedenAm = jetzt;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ausstehende.Count;
    }

    private async Task<B56SnapshotVergleich?> LadeVergleichAsync(
        Guid projektId,
        Guid vorgaengerSnapshotId,
        Guid nachfolgerSnapshotId,
        CancellationToken cancellationToken)
    {
        var entity =
            await _dbContext.B56SnapshotVergleiche
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    v =>
                        v.ProjektId == projektId &&
                        v.VorgaengerSnapshotId == vorgaengerSnapshotId &&
                        v.NachfolgerSnapshotId == nachfolgerSnapshotId,
                    cancellationToken);

        if (entity is null ||
            string.IsNullOrWhiteSpace(entity.VergleichJson))
        {
            return null;
        }

        return JsonSerializer.Deserialize<B56SnapshotVergleich>(
            entity.VergleichJson,
            JsonOptionen);
    }

    private static IReadOnlyList<B56KonfliktEintragEntity> ErstelleEintraege(
        Guid projektId,
        Guid vorgaengerSnapshotId,
        Guid nachfolgerSnapshotId,
        B56SnapshotVergleich vergleich)
    {
        var kennwerteNachName =
            vergleich.BestandskennwertVergleiche
                .ToDictionary(
                    k => k.Name,
                    StringComparer.OrdinalIgnoreCase);

        var bauteileNachCode =
            vergleich.GesamtbauteilVergleiche
                .ToDictionary(
                    b => b.Bauteilcode,
                    StringComparer.OrdinalIgnoreCase);

        var alternativenNachPosition =
            vergleich.AlternativVergleiche
                .ToDictionary(a => a.B56Position.ToString());

        var eintraege =
            new List<B56KonfliktEintragEntity>(
                vergleich.Konflikte.Count);

        foreach (var konflikt in vergleich.Konflikte)
        {
            var (alterWert, neuerWert) =
                ErmittleWerte(
                    konflikt,
                    kennwerteNachName,
                    bauteileNachCode,
                    alternativenNachPosition);

            eintraege.Add(
                new B56KonfliktEintragEntity
                {
                    KonfliktId = Guid.NewGuid(),
                    ProjektId = projektId,
                    VorgaengerSnapshotId = vorgaengerSnapshotId,
                    NachfolgerSnapshotId = nachfolgerSnapshotId,
                    Bereich = konflikt.Bereich,
                    Schluessel = konflikt.Schluessel,
                    Feld = konflikt.Feld,
                    Aenderung = (int)konflikt.Aenderung,
                    AlterWert = alterWert,
                    NeuerWert = neuerWert,
                    Entscheidung =
                        (int)B56KonfliktEntscheidungsTyp.Ausstehend,
                    EntschiedenAm = null
                });
        }

        return eintraege;
    }

    private static (string? AlterWert, string? NeuerWert) ErmittleWerte(
        B56Vergleichskonflikt konflikt,
        Dictionary<string, B56KennwertVergleich> kennwerte,
        Dictionary<string, B56BauteilVergleich> bauteile,
        Dictionary<string, B56AlternativeVergleich> alternativen)
    {
        if (konflikt.Bereich == "Bestandskennwert" &&
            kennwerte.TryGetValue(
                konflikt.Schluessel,
                out var kennwert))
        {
            return (
                kennwert.AlterWert?.ToString("G"),
                kennwert.NeuerWert?.ToString("G"));
        }

        if (konflikt.Bereich == "Bauteil" &&
            bauteile.TryGetValue(
                konflikt.Schluessel,
                out var bauteil))
        {
            var alt =
                bauteil.AlterUWert.HasValue || bauteil.AlteFlaeche.HasValue
                    ? $"U-Wert: {bauteil.AlterUWert?.ToString("G") ?? "-"}; Fläche: {bauteil.AlteFlaeche?.ToString("G") ?? "-"}"
                    : null;

            var neu =
                bauteil.NeuerUWert.HasValue || bauteil.NeueFlaeche.HasValue
                    ? $"U-Wert: {bauteil.NeuerUWert?.ToString("G") ?? "-"}; Fläche: {bauteil.NeueFlaeche?.ToString("G") ?? "-"}"
                    : null;

            return (alt, neu);
        }

        if (konflikt.Bereich == "Modernisierungsalternative" &&
            alternativen.TryGetValue(
                konflikt.Schluessel,
                out var alternative))
        {
            return (
                string.IsNullOrEmpty(alternative.AlteBezeichnung)
                    ? null
                    : alternative.AlteBezeichnung,
                string.IsNullOrEmpty(alternative.NeueBezeichnung)
                    ? null
                    : alternative.NeueBezeichnung);
        }

        return (null, null);
    }

    private static B56KonfliktEintrag ZuModell(
        B56KonfliktEintragEntity entity)
    {
        return new B56KonfliktEintrag(
            entity.KonfliktId,
            entity.ProjektId,
            entity.VorgaengerSnapshotId,
            entity.NachfolgerSnapshotId,
            entity.Bereich,
            entity.Schluessel,
            entity.Feld,
            (B56VergleichsAenderung)entity.Aenderung,
            entity.AlterWert,
            entity.NeuerWert,
            (B56KonfliktEntscheidungsTyp)entity.Entscheidung,
            entity.EntschiedenAm);
    }
}
