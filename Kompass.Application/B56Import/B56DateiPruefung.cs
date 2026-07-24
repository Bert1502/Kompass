namespace Kompass.Application.B56Import;

public sealed record B56DateiPruefung
{
    public bool IstGueltig { get; init; }

    public string Fehlercode { get; init; } = string.Empty;

    public string Fehlermeldung { get; init; } = string.Empty;

    public string VollstaendigerDateipfad { get; init; } = string.Empty;

    public string Dateiname { get; init; } = string.Empty;

    public string Dateiendung { get; init; } = string.Empty;

    public long DateigroesseBytes { get; init; }

    public static B56DateiPruefung Gueltig(
        string vollstaendigerDateipfad,
        string dateiname,
        string dateiendung,
        long dateigroesseBytes)
    {
        return new B56DateiPruefung
        {
            IstGueltig = true,
            VollstaendigerDateipfad = vollstaendigerDateipfad,
            Dateiname = dateiname,
            Dateiendung = dateiendung,
            DateigroesseBytes = dateigroesseBytes
        };
    }

    public static B56DateiPruefung Ungueltig(
        string fehlercode,
        string fehlermeldung)
    {
        return new B56DateiPruefung
        {
            IstGueltig = false,
            Fehlercode = fehlercode,
            Fehlermeldung = fehlermeldung
        };
    }
}
