namespace Kompass.Application.B56Import.Domain;

/// <summary>
/// Fachliches Gebäudemodell des B56-Imports.
/// </summary>
public sealed class B56Gebaeude
{
    public Guid Id { get; init; }

    public string Bezeichnung { get; set; } = string.Empty;

    public string Adresse { get; set; } = string.Empty;

    public double Energiebezugsflaeche { get; set; }

    public double Bruttogrundflaeche { get; set; }

    public double Bruttorauminhalt { get; set; }

    public int Baujahr { get; set; }
}
