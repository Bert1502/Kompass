using Kompass.Desktop.Models;
using Kompass.Desktop.Mvvm;
using Kompass.Desktop.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Kompass.Desktop.ViewModels;

public sealed class WirtschaftlichkeitViewModel : ViewModelBase
{
    private readonly IWirtschaftlichkeitApiClient _apiClient;
    private readonly IDialogService _dialogService;

    private Guid _projektId;
    private string _projektname = string.Empty;
    private string _statusText = "Bitte Projekt laden.";

    public WirtschaftlichkeitViewModel(
        IWirtschaftlichkeitApiClient apiClient,
        IDialogService dialogService)
    {
        _apiClient = apiClient;
        _dialogService = dialogService;

        BilanzierteZeilen =
            new ObservableCollection<WirtschaftlichkeitsberichtZeileDto>();

        PraktischeZeilen =
            new ObservableCollection<WirtschaftlichkeitsberichtZeileDto>();

        LadenCommand =
            new AsyncRelayCommand(
                LadenAsync,
                () => _projektId != Guid.Empty);
    }

    public ObservableCollection<WirtschaftlichkeitsberichtZeileDto> BilanzierteZeilen { get; }

    public ObservableCollection<WirtschaftlichkeitsberichtZeileDto> PraktischeZeilen { get; }

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

    public ICommand LadenCommand { get; }

    public void ProjektSetzen(
        Guid projektId,
        string projektname)
    {
        _projektId = projektId;
        Projektname = projektname;

        ((AsyncRelayCommand)LadenCommand).Aktualisieren();
    }

    public async Task LadenAsync()
    {
        if (_projektId == Guid.Empty)
        {
            return;
        }

        StatusText = "Wirtschaftlichkeitsdaten werden geladen…";

        BilanzierteZeilen.Clear();
        PraktischeZeilen.Clear();

        try
        {
            var bilanziert =
                await _apiClient.BerichtAbrufenAsync(
                    _projektId,
                    "Bilanziert");

            if (bilanziert is not null)
            {
                foreach (var zeile in bilanziert.Alternativen)
                {
                    BilanzierteZeilen.Add(zeile);
                }
            }

            var praktisch =
                await _apiClient.BerichtAbrufenAsync(
                    _projektId,
                    "Praktisch");

            if (praktisch is not null)
            {
                foreach (var zeile in praktisch.Alternativen)
                {
                    PraktischeZeilen.Add(zeile);
                }
            }

            var gesamt = BilanzierteZeilen.Count + PraktischeZeilen.Count;

            StatusText = gesamt == 0
                ? "Keine Wirtschaftlichkeitsannahmen vorhanden. Bitte zuerst Annahmen über die API erfassen."
                : $"{BilanzierteZeilen.Count} bilanzierte und {PraktischeZeilen.Count} praktische Berechnungen geladen.";
        }
        catch (ProjektApiException exception)
        {
            StatusText = $"Fehler: {exception.Message}";

            _dialogService.FehlerAnzeigen(exception.Message);
        }
    }
}
