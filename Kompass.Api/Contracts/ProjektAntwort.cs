namespace Kompass.Api.Contracts;

public sealed record ProjektAntwort(
    Guid Id,
    string Name,
    int AnzahlAlternativen);