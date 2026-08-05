using Kompass.Desktop.Models;
using Kompass.Desktop.Mvvm;
using Kompass.Desktop.Services;
using System.Collections.ObjectModel;

namespace Kompass.Desktop.ViewModels;

public sealed class ModernisierungsalternativenViewModel : ViewModelBase
{
    private readonly IKostenApiClient _apiClient;
    private Guid _projektId;
    private string _projektname = string.Empty;
    private string _statusText = "Bitte Projekt laden.";

    public ModernisierungsalternativenViewModel(IKostenApiClient apiClient)
    {
        _apiClient = apiClient;
        Alternativen = new ObservableCollection<KostenAlternativeDto>();
    }

    public ObservableCollection<KostenAlternativeDto> Alternativen { get; }

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

    public void ProjektSetzen(Guid projektId, string projektname)
    {
        _projektId = projektId;
        Projektname = projektname;
    }

    public async Task LadenAsync()
    {
        Alternativen.Clear();
        if (_projektId == Guid.Empty)
        {
            StatusText = "Kein Projekt geladen.";
            return;
        }

        StatusText = "Modernisierungsalternativen werden geladen…";
        try
        {
            foreach (var alternative in await _apiClient.AlternativenAbrufenAsync(_projektId))
            {
                Alternativen.Add(alternative);
            }

            StatusText = Alternativen.Count == 0
                ? "Noch keine Modernisierungsalternativen vorhanden. Bitte zuerst einen B56-Snapshot übernehmen."
                : $"{Alternativen.Count} Modernisierungsalternative(n) im aktuellen Projektmodell.";
        }
        catch (ProjektApiException exception)
        {
            StatusText = $"Fehler: {exception.Message}";
        }
    }
}
