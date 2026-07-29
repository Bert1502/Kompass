namespace Kompass.Persistence.Data.Entities;

public sealed class B56SnapshotVergleichEntity
{
    public Guid VergleichId { get; set; }

    public Guid ProjektId { get; set; }

    public Guid VorgaengerSnapshotId { get; set; }

    public Guid NachfolgerSnapshotId { get; set; }

    public bool HatAenderungen { get; set; }

    public string VergleichJson { get; set; } = string.Empty;

    public DateTimeOffset ErstelltAm { get; set; }
}
