using Kompass.Domain.Common;

namespace Kompass.Domain.Funding;

public sealed class Foerderprogramm : AggregateRoot
{
    private readonly List<FoerderquoteRegel> _foerderquoten = new();
    private readonly List<HoechstbetragRegel> _hoechstbetraege = new();
    private readonly List<Kumulierbarkeitsregel> _kumulierbarkeitsregeln = new();
    private readonly List<PflichtnachweisRegel> _pflichtnachweisregeln = new();
    private readonly List<Gueltigkeitsregel> _gueltigkeitsregeln = new();

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
        string quellenstand,
        IEnumerable<FoerderquoteRegel>? foerderquoten = null,
        IEnumerable<HoechstbetragRegel>? hoechstbetraege = null,
        IEnumerable<Kumulierbarkeitsregel>? kumulierbarkeitsregeln = null,
        IEnumerable<PflichtnachweisRegel>? pflichtnachweisregeln = null,
        IEnumerable<Gueltigkeitsregel>? gueltigkeitsregeln = null)
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

        InitialisiereFoerderquoten(
            foerderquoten,
            gueltigAb,
            GueltigBis);
        InitialisiereHoechstbetraege(
            hoechstbetraege,
            gueltigAb,
            GueltigBis);
        InitialisiereKumulierbarkeitsregeln(
            kumulierbarkeitsregeln,
            gueltigAb,
            GueltigBis);
        InitialisierePflichtnachweisregeln(
            pflichtnachweisregeln,
            gueltigAb,
            GueltigBis);
        InitialisiereGueltigkeitsregeln(
            gueltigkeitsregeln,
            gueltigAb,
            GueltigBis);
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

    public IReadOnlyCollection<FoerderquoteRegel> Foerderquoten =>
        _foerderquoten.AsReadOnly();

    public IReadOnlyCollection<HoechstbetragRegel> Hoechstbetraege =>
        _hoechstbetraege.AsReadOnly();

    public IReadOnlyCollection<Kumulierbarkeitsregel> Kumulierbarkeitsregeln =>
        _kumulierbarkeitsregeln.AsReadOnly();

    public IReadOnlyCollection<PflichtnachweisRegel> Pflichtnachweisregeln =>
        _pflichtnachweisregeln.AsReadOnly();

    public IReadOnlyCollection<Gueltigkeitsregel> Gueltigkeitsregeln =>
        _gueltigkeitsregeln.AsReadOnly();

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

    private void InitialisiereFoerderquoten(
        IEnumerable<FoerderquoteRegel>? foerderquoten,
        DateOnly gueltigAb,
        DateOnly? gueltigBis)
    {
        _foerderquoten.Clear();

        var detailregeln = MaterialisiereRegeln(foerderquoten);

        if (detailregeln.Count == 0)
        {
            _foerderquoten.Add(
                FoerderquoteRegel.AusPauschalerQuote(
                    Foerdersatz,
                    gueltigAb,
                    gueltigBis));
            return;
        }

        _foerderquoten.AddRange(detailregeln);
    }

    private void InitialisiereHoechstbetraege(
        IEnumerable<HoechstbetragRegel>? hoechstbetraege,
        DateOnly gueltigAb,
        DateOnly? gueltigBis)
    {
        _hoechstbetraege.Clear();

        var detailregeln = MaterialisiereRegeln(hoechstbetraege);

        if (detailregeln.Count > 0)
        {
            _hoechstbetraege.AddRange(detailregeln);
            return;
        }

        if (Hoechstbetrag.HasValue)
        {
            _hoechstbetraege.Add(
                HoechstbetragRegel.AusPauschalemBetrag(
                    Hoechstbetrag.Value,
                    gueltigAb,
                    gueltigBis));
        }
    }

    private void InitialisiereKumulierbarkeitsregeln(
        IEnumerable<Kumulierbarkeitsregel>? kumulierbarkeitsregeln,
        DateOnly gueltigAb,
        DateOnly? gueltigBis)
    {
        _kumulierbarkeitsregeln.Clear();

        var detailregeln = MaterialisiereRegeln(kumulierbarkeitsregeln);

        if (detailregeln.Count == 0)
        {
            _kumulierbarkeitsregeln.Add(
                Kumulierbarkeitsregel.AusPauschalerBeschreibung(
                    Kumulierbarkeit,
                    gueltigAb,
                    gueltigBis));
            return;
        }

        _kumulierbarkeitsregeln.AddRange(detailregeln);
    }

    private void InitialisierePflichtnachweisregeln(
        IEnumerable<PflichtnachweisRegel>? pflichtnachweisregeln,
        DateOnly gueltigAb,
        DateOnly? gueltigBis)
    {
        _pflichtnachweisregeln.Clear();

        var detailregeln = MaterialisiereRegeln(pflichtnachweisregeln);

        if (detailregeln.Count == 0)
        {
            _pflichtnachweisregeln.Add(
                PflichtnachweisRegel.AusPauschalemNachweis(
                    Pflichtnachweise,
                    gueltigAb,
                    gueltigBis));
            return;
        }

        _pflichtnachweisregeln.AddRange(detailregeln);
    }

    private void InitialisiereGueltigkeitsregeln(
        IEnumerable<Gueltigkeitsregel>? gueltigkeitsregeln,
        DateOnly gueltigAb,
        DateOnly? gueltigBis)
    {
        _gueltigkeitsregeln.Clear();

        var detailregeln = MaterialisiereRegeln(gueltigkeitsregeln);

        if (detailregeln.Count == 0)
        {
            _gueltigkeitsregeln.Add(
                Gueltigkeitsregel.AusProgrammzeitraum(
                    gueltigAb,
                    gueltigBis));
            return;
        }

        _gueltigkeitsregeln.AddRange(detailregeln);
    }

    private static List<TRegel> MaterialisiereRegeln<TRegel>(
        IEnumerable<TRegel>? regeln)
        where TRegel : Entity
    {
        if (regeln is null)
        {
            return [];
        }

        var materialisiert = new List<TRegel>();

        foreach (var regel in regeln)
        {
            ArgumentNullException.ThrowIfNull(regel);
            materialisiert.Add(regel);
        }

        return materialisiert;
    }
}
