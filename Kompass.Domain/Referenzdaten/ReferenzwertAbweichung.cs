using Kompass.Domain.Common;

namespace Kompass.Domain.Referenzdaten;

public sealed class ReferenzwertAbweichung : AggregateRoot
{
    private ReferenzwertAbweichung()
    {
        Parameterart = string.Empty;
        UrspruenglicherReferenzwert = string.Empty;
        VerwendeterProjektwert = string.Empty;
        Begruendung = string.Empty;
        Benutzer = string.Empty;
    }

    public ReferenzwertAbweichung(
        Guid id,
        Guid projektId,
        string parameterart,
        string urspruenglicherReferenzwert,
        string verwendeterProjektwert,
        string begruendung,
        string benutzer,
        DateTimeOffset aenderungszeitpunktUtc,
        Guid? referenzdatensatzId = null,
        string? bezugsgroesse = null,
        string? energietraegerOderKategorie = null)
        : base(id)
    {
        if (projektId == Guid.Empty)
        {
            throw new DomainException("Die Projekt-ID ist erforderlich.");
        }

        ProjektId = projektId;
        Parameterart = Pflichtwert(parameterart, "Die Parameterart ist erforderlich.");
        UrspruenglicherReferenzwert = Pflichtwert(urspruenglicherReferenzwert, "Der ursprüngliche Referenzwert ist erforderlich.");
        VerwendeterProjektwert = Pflichtwert(verwendeterProjektwert, "Der verwendete Projektwert ist erforderlich.");
        Begruendung = Pflichtwert(begruendung, "Die Begründung ist erforderlich.");
        Benutzer = Pflichtwert(benutzer, "Der Benutzer ist erforderlich.");
        AenderungszeitpunktUtc = aenderungszeitpunktUtc;
        ReferenzdatensatzId = referenzdatensatzId;
        Bezugsgroesse = string.IsNullOrWhiteSpace(bezugsgroesse) ? null : bezugsgroesse.Trim();
        EnergietraegerOderKategorie = string.IsNullOrWhiteSpace(energietraegerOderKategorie) ? null : energietraegerOderKategorie.Trim();
    }

    public Guid ProjektId { get; private set; }

    public Guid? ReferenzdatensatzId { get; private set; }

    public string Parameterart { get; private set; }

    public string? Bezugsgroesse { get; private set; }

    public string? EnergietraegerOderKategorie { get; private set; }

    public string UrspruenglicherReferenzwert { get; private set; }

    public string VerwendeterProjektwert { get; private set; }

    public string Begruendung { get; private set; }

    public string Benutzer { get; private set; }

    public DateTimeOffset AenderungszeitpunktUtc { get; private set; }

    private static string Pflichtwert(string text, string fehlermeldung)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException(fehlermeldung);
        }

        return text.Trim();
    }
}
