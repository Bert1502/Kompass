namespace Kompass.Application.B56Import.Domain;

/// <summary>
/// Entspricht einer Modernisierungsalternative
/// innerhalb einer Variante.
/// </summary>
public sealed class B56Modernisierungsalternative
{
    public Guid Id { get; init; }

    public int Nummer { get; set; }

    public string Kurztext { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public bool Aktiv { get; set; }
}
