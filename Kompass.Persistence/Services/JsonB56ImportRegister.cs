using Kompass.Application.B56Import;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Kompass.Persistence.B56Import;

public sealed class JsonB56ImportRegister
    : IB56ImportRegister
{
    private static readonly SemaphoreSlim
        Schreibsperre = new(1, 1);

    private static readonly JsonSerializerOptions
        JsonOptionen =
        new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

    private readonly B56ImportOptionen _optionen;

    public JsonB56ImportRegister(
        IOptions<B56ImportOptionen> optionen)
    {
        _optionen = optionen.Value;
    }

    public async Task<B56ImportEintrag?>
        NachHashSuchenAsync(
            Guid projektId,
            string sha256,
            CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            sha256);

        var eintraege =
            await RegisterLesenAsync(
                projektId,
                cancellationToken);

        return eintraege
            .Where(
                eintrag =>
                    string.Equals(
                        eintrag.Sha256,
                        sha256,
                        StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(
                eintrag => eintrag.ImportiertAm)
            .FirstOrDefault();
    }

    public async Task EintragSpeichernAsync(
        B56ImportEintrag eintrag,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            eintrag);

        await Schreibsperre.WaitAsync(
            cancellationToken);

        try
        {
            var eintraege =
                await RegisterLesenOhneSperreAsync(
                    eintrag.ProjektId,
                    cancellationToken);

            if (eintraege.Any(
                    vorhandenerEintrag =>
                        vorhandenerEintrag.ImportId ==
                        eintrag.ImportId))
            {
                throw new InvalidOperationException(
                    $"Der Importeintrag '{eintrag.ImportId}' ist bereits vorhanden.");
            }

            eintraege.Add(eintrag);

            eintraege =
                eintraege
                    .OrderByDescending(
                        vorhandenerEintrag =>
                            vorhandenerEintrag.ImportiertAm)
                    .ToList();

            await RegisterSchreibenOhneSperreAsync(
                eintrag.ProjektId,
                eintraege,
                cancellationToken);
        }
        finally
        {
            Schreibsperre.Release();
        }
    }

    public async Task<IReadOnlyList<B56ImportEintrag>>
        AlleFuerProjektAbrufenAsync(
            Guid projektId,
            CancellationToken cancellationToken = default)
    {
        var eintraege =
            await RegisterLesenAsync(
                projektId,
                cancellationToken);

        return eintraege
            .OrderByDescending(
                eintrag => eintrag.ImportiertAm)
            .ToList()
            .AsReadOnly();
    }

    private async Task<List<B56ImportEintrag>>
        RegisterLesenAsync(
            Guid projektId,
            CancellationToken cancellationToken)
    {
        await Schreibsperre.WaitAsync(
            cancellationToken);

        try
        {
            return await RegisterLesenOhneSperreAsync(
                projektId,
                cancellationToken);
        }
        finally
        {
            Schreibsperre.Release();
        }
    }

    private async Task<List<B56ImportEintrag>>
        RegisterLesenOhneSperreAsync(
            Guid projektId,
            CancellationToken cancellationToken)
    {
        var registerpfad =
            ErmittleRegisterpfad(
                projektId);

        if (!File.Exists(registerpfad))
        {
            return [];
        }

        await using var stream =
            new FileStream(
                registerpfad,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 8192,
                useAsync: true);

        if (stream.Length == 0)
        {
            return [];
        }

        var eintraege =
            await JsonSerializer
                .DeserializeAsync<List<B56ImportEintrag>>(
                    stream,
                    JsonOptionen,
                    cancellationToken);

        return eintraege ?? [];
    }

    private async Task RegisterSchreibenOhneSperreAsync(
        Guid projektId,
        IReadOnlyCollection<B56ImportEintrag> eintraege,
        CancellationToken cancellationToken)
    {
        var registerpfad =
            ErmittleRegisterpfad(
                projektId);

        var verzeichnis =
            Path.GetDirectoryName(
                registerpfad)
            ?? throw new InvalidOperationException(
                "Das Verzeichnis des Importregisters konnte nicht ermittelt werden.");

        Directory.CreateDirectory(
            verzeichnis);

        var temporaererPfad =
            $"{registerpfad}.{Guid.NewGuid():N}.tmp";

        try
        {
            await using (
                var stream =
                    new FileStream(
                        temporaererPfad,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize: 8192,
                        useAsync: true))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    eintraege,
                    JsonOptionen,
                    cancellationToken);

                await stream.FlushAsync(
                    cancellationToken);
            }

            File.Move(
                temporaererPfad,
                registerpfad,
                overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaererPfad))
            {
                try
                {
                    File.Delete(temporaererPfad);
                }
                catch
                {
                    // Temporäre Datei wird beim nächsten Lauf ignoriert.
                }
            }
        }
    }

    private string ErmittleRegisterpfad(
        Guid projektId)
    {
        if (projektId == Guid.Empty)
        {
            throw new ArgumentException(
                "Die Projekt-ID darf nicht leer sein.",
                nameof(projektId));
        }

        var Archivverzeichnis =
            ErmittleArchivverzeichnis();

        return Path.Combine(
            Archivverzeichnis,
            "_Importregister",
            $"{projektId:N}.json");
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
}
