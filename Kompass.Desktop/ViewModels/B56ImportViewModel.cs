using Kompass.Application.B56Import;
using Kompass.Desktop.Models;
using Kompass.Desktop.Mvvm;
using Kompass.Desktop.Services;
using System.IO;
using System.Windows.Input;

namespace Kompass.Desktop.ViewModels;

public sealed class B56ImportViewModel : ViewModelBase
{
    private readonly IDateiDialogService _dateiDialogService;
    private readonly IDialogService _dialogService;
    private readonly IB56ImportApiClient _importApiClient;

    private Guid? _projektId;
    private string _projektname = string.Empty;
    private string _ausgewaehlterDateipfad = string.Empty;
    private string _dateiname = "Keine Datei ausgewählt";
    private string _dateigroesse = string.Empty;
    private string _dateityp = string.Empty;
    private string _statusText = "Bitte wählen Sie eine B56-Excel-Datei aus.";
    private bool _istDateiAusgewaehlt;
    private bool _istBeschaeftigt;

    public B56ImportViewModel(
        IDateiDialogService dateiDialogService,
        IDialogService dialogService,
        IB56ImportApiClient importApiClient)
    {
        _dateiDialogService = dateiDialogService;
        _dialogService = dialogService;
        _importApiClient = importApiClient;

        DateiAuswaehlenCommand =
            new RelayCommand(
                DateiAuswaehlen,
                KannDateiAuswaehlen);

        AuswahlEntfernenCommand =
            new RelayCommand(
                AuswahlEntfernen,
                KannAuswahlEntfernen);

        ImportStartenCommand =
            new AsyncRelayCommand(
                ImportStartenAsync,
                KannImportStarten);
    }

    public Guid? ProjektId
    {
        get => _projektId;

        private set =>
            SetProperty(
                ref _projektId,
                value);
    }

    public string Projektname
    {
        get => _projektname;

        private set =>
            SetProperty(
                ref _projektname,
                value);
    }

    public string AusgewaehlterDateipfad
    {
        get => _ausgewaehlterDateipfad;

        private set =>
            SetProperty(
                ref _ausgewaehlterDateipfad,
                value);
    }

    public string Dateiname
    {
        get => _dateiname;

        private set =>
            SetProperty(
                ref _dateiname,
                value);
    }

    public string Dateigroesse
    {
        get => _dateigroesse;

        private set =>
            SetProperty(
                ref _dateigroesse,
                value);
    }

    public string Dateityp
    {
        get => _dateityp;

        private set =>
            SetProperty(
                ref _dateityp,
                value);
    }

    public string StatusText
    {
        get => _statusText;

        private set =>
            SetProperty(
                ref _statusText,
                value);
    }

    public bool IstDateiAusgewaehlt
    {
        get => _istDateiAusgewaehlt;

        private set
        {
            if (SetProperty(
                    ref _istDateiAusgewaehlt,
                    value))
            {
                AktualisiereBefehle();
            }
        }
    }

    public bool IstBeschaeftigt
    {
        get => _istBeschaeftigt;

        private set
        {
            if (SetProperty(
                    ref _istBeschaeftigt,
                    value))
            {
                AktualisiereBefehle();
            }
        }
    }

    public ICommand DateiAuswaehlenCommand { get; }

    public ICommand AuswahlEntfernenCommand { get; }

    public ICommand ImportStartenCommand { get; }

    public void ProjektSetzen(
        Guid projektId,
        string projektname)
    {
        ProjektId = projektId;
        Projektname = projektname;
    }

    private void DateiAuswaehlen()
    {
        var dateipfad =
            _dateiDialogService
                .B56DateiAuswaehlen();

        if (string.IsNullOrWhiteSpace(dateipfad))
        {
            StatusText =
                "Es wurde keine Datei ausgewählt.";

            return;
        }

        try
        {
            DateiPruefenUndUebernehmen(
                dateipfad);
        }
        catch (Exception exception)
        {
            AuswahlEntfernen();

            StatusText =
                $"Die Datei konnte nicht übernommen werden: {exception.Message}";

            _dialogService.FehlerAnzeigen(
                StatusText);
        }
    }

    private void DateiPruefenUndUebernehmen(
        string dateipfad)
    {
        if (!File.Exists(dateipfad))
        {
            throw new FileNotFoundException(
                "Die ausgewählte Datei wurde nicht gefunden.",
                dateipfad);
        }

        var dateiendung =
            Path.GetExtension(dateipfad);

        if (!IstUnterstuetzteDateiendung(
                dateiendung))
        {
            throw new InvalidOperationException(
                "Es werden nur Dateien im Format XLSX oder XLSM unterstützt.");
        }

        var dateiInfo =
            new FileInfo(dateipfad);

        if (dateiInfo.Length == 0)
        {
            throw new InvalidOperationException(
                "Die ausgewählte Datei ist leer.");
        }

        AusgewaehlterDateipfad =
            dateiInfo.FullName;

        Dateiname =
            dateiInfo.Name;

        Dateigroesse =
            FormatiereDateigroesse(
                dateiInfo.Length);

        Dateityp =
            dateiendung.Equals(
                ".xlsm",
                StringComparison.OrdinalIgnoreCase)
                ? "Excel-Arbeitsmappe mit Makros"
                : "Excel-Arbeitsmappe";

        IstDateiAusgewaehlt = true;

        StatusText =
            "Die Datei wurde ausgewählt und kann importiert werden.";
    }

