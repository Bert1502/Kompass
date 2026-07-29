using Kompass.Domain.Waermebruecken;

namespace Kompass.Api.Waermebruecken;

public sealed class WaermebrueckeAnlegenRequest
{
    public string InterneNummer { get; set; } = string.Empty;

    public string Bezeichnung { get; set; } = string.Empty;

    public WaermebrueckeTyp Typ { get; set; }

    public string? Lage { get; set; }

    public string? Planreferenz { get; set; }

    public string? Detailreferenz { get; set; }

    public string? Fremdnummer { get; set; }

    public decimal? Laenge { get; set; }
}
