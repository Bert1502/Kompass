namespace Kompass.Application.B56Import;

public enum B56SnapshotAktionStatus
{
    Erfolgreich = 0,
    NichtGefunden = 1,
    NichtZulaessig = 2
}

public sealed record B56SnapshotAktionErgebnis(
    B56SnapshotAktionStatus Status,
    B56ImportEintrag? Snapshot,
    string Nachricht);
