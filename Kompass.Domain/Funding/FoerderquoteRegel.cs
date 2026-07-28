using Kompass.Domain.Common;

namespace Kompass.Domain.Funding;

public sealed class FoerderquoteRegel : Entity
{
    private FoerderquoteRegel()
    {
        Bezeichnung = string.Empty;
        Bezugsbasis = string.Empty;
    }

    public FoerderquoteRegel(
        Guid id,
        string bezeichnung,
        decimal quote,
        string bezugsbasis,
        DateOnly gueltigAb,
        DateOnly? gueltigBis,
        string? beschreibung = null)
        : base(id)
    {
        Bezeichnung = BereinigePflichttext(
            bezeichnung,
            "Die Bezeichnung der Förderquote darf nicht leer sein.");
        Quote = ValidiereQuote(quote);
        Bezugsbasis = BereinigePflichttext(
            bezugsbasis,
            "Die Bezugsbasis der Förderquote darf nicht leer sein.");
        GueltigAb = gueltigAb;
        GueltigBis = ValidiereZeitraum(
            gueltigAb,
            gueltigBis);
        Beschreibung = BereinigeOptionalenText(beschreibung);
    }

    public string Bezeichnung { get; private set; }

    public decimal Quote { get; private set; }

    public string Bezugsbasis { get; private set; }

    public DateOnly GueltigAb { get; private set; }

    public DateOnly? GueltigBis { get; private set; }

    public string? Beschreibung { get; private set; }

    public static FoerderquoteRegel AusPauschalerQuote(
        decimal quote,
        DateOnly gueltigAb,
        DateOnly? gueltigBis)
    {
        return new FoerderquoteRegel(
            Guid.NewGuid(),
            "Standardquote",
            quote,
            "Gesamtkosten",
            gueltigAb,
            gueltigBis,
            "Automatisch aus dem pauschalen Fördersatz des Programms abgeleitet.");
    }

    private static decimal ValidiereQuote(
        decimal quote)
    {
        if (quote < 0 || quote > 1)
        {
            throw new DomainException(
                "Die Förderquote muss zwischen 0 und 1 liegen.");
        }

        return quote;
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
