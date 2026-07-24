using Microsoft.Extensions.Options;

namespace Kompass.Application.B56Import;

public sealed class B56DateiPruefer : IB56DateiPruefer
{
    private readonly B56ImportOptionen _optionen;

    public B56DateiPruefer(
        IOptions<B56ImportOptionen> optionen)
    {
        ArgumentNullException.ThrowIfNull(optionen);

        _optionen = optionen.Value;
    }

    public B56DateiPruefung Pruefen(
        string dateipfad)
    {
        if (string.IsNullOrWhiteSpace(dateipfad))
        {
            return Fehler(
                "B56_DATEIPFAD_LEER",
                "Es wurde kein Dateipfad angegeben.");
        }

        if (!File.Exists(dateipfad))
        {
            return Fehler(
                "B56_DATEI_NICHT_GEFUNDEN",
                $"Die Datei wurde nicht gefunden: {dateipfad}");
        }

        var dateiendung = Path.GetExtension(dateipfad);

        var erlaubteDateiendung =
            _optionen.ErlaubteDateiendungen.Any(
                erlaubteEndung =>
                    string.Equals(
                        erlaubteEndung,
                        dateiendung,
                        StringComparison.OrdinalIgnoreCase));

        if (!erlaubteDateiendung)
        {
            return Fehler(
                "B56_DATEIFORMAT_UNGUELTIG",
                $"Das Dateiformat '{dateiendung}' wird nicht unterstützt.");
        }

        var dateiinformation = new FileInfo(dateipfad);

        if (dateiinformation.Length == 0)
        {
            return Fehler(
                "B56_DATEI_LEER",
                "Die ausgewählte B56-Datei ist leer.");
        }

        if (dateiinformation.Length >
            _optionen.MaximaleDateigroesseBytes)
        {
            return Fehler(
                "B56_DATEI_ZU_GROSS",
                $"Die Datei überschreitet die maximal zulässige Größe von " +
                $"{_optionen.MaximaleDateigroesseBytes} Bytes.");
        }

        return new B56DateiPruefung
        {
            IstGueltig = true,
            Fehlercode = string.Empty,
            Fehlermeldung = string.Empty
        };
    }

    private static B56DateiPruefung Fehler(
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