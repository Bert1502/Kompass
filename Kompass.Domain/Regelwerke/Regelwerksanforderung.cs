using Kompass.Domain.Common;

namespace Kompass.Domain.Regelwerke;

public sealed class Regelwerksanforderung : Entity
{
    private Regelwerksanforderung()
    {
        FachlicheId = string.Empty;
        Anforderungsart = string.Empty;
        Bezeichnung = string.Empty;
    }

    public Regelwerksanforderung(Guid id, string fachlicheId, string anforderungsart, string bezeichnung, DateOnly gueltigAb, decimal? grenzwert = null, string? textwert = null, string? vergleichsoperator = null, string? einheit = null)
        : base(id)
    {
        if (grenzwert.HasValue == !string.IsNullOrWhiteSpace(textwert))
        {
            throw new DomainException("Genau ein Grenzwert oder Textwert ist erforderlich.");
        }

        if (grenzwert.HasValue && (string.IsNullOrWhiteSpace(vergleichsoperator) || string.IsNullOrWhiteSpace(einheit)))
        {
            throw new DomainException("Numerische Anforderungen benötigen Operator und Einheit.");
        }

        FachlicheId = Pflichtwert(fachlicheId);
        Anforderungsart = Pflichtwert(anforderungsart);
        Bezeichnung = Pflichtwert(bezeichnung);
        GueltigAb = gueltigAb;
        Grenzwert = grenzwert;
        Textwert = Optional(textwert);
        Vergleichsoperator = Optional(vergleichsoperator);
        Einheit = Optional(einheit);
    }

    public Guid RegelwerkId { get; private set; }
    public string FachlicheId { get; private set; }
    public string Anforderungsart { get; private set; }
    public string Bezeichnung { get; private set; }
    public string? GebaeudekategorieCode { get; private set; }
    public string? BauteiltypCode { get; private set; }
    public string? RandbedingungCode { get; private set; }
    public string? TemperaturkategorieCode { get; private set; }
    public string? Vergleichsoperator { get; private set; }
    public decimal? Grenzwert { get; private set; }
    public string? Einheit { get; private set; }
    public string? Textwert { get; private set; }
    public DateOnly GueltigAb { get; private set; }
    public DateOnly? GueltigBis { get; private set; }
    public bool FachlichBestaetigt { get; private set; }

    private static string Pflichtwert(string value) => string.IsNullOrWhiteSpace(value) ? throw new DomainException("Ein Pflichtwert fehlt.") : value.Trim();
    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
