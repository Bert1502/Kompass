namespace Kompass.Application.B56Import.Domain;

/// <summary>
/// Fenster bzw. Fenstertür.
/// </summary>
public sealed class B56Fenster
{
    public Guid Id { get; init; }

    public string Bezeichnung { get; set; } = string.Empty;

    public double Flaeche { get; set; }

    public double UWert { get; set; }

    public double GWert { get; set; }

    public double Rahmenanteil { get; set; }

    public string Orientierung { get; set; } = string.Empty;
}
