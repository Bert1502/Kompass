namespace Kompass.Application.B56Import;

public sealed class B56KonfliktEintrag
{
    public Guid Id { get; init; }

    public Guid ProjektId { get; init; }

    public Guid VorgaengerImportId { get; init; }

    public Guid NachfolgerImportId { get; init; }

    public string Bereich { get; init; } = string.Empty;

    public string Schluessel { get; init; } = string.Empty;

    public string Feld { get; init; } = string.Empty;

    public B56VergleichsAenderung Aenderung { get; init; }

    public string? AlterWert { get; init; }

    public string? NeuerWert { get; init; }

    public B56KonfliktEntscheidungsTyp Entscheidung { get; set; }

    public DateTimeOffset? EntschiedenAm { get; set; }

    public DateTimeOffset ErstelltAm { get; init; }
}
