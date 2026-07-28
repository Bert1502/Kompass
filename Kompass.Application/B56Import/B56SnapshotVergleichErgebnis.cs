namespace Kompass.Application.B56Import;

public enum B56SnapshotVergleichStatus
{
    Erfolgreich = 0,
    NichtGefunden = 1,
    NichtVergleichbar = 2
}

public sealed record B56SnapshotVergleichErgebnis(
    B56SnapshotVergleichStatus Status,
    B56SnapshotVergleich? Vergleich,
    string Nachricht);
