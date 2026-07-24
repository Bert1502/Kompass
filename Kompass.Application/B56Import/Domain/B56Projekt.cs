namespace Kompass.Application.B56Import.Domain;

/// <summary>
/// Fachliches Modell eines vollständigen B56-Projekts.
/// Dieses Objekt bildet den Inhalt einer importierten
/// B56-Datei unabhängig vom Dateiformat ab.
/// </summary>
public sealed class B56Projekt
{
    private readonly List<B56Gebaeude> _gebaeude = [];
    private readonly List<B56Variante> _varianten = [];
    private readonly List<B56Modernisierungsalternative> _modernisierungsalternativen = [];

    public Guid Id { get; init; }

    public string Projektname { get; set; } = string.Empty;

    public string ProjektNummer { get; set; } = string.Empty;

    public DateTimeOffset Importzeitpunkt { get; set; }

    public IReadOnlyCollection<B56Gebaeude> Gebaeude =>
        _gebaeude;

    public B56Bestand Bestand { get; } = new();

    public IReadOnlyCollection<B56Variante> Varianten =>
        _varianten;

    public IReadOnlyCollection<B56Modernisierungsalternative> Modernisierungsalternativen =>
        _modernisierungsalternativen;

    public void GebaeudeHinzufuegen(
        B56Gebaeude gebaeude)
    {
        ArgumentNullException.ThrowIfNull(gebaeude);

        _gebaeude.Add(gebaeude);
    }

    public void VarianteHinzufuegen(
        B56Variante variante)
    {
        ArgumentNullException.ThrowIfNull(variante);

        _varianten.Add(variante);
    }

    public void ModernisierungsalternativeHinzufuegen(
        B56Modernisierungsalternative alternative)
    {
        ArgumentNullException.ThrowIfNull(alternative);

        _modernisierungsalternativen.Add(alternative);
    }
}
