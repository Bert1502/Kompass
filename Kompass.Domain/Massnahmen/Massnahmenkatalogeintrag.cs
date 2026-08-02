using Kompass.Domain.Common;
using Kompass.Domain.Fachdaten;

namespace Kompass.Domain.Massnahmen;

public sealed class Massnahmenkatalogeintrag : AggregateRoot
{
    private Massnahmenkatalogeintrag() { Code = string.Empty; Bezeichnung = string.Empty; Mengeneinheit = string.Empty; }
    public Massnahmenkatalogeintrag(Guid id, string code, int version, string bezeichnung, Guid kategorieId, string mengeneinheit, DateOnly gueltigAb, Guid? quelleId = null) : base(id)
    {
        Code = Pflichtwert(code);
        Version = version > 0 ? version : throw new DomainException("Die Version muss positiv sein.");
        Bezeichnung = Pflichtwert(bezeichnung);
        KategorieId = kategorieId != Guid.Empty ? kategorieId : throw new DomainException("Eine Kategorie ist erforderlich.");
        Mengeneinheit = Pflichtwert(mengeneinheit);
        GueltigAb = gueltigAb;
        QuelleId = quelleId;
    }
    public string Code { get; private set; }
    public int Version { get; private set; }
    public string Bezeichnung { get; private set; }
    public string? Beschreibung { get; private set; }
    public Guid KategorieId { get; private set; }
    public string Mengeneinheit { get; private set; }
    public DateOnly GueltigAb { get; private set; }
    public DateOnly? GueltigBis { get; private set; }
    public FachdatenStatus Status { get; private set; } = FachdatenStatus.Entwurf;
    public Guid? QuelleId { get; private set; }
    public bool Aktiv { get; private set; } = true;
    private static string Pflichtwert(string value) => string.IsNullOrWhiteSpace(value) ? throw new DomainException("Ein Pflichtwert fehlt.") : value.Trim();
}
