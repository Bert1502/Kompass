using System.Diagnostics;
using Kompass.Application.B56Import;

namespace Kompass.Persistence.Services;

/// <summary>
/// Zentraler Orchestrator für den Import einer B56-Datei.
/// </summary>
public sealed class B56ImportService : IB56ImportService
{
    private readonly IB56DateiPruefer _dateiPruefer;
    private readonly IB56HashService _hashService;
    private readonly IB56ArchivService _archivService;
    private readonly IB56ArbeitsmappenLeser _arbeitsmappenLeser;
    private readonly IB56TabellenFinder _tabellenFinder;
    private readonly IB56ImportRegister _importRegister;

    public B56ImportService(
        IB56DateiPruefer dateiPruefer,
        IB56HashService hashService,
        IB56ArchivService archivService,
        IB56ArbeitsmappenLeser arbeitsmappenLeser,
        IB56TabellenFinder tabellenFinder,
        IB56ImportRegister importRegister)
    {
        _dateiPruefer = dateiPruefer;
        _hashService = hashService;
        _archivService = archivService;
        _arbeitsmappenLeser = arbeitsmappenLeser;
        _tabellenFinder = tabellenFinder;
        _importRegister = importRegister;
    }

    public async Task<B56ImportErgebnis> ImportierenAsync(
        Guid projektId,
        string projektname,
        string dateipfad,
        CancellationToken cancellationToken = default)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        B56DateiPruefung pruefung =
            _dateiPruefer.Pruefen(dateipfad);

        if (!pruefung.IstGueltig)
        {
            return new B56ImportErgebnis
            {
                Erfolgreich = false,
                Fehler = new[]
                {
                    pruefung.Fehlermeldung
                }
            };
        }

        string sha256 =
            await _hashService.BerechnenAsync(
                dateipfad,
                cancellationToken);

        string archivDatei =
            await _archivService.ArchivierenAsync(
                projektId,
                projektname,
                dateipfad,
                sha256,
                DateTimeOffset.UtcNow,
                cancellationToken);

        var arbeitsmappe =
            await _arbeitsmappenLeser.LesenAsync(
                dateipfad,
                cancellationToken);

        var tabellen =
            _tabellenFinder.Finde(
                arbeitsmappe);

        stopwatch.Stop();

        return new B56ImportErgebnis
        {
            Erfolgreich = true,
            ImportId = Guid.NewGuid(),
            SHA256 = sha256,
            ArchivDatei = archivDatei,
            ArbeitsblattAnzahl =
                arbeitsmappe.Arbeitsblaetter.Count,
            ErkannteTabellen =
                tabellen.Count,
            ErkannteBauteile = 0,
            Dauer = stopwatch.Elapsed
        };
    }
}