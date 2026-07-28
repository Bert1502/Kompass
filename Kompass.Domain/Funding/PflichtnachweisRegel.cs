using Kompass.Domain.Common;

namespace Kompass.Domain.Funding;

public sealed class PflichtnachweisRegel : Entity
{
    private PflichtnachweisRegel()
    {
        Bezeichnung = string.Empty;
        Beschreibung = string.Empty;
    }

    public PflichtnachweisRegel(
        Guid id,
        string bezeichnung,
        string beschreibung,
        Nachweiszeitpunkt zeitpunkt,
        bool istPflicht,
        DateOnly gueltigAb,
        DateOnly? gueltigBis)
        : base(id)
    {
        Bezeichnung = BereinigePflichttext(
            bezeichnung,
            "Die Bezeichnung des Pflichtnachweises darf nicht leer sein.");
        Beschreibung = BereinigePflichttext(
            beschreibung,
            "Die Beschreibung des Pflichtnachweises darf nicht leer sein.");
        Zeitpunkt = zeitpunkt;
        IstPflicht = istPflicht;
        GueltigAb = gueltigAb;
        GueltigBis = ValidiereZeitraum(
            gueltigAb,
            gueltigBis);
    }

    public string Bezeichnung { get; private set; }

    public string Beschreibung { get; private set; }

    public Nachweiszeitpunkt Zeitpunkt { get; private set; }

    public bool IstPflicht { get; private set; }

    public DateOnly GueltigAb { get; private set; }

    public DateOnly? GueltigBis { get; private set; }

    public static PflichtnachweisRegel AusPauschalemNachweis(
        string beschreibung,
        DateOnly gueltigAb,
        DateOnly? gueltigBis)
    {
        return new PflichtnachweisRegel(
            Guid.NewGuid(),
            "Standardnachweis",
            beschreibung,
            Nachweiszeitpunkt.Unbestimmt,
            true,
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
