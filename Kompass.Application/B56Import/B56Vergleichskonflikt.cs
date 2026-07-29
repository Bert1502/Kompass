namespace Kompass.Application.B56Import;

public sealed record B56Vergleichskonflikt(
    string Bereich,
    string Schluessel,
    string Feld,
    B56VergleichsAenderung Aenderung);
