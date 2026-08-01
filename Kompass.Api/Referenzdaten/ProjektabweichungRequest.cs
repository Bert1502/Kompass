namespace Kompass.Api.Referenzdaten;

public sealed class ProjektabweichungRequest
{
    public Guid ProjektId { get; set; }

    public string Parameterart { get; set; } = string.Empty;

    public string VerwendeterProjektwert { get; set; } = string.Empty;

    public string Begruendung { get; set; } = string.Empty;

    public string Benutzer { get; set; } = string.Empty;

    public string? Bezugsgroesse { get; set; }

    public string? EnergietraegerOderKategorie { get; set; }
}
