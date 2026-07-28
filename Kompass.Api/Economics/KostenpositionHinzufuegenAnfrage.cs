using Kompass.Domain.Economics;

namespace Kompass.Api.Economics;

public sealed class KostenpositionHinzufuegenAnfrage
{
    /// <summary>Kurze Bezeichnung der Kostenposition.</summary>
    public string Bezeichnung { get; init; } = string.Empty;

    /// <summary>Betrag in Euro (nicht negativ).</summary>
    public decimal Betrag { get; init; }

    /// <summary>Kostenart der Position.</summary>
    public Kostenart Kostenart { get; init; }
}
