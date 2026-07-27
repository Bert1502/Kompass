using Kompass.Domain.Common;

namespace Kompass.Domain.Projects;

public sealed class Projekt : AggregateRoot
{
    public const int MaxInterneBezeichnungLaenge = 200;

    private readonly List<Modernisierungsalternative> _alternativen = new();

    private Projekt()
    {
        Name = string.Empty;
    }

    public Projekt(Guid id, string name)
        : base(id)
    {
        Name = BereinigeName(name);
    }

    public string Name { get; private set; }

    public string? InterneBezeichnung { get; private set; }

    public Bearbeitungsstatus Bearbeitungsstatus { get; private set; }
        = Bearbeitungsstatus.InBearbeitung;

    public Guid? QuellSnapshotId { get; private set; }

    public int ProjektmodellVersion { get; private set; }

    public IReadOnlyCollection<Modernisierungsalternative> Alternativen =>
        _alternativen.AsReadOnly();

    public void Umbenennen(string name)
    {
        Name = BereinigeName(name);
    }

    public void ProjektdatenAktualisieren(
        string? interneBezeichnung,
        Bearbeitungsstatus bearbeitungsstatus)
    {
        if (interneBezeichnung is not null)
        {
            var bereinigt = interneBezeichnung.Trim();

            if (bereinigt.Length > MaxInterneBezeichnungLaenge)
            {
                throw new DomainException(
                    $"Die interne Bezeichnung darf höchstens {MaxInterneBezeichnungLaenge} Zeichen enthalten.");
            }

            InterneBezeichnung = bereinigt.Length == 0 ? null : bereinigt;
        }
        else
        {
            InterneBezeichnung = null;
        }

        Bearbeitungsstatus = bearbeitungsstatus;
    }

    public void AlternativeHinzufuegen(
        Modernisierungsalternative alternative)
    {
        ArgumentNullException.ThrowIfNull(alternative);

        if (_alternativen.Any(
                vorhandeneAlternative =>
                    vorhandeneAlternative.Id == alternative.Id))
        {
            throw new DomainException(
                "Die Modernisierungsalternative ist dem Projekt bereits zugeordnet.");
        }

        _alternativen.Add(alternative);
    }

    public void AusSnapshotErzeugen(
        Guid snapshotId,
        IEnumerable<Modernisierungsalternative> alternativen)
    {
        if (snapshotId == Guid.Empty)
        {
            throw new DomainException(
                "Für die Projektübernahme ist eine gültige Snapshot-ID erforderlich.");
        }

        ArgumentNullException.ThrowIfNull(alternativen);

        if (QuellSnapshotId.HasValue)
        {
            if (QuellSnapshotId == snapshotId)
            {
                return;
            }

            throw new DomainException(
                "Das Projektmodell wurde bereits aus einem anderen B56-Snapshot erzeugt.");
        }

        foreach (var alternative in alternativen)
        {
            AlternativeHinzufuegen(
                alternative);
        }

        QuellSnapshotId =
            snapshotId;
        ProjektmodellVersion = 1;
    }

    private static string BereinigeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(
                "Der Projektname darf nicht leer sein.");
        }

        var bereinigterName = name.Trim();

        if (bereinigterName.Length > 200)
        {
            throw new DomainException(
                "Der Projektname darf höchstens 200 Zeichen enthalten.");
        }

        return bereinigterName;
    }
}
