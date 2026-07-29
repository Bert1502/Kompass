namespace Kompass.Api.Projects;

public sealed class ProjektStammdatenAktualisierenRequest
{
    public string? Auftraggeber { get; init; }

    public string? Ansprechpartner { get; init; }

    public string? Strasse { get; init; }

    public string? Ort { get; init; }

    public string? Postleitzahl { get; init; }

    public string? Gebaeudeart { get; init; }
}
