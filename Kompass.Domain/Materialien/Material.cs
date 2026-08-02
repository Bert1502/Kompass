using Kompass.Domain.Common;
using Kompass.Domain.Fachdaten;

namespace Kompass.Domain.Materialien;

public sealed class Material : AggregateRoot
{
    private Material() { FachlicheId = string.Empty; Name = string.Empty; }
    public Material(Guid id, string fachlicheId, int version, string name, Guid kategorieId, DateOnly gueltigAb, Guid? quelleId = null) : base(id)
    {
        FachlicheId = Pflichtwert(fachlicheId);
        Version = version > 0 ? version : throw new DomainException("Die Version muss positiv sein.");
        Name = Pflichtwert(name);
        KategorieId = kategorieId != Guid.Empty ? kategorieId : throw new DomainException("Eine Kategorie ist erforderlich.");
        GueltigAb = gueltigAb;
        QuelleId = quelleId;
    }
    public string FachlicheId { get; private set; }
    public int Version { get; private set; }
    public string Name { get; private set; }
    public Guid KategorieId { get; private set; }
    public string? Hersteller { get; private set; }
    public string? Produktname { get; private set; }
    public string? Produktkennung { get; private set; }
    public bool Generisch { get; private set; }
    public DateOnly GueltigAb { get; private set; }
    public DateOnly? GueltigBis { get; private set; }
    public FachdatenStatus Status { get; private set; } = FachdatenStatus.Entwurf;
    public Guid? QuelleId { get; private set; }
    private static string Pflichtwert(string value) => string.IsNullOrWhiteSpace(value) ? throw new DomainException("Ein Pflichtwert fehlt.") : value.Trim();
}
