namespace Kompass.Application.B56Import.Domain;

/// <summary>
/// Eine B56-Variante (Bestand oder Berechnungsvariante).
/// </summary>
public sealed class B56Variante
{
    private readonly List<B56Modernisierungsalternative> _alternativen = [];

    public Guid Id { get; init; }

    public int Nummer { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IstBestand { get; set; }

    public IReadOnlyCollection<B56Modernisierungsalternative>
        Modernisierungsalternativen =>
        _alternativen;

    public void AlternativeHinzufuegen(
        B56Modernisierungsalternative alternative)
    {
        ArgumentNullException.ThrowIfNull(alternative);

        _alternativen.Add(alternative);
    }
}
