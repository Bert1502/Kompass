namespace Kompass.Application.B56Import;

public sealed class B56Bauteilregel
{
    public string Suchbegriff { get; init; } = string.Empty;

    public string Kategorie { get; init; } = string.Empty;

    public int Prioritaet { get; init; }

    public bool GrossKleinschreibungBeachten { get; init; }
}
