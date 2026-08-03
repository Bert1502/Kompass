namespace Kompass.Application.B56Import;

/// <summary>
/// Ergebnis des Tabellenimports.
/// </summary>
public sealed class B56TabellenImportErgebnis
{
    public int TabellenGesamt { get; init; }

    public int ErfolgreichImportiert { get; init; }

    public IReadOnlyList<B56Bauteil> Bauteile { get; init; }
        = Array.Empty<B56Bauteil>();

    public IReadOnlyList<B56Kennwert> Bestandskennwerte { get; init; }
        = Array.Empty<B56Kennwert>();

    public IReadOnlyList<B56Modernisierungsalternative>
        Modernisierungsalternativen { get; init; }
        = Array.Empty<B56Modernisierungsalternative>();

    public B56EffizienzstandardKontrollwert?
        EffizienzstandardKontrollwert { get; init; }

    public IReadOnlyList<string> Warnungen { get; init; }
        = Array.Empty<string>();
}
