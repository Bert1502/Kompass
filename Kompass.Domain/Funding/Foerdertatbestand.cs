using Kompass.Domain.Common;

namespace Kompass.Domain.Funding;

public sealed class Foerdertatbestand : Entity
{
    private Foerdertatbestand() { Code = string.Empty; Bezeichnung = string.Empty; }
    public Foerdertatbestand(Guid id, Guid foerderprogrammId, string code, string bezeichnung) : base(id)
    {
        FoerderprogrammId = foerderprogrammId != Guid.Empty ? foerderprogrammId : throw new DomainException("Ein Förderprogramm ist erforderlich.");
        Code = Pflichtwert(code); Bezeichnung = Pflichtwert(bezeichnung);
    }
    public Guid FoerderprogrammId { get; private set; }
    public string Code { get; private set; }
    public string Bezeichnung { get; private set; }
    public Guid? MassnahmenkatalogeintragId { get; private set; }
    public Guid? RegelwerksanforderungId { get; private set; }
    private static string Pflichtwert(string value) => string.IsNullOrWhiteSpace(value) ? throw new DomainException("Ein Pflichtwert fehlt.") : value.Trim();
}
