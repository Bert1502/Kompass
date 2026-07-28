using Kompass.Domain.Common;

namespace Kompass.Domain.Funding;

public sealed class Gueltigkeitsregel : Entity
{
    private Gueltigkeitsregel()
    {
        Bezeichnung = string.Empty;
    }

    public Gueltigkeitsregel(
        Guid id,
        string bezeichnung,
        Gueltigkeitsbezug bezug,
        DateOnly gueltigAb,
        DateOnly? gueltigBis,
        string? beschreibung = null)
        : base(id)
    {
        Bezeichnung = BereinigePflichttext(
            bezeichnung,
            "Die Bezeichnung der Gültigkeitsregel darf nicht leer sein.");
        Bezug = bezug;
        GueltigAb = gueltigAb;
        GueltigBis = ValidiereZeitraum(
            gueltigAb,
            gueltigBis);
        Beschreibung = BereinigeOptionalenText(beschreibung);
    }

    public string Bezeichnung { get; private set; }

    public Gueltigkeitsbezug Bezug { get; private set; }

    public DateOnly GueltigAb { get; private set; }

    public DateOnly? GueltigBis { get; private set; }

    public string? Beschreibung { get; private set; }

    public static Gueltigkeitsregel AusProgrammzeitraum(
        DateOnly gueltigAb,
        DateOnly? gueltigBis)
    {
        return new Gueltigkeitsregel(
            Guid.NewGuid(),
            "Programmgueltigkeit",
            Gueltigkeitsbezug.Programm,
            gueltigAb,
            gueltigBis,
            "Automatisch aus dem Gültigkeitszeitraum des Programms abgeleitet.");
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
