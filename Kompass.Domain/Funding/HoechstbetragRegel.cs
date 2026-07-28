using Kompass.Domain.Common;

namespace Kompass.Domain.Funding;

public sealed class HoechstbetragRegel : Entity
{
    private HoechstbetragRegel()
    {
        Bezeichnung = string.Empty;
        Waehrung = string.Empty;
        Bezugsbasis = string.Empty;
    }

    public HoechstbetragRegel(
        Guid id,
        string bezeichnung,
        decimal betrag,
        string waehrung,
        string bezugsbasis,
        DateOnly gueltigAb,
        DateOnly? gueltigBis,
        string? beschreibung = null)
        : base(id)
    {
        Bezeichnung = BereinigePflichttext(
            bezeichnung,
            "Die Bezeichnung des Höchstbetrags darf nicht leer sein.");
        Betrag = ValidiereBetrag(betrag);
        Waehrung = BereinigePflichttext(
            waehrung,
            "Die Währung des Höchstbetrags darf nicht leer sein.");
        Bezugsbasis = BereinigePflichttext(
            bezugsbasis,
            "Die Bezugsbasis des Höchstbetrags darf nicht leer sein.");
        GueltigAb = gueltigAb;
        GueltigBis = ValidiereZeitraum(
            gueltigAb,
            gueltigBis);
        Beschreibung = BereinigeOptionalenText(beschreibung);
    }

    public string Bezeichnung { get; private set; }

    public decimal Betrag { get; private set; }

    public string Waehrung { get; private set; }

    public string Bezugsbasis { get; private set; }

    public DateOnly GueltigAb { get; private set; }

    public DateOnly? GueltigBis { get; private set; }

    public string? Beschreibung { get; private set; }

    public static HoechstbetragRegel AusPauschalemBetrag(
        decimal betrag,
        DateOnly gueltigAb,
        DateOnly? gueltigBis)
    {
        return new HoechstbetragRegel(
            Guid.NewGuid(),
            "Standardhöchstbetrag",
            betrag,
            "EUR",
            "je Vorhaben",
            gueltigAb,
            gueltigBis,
            "Automatisch aus dem pauschalen Höchstbetrag des Programms abgeleitet.");
    }

    private static decimal ValidiereBetrag(
        decimal betrag)
    {
        if (betrag < 0)
        {
            throw new DomainException(
                "Der Höchstbetrag darf nicht negativ sein.");
        }

        return betrag;
    }

    private static DateOnly? ValidiereZeitraum(
        DateOnly gueltigAb,
        DateOnly? gueltigBis)
    {
        if (gueltigBis.HasValue &&
            gueltigBis.Value < gueltigAb)
        {
            throw new DomainException(
                "GueltigBis darf nicht vor GueltigAb liegen.");
        }

        return gueltigBis;
    }

    private static string BereinigePflichttext(
        string wert,
        string fehlermeldung)
    {
        if (string.IsNullOrWhiteSpace(wert))
        {
            throw new DomainException(fehlermeldung);
        }

        return wert.Trim();
    }

    private static string? BereinigeOptionalenText(
        string? wert)
    {
        if (wert is null)
        {
            return null;
        }

        var bereinigt = wert.Trim();
        return bereinigt.Length == 0 ? null : bereinigt;
    }
}
