using Kompass.Application.B56Import;

namespace Kompass.Persistence.Data.Entities;

public sealed class B56ImportEintragEntity
{
    public Guid ImportId { get; set; }

    public Guid ProjektId { get; set; }

    public string Projektname { get; set; } = string.Empty;

    public string Originaldateiname { get; set; } = string.Empty;

    public string Archivdateipfad { get; set; } = string.Empty;

    public string Sha256 { get; set; } = string.Empty;

    public long DateigroesseBytes { get; set; }

    public DateTimeOffset ImportiertAm { get; set; }

    public string Dateiendung { get; set; } = string.Empty;

    public string? FachdatenJson { get; set; }

    public int SnapshotSchemaVersion { get; set; }

    public string ParserVersion { get; set; } = string.Empty;

    public B56SnapshotStatus SnapshotStatus { get; set; }

    public DateTimeOffset? BestaetigtAm { get; set; }

    public DateTimeOffset? VerworfenAm { get; set; }
}
