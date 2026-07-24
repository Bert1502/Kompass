namespace Kompass.Application.B56Import;

public sealed class B56Arbeitsblatt
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<B56Zeile> Zeilen { get; init; }
        = Array.Empty<B56Zeile>();
}
