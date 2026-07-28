namespace Kompass.Application.B56Import;

public sealed record B56KennwertVergleich(
    string Name,
    string Einheit,
    double? AlterWert,
    double? NeuerWert,
    B56VergleichsAenderung Aenderung);
