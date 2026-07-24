namespace Kompass.Application.B56Import;

public sealed class B56Zelle
{
    public string Adresse { get; init; } = string.Empty;

    public string Spalte { get; init; } = string.Empty;

    public int Zeile { get; init; }

    public string Wert { get; init; } = string.Empty;
}
