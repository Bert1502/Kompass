namespace Kompass.Application.B56Import;

public sealed class B56Arbeitsmappe
{
    public string Dateipfad { get; init; } = string.Empty;

    public IReadOnlyList<B56Arbeitsblatt> Arbeitsblaetter { get; init; }
        = Array.Empty<B56Arbeitsblatt>();

    public B56Arbeitsblatt? ArbeitsblattSuchen(
        string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return Arbeitsblaetter.FirstOrDefault(
            x => string.Equals(
                x.Name,
                name,
                StringComparison.OrdinalIgnoreCase));
    }
}
