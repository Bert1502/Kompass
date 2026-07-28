namespace Kompass.Api.Funding;

public sealed class FoerderprogrammAnlegenRequest
{
    public string Programmkennung { get; set; } = string.Empty;

    public int Version { get; set; }

    public DateOnly GueltigAb { get; set; }

    public DateOnly? GueltigBis { get; set; }

    public string Zielgruppe { get; set; } = string.Empty;

    public string Foerdergegenstand { get; set; } = string.Empty;

    public string TechnischeMindestanforderungen { get; set; } = string.Empty;

    public decimal Foerdersatz { get; set; }

    public decimal? Hoechstbetrag { get; set; }

    public string Kumulierbarkeit { get; set; } = string.Empty;

    public string Pflichtnachweise { get; set; } = string.Empty;

    public string Quellenstand { get; set; } = string.Empty;
}
