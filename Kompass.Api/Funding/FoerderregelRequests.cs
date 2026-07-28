using Kompass.Domain.Funding;

namespace Kompass.Api.Funding;

public sealed class FoerderquoteRegelRequest
{
    public string Bezeichnung { get; set; } = string.Empty;

    public decimal Quote { get; set; }

    public string Bezugsbasis { get; set; } = string.Empty;

    public DateOnly GueltigAb { get; set; }

    public DateOnly? GueltigBis { get; set; }

    public string? Beschreibung { get; set; }
}

public sealed class HoechstbetragRegelRequest
{
    public string Bezeichnung { get; set; } = string.Empty;

    public decimal Betrag { get; set; }

    public string Waehrung { get; set; } = string.Empty;

    public string Bezugsbasis { get; set; } = string.Empty;

    public DateOnly GueltigAb { get; set; }

    public DateOnly? GueltigBis { get; set; }

    public string? Beschreibung { get; set; }
}

public sealed class KumulierbarkeitsregelRequest
{
    public string Bezeichnung { get; set; } = string.Empty;

    public KumulierbarkeitStatus Status { get; set; }

    public string Beschreibung { get; set; } = string.Empty;

    public DateOnly GueltigAb { get; set; }

    public DateOnly? GueltigBis { get; set; }
}

public sealed class PflichtnachweisRegelRequest
{
    public string Bezeichnung { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public Nachweiszeitpunkt Zeitpunkt { get; set; }

    public bool IstPflicht { get; set; } = true;

    public DateOnly GueltigAb { get; set; }

    public DateOnly? GueltigBis { get; set; }
}

public sealed class GueltigkeitsregelRequest
{
    public string Bezeichnung { get; set; } = string.Empty;

    public Gueltigkeitsbezug Bezug { get; set; }

    public DateOnly GueltigAb { get; set; }

    public DateOnly? GueltigBis { get; set; }

    public string? Beschreibung { get; set; }
}
