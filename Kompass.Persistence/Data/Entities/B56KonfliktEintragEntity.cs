using Kompass.Application.B56Import;

namespace Kompass.Persistence.Data.Entities;

public sealed class B56KonfliktEintragEntity
{
    public Guid Id { get; set; }

    public Guid ProjektId { get; set; }

    public Guid VorgaengerImportId { get; set; }

    public Guid NachfolgerImportId { get; set; }

    public string Bereich { get; set; } = string.Empty;

    public string Schluessel { get; set; } = string.Empty;

    public string Feld { get; set; } = string.Empty;

    public B56VergleichsAenderung Aenderung { get; set; }

    public string? AlterWert { get; set; }

    public string? NeuerWert { get; set; }

    public B56KonfliktEntscheidungsTyp Entscheidung { get; set; }

    public DateTimeOffset? EntschiedenAm { get; set; }

    public DateTimeOffset ErstelltAm { get; set; }
}
