using Kompass.Domain.Common;
using Kompass.Domain.Economics;

namespace Kompass.Domain.Projects;

public sealed class Modernisierungsalternative : Entity
{
    private readonly List<AlternativeBauteil> _bauteile = new();
    private readonly List<Kostenposition> _kostenpositionen = new();

    private Modernisierungsalternative()
    {
        Bezeichnung = string.Empty;
        Kurztext = string.Empty;
    }

    public Modernisierungsalternative(
        Guid id,
        string bezeichnung,
        string kurztext,
        Guid? quellSnapshotId = null)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(bezeichnung))
        {
            throw new DomainException(
                "Die Bezeichnung der Modernisierungsalternative darf nicht leer sein.");
        }

        Bezeichnung = bezeichnung.Trim();
        Kurztext = kurztext?.Trim() ?? string.Empty;
        QuellSnapshotId = quellSnapshotId;
    }

    public string Bezeichnung { get; private set; }

    public string Kurztext { get; private set; }

    public Guid? QuellSnapshotId { get; private set; }

    public IReadOnlyCollection<AlternativeBauteil> Bauteile =>
        _bauteile.AsReadOnly();

    public IReadOnlyCollection<Kostenposition> Kostenpositionen =>
        _kostenpositionen.AsReadOnly();

    public decimal Gesamtkosten =>
        _kostenpositionen.Sum(
            kostenposition => kostenposition.Betrag);

    public void BezeichnungAendern(
        string bezeichnung)
    {
        if (string.IsNullOrWhiteSpace(bezeichnung))
        {
            throw new DomainException(
                "Die Bezeichnung der Modernisierungsalternative darf nicht leer sein.");
        }

        Bezeichnung = bezeichnung.Trim();
    }

    public void KurztextAendern(
        string kurztext)
    {
        Kurztext = kurztext?.Trim() ?? string.Empty;
    }

    public void BauteilHinzufuegen(
        AlternativeBauteil bauteil)
    {
        ArgumentNullException.ThrowIfNull(bauteil);

        _bauteile.Add(bauteil);
    }

    public void KostenpositionHinzufuegen(
        Kostenposition kostenposition)
    {
        ArgumentNullException.ThrowIfNull(kostenposition);

        _kostenpositionen.Add(kostenposition);
    }
}
