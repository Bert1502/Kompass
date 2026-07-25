namespace Kompass.Desktop.Models;

public sealed record ProjektUebersichtDto(
    Guid Id,
    string Name,
    int AnzahlAlternativen,
    Guid? QuellSnapshotId = null,
    int ProjektmodellVersion = 0);
