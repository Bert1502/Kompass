using Kompass.Desktop.Models;
using Kompass.Desktop.Mvvm;
using System.Windows.Input;

namespace Kompass.Desktop.ViewModels;

public sealed class ProjektWorkspaceViewModel : ViewModelBase
{
    private readonly B56ImportViewModel _b56ImportViewModel;

    private ProjektUebersichtDto? _projekt;
    private object? _aktuellerInhalt;
    private string _aktiverBereich = "Projektübersicht";
    private string _statusText = "Bereit";

    public ProjektWorkspaceViewModel(
        B56ImportViewModel b56ImportViewModel)
    {
        _b56ImportViewModel = b56ImportViewModel;

        ProjektuebersichtCommand =
            new RelayCommand(
                ProjektuebersichtAnzeigen);

        B56ImportCommand =
            new AsyncRelayCommand(
                B56ImportAnzeigenAsync);

        KostenCommand =
            new RelayCommand(
                () => BereichOhneInhaltAnzeigen(
                    "Kosten"));

        ModernisierungsalternativenCommand =
            new RelayCommand(
                () => BereichOhneInhaltAnzeigen(
                    "Modernisierungsalternativen"));

        WirtschaftlichkeitCommand =
            new RelayCommand(
                () => BereichOhneInhaltAnzeigen(
                    "Wirtschaftlichkeit"));

        FoerderungCommand =
            new RelayCommand(
                () => BereichOhneInhaltAnzeigen(
                    "Förderung"));

        BerichtCommand =
            new RelayCommand(
                () => BereichOhneInhaltAnzeigen(
                    "Bericht"));
    }

    public ProjektUebersichtDto? Projekt
    {
        get => _projekt;

        private set
        {
            if (!SetProperty(
                    ref _projekt,
                    value))
            {
                return;
            }

            OnPropertyChanged(
                nameof(Fenstertitel));

            OnPropertyChanged(
                nameof(Projektname));

            OnPropertyChanged(
                nameof(ProjektId));

            OnPropertyChanged(
                nameof(AnzahlAlternativen));

            OnPropertyChanged(
                nameof(ProjektmodellStatus));

            OnPropertyChanged(
                nameof(QuellSnapshotId));
        }
    }

    public object? AktuellerInhalt
    {
        get => _aktuellerInhalt;

        private set =>
            SetProperty(
                ref _aktuellerInhalt,
                value);
    }

    public string Fenstertitel =>
        Projekt is null
            ? "KOMPASS – Projekt"
            : $"KOMPASS – {Projekt.Name}";

    public string Projektname =>
        Projekt?.Name
        ?? "Kein Projekt geladen";

    public string ProjektId =>
        Projekt?.Id.ToString()
        ?? string.Empty;

    public int AnzahlAlternativen =>
        Projekt?.AnzahlAlternativen
        ?? 0;

    public string ProjektmodellStatus =>
        Projekt?.ProjektmodellVersion > 0
            ? $"Version {Projekt.ProjektmodellVersion}"
            : "Noch nicht erzeugt";

    public string QuellSnapshotId =>
        Projekt?.QuellSnapshotId?.ToString()
        ?? "Kein B56-Snapshot übernommen";

    public string AktiverBereich
    {
        get => _aktiverBereich;

        private set =>
            SetProperty(
                ref _aktiverBereich,
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

    public ICommand ProjektuebersichtCommand { get; }

    public ICommand B56ImportCommand { get; }

    public ICommand KostenCommand { get; }

    public ICommand ModernisierungsalternativenCommand { get; }

    public ICommand WirtschaftlichkeitCommand { get; }

    public ICommand FoerderungCommand { get; }

    public ICommand BerichtCommand { get; }

    public void ProjektLaden(
        ProjektUebersichtDto projekt)
    {
        ArgumentNullException.ThrowIfNull(
            projekt);

        Projekt = projekt;

        _b56ImportViewModel.ProjektSetzen(
            projekt.Id,
            projekt.Name);

        ProjektuebersichtAnzeigen();

        StatusText =
            $"Projekt '{projekt.Name}' wurde geöffnet.";
    }

    private void ProjektuebersichtAnzeigen()
    {
        AktiverBereich =
            "Projektübersicht";

        AktuellerInhalt =
            null;

        StatusText =
            "Projektübersicht wurde ausgewählt.";
    }

    private async Task B56ImportAnzeigenAsync()
    {
        AktiverBereich =
            "B56-Import";

        AktuellerInhalt =
            _b56ImportViewModel;

        StatusText =
            "B56-Import wurde ausgewählt.";

        await _b56ImportViewModel
            .HistorieLadenAsync();
    }

    private void BereichOhneInhaltAnzeigen(
        string bereich)
    {
        AktiverBereich =
            bereich;

        AktuellerInhalt =
            null;

        StatusText =
            $"Bereich '{bereich}' wurde ausgewählt.";
    }
}
