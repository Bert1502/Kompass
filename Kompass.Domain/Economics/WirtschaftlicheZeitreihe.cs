using Kompass.Domain.Common;
using Kompass.Domain.Fachdaten;

namespace Kompass.Domain.Economics;

public sealed class WirtschaftlicheZeitreihe : AggregateRoot
{
    private readonly List<WirtschaftlicherZeitwert> _werte = [];
    private WirtschaftlicheZeitreihe() { FachlicheId = string.Empty; Typ = string.Empty; Bezeichnung = string.Empty; Einheit = string.Empty; Szenario = string.Empty; }
    public WirtschaftlicheZeitreihe(Guid id, string fachlicheId, int version, string typ, string bezeichnung, string einheit, string szenario, Guid? quelleId = null) : base(id)
    {
        FachlicheId = Pflichtwert(fachlicheId); Version = version > 0 ? version : throw new DomainException("Die Version muss positiv sein.");
        Typ = Pflichtwert(typ); Bezeichnung = Pflichtwert(bezeichnung); Einheit = Pflichtwert(einheit); Szenario = Pflichtwert(szenario); QuelleId = quelleId;
    }
    public string FachlicheId { get; private set; }
    public int Version { get; private set; }
    public string Typ { get; private set; }
    public string Bezeichnung { get; private set; }
    public string? EnergietraegerCode { get; private set; }
    public string Einheit { get; private set; }
    public string Szenario { get; private set; }
    public FachdatenStatus Status { get; private set; } = FachdatenStatus.Entwurf;
    public Guid? QuelleId { get; private set; }
    public IReadOnlyCollection<WirtschaftlicherZeitwert> Werte => _werte.AsReadOnly();
    public void WertHinzufuegen(Guid id, DateOnly stichtag, decimal wert)
    {
        if (_werte.Any(x => x.Stichtag == stichtag)) throw new DomainException("Für den Stichtag existiert bereits ein Wert.");
        _werte.Add(new WirtschaftlicherZeitwert(id, stichtag, wert));
    }
    private static string Pflichtwert(string value) => string.IsNullOrWhiteSpace(value) ? throw new DomainException("Ein Pflichtwert fehlt.") : value.Trim();
}

public sealed class WirtschaftlicherZeitwert : Entity
{
    private WirtschaftlicherZeitwert() { }
    internal WirtschaftlicherZeitwert(Guid id, DateOnly stichtag, decimal wert) : base(id) { Stichtag = stichtag; Wert = wert; }
    public Guid ZeitreiheId { get; private set; }
    public DateOnly Stichtag { get; private set; }
    public decimal Wert { get; private set; }
}
