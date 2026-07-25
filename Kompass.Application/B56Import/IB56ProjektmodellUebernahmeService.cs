namespace Kompass.Application.B56Import;

public interface IB56ProjektmodellUebernahmeService
{
    Task<B56ProjektmodellUebernahmeErgebnis> UebernehmenAsync(
        Guid projektId,
        Guid importId,
        CancellationToken cancellationToken = default);
}

public enum B56ProjektmodellUebernahmeStatus
{
    Erfolgreich = 0,
    NichtGefunden = 1,
    NichtZulaessig = 2
}

public sealed record B56ProjektmodellUebernahmeErgebnis(
    B56ProjektmodellUebernahmeStatus Status,
    Guid ProjektId,
    Guid ImportId,
    int ProjektmodellVersion,
    int UebernommeneAlternativen,
    string Nachricht);
