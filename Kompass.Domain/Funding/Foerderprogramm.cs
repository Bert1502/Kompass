using Kompass.Domain.Common;

namespace Kompass.Domain.Funding;

public sealed class Foerderprogramm : AggregateRoot
{
    private Foerderprogramm()
    {
        Programmkennung = string.Empty;
        Zielgruppe = string.Empty;
        Foerdergegenstand = string.Empty;
        TechnischeMindestanforderungen = string.Empty;
        Kumulierbarkeit = string.Empty;
        Pflichtnachweise = string.Empty;
        Quellenstand = string.Empty;
    }

    public Foerderprogramm(
        Guid id,
        string programmkennung,
        int version,
        DateOnly gueltigAb,
        DateOnly? gueltigBis,
        string zielgruppe,
        string foerdergegenstand,
        string technischeMindestanforderungen,
        decimal foerdersatz,
        decimal? hoechstbetrag,
        string kumulierbarkeit,
        string pflichtnachweise,
        string quellenstand)
        : base(id)
    {
        Programmkennung = BereinigePflichttext(
            programmkennung,
            "Die Programmkennung darf nicht leer sein.");
        Version = ValidiereVersion(version);
        GueltigAb = gueltigAb;
        GueltigBis = ValidiereZeitraum(
            gueltigAb,
            gueltigBis);
        Zielgruppe = BereinigePflichttext(
            zielgruppe,
            "Die Zielgruppe darf nicht leer sein.");
        Foerdergegenstand = BereinigePflichttext(
            foerdergegenstand,
            "Der Fördergegenstand darf nicht leer sein.");
        TechnischeMindestanforderungen = BereinigePflichttext(
            technischeMindestanforderungen,
            "Die technischen Mindestanforderungen dürfen nicht leer sein.");
        Foerdersatz = ValidiereFoerdersatz(foerdersatz);
        Hoechstbetrag = ValidiereHoechstbetrag(hoechstbetrag);
        Kumulierbarkeit = BereinigePflichttext(
            kumulierbarkeit,
            "Die Kumulierbarkeit darf nicht leer sein.");
        Pflichtnachweise = BereinigePflichttext(
            pflichtnachweise,
            "Die Pflichtnachweise dürfen nicht leer sein.");
        Quellenstand = BereinigePflichttext(
            quellenstand,
            "Der Quellenstand darf nicht leer sein.");
    }

    public string Programmkennung { get; private set; }

    public int Version { get; private set; }

    public DateOnly GueltigAb { get; private set; }

    public DateOnly? GueltigBis { get; private set; }

    public string Zielgruppe { get; private set; }

    public string Foerdergegenstand { get; private set; }

    public string TechnischeMindestanforderungen { get; private set; }

    public decimal Foerdersatz { get; private set; }

    public decimal? Hoechstbetrag { get; private set; }

    public string Kumulierbarkeit { get; private set; }

    public string Pflichtnachweise { get; private set; }

    public string Quellenstand { get; private set; }

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

    private static int ValidiereVersion(
        int version)
    {
        if (version < 1)
        {
            throw new DomainException(
                "Die Version muss mindestens 1 sein.");
        }

        return version;
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

    private static decimal ValidiereFoerdersatz(
        decimal foerdersatz)
    {
        if (foerdersatz < 0)
        {
            throw new DomainException(
                "Der Fördersatz darf nicht negativ sein.");
        }

        return foerdersatz;
    }

    private static decimal? ValidiereHoechstbetrag(
        decimal? hoechstbetrag)
    {
        if (hoechstbetrag.HasValue &&
            hoechstbetrag.Value < 0)
        {
            throw new DomainException(
                "Der Höchstbetrag darf nicht negativ sein.");
        }

        return hoechstbetrag;
    }
}
