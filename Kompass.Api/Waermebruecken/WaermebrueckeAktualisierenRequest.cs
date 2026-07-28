using Kompass.Domain.Waermebruecken;

namespace Kompass.Api.Waermebruecken;

public sealed class WaermebrueckeAktualisierenRequest
{
    public string InterneNummer { get; set; } = string.Empty;

    public string Bezeichnung { get; set; } = string.Empty;

    public WaermebrueckeTyp Typ { get; set; }

    public WaermebrueckeStatus Status { get; set; }

    public GleichwertigkeitStatus GleichwertigkeitStatus { get; set; }

    public string? Lage { get; set; }

    public string? Planreferenz { get; set; }

    public string? Detailreferenz { get; set; }

    public string? Fremdnummer { get; set; }

    public decimal? Laenge { get; set; }

    public string? Beiblatt2Referenz { get; set; }

    public string? ThermCadReferenz { get; set; }

    public decimal? PsiWert { get; set; }

    public decimal? FRsi { get; set; }

    public string? Pruefanmerkung { get; set; }

    public string? Berichtsdarstellung { get; set; }
}
