namespace Kompass.Application.B56Import;

/// <summary>
/// Ergebnis des Tabellenimports.
/// </summary>
public sealed class B56TabellenImportErgebnis
{
    public int TabellenGesamt { get; init; }

    public int ErfolgreichImportiert { get; init; }

    public IReadOnlyList<string> Warnungen { get; init; }
        = Array.Empty<string>();
}
