namespace Kompass.Application.B56Import;

public sealed record B56ImportEintrag
{
    public Guid ImportId { get; init; }

    public Guid ProjektId { get; init; }

    public string Projektname { get; init; } =
        string.Empty;

    public string Originaldateiname { get; init; } =
        string.Empty;

    public string Archivdateipfad { get; init; } =
        string.Empty;

    public string Sha256 { get; init; } =
        string.Empty;

    public long DateigroesseBytes { get; init; }

    public DateTimeOffset ImportiertAm { get; init; }

    public string Dateiendung { get; init; } =
        string.Empty;
}
