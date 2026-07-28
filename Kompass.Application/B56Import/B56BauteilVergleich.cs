namespace Kompass.Application.B56Import;

public sealed record B56BauteilVergleich(
    string Bauteilcode,
    string Bezeichnung,
    double? AlterUWert,
    double? NeuerUWert,
    double? AlteFlaeche,
    double? NeueFlaeche,
    B56VergleichsAenderung Aenderung);
