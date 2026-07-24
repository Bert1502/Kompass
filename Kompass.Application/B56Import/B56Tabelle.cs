namespace Kompass.Application.B56Import;

public sealed class B56Tabelle
{
    public string Arbeitsblatt { get; init; } = string.Empty;

    public string Titel { get; init; } = string.Empty;

    public int Kopfzeile { get; init; }

    public int ErsteDatenzeile { get; init; }

    public int LetzteDatenzeile { get; init; }

    public IReadOnlyList<string> Spalten
        { get; init; } = Array.Empty<string>();
}
