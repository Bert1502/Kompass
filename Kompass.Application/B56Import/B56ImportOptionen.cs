namespace Kompass.Application.B56Import;

public sealed class B56ImportOptionen
{
    public bool DoppelteImporteZulassen { get; init; }

    public bool ArchivHashPruefen { get; init; }

    public long MaximaleDateigroesseBytes { get; init; }
        = 100L * 1024L * 1024L;

    public string Archivverzeichnis { get; init; }
        = string.Empty;

    public string Bauteilzuordnungsdatei { get; init; }
        = string.Empty;
}