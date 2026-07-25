namespace Kompass.Application.Projects;

public sealed record ProjektUebersicht(
    Guid Id,
    string Name,
    int AnzahlAlternativen,
    Guid? QuellSnapshotId = null,
    int ProjektmodellVersion = 0);
