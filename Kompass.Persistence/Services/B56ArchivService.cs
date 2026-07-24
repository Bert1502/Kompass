using Kompass.Application.B56Import;
using Microsoft.Extensions.Options;
using System.Text;

namespace Kompass.Persistence.B56Import;

public sealed class B56ArchivService : IB56ArchivService
{
    private readonly B56ImportOptionen _optionen;

    public B56ArchivService(
        IOptions<B56ImportOptionen> optionen)
    {
        _optionen = optionen.Value;
    }

    public async Task<string> ArchivierenAsync(
        Guid projektId,
        string projektname,
        string quelldateipfad,
        string sha256,
        DateTimeOffset importzeitpunkt,
        CancellationToken cancellationToken = default)
    {
        if (projektId == Guid.Empty)
        {
            throw new ArgumentException(
                "Die Projekt-ID darf nicht leer sein.",
                nameof(projektId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            projektname);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            quelldateipfad);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            sha256);

        var Archivverzeichnis =
            ErmittleArchivverzeichnis();

        var projektOrdnername =
            $"{BereinigeDateiname(projektname)}_{projektId:N}";

        var zielverzeichnis =
            Path.Combine(
                Archivverzeichnis,
                projektOrdnername,
                importzeitpunkt.Year.ToString("0000"),
                importzeitpunkt.Month.ToString("00"));

        Directory.CreateDirectory(
            zielverzeichnis);

        var originaldateiname =
            Path.GetFileNameWithoutExtension(
                quelldateipfad);

        var dateiendung =
            Path.GetExtension(
                quelldateipfad)
            .ToLowerInvariant();

        var kurzerHash =
            sha256.Length >= 12
                ? sha256[..12]
                : sha256;

        var zeitstempel =
            importzeitpunkt
                .ToUniversalTime()
                .ToString("yyyyMMdd_HHmmss_fff");

        var archivdateiname =
            $"{zeitstempel}_{BereinigeDateiname(originaldateiname)}_{kurzerHash}{dateiendung}";

        var archivdateipfad =
            Path.Combine(
                zielverzeichnis,
                archivdateiname);

        archivdateipfad =
            ErzeugeEindeutigenDateipfad(
                archivdateipfad);

        await DateiKopierenAsync(
            quelldateipfad,
            archivdateipfad,
            cancellationToken);

        return Path.GetFullPath(
            archivdateipfad);
    }

    private string ErmittleArchivverzeichnis()
    {
        var konfigurierterPfad =
            _optionen.Archivverzeichnis;

        if (string.IsNullOrWhiteSpace(
                konfigurierterPfad))
        {
            konfigurierterPfad =
                "Daten/B56Archiv";
        }

        if (Path.IsPathRooted(
                konfigurierterPfad))
        {
            return Path.GetFullPath(
                konfigurierterPfad);
        }

        return Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                konfigurierterPfad));
    }

    private static async Task DateiKopierenAsync(
        string quellpfad,
        string zielpfad,
        CancellationToken cancellationToken)
    {
        await using var quellstream =
            new FileStream(
                quellpfad,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                bufferSize: 1024 * 128,
                useAsync: true);

        await using var zielstream =
            new FileStream(
                zielpfad,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 1024 * 128,
                useAsync: true);

        await quellstream.CopyToAsync(
            zielstream,
            cancellationToken);

        await zielstream.FlushAsync(
            cancellationToken);
    }

    private static string ErzeugeEindeutigenDateipfad(
        string vorgesehenerDateipfad)
    {
        if (!File.Exists(vorgesehenerDateipfad))
        {
            return vorgesehenerDateipfad;
        }

        var verzeichnis =
            Path.GetDirectoryName(
                vorgesehenerDateipfad)
            ?? throw new InvalidOperationException(
                "Das Zielverzeichnis konnte nicht ermittelt werden.");

        var dateinameOhneEndung =
            Path.GetFileNameWithoutExtension(
                vorgesehenerDateipfad);

        var dateiendung =
            Path.GetExtension(
                vorgesehenerDateipfad);

        for (var nummer = 2;
             nummer <= 9999;
             nummer++)
        {
            var kandidat =
                Path.Combine(
                    verzeichnis,
                    $"{dateinameOhneEndung}_{nummer}{dateiendung}");

            if (!File.Exists(kandidat))
            {
                return kandidat;
            }
        }

        throw new IOException(
            "Es konnte kein eindeutiger Archivdateiname erzeugt werden.");
    }

    private static string BereinigeDateiname(
        string wert)
    {
        if (string.IsNullOrWhiteSpace(wert))
        {
            return "Unbenannt";
        }

        var ungueltigeZeichen =
            Path.GetInvalidFileNameChars();

        var builder =
            new StringBuilder(
                wert.Length);

        foreach (var zeichen in wert.Trim())
        {
            if (ungueltigeZeichen.Contains(
                    zeichen))
            {
                builder.Append('_');
                continue;
            }

            builder.Append(
                char.IsWhiteSpace(zeichen)
                    ? '_'
                    : zeichen);
        }

        var bereinigterWert =
            builder
                .ToString()
                .Trim('_', '.');

        while (bereinigterWert.Contains(
                   "__",
                   StringComparison.Ordinal))
        {
            bereinigterWert =
                bereinigterWert.Replace(
                    "__",
                    "_",
                    StringComparison.Ordinal);
        }

        if (bereinigterWert.Length > 80)
        {
            bereinigterWert =
                bereinigterWert[..80];
        }

        return string.IsNullOrWhiteSpace(
            bereinigterWert)
            ? "Unbenannt"
            : bereinigterWert;
    }
}
