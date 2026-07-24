namespace Kompass.Application.B56Import;

/// <summary>
/// Konfigurationsoptionen für den Import von B56-Exceldateien.
/// </summary>
public sealed class B56ImportOptionen
{
    /// <summary>
    /// Gibt an, ob identische Dateien mehrfach importiert werden dürfen.
    /// Standard: false.
    /// </summary>
    public bool DoppelteImporteZulassen { get; init; } = false;

    /// <summary>
    /// Aktiviert die SHA-256-Prüfung auf bereits importierte Dateien.
    /// Standard: true.
    /// </summary>
    public bool ArchivHashPruefen { get; init; } = true;

    /// <summary>
    /// Basisverzeichnis für das Archiv aller importierten B56-Dateien.
    /// </summary>
    public string ArchivBasisverzeichnis { get; init; } =
        Path.Combine("Daten", "B56Archiv");

    /// <summary>
    /// Kompatibler Zugriff für bestehende Registerimplementierungen.
    /// </summary>
    public string Archivverzeichnis =>
        ArchivBasisverzeichnis;

    /// <summary>
    /// Optionaler Pfad zu einer benutzerdefinierten
    /// Bauteilzuordnungsdatei.
    /// </summary>
    public string Bauteilzuordnungsdatei { get; init; } =
        string.Empty;

    /// <summary>
    /// Zulässige Dateiendungen.
    /// </summary>
    public string[] ErlaubteDateiendungen { get; init; } =
        [];

    /// <summary>
    /// Maximale Größe einer Importdatei.
    /// Standard: 50 MB.
    /// </summary>
    public long MaximaleDateigroesseBytes { get; init; } =
        50L * 1024L * 1024L;

    /// <summary>
    /// Archiviert jede importierte Datei automatisch.
    /// </summary>
    public bool ImportdateiArchivieren { get; init; } = true;

    /// <summary>
    /// Erstellt für jede Importdatei einen SHA-256-Hash.
    /// </summary>
    public bool HashBerechnen { get; init; } = true;

    /// <summary>
    /// Überschreibt vorhandene Archivdateien niemals.
    /// </summary>
    public bool VorhandeneArchivdateienUeberschreiben { get; init; } = false;

    /// <summary>
    /// Erstellt projektbezogene Unterordner im Archiv.
    /// </summary>
    public bool ProjektUnterordnerErstellen { get; init; } = true;

    /// <summary>
    /// Speichert zusätzlich Datum und Uhrzeit des Imports im Archivpfad.
    /// </summary>
    public bool ZeitstempelImArchivPfad { get; init; } = true;
}
