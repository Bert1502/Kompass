using Kompass.Domain.Common;

namespace Kompass.Domain.Massnahmen;

public sealed class Massnahmenkategorie : AggregateRoot
{
    private Massnahmenkategorie() { Code = string.Empty; Bezeichnung = string.Empty; }
    public Massnahmenkategorie(Guid id, string code, string bezeichnung) : base(id)
    {
        Code = Pflichtwert(code); Bezeichnung = Pflichtwert(bezeichnung);
    }
    public string Code { get; private set; }
    public string Bezeichnung { get; private set; }
    private static string Pflichtwert(string value) => string.IsNullOrWhiteSpace(value) ? throw new DomainException("Ein Pflichtwert fehlt.") : value.Trim();
}
