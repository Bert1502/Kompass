namespace Kompass.Desktop.Models;

public sealed record ProjektUebersichtDto(
    Guid Id,
    string Name,
    int AnzahlAlternativen);
