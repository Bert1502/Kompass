using Kompass.Application.B56Import;

namespace Kompass.Persistence.Services;

/// <summary>
/// Archiviert jede importierte B56-Datei revisionssicher.
/// </summary>
public sealed class B56ArchivService : IB56ArchivService
{
    private readonly B56ImportOptionen _optionen;

    public B56ArchivService(
        B56ImportOptionen optionen)
    {
        ArgumentNullException.ThrowIfNull(optionen);

        _optionen = optionen;
    }

    public async Task<string> ArchivierenAsync(
        Guid projektId,
        string projektname,
        string quelldateipfad,
        string sha256,
        DateTimeOffset importzeitpunkt,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projektname);
        ArgumentException.ThrowIfNullOrWhiteSpace(quelldateipfad);
        ArgumentException.ThrowIfNullOrWhiteSpace(sha256);

        if (!File.Exists(quelldateipfad))
        {
            throw new FileNotFoundException(
                "Die zu archivierende Datei wurde nicht gefunden.",
                quelldateipfad);
        }

        string projektOrdner = ErzeugeProjektOrdner(
            projektId,
            projektname);

        Directory.CreateDirectory(projektOrdner);

        string archivDateiname =
            ErzeugeArchivDateinamen(
                quelldateipfad,
                sha256,
                importzeitpunkt);

        string zielDatei =
            Path.Combine(
                projektOrdner,
                archivDateiname);

        await using FileStream quelle =
            new(
                quelldateipfad,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                FileOptions.Asynchronous);

        await using FileStream ziel =
            new(
                zielDatei,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous);

        await quelle.CopyToAsync(
            ziel,
            cancellationToken);

        await ziel.FlushAsync(cancellationToken);

        return zielDatei;
    }

    private string ErzeugeProjektOrdner(
        Guid projektId,
        string projektname)
    {
        string gueltigerProjektname =
            BereinigeDateiname(projektname);

        return Path.Combine(
            _optionen.ArchivBasisverzeichnis,
            projektId.ToString("N"),
            gueltigerProjektname);
    }

    private static string ErzeugeArchivDateinamen(
        string quelldatei,
        string sha256,
        DateTimeOffset zeitpunkt)
    {
        string dateiname =
            Path.GetFileNameWithoutExtension(
                quelldatei);

        string endung =
            Path.GetExtension(
                quelldatei);

        string kurzerHash =
            sha256.Length >= 8
                ? sha256[..8]
                : sha256;

        return
            $"{zeitpunkt:yyyyMMdd_HHmmss}_{dateiname}_{kurzerHash}{endung}";
    }

    private static string BereinigeDateiname(
        string text)
    {
        foreach (char zeichen in Path.GetInvalidFileNameChars())
        {
            text = text.Replace(
                zeichen,
                '_');
        }

        return text.Trim();
    }
}