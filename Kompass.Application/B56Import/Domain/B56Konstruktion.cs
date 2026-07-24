namespace Kompass.Application.B56Import.Domain;

/// <summary>
/// Schichtaufbau eines Bauteils.
/// </summary>
public sealed class B56Konstruktion
{
    private readonly List<B56Schicht> _schichten = [];

    public Guid Id { get; init; }

    public string Name { get; set; } = string.Empty;

    public IReadOnlyCollection<B56Schicht> Schichten =>
        _schichten;

    public void SchichtHinzufuegen(
        B56Schicht schicht)
    {
        ArgumentNullException.ThrowIfNull(schicht);

        _schichten.Add(schicht);
    }
}
