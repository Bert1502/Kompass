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
        Guid? quellSnapshotId = null,
        int? b56Position = null)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(bezeichnung))
        {
            throw new DomainException(
                "Die Bezeichnung der Modernisierungsalternative darf nicht leer sein.");
        }

        if (b56Position is < 1 or > 9)
        {
            throw new DomainException(
                "Die B56-Position muss zwischen 1 und 9 liegen.");
        }

        Bezeichnung = bezeichnung.Trim();
        Kurztext = kurztext?.Trim() ?? string.Empty;
        QuellSnapshotId = quellSnapshotId;
        B56Position = b56Position;
        IstImAktuellenB56SnapshotVorhanden = true;
    }

    public string Bezeichnung { get; private set; }

    public string Kurztext { get; private set; }

    public Guid? QuellSnapshotId { get; private set; }

    public int? B56Position { get; private set; }

    public bool IstImAktuellenB56SnapshotVorhanden { get; private set; } = true;

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

    public void AlsNichtMehrImAktuellenB56SnapshotVorhandenKennzeichnen()
    {
        IstImAktuellenB56SnapshotVorhanden = false;
    }

    public void AlsImAktuellenB56SnapshotVorhandenKennzeichnen()
    {
        IstImAktuellenB56SnapshotVorhanden = true;
    }

    public void AusB56SnapshotAktualisieren(
        Guid snapshotId,
        string bezeichnung,
        string kurztext)
    {
        if (snapshotId == Guid.Empty)
        {
            throw new DomainException(
                "Für die Aktualisierung ist eine gültige Snapshot-ID erforderlich.");
        }

        BezeichnungAendern(
            bezeichnung);
        KurztextAendern(
            kurztext);
        QuellSnapshotId = snapshotId;
        AlsImAktuellenB56SnapshotVorhandenKennzeichnen();
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
