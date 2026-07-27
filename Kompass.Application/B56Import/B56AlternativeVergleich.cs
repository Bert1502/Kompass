namespace Kompass.Application.B56Import;

public sealed record B56AlternativeVergleich(
    int B56Position,
    string AlteBezeichnung,
    string NeueBezeichnung,
    B56VergleichsAenderung Aenderung,
    IReadOnlyList<B56KennwertVergleich> KennwertVergleiche,
    IReadOnlyList<B56BauteilVergleich> BauteilVergleiche);
