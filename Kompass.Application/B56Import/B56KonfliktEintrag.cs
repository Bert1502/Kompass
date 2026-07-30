namespace Kompass.Application.B56Import;

public sealed record B56KonfliktEintrag(
    Guid KonfliktId,
    Guid ProjektId,
    Guid VorgaengerSnapshotId,
    Guid NachfolgerSnapshotId,
    string Bereich,
    string Schluessel,
    string Feld,
    B56VergleichsAenderung Aenderung,
    string? AlterWert,
    string? NeuerWert,
    B56KonfliktEntscheidungsTyp Entscheidung,
    DateTimeOffset? EntschiedenAm);
