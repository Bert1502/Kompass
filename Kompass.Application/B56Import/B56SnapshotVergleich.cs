namespace Kompass.Application.B56Import;

<<<<<<< HEAD
public sealed class B56SnapshotVergleich
{
    public Guid ProjektId { get; init; }

    public Guid VorgaengerSnapshotId { get; init; }

    public Guid NachfolgerSnapshotId { get; init; }

    public IReadOnlyList<B56KennwertVergleich> BestandskennwertVergleiche
        { get; init; } = Array.Empty<B56KennwertVergleich>();

    public IReadOnlyList<B56AlternativeVergleich> AlternativVergleiche
        { get; init; } = Array.Empty<B56AlternativeVergleich>();

    public IReadOnlyList<B56BauteilVergleich> GesamtbauteilVergleiche
        { get; init; } = Array.Empty<B56BauteilVergleich>();

    public bool HatAenderungen =>
        BestandskennwertVergleiche.Any(
            k => k.Aenderung != B56VergleichsAenderung.Unveraendert) ||
        AlternativVergleiche.Any(
            a => a.Aenderung != B56VergleichsAenderung.Unveraendert) ||
        GesamtbauteilVergleiche.Any(
            b => b.Aenderung != B56VergleichsAenderung.Unveraendert);
=======
/// <summary>
/// Ergebnis des Vergleichs zweier B56-Snapshots.
/// </summary>
public sealed record B56SnapshotVergleichErgebnis(
    Guid AltSnapshotId,
    Guid NeuSnapshotId,
    IReadOnlyList<B56AlternativenVergleich> Alternativen,
    IReadOnlyList<B56KennwertVergleich> Bestandskennwerte,
    IReadOnlyList<B56BauteilVergleich> Bauteile);

/// <summary>
/// Vergleich einer Modernisierungsalternative zwischen zwei Snapshots,
/// identifiziert über die B56-Position (1–9).
/// </summary>
public sealed record B56AlternativenVergleich(
    int Position,
    B56VergleichsArt Art,
    string? AlteBezeichnung,
    string? NeueBezeichnung,
    IReadOnlyList<B56KennwertVergleich> Kennwerte,
    IReadOnlyList<B56BauteilVergleich> Bauteile);

/// <summary>
/// Vergleich eines Kennwerts zwischen zwei Snapshots,
/// identifiziert über den Namen.
/// </summary>
public sealed record B56KennwertVergleich(
    string Name,
    string Einheit,
    B56VergleichsArt Art,
    double? AlterWert,
    double? NeuerWert);

/// <summary>
/// Vergleich eines Bauteils zwischen zwei Snapshots,
/// identifiziert über den Bauteilcode.
/// </summary>
public sealed record B56BauteilVergleich(
    string Bauteilcode,
    B56VergleichsArt Art,
    string? AlteBezeichnung,
    string? NeueBezeichnung,
    double? AlterUWert,
    double? NeuerUWert,
    double? AlteFlaeche,
    double? NeueFlaeche);

public enum B56VergleichsArt
{
    Unveraendert = 0,
    Hinzugefuegt = 1,
    Entfernt = 2,
    Geaendert = 3
>>>>>>> origin/main
}
