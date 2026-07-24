namespace Kompass.Application.B56Import;

public sealed class B56Zeile
{
    public int Zeilennummer { get; init; }

    public IReadOnlyList<B56Zelle> Zellen { get; init; }
        = Array.Empty<B56Zelle>();

    public string? WertAusSpalte(string spalte)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(spalte);

        return Zellen
            .FirstOrDefault(x =>
                string.Equals(
                    x.Spalte,
                    spalte,
                    StringComparison.OrdinalIgnoreCase))
            ?.Wert;
    }
}
