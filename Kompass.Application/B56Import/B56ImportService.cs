namespace Kompass.Application.B56Import;

public sealed class B56ImportService : IB56ImportService
{
    private readonly IB56DateiPruefer _dateiPruefer;
    private readonly IB56HashService _hashService;
    private readonly IB56ArchivService _archivService;
    private readonly IB56ImportRegister _importRegister;
    private readonly IB56ArbeitsmappenLeser _arbeitsmappenLeser;
    private readonly IB56ImportPipeline _importPipeline;
    private readonly B56ImportOptionen _optionen;

    public B56ImportService(
        IB56DateiPruefer dateiPruefer,
        IB56HashService hashService,
        IB56ArchivService archivService,
        IB56ImportRegister importRegister,
        IB56ArbeitsmappenLeser arbeitsmappenLeser,
        IB56ImportPipeline importPipeline,
        B56ImportOptionen optionen)
    {
        _dateiPruefer = dateiPruefer;
        _hashService = hashService;
        _archivService = archivService;
        _importRegister = importRegister;
        _arbeitsmappenLeser = arbeitsmappenLeser;
        _importPipeline = importPipeline;
        _optionen = optionen;
    }

    public async Task<B56ImportErgebnis> ImportierenAsync(
        B56ImportAnfrage anfrage,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(anfrage);

        if (anfrage.ProjektId == Guid.Empty)
        {
            return B56ImportErgebnis.Abgelehnt(
                anfrage.ProjektId,
                anfrage.Quelldateipfad,
                "B56-PROJEKT-ID-FEHLT",
                "Für den B56-Import wurde keine gültige Projekt-ID angegeben.");
        }

        if (string.IsNullOrWhiteSpace(anfrage.Projektname))
        {
            return B56ImportErgebnis.Abgelehnt(
                anfrage.ProjektId,
                anfrage.Quelldateipfad,
                "B56-PROJEKTNAME-FEHLT",
                "Für den B56-Import wurde kein Projektname angegeben.");
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pruefung =
                _dateiPruefer.Pruefen(
                    anfrage.Quelldateipfad);

            if (!pruefung.IstGueltig)
            {
                return B56ImportErgebnis.Abgelehnt(
                    anfrage.ProjektId,
                    anfrage.Quelldateipfad,
                    pruefung.Fehlercode,
                    pruefung.Fehlermeldung);
            }

            var sha256 =
                await _hashService.BerechnenAsync(
                    pruefung.VollstaendigerDateipfad,
                    cancellationToken);

            var vorhandenerEintrag =
                await _importRegister.NachHashSuchenAsync(
                    anfrage.ProjektId,
                    sha256,
                    cancellationToken);

            var wurdeMitAktuellemParserImportiert =
                vorhandenerEintrag is not null &&
                string.Equals(
                    vorhandenerEintrag.ParserVersion,
                    B56SnapshotVersionen.AktuelleParserVersion,
                    StringComparison.Ordinal);

            if (wurdeMitAktuellemParserImportiert &&
                !_optionen.DoppelteImporteZulassen)
            {
                return B56ImportErgebnis.BereitsImportiert(
                    vorhandenerEintrag!,
                    pruefung.VollstaendigerDateipfad);
            }

            var importzeitpunkt = DateTimeOffset.UtcNow;

            var archivdateipfad =
                await _archivService.ArchivierenAsync(
                    anfrage.ProjektId,
                    anfrage.Projektname,
                    pruefung.VollstaendigerDateipfad,
                    sha256,
                    importzeitpunkt,
                    cancellationToken);

            if (_optionen.ArchivHashPruefen)
            {
                var archivHash =
                    await _hashService.BerechnenAsync(
                        archivdateipfad,
                        cancellationToken);

                if (!string.Equals(
                        sha256,
                        archivHash,
                        StringComparison.OrdinalIgnoreCase))
                {
                    VersucheDateiZuLoeschen(
                        archivdateipfad);

                    return B56ImportErgebnis.Fehlgeschlagen(
                        anfrage.ProjektId,
                        pruefung.VollstaendigerDateipfad,
                        "Die archivierte Datei stimmt nicht mit der Originaldatei überein.");
                }
            }

            var eintrag = new B56ImportEintrag
            {
                ImportId = Guid.NewGuid(),
                ProjektId = anfrage.ProjektId,
                Projektname = anfrage.Projektname.Trim(),
                Originaldateiname = pruefung.Dateiname,
                Archivdateipfad = archivdateipfad,
                Sha256 = sha256,
                DateigroesseBytes = pruefung.DateigroesseBytes,
                ImportiertAm = importzeitpunkt,
                Dateiendung = pruefung.Dateiendung
            };

            B56ImportPipelineErgebnis pipelineErgebnis;

            try
            {
                var arbeitsmappe =
                    await _arbeitsmappenLeser.LesenAsync(
                        archivdateipfad,
                        cancellationToken);

                pipelineErgebnis =
                    await _importPipeline.ImportierenAsync(
                        new B56ImportKontext
                        {
                            ImportId = eintrag.ImportId,
                            ProjektId = eintrag.ProjektId,
                            Projektname = eintrag.Projektname,
                            Quelldatei = pruefung.VollstaendigerDateipfad,
                            Archivdatei = archivdateipfad,
                            SHA256 = sha256,
                            Importzeitpunkt = importzeitpunkt,
                            Arbeitsmappe = arbeitsmappe
                        },
                        cancellationToken);

                eintrag =
                    eintrag with
                    {
                        SnapshotStatus =
                            ErmittleSnapshotStatus(
                                pipelineErgebnis)
                    };

                await _importRegister
                    .EintragMitFachdatenSpeichernAsync(
                    eintrag,
                    pipelineErgebnis,
                    cancellationToken);
            }
            catch
            {
                VersucheDateiZuLoeschen(
                    archivdateipfad);

                throw;
            }

            var ergebnis =
                B56ImportErgebnis.Erfolgreich(
                    eintrag,
                    pruefung.VollstaendigerDateipfad,
                    pipelineErgebnis);

            if (vorhandenerEintrag is not null)
            {
                ergebnis.MeldungHinzufuegen(
                    B56Meldungstyp.Warnung,
                    wurdeMitAktuellemParserImportiert
                        ? "B56-DUBLETTE-ZUGELASSEN"
                        : "B56-MIT-NEUEM-PARSER-IMPORTIERT",
                    wurdeMitAktuellemParserImportiert
                        ? "Die Datei war bereits vorhanden, wurde aufgrund der Konfiguration jedoch erneut archiviert."
                        : "Die Datei wurde erneut importiert, weil inzwischen eine neuere Parser-Version verfügbar ist.");
            }

            return ergebnis;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return B56ImportErgebnis.Fehlgeschlagen(
                anfrage.ProjektId,
                anfrage.Quelldateipfad,
                exception.Message);
        }
    }

    private static void VersucheDateiZuLoeschen(
        string dateipfad)
    {
        if (string.IsNullOrWhiteSpace(dateipfad))
        {
            return;
        }

        try
        {
            if (File.Exists(dateipfad))
            {
                File.Delete(dateipfad);
            }
        }
        catch
        {
            // Der ursprüngliche Fehler bleibt maßgeblich.
        }
    }

    private static B56SnapshotStatus ErmittleSnapshotStatus(
        B56ImportPipelineErgebnis pipelineErgebnis)
    {
        if (pipelineErgebnis.BlockierendeFehler.Count > 0)
        {
            return B56SnapshotStatus.Blockiert;
        }

        return pipelineErgebnis.Warnungen.Count > 0
            ? B56SnapshotStatus.MitWarnungen
            : B56SnapshotStatus.TechnischGeprueft;
    }
}
