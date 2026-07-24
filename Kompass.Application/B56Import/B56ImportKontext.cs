namespace Kompass.Application.B56Import;

/// <summary>
/// Gemeinsamer Kontext eines vollständigen B56-Imports.
/// Alle Importschritte arbeiten mit derselben Instanz.
/// </summary>
public sealed class B56ImportKontext
{
    public Guid ImportId { get; init; }

    public Guid ProjektId { get; init; }

    public string Projektname { get; init; } = string.Empty;

    public string Quelldatei { get; init; } = string.Empty;

    public string Archivdatei { get; set; } = string.Empty;

    public string SHA256 { get; set; } = string.Empty;

    public DateTimeOffset Importzeitpunkt { get; init; }

    public B56Arbeitsmappe Arbeitsmappe { get; set; } = default!;
}
