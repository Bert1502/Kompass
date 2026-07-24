using Kompass.Domain.Common;

namespace Kompass.Domain.Projects;

public sealed class Projekt : AggregateRoot
{
    private readonly List<Modernisierungsalternative> _alternativen = new();

    private Projekt()
    {
        Name = string.Empty;
    }

    public Projekt(Guid id, string name)
        : base(id)
    {
        Name = BereinigeName(name);
    }

    public string Name { get; private set; }

    public IReadOnlyCollection<Modernisierungsalternative> Alternativen =>
        _alternativen.AsReadOnly();

    public void Umbenennen(string name)
    {
        Name = BereinigeName(name);
    }

    public void AlternativeHinzufuegen(
        Modernisierungsalternative alternative)
    {
        ArgumentNullException.ThrowIfNull(alternative);

        if (_alternativen.Any(
                vorhandeneAlternative =>
                    vorhandeneAlternative.Id == alternative.Id))
        {
            throw new DomainException(
                "Die Modernisierungsalternative ist dem Projekt bereits zugeordnet.");
        }

        _alternativen.Add(alternative);
    }

    private static string BereinigeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(
                "Der Projektname darf nicht leer sein.");
        }

        var bereinigterName = name.Trim();

        if (bereinigterName.Length > 200)
        {
            throw new DomainException(
                "Der Projektname darf höchstens 200 Zeichen enthalten.");
        }

        return bereinigterName;
    }
}