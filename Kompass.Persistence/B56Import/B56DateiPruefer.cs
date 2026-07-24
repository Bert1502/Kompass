using Kompass.Application.B56Import;
using Microsoft.Extensions.Options;

namespace Kompass.Persistence.B56Import;

public sealed class B56DateiPruefer : IB56DateiPruefer
{
    private static readonly HashSet<string>
        UnterstuetzteDateiendungen =
        new(
            StringComparer.OrdinalIgnoreCase)
        {
            ".xlsx",
            ".xlsm"
        };

    private readonly B56ImportOptionen _optionen;

    public B56DateiPruefer(
        IOptions<B56ImportOptionen> optionen)
    {
        _optionen = optionen.Value;
    }

    public B56DateiPruefung Pruefen(
        string dateipfad)
    {
        if (string.IsNullOrWhiteSpace(dateipfad))
        {
            return B56DateiPruefung.Ungueltig(
                "B56-DATEIPFAD-FEHLT",
                "Es wurde keine B56-Datei angegeben.");
        }

        string vollstaendigerDateipfad;

        try
        {
            vollstaendigerDateipfad =
                Path.GetFullPath(
                    dateipfad.Trim());
        }
        catch (Exception exception)
        {
            return B56DateiPruefung.Ungueltig(
                "B56-DATEIPFAD-UNGUELTIG",
                $"Der angegebene Dateipfad ist ungültig: {exception.Message}");
        }

        if (!File.Exists(vollstaendigerDateipfad))
        {
            return B56DateiPruefung.Ungueltig(
                "B56-DATEI-NICHT-GEFUNDEN",
                "Die angegebene B56-Datei wurde nicht gefunden.");
        }

        FileInfo dateiInfo;

        try
        {
            dateiInfo =
                new FileInfo(
                    vollstaendigerDateipfad);
        }
        catch (Exception exception)
        {
            return B56DateiPruefung.Ungueltig(
                "B56-DATEI-NICHT-LESBAR",
                $"Die Dateiinformationen konnten nicht gelesen werden: {exception.Message}");
        }

        var dateiendung =
            dateiInfo.Extension;

        if (!UnterstuetzteDateiendungen.Contains(
                dateiendung))
        {
            return B56DateiPruefung.Ungueltig(
                "B56-DATEIFORMAT-NICHT-UNTERSTUETZT",
                "Es werden ausschließlich XLSX- und XLSM-Dateien unterstützt.");
        }

        if (dateiInfo.Length <= 0)
        {
            return B56DateiPruefung.Ungueltig(
                "B56-DATEI-LEER",
                "Die ausgewählte B56-Datei ist leer.");
        }

        if (dateiInfo.Length >
            _optionen.MaximaleDateigroesseBytes)
        {
            return B56DateiPruefung.Ungueltig(
                "B56-DATEI-ZU-GROSS",
                $"Die Datei überschreitet die zulässige Größe von {FormatiereDateigroesse(_optionen.MaximaleDateigroesseBytes)}.");
        }

        try
        {
            using var stream =
                new FileStream(
                    vollstaendigerDateipfad,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);

            if (!IstOpenXmlZipDatei(stream))
            {
                return B56DateiPruefung.Ungueltig(
                    "B56-DATEI-KEINE-GUELTIGE-EXCEL-DATEI",
                    "Die Datei besitzt keine gültige OpenXML-Dateistruktur.");
            }
        }
        catch (UnauthorizedAccessException)
        {
            return B56DateiPruefung.Ungueltig(
                "B56-ZUGRIFF-VERWEIGERT",
                "Auf die ausgewählte Datei kann nicht zugegriffen werden.");
        }
        catch (IOException exception)
        {
            return B56DateiPruefung.Ungueltig(
                "B56-DATEI-GESPERRT",
                $"Die Datei konnte nicht geöffnet werden: {exception.Message}");
        }

        return B56DateiPruefung.Gueltig(
            vollstaendigerDateipfad,
            dateiInfo.Name,
            dateiendung.ToLowerInvariant(),
            dateiInfo.Length);
    }

    private static bool IstOpenXmlZipDatei(
        Stream stream)
    {
        if (!stream.CanRead || stream.Length < 4)
        {
            return false;
        }

        Span<byte> signatur =
            stackalloc byte[4];

        var geleseneBytes =
            stream.Read(signatur);

        if (geleseneBytes < 4)
        {
            return false;
        }

        return signatur[0] == 0x50
            && signatur[1] == 0x4B
            && (
                signatur[2] == 0x03
                || signatur[2] == 0x05
                || signatur[2] == 0x07
            )
            && (
                signatur[3] == 0x04
                || signatur[3] == 0x06
                || signatur[3] == 0x08
            );
    }

    private static string FormatiereDateigroesse(
        long bytes)
    {
        const double megabyte =
            1024d * 1024d;

        return $"{bytes / megabyte:N1} MB";
    }
}
