using Kompass.Domain.Common;

namespace Kompass.Domain.Funding;

public sealed class Foerdergeber : AggregateRoot
{
    private Foerdergeber() { FachlicheId = string.Empty; Name = string.Empty; Ebene = string.Empty; }
    public Foerdergeber(Guid id, string fachlicheId, string name, string ebene) : base(id)
    {
        FachlicheId = Pflichtwert(fachlicheId); Name = Pflichtwert(name); Ebene = Pflichtwert(ebene);
    }
    public string FachlicheId { get; private set; }
    public string Name { get; private set; }
    public string Ebene { get; private set; }
    private static string Pflichtwert(string value) => string.IsNullOrWhiteSpace(value) ? throw new DomainException("Ein Pflichtwert fehlt.") : value.Trim();
}
