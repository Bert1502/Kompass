namespace Kompass.Application.B56Import;

public sealed class B56Bauteilzuordnung
{
    public string Code { get; init; } = string.Empty;

    public string Kategorie { get; init; } = string.Empty;

    public string KompassTyp { get; init; } = string.Empty;

    public string Beschreibung { get; init; } = string.Empty;

    public int Prioritaet { get; init; }
}
