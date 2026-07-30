namespace Kompass.Persistence.Data.Entities;

public sealed class B56KonfliktEintragEntity
{
    public Guid KonfliktId { get; set; }

    public Guid ProjektId { get; set; }

    public Guid VorgaengerSnapshotId { get; set; }

    public Guid NachfolgerSnapshotId { get; set; }

    public string Bereich { get; set; } = string.Empty;

    public string Schluessel { get; set; } = string.Empty;

    public string Feld { get; set; } = string.Empty;

    /// <summary>B56VergleichsAenderung als Ganzzahl gespeichert.</summary>
    public int Aenderung { get; set; }

    public string? AlterWert { get; set; }

    public string? NeuerWert { get; set; }

    /// <summary>B56KonfliktEntscheidungsTyp als Ganzzahl gespeichert.</summary>
    public int Entscheidung { get; set; }

    public DateTimeOffset? EntschiedenAm { get; set; }
}
