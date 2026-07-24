namespace Kompass.Application.B56Import.Domain;

/// <summary>
/// Fachliches Bauteil aus einer B56-Datei.
/// </summary>
public sealed class B56Bauteil
{
    private readonly List<B56Konstruktion> _konstruktionen = [];

    public Guid Id { get; init; }

    /// <summary>
    /// Original-B56-Bauteilcode.
    /// </summary>
    public string Bauteilcode { get; set; } = string.Empty;

    /// <summary>
    /// Kurzbezeichnung.
    /// </summary>
    public string Bezeichnung { get; set; } = string.Empty;

    /// <summary>
    /// Fläche [m²]
    /// </summary>
    public double Flaeche { get; set; }

    /// <summary>
    /// U-Wert [W/(m²K)]
    /// </summary>
    public double UWert { get; set; }

    /// <summary>
    /// Orientierung
    /// </summary>
    public string Orientierung { get; set; } = string.Empty;

    /// <summary>
    /// Neigung
    /// </summary>
    public double Neigung { get; set; }

    /// <summary>
    /// Konstruktionen
    /// </summary>
    public IReadOnlyCollection<B56Konstruktion> Konstruktionen =>
        _konstruktionen;

    public void KonstruktionHinzufuegen(
        B56Konstruktion konstruktion)
    {
        ArgumentNullException.ThrowIfNull(konstruktion);

        _konstruktionen.Add(konstruktion);
    }
}
