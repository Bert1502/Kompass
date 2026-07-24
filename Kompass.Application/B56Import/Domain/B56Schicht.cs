namespace Kompass.Application.B56Import.Domain;

/// <summary>
/// Eine einzelne Materialschicht.
/// </summary>
public sealed class B56Schicht
{
    public Guid Id { get; init; }

    public int Reihenfolge { get; set; }

    public string Materialname { get; set; } = string.Empty;

    public double Dicke { get; set; }

    public double Lambda { get; set; }

    public double Rohdichte { get; set; }

    public double SpezifischeWaermekapazitaet { get; set; }
}