    private async Task ImportStartenAsync()
    {
        if (!ProjektId.HasValue)
        {
            _dialogService.FehlerAnzeigen(
                "Es wurde kein Projekt für den Import festgelegt.");

            return;
        }

        if (!IstDateiAusgewaehlt)
        {
            return;
        }

        try
        {
            IstBeschaeftigt = true;

            StatusText =
                "Die B56-Datei wird importiert …";

            var ergebnis =
                await _importApiClient.ImportierenAsync(
                    ProjektId.Value,
                    AusgewaehlterDateipfad);

            StatusText =
                ErzeugeStatusText(
                    ergebnis);

            if (ergebnis.Status is
                B56ImportStatus.Abgelehnt or
                B56ImportStatus.Fehlgeschlagen)
            {
                _dialogService.FehlerAnzeigen(
                    StatusText);
            }
        }
        catch (Exception exception)
        {
            StatusText =
                $"Fehler beim Vorbereiten des Imports: {exception.Message}";

            _dialogService.FehlerAnzeigen(
                exception.Message);
        }
        finally
        {
            IstBeschaeftigt = false;
        }
    }

    private void AuswahlEntfernen()
    {
        AusgewaehlterDateipfad =
            string.Empty;

        Dateiname =
            "Keine Datei ausgewählt";

        Dateigroesse =
            string.Empty;

        Dateityp =
            string.Empty;

        IstDateiAusgewaehlt =
            false;

        StatusText =
            "Bitte wählen Sie eine B56-Excel-Datei aus.";
    }

    private bool KannDateiAuswaehlen()
    {
        return !IstBeschaeftigt;
    }

    private bool KannAuswahlEntfernen()
    {
        return !IstBeschaeftigt
            && IstDateiAusgewaehlt;
    }

    private bool KannImportStarten()
    {
        return !IstBeschaeftigt
            && ProjektId.HasValue
            && IstDateiAusgewaehlt
            && File.Exists(
                AusgewaehlterDateipfad);
    }

    private void AktualisiereBefehle()
    {
        AktualisiereCommand(
            DateiAuswaehlenCommand);

        AktualisiereCommand(
            AuswahlEntfernenCommand);

        AktualisiereCommand(
            ImportStartenCommand);
    }

    private static void AktualisiereCommand(
        ICommand command)
    {
        switch (command)
        {
            case RelayCommand relayCommand:
                relayCommand.Aktualisieren();
                break;

            case AsyncRelayCommand asyncCommand:
                asyncCommand.Aktualisieren();
                break;
        }
    }

    private static bool IstUnterstuetzteDateiendung(
        string dateiendung)
    {
        return dateiendung.Equals(
                   ".xlsx",
                   StringComparison.OrdinalIgnoreCase)
            || dateiendung.Equals(
                   ".xlsm",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatiereDateigroesse(
        long bytes)
    {
        const double kilobyte = 1024;
        const double megabyte = kilobyte * 1024;
        const double gigabyte = megabyte * 1024;

        if (bytes >= gigabyte)
        {
            return $"{bytes / gigabyte:N2} GB";
        }

        if (bytes >= megabyte)
        {
            return $"{bytes / megabyte:N2} MB";
        }

        if (bytes >= kilobyte)
        {
            return $"{bytes / kilobyte:N1} KB";
        }

        return $"{bytes} Byte";
    }

    private static string ErzeugeStatusText(
        B56ImportAntwortDto ergebnis)
    {
        return ergebnis.Status switch
        {
            B56ImportStatus.Erfolgreich =>
                $"Die B56-Datei wurde erfolgreich importiert. " +
                $"Analysiert: {ergebnis.Pipeline?.ImportierteArbeitsblaetter ?? 0} " +
                $"Arbeitsblätter, {ergebnis.Pipeline?.ErkannteTabellen ?? 0} Tabellen erkannt " +
                $"und {ergebnis.Pipeline?.ImportierteTabellen ?? 0} fachlich importiert. " +
                $"{ErzeugePipelineWarnungen(ergebnis.Pipeline)}" +
                $"Import-ID: {ergebnis.ImportId}.",

            B56ImportStatus.BereitsImportiert =>
                "Diese B56-Datei wurde für das Projekt bereits importiert.",

            _ =>
                ergebnis.Meldungen.Count > 0
                    ? string.Join(
                        Environment.NewLine,
                        ergebnis.Meldungen.Select(
                            meldung => meldung.Text))
                    : "Der B56-Import ist fehlgeschlagen."
        };
    }

    private static string ErzeugePipelineWarnungen(
        B56ImportPipelineAntwortDto? pipeline)
    {
        if (pipeline?.Warnungen.Count > 0)
        {
            return
                $"Hinweis: {string.Join(" ", pipeline.Warnungen)} ";
        }

        return string.Empty;
    }
}
