namespace Kompass.Application.B56Import.Domain;

/// <summary>
/// Fachliches Anlagenobjekt.
/// </summary>
public sealed class B56Anlage
{
    public Guid Id { get; init; }

    public string Bezeichnung { get; set; } = string.Empty;

    public string Kategorie { get; set; } = string.Empty;

    public string Energietraeger { get; set; } = string.Empty;

    public double Wirkungsgrad { get; set; }
}
