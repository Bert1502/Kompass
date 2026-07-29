namespace Kompass.Application.B56Import;

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

    public IReadOnlyList<B56Vergleichskonflikt> Konflikte
        { get; init; } = Array.Empty<B56Vergleichskonflikt>();

    public bool HatAenderungen =>
        BestandskennwertVergleiche.Any(
            k => k.Aenderung != B56VergleichsAenderung.Unveraendert) ||
        AlternativVergleiche.Any(
            a => a.Aenderung != B56VergleichsAenderung.Unveraendert) ||
        GesamtbauteilVergleiche.Any(
            b => b.Aenderung != B56VergleichsAenderung.Unveraendert);
}
