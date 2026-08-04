using Kompass.Desktop.Models;
using Kompass.Desktop.Mvvm;
using Kompass.Desktop.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace Kompass.Desktop.ViewModels;

public sealed class KostenViewModel : ViewModelBase
{
    private readonly IKostenApiClient _apiClient;
    private readonly IDialogService _dialogService;

    private Guid _projektId;
    private string _projektname = string.Empty;
    private KostenAlternativeDto? _ausgewaehlteAlternative;
    private KostenpositionDto? _ausgewaehltePosition;
    private KostenartAuswahlDto? _ausgewaehlteKostenart;
    private string _bezeichnung = string.Empty;
    private string _betrag = string.Empty;
    private string _statusText = "Bitte Projekt laden.";

    public KostenViewModel(
        IKostenApiClient apiClient,
        IDialogService dialogService)
    {
        _apiClient = apiClient;
        _dialogService = dialogService;

        Alternativen = new ObservableCollection<KostenAlternativeDto>();
        Positionen = new ObservableCollection<KostenpositionDto>();
        Kostenarten = new ObservableCollection<KostenartAuswahlDto>(
            new[]
            {
                new KostenartAuswahlDto(1, "Architektur"),
                new KostenartAuswahlDto(2, "TGA"),
                new KostenartAuswahlDto(3, "Sowieso-Kosten"),
                new KostenartAuswahlDto(4, "Umfeldmaßnahme"),
                new KostenartAuswahlDto(5, "Fachplanung"),
                new KostenartAuswahlDto(6, "Eigenleistung"),
                new KostenartAuswahlDto(7, "Sonstige")
            });

        _ausgewaehlteKostenart = Kostenarten[0];

        LadenCommand = new AsyncRelayCommand(
            LadenAsync,
            () => _projektId != Guid.Empty);
        HinzufuegenCommand = new AsyncRelayCommand(
            HinzufuegenAsync,
            KannHinzufuegen);
        EntfernenCommand = new AsyncRelayCommand(
            EntfernenAsync,
            () => AusgewaehlteAlternative is not null && AusgewaehltePosition is not null);
    }

    public ObservableCollection<KostenAlternativeDto> Alternativen { get; }
    public ObservableCollection<KostenpositionDto> Positionen { get; }
    public ObservableCollection<KostenartAuswahlDto> Kostenarten { get; }

    public string Projektname
    {
        get => _projektname;
        private set => SetProperty(ref _projektname, value);
    }

    public KostenAlternativeDto? AusgewaehlteAlternative
    {
        get => _ausgewaehlteAlternative;
        set
        {
            if (!SetProperty(ref _ausgewaehlteAlternative, value))
            {
                return;
            }

            BefehleAktualisieren();
            _ = PositionenLadenAsync();
        }
    }

    public KostenpositionDto? AusgewaehltePosition
    {
        get => _ausgewaehltePosition;
        set
        {
            if (SetProperty(ref _ausgewaehltePosition, value))
            {
                BefehleAktualisieren();
            }
        }
    }

    public KostenartAuswahlDto? AusgewaehlteKostenart
    {
        get => _ausgewaehlteKostenart;
        set
        {
            if (SetProperty(ref _ausgewaehlteKostenart, value))
            {
                BefehleAktualisieren();
            }
        }
    }

    public string Bezeichnung
    {
        get => _bezeichnung;
        set
        {
            if (SetProperty(ref _bezeichnung, value))
            {
                BefehleAktualisieren();
            }
        }
    }

