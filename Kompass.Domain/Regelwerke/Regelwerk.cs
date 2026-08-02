using Kompass.Domain.Common;
using Kompass.Domain.Fachdaten;

namespace Kompass.Domain.Regelwerke;

public sealed class Regelwerk : AggregateRoot
{
    private readonly List<Regelwerksanforderung> _anforderungen = [];

    private Regelwerk()
    {
        Code = string.Empty;
        Titel = string.Empty;
        Fassung = string.Empty;
    }

    public Regelwerk(Guid id, string code, int version, string titel, string fassung, DateOnly gueltigAb, Guid? quelleId = null)
        : base(id)
    {
        Code = Pflichtwert(code);
        Version = version > 0 ? version : throw new DomainException("Die Version muss positiv sein.");
        Titel = Pflichtwert(titel);
        Fassung = Pflichtwert(fassung);
        GueltigAb = gueltigAb;
        QuelleId = quelleId;
    }

    public string Code { get; private set; }
    public int Version { get; private set; }
    public string Titel { get; private set; }
    public string? Herausgeber { get; private set; }
    public string Fassung { get; private set; }
    public DateOnly GueltigAb { get; private set; }
    public DateOnly? GueltigBis { get; private set; }
    public FachdatenStatus Status { get; private set; } = FachdatenStatus.Entwurf;
    public Guid? QuelleId { get; private set; }
    public IReadOnlyCollection<Regelwerksanforderung> Anforderungen => _anforderungen.AsReadOnly();

    public void AnforderungHinzufuegen(Regelwerksanforderung anforderung)
    {
        if (Status == FachdatenStatus.Freigegeben)
        {
            throw new DomainException("Ein freigegebenes Regelwerk ist unveränderlich.");
        }

        _anforderungen.Add(anforderung);
    }

    public void Freigeben()
    {
        if (QuelleId is null)
        {
            throw new DomainException("Ein Regelwerk ohne Quelle darf nicht freigegeben werden.");
        }

        Status = FachdatenStatus.Freigegeben;
    }

    private static string Pflichtwert(string value) =>
        string.IsNullOrWhiteSpace(value) ? throw new DomainException("Ein Pflichtwert fehlt.") : value.Trim();
}
