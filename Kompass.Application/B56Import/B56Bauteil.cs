namespace Kompass.Application.B56Import;

public sealed class B56Bauteil
{
    public string Bauteilcode { get; init; } = string.Empty;

    public string Bezeichnung { get; init; } = string.Empty;

    public string Nachbarseite { get; init; } = string.Empty;

    public double Flaeche { get; init; }

    public double UWert { get; init; }
}
