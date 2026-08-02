using Kompass.Desktop.Models;
using Kompass.Desktop.Mvvm;
using Kompass.Desktop.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Kompass.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly IProjektApiClient _projektApiClient;
    private readonly IDialogService _dialogService;
    private readonly IProjektNavigationService _projektNavigationService;
    private readonly IFachdatenApiClient _fachdatenApiClient;

    private ProjektUebersichtDto? _ausgewaehltesProjekt;
    private string _neuerProjektname = string.Empty;
    private string _bearbeiteterProjektname = string.Empty;
    private string _statusText = "Bereit";
    private bool _istBeschaeftigt;
    private string _fachdatenStatusText = "Fachdaten wurden noch nicht geprüft.";

    public MainWindowViewModel(
        IProjektApiClient projektApiClient,
        IDialogService dialogService,
        IProjektNavigationService projektNavigationService,
        IFachdatenApiClient fachdatenApiClient)
    {
        _projektApiClient = projektApiClient;
        _dialogService = dialogService;
        _projektNavigationService = projektNavigationService;
        _fachdatenApiClient = fachdatenApiClient;

        Projekte =
            new ObservableCollection<ProjektUebersichtDto>();

        ProjekteLadenCommand =
            new AsyncRelayCommand(
                ProjekteLadenAsync,
                () => !IstBeschaeftigt);

        ProjektErstellenCommand =
            new AsyncRelayCommand(
                ProjektErstellenAsync,
                KannProjektErstellen);

        ProjektAktualisierenCommand =
            new AsyncRelayCommand(
                ProjektAktualisierenAsync,
                KannProjektAktualisieren);

        ProjektOeffnenCommand =
            new RelayCommand(
                ProjektOeffnen,
                KannProjektOeffnen);

        ProjektLoeschenCommand =
            new AsyncRelayCommand(
                ProjektLoeschenAsync,
                KannProjektLoeschen);

        FachdatenPruefenCommand = new AsyncRelayCommand(FachdatenPruefenAsync, () => !IstBeschaeftigt);
        FachdatenImportierenCommand = new AsyncRelayCommand(FachdatenImportierenAsync, () => !IstBeschaeftigt);
    }

    public ObservableCollection<ProjektUebersichtDto>
        Projekte { get; }

    public ProjektUebersichtDto?
        AusgewaehltesProjekt
    {
        get => _ausgewaehltesProjekt;

        set
        {
            if (!SetProperty(
                    ref _ausgewaehltesProjekt,
                    value))
            {
                return;
            }

            BearbeiteterProjektname =
                value?.Name ?? string.Empty;

            AktualisiereBefehle();
        }
    }

    public string NeuerProjektname
    {
        get => _neuerProjektname;

        set
        {
            if (SetProperty(
                    ref _neuerProjektname,
                    value))
            {
                AktualisiereBefehle();
            }
        }
    }

    public string BearbeiteterProjektname
    {
        get => _bearbeiteterProjektname;

        set
        {
            if (SetProperty(
                    ref _bearbeiteterProjektname,
                    value))
            {
                AktualisiereBefehle();
            }
        }
    }

    public string StatusText
    {
        get => _statusText;

        private set =>
            SetProperty(
                ref _statusText,
                value);
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

    public string FachdatenStatusText
    {
        get => _fachdatenStatusText;
        private set => SetProperty(ref _fachdatenStatusText, value);
    }

    public ICommand ProjekteLadenCommand { get; }

    public ICommand ProjektErstellenCommand { get; }

    public ICommand ProjektAktualisierenCommand { get; }

    public ICommand ProjektOeffnenCommand { get; }

    public ICommand ProjektLoeschenCommand { get; }
    public ICommand FachdatenPruefenCommand { get; }
    public ICommand FachdatenImportierenCommand { get; }

    public async Task InitialisierenAsync()
    {
        await ProjekteLadenAsync();
    }

    private async Task ProjekteLadenAsync()
    {
        try
        {
            IstBeschaeftigt = true;
            StatusText = "Projekte werden geladen …";

            var projekte =
                await _projektApiClient
                    .AlleAbrufenAsync();

            Projekte.Clear();

            foreach (var projekt in projekte)
            {
                Projekte.Add(projekt);
            }

            AusgewaehltesProjekt = null;

            StatusText =
                $"{Projekte.Count} Projekt(e) geladen.";
        }
        catch (Exception exception)
        {
            FehlerBehandeln(exception);
        }
        finally
        {
            IstBeschaeftigt = false;
        }
    }

    private async Task ProjektErstellenAsync()
    {
        try
        {
            IstBeschaeftigt = true;
            StatusText = "Projekt wird angelegt …";

            var projekt =
                await _projektApiClient
                    .ErstellenAsync(
                        NeuerProjektname);

            Projekte.Add(projekt);

            AusgewaehltesProjekt = projekt;
            NeuerProjektname = string.Empty;

            StatusText =
                $"Projekt '{projekt.Name}' wurde angelegt.";
        }
        catch (Exception exception)
        {
            FehlerBehandeln(exception);
        }
        finally
        {
            IstBeschaeftigt = false;
        }
    }

    private async Task ProjektAktualisierenAsync()
    {
        var bisherigesProjekt =
            AusgewaehltesProjekt;

        if (bisherigesProjekt is null)
        {
            return;
        }

        try
        {
            IstBeschaeftigt = true;
            StatusText = "Projekt wird aktualisiert …";

            var aktualisiertesProjekt =
                await _projektApiClient
                    .AktualisierenAsync(
                        bisherigesProjekt.Id,
                        BearbeiteterProjektname);

            if (aktualisiertesProjekt is null)
            {
                StatusText =
                    "Das Projekt wurde nicht gefunden.";

                return;
            }

            var index =
                Projekte.IndexOf(
                    bisherigesProjekt);

            if (index >= 0)
            {
                Projekte[index] =
                    aktualisiertesProjekt;
            }

            AusgewaehltesProjekt =
                aktualisiertesProjekt;

            StatusText =
                $"Projekt wurde in '{aktualisiertesProjekt.Name}' umbenannt.";
        }
        catch (Exception exception)
        {
            FehlerBehandeln(exception);
        }
        finally
        {
            IstBeschaeftigt = false;
        }
    }

    private void ProjektOeffnen()
    {
        var projekt =
            AusgewaehltesProjekt;

        if (projekt is null)
        {
            return;
        }

        _projektNavigationService
            .ProjektOeffnen(projekt);

        StatusText =
            $"Projekt '{projekt.Name}' wurde geöffnet.";
    }

    private async Task ProjektLoeschenAsync()
    {
        var projekt =
            AusgewaehltesProjekt;

        if (projekt is null)
        {
            return;
        }

        var bestaetigt =
            _dialogService.LoeschenBestaetigen(
                projekt.Name);

        if (!bestaetigt)
        {
            StatusText =
                "Löschen wurde abgebrochen.";

            return;
        }

        try
        {
            IstBeschaeftigt = true;

            StatusText =
                $"Projekt '{projekt.Name}' wird gelöscht …";

            var wurdeGeloescht =
                await _projektApiClient
                    .LoeschenAsync(
                        projekt.Id);

            if (!wurdeGeloescht)
            {
                StatusText =
                    "Das Projekt wurde nicht gefunden.";

                return;
            }

            Projekte.Remove(projekt);

            AusgewaehltesProjekt = null;

            StatusText =
                $"Projekt '{projekt.Name}' wurde gelöscht.";
        }
        catch (Exception exception)
        {
            FehlerBehandeln(exception);
        }
        finally
        {
            IstBeschaeftigt = false;
        }
    }

    private bool KannProjektErstellen()
    {
        return !IstBeschaeftigt
            && !string.IsNullOrWhiteSpace(
                NeuerProjektname);
    }

    private async Task FachdatenPruefenAsync()
    {
        try
        {
            IstBeschaeftigt = true;
            FachdatenStatusText = "Sechs Fachdatenbanken werden geprüft …";
            var ergebnis = await _fachdatenApiClient.PruefenAsync();
            var warnungen = ergebnis.Datenbanken.Sum(x => x.Warnungen.Count);
            FachdatenStatusText = ergebnis.IstGueltig
                ? $"{ergebnis.Datenbanken.Count}/6 Datenbanken gültig, {warnungen} Warnung(en)."
                : $"Prüfung fehlgeschlagen: {string.Join("; ", ergebnis.Datenbanken.SelectMany(x => x.Fehler))}";
        }
        catch (Exception exception)
        {
            FachdatenStatusText = $"Fehler: {exception.Message}";
            _dialogService.FehlerAnzeigen(exception.Message);
        }
        finally { IstBeschaeftigt = false; }
    }

    private async Task FachdatenImportierenAsync()
    {
        try
        {
            IstBeschaeftigt = true;
            FachdatenStatusText = "Fachdaten werden geprüft und importiert …";
            var ergebnis = await _fachdatenApiClient.ImportierenAsync();
            FachdatenStatusText = ergebnis.AngelegteDatensaetze == 0
                ? "Import abgeschlossen; der Datenstand war bereits aktuell."
                : $"Import abgeschlossen: {ergebnis.AngelegteDatensaetze} Datensätze neu angelegt.";
        }
        catch (Exception exception)
        {
            FachdatenStatusText = $"Fehler: {exception.Message}";
            _dialogService.FehlerAnzeigen(exception.Message);
        }
        finally { IstBeschaeftigt = false; }
    }

    private bool KannProjektAktualisieren()
    {
        return !IstBeschaeftigt
            && AusgewaehltesProjekt is not null
            && !string.IsNullOrWhiteSpace(
                BearbeiteterProjektname)
            && !string.Equals(
                BearbeiteterProjektname.Trim(),
                AusgewaehltesProjekt.Name,
                StringComparison.Ordinal);
    }

    private bool KannProjektOeffnen()
    {
        return !IstBeschaeftigt
            && AusgewaehltesProjekt is not null;
    }

    private bool KannProjektLoeschen()
    {
        return !IstBeschaeftigt
            && AusgewaehltesProjekt is not null;
    }

    private void FehlerBehandeln(
        Exception exception)
    {
        StatusText =
            $"Fehler: {exception.Message}";

        _dialogService.FehlerAnzeigen(
            exception.Message);
    }

    private void AktualisiereBefehle()
    {
        AktualisiereCommand(
            ProjekteLadenCommand);

        AktualisiereCommand(
            ProjektErstellenCommand);

        AktualisiereCommand(
            ProjektAktualisierenCommand);

        AktualisiereCommand(
            ProjektOeffnenCommand);

        AktualisiereCommand(
            ProjektLoeschenCommand);

        AktualisiereCommand(FachdatenPruefenCommand);
        AktualisiereCommand(FachdatenImportierenCommand);
    }

    private static void AktualisiereCommand(
        ICommand command)
    {
        switch (command)
        {
            case AsyncRelayCommand asyncCommand:
                asyncCommand.Aktualisieren();
                break;

            case RelayCommand relayCommand:
                relayCommand.Aktualisieren();
                break;
        }
    }
}
