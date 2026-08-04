namespace Kompass.Desktop.Models;

public sealed record KostenAlternativeDto(
    Guid Id,
    string Bezeichnung,
    Guid ProjektId,
    decimal Gesamtkosten)
{
    public string Anzeige => $"{Bezeichnung} ({Gesamtkosten:N2} EUR)";
}

public sealed record KostenpositionDto(
    Guid Id,
    string Bezeichnung,
    decimal Betrag,
    int Kostenart)
{
    public string BetragText => $"{Betrag:N2} EUR";

    public string KostenartText => Kostenart switch
    {
        1 => "Architektur",
        2 => "TGA",
        3 => "Sowieso-Kosten",
        4 => "Umfeldmaßnahme",
        5 => "Fachplanung",
        6 => "Eigenleistung",
        7 => "Sonstige",
        _ => "Unbekannt"
    };
}

public sealed record KostenartAuswahlDto(
    int Wert,
    string Bezeichnung);

public sealed record KostenpositionErstellenDto(
    string Bezeichnung,
    decimal Betrag,
    int Kostenart);
