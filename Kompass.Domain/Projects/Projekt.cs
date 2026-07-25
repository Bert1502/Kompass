using Kompass.Domain.Common;

namespace Kompass.Domain.Projects;

public sealed class Projekt : AggregateRoot
{
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

    public Guid? QuellSnapshotId { get; private set; }

    public int ProjektmodellVersion { get; private set; }

    public IReadOnlyCollection<Modernisierungsalternative> Alternativen =>
        _alternativen.AsReadOnly();

    public void Umbenennen(string name)
    {
        Name = BereinigeName(name);
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

    public IReadOnlyCollection<Modernisierungsalternative> AusSnapshotErzeugen(
        Guid snapshotId,
        IEnumerable<Modernisierungsalternative> alternativen)
    {
        if (snapshotId == Guid.Empty)
        {
            throw new DomainException(
                "Für die Projektübernahme ist eine gültige Snapshot-ID erforderlich.");
        }

        ArgumentNullException.ThrowIfNull(alternativen);

        if (QuellSnapshotId == snapshotId)
        {
            return Array.Empty<Modernisierungsalternative>();
        }

        var quellAlternativen =
            alternativen.ToList();

        if (quellAlternativen.Any(
                alternative =>
                    !alternative.B56Position.HasValue))
        {
            throw new DomainException(
                "B56-Alternativen benötigen für die Übernahme eine Position.");
        }

        var doppeltePosition =
            quellAlternativen
                .GroupBy(
                    alternative =>
                        alternative.B56Position)
                .FirstOrDefault(
                    gruppe =>
                        gruppe.Count() > 1);

        if (doppeltePosition is not null)
        {
            throw new DomainException(
                $"Die B56-Position {doppeltePosition.Key} ist mehrfach vorhanden.");
        }

        var hinzugefuegteAlternativen =
            new List<Modernisierungsalternative>();

        foreach (var vorhandeneAlternative in
                 _alternativen.Where(
                     alternative =>
                         alternative.B56Position.HasValue))
        {
            vorhandeneAlternative
                .AlsNichtMehrImAktuellenB56SnapshotVorhandenKennzeichnen();
        }

        foreach (var quellAlternative in quellAlternativen)
        {
            var vorhandeneAlternative =
                _alternativen.SingleOrDefault(
                    alternative =>
                        alternative.B56Position ==
                        quellAlternative.B56Position);

            if (vorhandeneAlternative is null)
            {
                AlternativeHinzufuegen(
                    quellAlternative);
                hinzugefuegteAlternativen.Add(
                    quellAlternative);
                continue;
            }

            vorhandeneAlternative.AusB56SnapshotAktualisieren(
                snapshotId,
                quellAlternative.Bezeichnung,
                quellAlternative.Kurztext);
        }

        QuellSnapshotId =
            snapshotId;
        ProjektmodellVersion++;

        return hinzugefuegteAlternativen.AsReadOnly();
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
