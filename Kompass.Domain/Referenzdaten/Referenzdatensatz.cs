using Kompass.Domain.Common;

namespace Kompass.Domain.Referenzdaten;

public sealed class Referenzdatensatz : AggregateRoot
{
    private Referenzdatensatz()
    {
        FachlicheBezeichnung = string.Empty;
        Parameterart = string.Empty;
        Wert = string.Empty;
        Quelle = string.Empty;
        Herausgeber = string.Empty;
        QuellenVerweis = string.Empty;
        Versionsstand = string.Empty;
    }

    public Referenzdatensatz(
        Guid id,
        string fachlicheBezeichnung,
        string parameterart,
        string wert,
        ReferenzdatenEbene ebene,
        string quelle,
        string herausgeber,
        string quellenVerweis,
        DateOnly gueltigAb,
        DateOnly? gueltigBis,
        string versionsstand,
        ReferenzdatenStatus datenstatus,
        Qualitaetsstatus qualitaetsstatus,
        ReferenzdatenImportart importart,
        DateTimeOffset letzteAktualisierungUtc,
        string? einheit = null,
        string? bezugsgroesse = null,
        string? energietraegerOderKategorie = null,
        DateOnly? veroeffentlichungsdatum = null,
        DateOnly? abrufdatum = null,
        Guid? projektId = null,
        Guid? unternehmenId = null)
        : base(id)
    {
        FachlicheBezeichnung = Pflichtwert(fachlicheBezeichnung, "Die fachliche Bezeichnung ist erforderlich.");
        Parameterart = Pflichtwert(parameterart, "Die Parameterart ist erforderlich.");
        Wert = Pflichtwert(wert, "Der Wert ist erforderlich.");
        Ebene = ebene;
        Quelle = Pflichtwert(quelle, "Die Quelle ist erforderlich.");
        Herausgeber = Pflichtwert(herausgeber, "Der Herausgeber ist erforderlich.");
        QuellenVerweis = Pflichtwert(quellenVerweis, "Der Quellenverweis ist erforderlich.");
        Einheit = OptionalTrim(einheit);
        Bezugsgroesse = OptionalTrim(bezugsgroesse);
        EnergietraegerOderKategorie = OptionalTrim(energietraegerOderKategorie);
        Veroeffentlichungsdatum = veroeffentlichungsdatum;
        Abrufdatum = abrufdatum;
        GueltigAb = gueltigAb;
        GueltigBis = ValidiereZeitraum(gueltigAb, gueltigBis);
        Versionsstand = Pflichtwert(versionsstand, "Der Versionsstand ist erforderlich.");
        Datenstatus = datenstatus;
        Qualitaetsstatus = qualitaetsstatus;
        Importart = importart;
        LetzteAktualisierungUtc = letzteAktualisierungUtc;
        ProjektId = projektId;
        UnternehmenId = unternehmenId;
    }

    public string FachlicheBezeichnung { get; private set; }

    public string Parameterart { get; private set; }

    public string Wert { get; private set; }

    public string? Einheit { get; private set; }

    public string? Bezugsgroesse { get; private set; }

    public string? EnergietraegerOderKategorie { get; private set; }

    public ReferenzdatenEbene Ebene { get; private set; }

    public Guid? ProjektId { get; private set; }

    public Guid? UnternehmenId { get; private set; }

    public string Quelle { get; private set; }

    public string Herausgeber { get; private set; }

    public string QuellenVerweis { get; private set; }

    public DateOnly? Veroeffentlichungsdatum { get; private set; }

    public DateOnly? Abrufdatum { get; private set; }

    public DateOnly GueltigAb { get; private set; }

    public DateOnly? GueltigBis { get; private set; }

    public string Versionsstand { get; private set; }

    public ReferenzdatenStatus Datenstatus { get; private set; }

    public Qualitaetsstatus Qualitaetsstatus { get; private set; }

    public ReferenzdatenImportart Importart { get; private set; }

    public DateTimeOffset LetzteAktualisierungUtc { get; private set; }

    public bool IstGueltigAm(DateOnly stichtag)
    {
        if (GueltigAb > stichtag)
        {
            return false;
        }

        return !GueltigBis.HasValue || GueltigBis.Value >= stichtag;
    }

    private static string Pflichtwert(string text, string fehlermeldung)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException(fehlermeldung);
        }

        return text.Trim();
    }

    private static string? OptionalTrim(string? text)
    {
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }

    private static DateOnly? ValidiereZeitraum(DateOnly gueltigAb, DateOnly? gueltigBis)
    {
        if (gueltigBis.HasValue && gueltigBis.Value < gueltigAb)
        {
            throw new DomainException("GueltigBis darf nicht vor GueltigAb liegen.");
        }

        return gueltigBis;
    }
}
