using Kompass.Domain.Funding;

namespace Kompass.Desktop.Models;

public sealed record FoerderuebersichtBerichtDto(
    FoerderuebersichtKopfDto Kopf,
    IReadOnlyList<FoerderuebersichtAlternativeDto> Alternativen);

public sealed record FoerderuebersichtKopfDto(
    Guid ProjektId,
    string ProjektName);

public sealed record FoerderuebersichtAlternativeDto(
    Guid AlternativeId,
    int? B56Position,
    string Bezeichnung,
    decimal Gesamtkosten,
    IReadOnlyList<FoerderprogrammKurzDto> ZugeordneteProgramme)
{
    public string GesamtkostenText =>
        $"{Gesamtkosten:N0} €";

    public string AnzahlProgrammeText =>
        ZugeordneteProgramme.Count == 0
            ? "Keine Programme zugeordnet"
            : $"{ZugeordneteProgramme.Count} Programm(e)";
}

public sealed record FoerderprogrammKurzDto(
    Guid Id,
    string Programmkennung,
    int Version,
    DateOnly GueltigAb,
    DateOnly? GueltigBis,
    decimal Foerdersatz,
    decimal? Hoechstbetrag)
{
    public string FoerdersatzText =>
        $"{Foerdersatz:P0}";

    public string HoechstbetragText =>
        Hoechstbetrag.HasValue
            ? $"{Hoechstbetrag.Value:N0} €"
            : "–";

    public string GueltigkeitText =>
        GueltigBis.HasValue
            ? $"{GueltigAb:d} – {GueltigBis.Value:d}"
            : $"ab {GueltigAb:d}";
}

public sealed record FoerderprogrammKatalogDto(
    Guid Id,
    string Programmkennung,
    int Version,
    DateOnly GueltigAb,
    DateOnly? GueltigBis,
    string Zielgruppe,
    string Foerdergegenstand,
    string TechnischeMindestanforderungen,
    decimal Foerdersatz,
    decimal? Hoechstbetrag,
    string Kumulierbarkeit,
    string Pflichtnachweise,
    string Quellenstand)
{
    public string FoerdersatzText =>
        $"{Foerdersatz:P0}";

    public string HoechstbetragText =>
        Hoechstbetrag.HasValue
            ? $"{Hoechstbetrag.Value:N0} €"
            : "–";

    public string GueltigkeitText =>
        GueltigBis.HasValue
            ? $"{GueltigAb:d} – {GueltigBis.Value:d}"
            : $"ab {GueltigAb:d}";
}
