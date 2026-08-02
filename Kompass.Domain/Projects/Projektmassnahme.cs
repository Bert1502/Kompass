using Kompass.Domain.Common;

namespace Kompass.Domain.Projects;

public sealed class Projektmassnahme : Entity
{
    private Projektmassnahme() { Bezeichnung = string.Empty; Status = string.Empty; }
    public Projektmassnahme(Guid id, Guid projektId, Guid massnahmenkatalogeintragId, string bezeichnung, string status) : base(id)
    {
        ProjektId = projektId != Guid.Empty ? projektId : throw new DomainException("Ein Projekt ist erforderlich.");
        MassnahmenkatalogeintragId = massnahmenkatalogeintragId != Guid.Empty ? massnahmenkatalogeintragId : throw new DomainException("Ein Katalogeintrag ist erforderlich.");
        Bezeichnung = Pflichtwert(bezeichnung);
        Status = Pflichtwert(status);
    }
    public Guid ProjektId { get; private set; }
    public Guid MassnahmenkatalogeintragId { get; private set; }
    public Guid? ModernisierungsalternativeId { get; private set; }
    public string Bezeichnung { get; private set; }
    public decimal? Menge { get; private set; }
    public string? Einheit { get; private set; }
    public string Status { get; private set; }
    private static string Pflichtwert(string value) => string.IsNullOrWhiteSpace(value) ? throw new DomainException("Ein Pflichtwert fehlt.") : value.Trim();
}
