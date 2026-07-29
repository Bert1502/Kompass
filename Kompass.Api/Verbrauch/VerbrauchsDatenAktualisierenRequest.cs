using Kompass.Domain.Economics;

namespace Kompass.Api.Verbrauch;

public sealed class VerbrauchsDatenAktualisierenRequest
{
    public DateOnly PeriodeVon { get; init; }
    public DateOnly PeriodeBis { get; init; }
    public Energietraeger Energietraeger { get; init; }

    /// <summary>Verbrauchsmenge in kWh.</summary>
    public decimal Menge { get; init; }

    /// <summary>Energiekosten in EUR.</summary>
    public decimal Kosten { get; init; }

    /// <summary>Witterungsbereinigungsfaktor (optional).</summary>
    public decimal? WitterungsbereinigungsFaktor { get; init; }

    /// <summary>Bezugsfläche in m² (optional).</summary>
    public decimal? Flaeche { get; init; }

    /// <summary>B56-Vergleichswert in kWh (optional).</summary>
    public decimal? B56VergleichsWert { get; init; }

    /// <summary>Anpassungsfaktor (optional).</summary>
    public decimal? AnpassungsFaktor { get; init; }

    /// <summary>Begründung für den Anpassungsfaktor (optional).</summary>
    public string? AnpassungsBegruendung { get; init; }

    /// <summary>Dokumentierte Abweichungsursache (optional).</summary>
    public string? Abweichungsursache { get; init; }
}
