namespace Kompass.Application.B56Import;

public sealed class B56Bauteilcode
{
    public string Originaltext { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public string Bezeichnung { get; init; } = string.Empty;

    public string Kategorie { get; init; } = string.Empty;

    public bool IstGueltig { get; init; }
}
