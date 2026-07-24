namespace Kompass.Application.Projects;

public sealed record ProjektUebersicht(
    Guid Id,
    string Name,
    int AnzahlAlternativen);