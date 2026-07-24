namespace Kompass.Application.B56Import;

public sealed class B56Modernisierungsalternative
{
    public string Bezeichnung { get; init; } = string.Empty;

    public string Beschreibung { get; init; } = string.Empty;

    public IList<B56Bauteil> Bauteile
        { get; init; } = new List<B56Bauteil>();

    public IList<B56Kennwert> Kennwerte
        { get; init; } = new List<B56Kennwert>();
}
