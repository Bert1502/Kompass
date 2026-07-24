namespace Kompass.Application.B56Import;

public sealed record B56DateiPruefung
{
    public bool IstGueltig { get; init; }

    public string Fehlercode { get; init; } = string.Empty;

    public string Fehlermeldung { get; init; } = string.Empty;
}
