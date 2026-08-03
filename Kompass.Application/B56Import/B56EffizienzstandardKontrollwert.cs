namespace Kompass.Application.B56Import;

/// <summary>
/// Unveränderter, ausschließlich zur Gegenkontrolle importierter
/// Effizienzstandard aus B56.
/// </summary>
public sealed class B56EffizienzstandardKontrollwert
{
    public Guid ImportId { get; init; }

    public string Feldname { get; init; } = "BEG_ZIEL";

    public string Originaltext { get; init; } = string.Empty;

    public string Arbeitsblatt { get; init; } = string.Empty;

    public string Zelladresse { get; init; } = string.Empty;
}
