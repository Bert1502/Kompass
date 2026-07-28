using Kompass.Domain.Common;

namespace Kompass.Domain.Funding;

public sealed class Kumulierbarkeitsregel : Entity
{
    private Kumulierbarkeitsregel()
    {
        Bezeichnung = string.Empty;
        Beschreibung = string.Empty;
    }

    public Kumulierbarkeitsregel(
        Guid id,
        string bezeichnung,
        KumulierbarkeitStatus status,
        string beschreibung,
        DateOnly gueltigAb,
        DateOnly? gueltigBis)
        : base(id)
    {
        Bezeichnung = BereinigePflichttext(
            bezeichnung,
            "Die Bezeichnung der Kumulierbarkeitsregel darf nicht leer sein.");
        Status = status;
        Beschreibung = BereinigePflichttext(
            beschreibung,
            "Die Beschreibung der Kumulierbarkeitsregel darf nicht leer sein.");
        GueltigAb = gueltigAb;
        GueltigBis = ValidiereZeitraum(
            gueltigAb,
            gueltigBis);
    }

    public string Bezeichnung { get; private set; }

    public KumulierbarkeitStatus Status { get; private set; }

    public string Beschreibung { get; private set; }

    public DateOnly GueltigAb { get; private set; }

    public DateOnly? GueltigBis { get; private set; }

    public static Kumulierbarkeitsregel AusPauschalerBeschreibung(
        string beschreibung,
        DateOnly gueltigAb,
        DateOnly? gueltigBis)
    {
        return new Kumulierbarkeitsregel(
            Guid.NewGuid(),
            "Standardkumulierbarkeit",
            KumulierbarkeitStatus.Unbestimmt,
            beschreibung,
            gueltigAb,
            gueltigBis);
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
}
