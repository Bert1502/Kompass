namespace Kompass.Domain.Reports;

/// <summary>
/// Eine Zeile im Alternativenvergleich-Bericht.
/// Fasst die wesentlichen Kennzahlen einer Modernisierungsalternative zusammen.
/// </summary>
public sealed record AlternativenvergleichZeile(
    Guid AlternativeId,
    int? B56Position,
    string Bezeichnung,
    string Kurztext,
    decimal Gesamtkosten,
    int AnzahlKostenpositionen,
    bool IstImAktuellenB56SnapshotVorhanden);
