using Kompass.Desktop.Models;
using Kompass.Desktop.Mvvm;
using Kompass.Desktop.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Kompass.Desktop.ViewModels;

public sealed class FoerderungViewModel : ViewModelBase
{
    private readonly IFoerderungApiClient _apiClient;
    private readonly IDialogService _dialogService;

    private Guid _projektId;
    private string _projektname = string.Empty;
    private string _statusText = "Bitte Projekt laden.";
    private string _katalogStatusText = "Förderprogrammkatalog wird beim Laden angezeigt.";
    private FoerdervoraussetzungenDto _voraussetzungen = new();

    public FoerderungViewModel(
        IFoerderungApiClient apiClient,
        IDialogService dialogService)
    {
        _apiClient = apiClient;
        _dialogService = dialogService;

        Alternativen =
            new ObservableCollection<FoerderuebersichtAlternativeDto>();

        Foerderprogramme =
            new ObservableCollection<FoerderprogrammKatalogDto>();

        LadenCommand =
            new AsyncRelayCommand(
                LadenAsync,
                () => _projektId != Guid.Empty);
        SpeichernCommand = new AsyncRelayCommand(SpeichernAsync, () => _projektId != Guid.Empty);
    }

    public ObservableCollection<FoerderuebersichtAlternativeDto> Alternativen { get; }

    public ObservableCollection<FoerderprogrammKatalogDto> Foerderprogramme { get; }

    public string Projektname
    {
        get => _projektname;
        private set => SetProperty(ref _projektname, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string KatalogStatusText
    {
        get => _katalogStatusText;
        private set => SetProperty(ref _katalogStatusText, value);
    }

    public ICommand LadenCommand { get; }
    public ICommand SpeichernCommand { get; }

    public FoerdervoraussetzungenDto Voraussetzungen
    {
        get => _voraussetzungen;
        private set => SetProperty(ref _voraussetzungen, value);
    }

    public Array Gebaeudearten => Enum.GetValues(typeof(Kompass.Domain.Funding.FoerderGebaeudeart));
    public Array Nutzungen => Enum.GetValues(typeof(Kompass.Domain.Funding.FoerderNutzung));
    public Array Eigentuemarten => Enum.GetValues(typeof(Kompass.Domain.Funding.Antragstellerart));

    public void ProjektSetzen(
        Guid projektId,
        string projektname)
    {
        _projektId = projektId;
        Projektname = projektname;

        ((AsyncRelayCommand)LadenCommand).Aktualisieren();
        ((AsyncRelayCommand)SpeichernCommand).Aktualisieren();
    }

    public async Task LadenAsync()
    {
        if (_projektId == Guid.Empty)
        {
            return;
        }

        StatusText = "Förderdaten werden geladen…";
        KatalogStatusText = "Förderprogrammkatalog wird geladen…";

        Alternativen.Clear();
        Foerderprogramme.Clear();

        try
        {
            Voraussetzungen = await _apiClient.VoraussetzungenAbrufenAsync(_projektId)
                ?? new FoerdervoraussetzungenDto();

            var katalog =
                await _apiClient.KatalogAbrufenAsync();

            foreach (var programm in katalog)
            {
                Foerderprogramme.Add(programm);
            }

            KatalogStatusText = Foerderprogramme.Count == 0
                ? "Keine Förderprogramme im Katalog vorhanden."
                : $"{Foerderprogramme.Count} Förderprogramm(e) im Katalog.";
        }
        catch (ProjektApiException exception)
        {
            KatalogStatusText = $"Fehler beim Laden des Katalogs: {exception.Message}";
        }

        try
        {
            var uebersicht =
                await _apiClient.UebersichtAbrufenAsync(
                    _projektId);

            if (uebersicht is not null)
            {
                foreach (var alternative in uebersicht.Alternativen)
                {
                    alternative.Berechnung = await _apiClient.BerechnenAsync(_projektId, alternative.AlternativeId);
                    Alternativen.Add(alternative);
                }
            }

            StatusText = Alternativen.Count == 0
                ? "Keine Förderungen zugeordnet. Bitte zuerst Förderprogramme über die API zuordnen."
                : $"Förderübersicht für {Alternativen.Count} Alternative(n) geladen.";
        }
        catch (ProjektApiException exception)
        {
            StatusText = $"Fehler: {exception.Message}";

            _dialogService.FehlerAnzeigen(exception.Message);
        }
    }

    private async Task SpeichernAsync()
    {
        try
        {
            Voraussetzungen = await _apiClient.VoraussetzungenSpeichernAsync(_projektId, Voraussetzungen)
                ?? Voraussetzungen;
            StatusText = "Fördervoraussetzungen gespeichert und WPB-Vorschlag aktualisiert.";
        }
        catch (Exception exception)
        {
            StatusText = $"Speichern fehlgeschlagen: {exception.Message}";
            _dialogService.FehlerAnzeigen(exception.Message);
        }
    }
}
