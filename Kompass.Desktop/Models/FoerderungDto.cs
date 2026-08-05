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

    public FoerderberechnungDto? Berechnung { get; set; }

    public IReadOnlyList<FoerderanforderungDto> Pruefanforderungen { get; set; }
        = Array.Empty<FoerderanforderungDto>();
}

public sealed record FoerderanforderungDto(
    string Programmkennung,
    string Bereich,
    string Anforderung,
    string Bezugsquelle,
    string Pruefstatus);

public sealed record FoerderberechnungDto(DateOnly Stichtag, decimal Investitionskosten,
    IReadOnlyList<ProgrammFoerderungsanteilDto> Programmfoerderungen, decimal GesamtFoerderung, decimal Eigenanteil);

public sealed record ProgrammFoerderungsanteilDto(Guid FoerderprogrammId, string Programmkennung, int Version,
    decimal Foerderbetrag, KumulierbarkeitStatus Kumulierbarkeit, Foerderpruefstatus Status,
    decimal FoerderfaehigeKosten, decimal Foerderhoechstbetrag, decimal Grundfoerderquote,
    decimal ISfpBonusquote, decimal WpbBonusquote, decimal Grundfoerderung, decimal ISfpBonus,
    decimal WpbBonus, decimal Eigenanteil, IReadOnlyList<string> FehlendeVoraussetzungen,
    IReadOnlyList<string> Ausschlussgruende)
{
    public string Hinweise => string.Join("; ", FehlendeVoraussetzungen.Concat(Ausschlussgruende));
}

public sealed class FoerdervoraussetzungenDto
{
    public int? Baujahr { get; set; }
    public DateOnly? Erstnutzung { get; set; }
    public FoerderGebaeudeart? Gebaeudeart { get; set; }
    public FoerderNutzung? Nutzung { get; set; }
    public int? Wohneinheiten { get; set; }
    public Antragstellerart? Eigentuemart { get; set; }
    public bool? Selbstnutzung { get; set; }
    public bool? Vermietung { get; set; }
    public bool? Denkmal { get; set; }
    public bool? BesondersErhaltenswerteBausubstanz { get; set; }
    public bool? Gemeinnuetzigkeit { get; set; }
    public bool? WirtschaftlicheTaetigkeit { get; set; }
    public bool? Vorsteuerabzug { get; set; }
    public bool? ISfp { get; set; }
    public bool? Energieausweis { get; set; }
    public string? Nachweise { get; set; }
    public decimal? Nettogrundflaeche { get; set; }
    public decimal? JahresPrimaerenergiebedarf { get; set; }
    public decimal? QpReferenz { get; set; }
    public string? QpReferenzQuelle { get; set; }
    public bool? WpbFachlichBestaetigt { get; set; }
    public decimal? WpbVerhaeltnis { get; set; }
    public WpbPruefstatus WpbRechnerischerVorschlag { get; set; }
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
    string Quellenstand,
    IReadOnlyList<FoerderquoteRegelDto>? Foerderquoten = null,
    IReadOnlyList<HoechstbetragRegelDto>? Hoechstbetraege = null,
    IReadOnlyList<KumulierbarkeitsregelDto>? Kumulierbarkeitsregeln = null,
    IReadOnlyList<PflichtnachweisRegelDto>? Pflichtnachweisregeln = null,
    IReadOnlyList<GueltigkeitsregelDto>? Gueltigkeitsregeln = null)
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

public sealed record FoerderquoteRegelDto(
    string Bezeichnung,
    decimal Quote,
    string Bezugsbasis,
    DateOnly GueltigAb,
    DateOnly? GueltigBis,
    string? Beschreibung);

public sealed record HoechstbetragRegelDto(
    string Bezeichnung,
    decimal Betrag,
    string Waehrung,
    string Bezugsbasis,
    DateOnly GueltigAb,
    DateOnly? GueltigBis,
    string? Beschreibung);

public sealed record KumulierbarkeitsregelDto(
    string Bezeichnung,
    KumulierbarkeitStatus Status,
    string Beschreibung,
    DateOnly GueltigAb,
    DateOnly? GueltigBis);

public sealed record PflichtnachweisRegelDto(
    string Bezeichnung,
    string Beschreibung,
    Nachweiszeitpunkt Zeitpunkt,
    bool IstPflicht,
    DateOnly GueltigAb,
    DateOnly? GueltigBis);

public sealed record GueltigkeitsregelDto(
    string Bezeichnung,
    Gueltigkeitsbezug Bezug,
    DateOnly GueltigAb,
    DateOnly? GueltigBis,
    string? Beschreibung);
