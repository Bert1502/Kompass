namespace Kompass.Application.B56Import;

/// <summary>
/// Ergebnis der fachlichen Verarbeitung einer B56-Datei.
/// </summary>
public sealed class B56ImportPipelineErgebnis
{
    public int ImportierteArbeitsblaetter { get; init; }

    public int ImportierteTabellen { get; init; }

    public int ImportierteBauteile { get; init; }

    public int ImportierteKennwerte { get; init; }

    public int ImportierteModernisierungsalternativen { get; init; }

    public IReadOnlyList<string> Warnungen { get; init; }
        = Array.Empty<string>();
}