    public string Betrag
    {
        get => _betrag;
        set
        {
            if (SetProperty(ref _betrag, value))
            {
                BefehleAktualisieren();
            }
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public decimal Gesamtkosten => Positionen.Sum(p => p.Betrag);
    public string GesamtkostenText => $"{Gesamtkosten:N2} EUR";

    public ICommand LadenCommand { get; }
    public ICommand HinzufuegenCommand { get; }
    public ICommand EntfernenCommand { get; }

    public void ProjektSetzen(Guid projektId, string projektname)
    {
        _projektId = projektId;
        Projektname = projektname;
        BefehleAktualisieren();
    }

    public async Task LadenAsync()
    {
        if (_projektId == Guid.Empty)
        {
            return;
        }

        StatusText = "Modernisierungsalternativen werden geladen…";
        Alternativen.Clear();
        Positionen.Clear();

        try
        {
            var alternativen = await _apiClient.AlternativenAbrufenAsync(_projektId);
            foreach (var alternative in alternativen)
            {
                Alternativen.Add(alternative);
            }

            AusgewaehlteAlternative = Alternativen.FirstOrDefault();
            StatusText = Alternativen.Count == 0
                ? "Keine Modernisierungsalternative vorhanden. Bitte zuerst einen B56-Snapshot übernehmen."
                : $"{Alternativen.Count} Modernisierungsalternative(n) geladen.";
        }
        catch (ProjektApiException exception)
        {
            FehlerAnzeigen(exception.Message);
        }
    }

    private async Task PositionenLadenAsync()
    {
        Positionen.Clear();
        OnPropertyChanged(nameof(Gesamtkosten));
        OnPropertyChanged(nameof(GesamtkostenText));

        if (AusgewaehlteAlternative is null)
        {
            return;
        }

        try
        {
            var positionen = await _apiClient.PositionenAbrufenAsync(
                _projektId,
                AusgewaehlteAlternative.Id);

            foreach (var position in positionen)
            {
                Positionen.Add(position);
            }

            OnPropertyChanged(nameof(Gesamtkosten));
            OnPropertyChanged(nameof(GesamtkostenText));
            StatusText = Positionen.Count == 0
                ? "Für diese Alternative sind noch keine Kosten erfasst."
                : $"{Positionen.Count} Kostenposition(en) geladen.";
        }
        catch (ProjektApiException exception)
        {
            FehlerAnzeigen(exception.Message);
        }
    }

    private bool KannHinzufuegen() =>
        AusgewaehlteAlternative is not null &&
        AusgewaehlteKostenart is not null &&
        !string.IsNullOrWhiteSpace(Bezeichnung) &&
        BetragLesen(out _);

    private async Task HinzufuegenAsync()
    {
        if (AusgewaehlteAlternative is null ||
            AusgewaehlteKostenart is null ||
            !BetragLesen(out var betrag))
        {
            return;
        }

        try
        {
            var position = await _apiClient.HinzufuegenAsync(
                _projektId,
                AusgewaehlteAlternative.Id,
                new KostenpositionErstellenDto(
                    Bezeichnung.Trim(),
                    betrag,
                    AusgewaehlteKostenart.Wert));

            Positionen.Add(position);
            Bezeichnung = string.Empty;
            Betrag = string.Empty;
            OnPropertyChanged(nameof(Gesamtkosten));
            OnPropertyChanged(nameof(GesamtkostenText));
            StatusText = "Kostenposition wurde gespeichert.";
        }
        catch (ProjektApiException exception)
        {
            FehlerAnzeigen(exception.Message);
        }
    }

    private async Task EntfernenAsync()
    {
        if (AusgewaehlteAlternative is null || AusgewaehltePosition is null)
        {
            return;
        }

        try
        {
            var position = AusgewaehltePosition;
            var entfernt = await _apiClient.EntfernenAsync(
                _projektId,
                AusgewaehlteAlternative.Id,
                position.Id);

            if (!entfernt)
            {
                FehlerAnzeigen("Die Kostenposition wurde nicht gefunden.");
                return;
            }

            Positionen.Remove(position);
            AusgewaehltePosition = null;
            OnPropertyChanged(nameof(Gesamtkosten));
            OnPropertyChanged(nameof(GesamtkostenText));
            StatusText = "Kostenposition wurde gelöscht.";
        }
        catch (ProjektApiException exception)
        {
            FehlerAnzeigen(exception.Message);
        }
    }

    private bool BetragLesen(out decimal betrag)
    {
        var gueltig =
            decimal.TryParse(
                Betrag,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out betrag) ||
            decimal.TryParse(
                Betrag,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out betrag);

        return gueltig && betrag >= 0;
    }

    private void FehlerAnzeigen(string nachricht)
    {
        StatusText = $"Fehler: {nachricht}";
        _dialogService.FehlerAnzeigen(nachricht);
    }

    private void BefehleAktualisieren()
    {
        ((AsyncRelayCommand)LadenCommand).Aktualisieren();
        ((AsyncRelayCommand)HinzufuegenCommand).Aktualisieren();
        ((AsyncRelayCommand)EntfernenCommand).Aktualisieren();
    }
}
