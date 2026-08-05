using Kompass.Desktop.Models;
using Kompass.Desktop.Mvvm;
using System.Windows.Input;

namespace Kompass.Desktop.ViewModels;

public sealed class ProjektWorkspaceViewModel : ViewModelBase
{
    private readonly B56ImportViewModel _b56ImportViewModel;
    private readonly KostenViewModel _kostenViewModel;
    private readonly ModernisierungsalternativenViewModel _modernisierungsalternativenViewModel;
    private readonly WirtschaftlichkeitViewModel _wirtschaftlichkeitViewModel;
    private readonly FoerderungViewModel _foerderungViewModel;

    private ProjektUebersichtDto? _projekt;
    private object? _aktuellerInhalt;
    private string _aktiverBereich = "Projektübersicht";
    private string _statusText = "Bereit";

    public ProjektWorkspaceViewModel(
        B56ImportViewModel b56ImportViewModel,
        KostenViewModel kostenViewModel,
        ModernisierungsalternativenViewModel modernisierungsalternativenViewModel,
        WirtschaftlichkeitViewModel wirtschaftlichkeitViewModel,
        FoerderungViewModel foerderungViewModel)
    {
        _b56ImportViewModel = b56ImportViewModel;
        _kostenViewModel = kostenViewModel;
        _modernisierungsalternativenViewModel = modernisierungsalternativenViewModel;
        _wirtschaftlichkeitViewModel = wirtschaftlichkeitViewModel;
        _foerderungViewModel = foerderungViewModel;

        ProjektuebersichtCommand =
            new RelayCommand(
                ProjektuebersichtAnzeigen);

        B56ImportCommand =
            new AsyncRelayCommand(
                B56ImportAnzeigenAsync);

        KostenCommand =
            new AsyncRelayCommand(
                KostenAnzeigenAsync);

        ModernisierungsalternativenCommand =
            new AsyncRelayCommand(ModernisierungsalternativenAnzeigenAsync);

        WirtschaftlichkeitCommand =
            new AsyncRelayCommand(
                WirtschaftlichkeitAnzeigenAsync);

        FoerderungCommand =
            new AsyncRelayCommand(
                FoerderungAnzeigenAsync);

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

            OnPropertyChanged(nameof(ProjektinformationenWorkflowStatus));
            OnPropertyChanged(nameof(B56WorkflowStatus));
            OnPropertyChanged(nameof(KostenWorkflowStatus));
            OnPropertyChanged(nameof(WirtschaftlichkeitWorkflowStatus));
            OnPropertyChanged(nameof(BerichtWorkflowStatus));
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

    public string ProjektinformationenWorkflowStatus =>
        Projekt is not null &&
        !string.IsNullOrWhiteSpace(Projekt.Auftraggeber) &&
        !string.IsNullOrWhiteSpace(Projekt.Ort)
            ? "Erledigt"
            : "Offen";

    public string B56WorkflowStatus =>
        Projekt?.QuellSnapshotId is not null ? "Erledigt" : "Offen";

    public string KostenWorkflowStatus =>
        Projekt?.AnzahlAlternativen > 0 ? "Bereit" : "Gesperrt";

    public string WirtschaftlichkeitWorkflowStatus =>
        Projekt?.AnzahlAlternativen > 0 ? "Bereit" : "Gesperrt";

    public string BerichtWorkflowStatus =>
        Projekt?.QuellSnapshotId is not null ? "Bereit" : "Gesperrt";

    public string AktiverBereich
    {
        get => _aktiverBereich;

        private set
        {
            if (!SetProperty(ref _aktiverBereich, value))
            {
                return;
            }

            OnPropertyChanged(nameof(IstProjektuebersichtAktiv));
            OnPropertyChanged(nameof(IstB56ImportAktiv));
            OnPropertyChanged(nameof(IstKostenAktiv));
            OnPropertyChanged(nameof(IstModernisierungsalternativenAktiv));
            OnPropertyChanged(nameof(IstWirtschaftlichkeitAktiv));
            OnPropertyChanged(nameof(IstFoerderungAktiv));
            OnPropertyChanged(nameof(IstBerichtAktiv));
        }
    }

    public bool IstProjektuebersichtAktiv => AktiverBereich == "Projektübersicht";
    public bool IstB56ImportAktiv => AktiverBereich == "B56-Import";
    public bool IstKostenAktiv => AktiverBereich == "Kosten";
    public bool IstModernisierungsalternativenAktiv => AktiverBereich == "Modernisierungsalternativen";
    public bool IstWirtschaftlichkeitAktiv => AktiverBereich == "Wirtschaftlichkeit";
    public bool IstFoerderungAktiv => AktiverBereich == "Förderung";
    public bool IstBerichtAktiv => AktiverBereich == "Bericht";

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

        _kostenViewModel.ProjektSetzen(
            projekt.Id,
            projekt.Name);

        _modernisierungsalternativenViewModel.ProjektSetzen(projekt.Id, projekt.Name);

        _wirtschaftlichkeitViewModel.ProjektSetzen(
            projekt.Id,
            projekt.Name);

        _foerderungViewModel.ProjektSetzen(
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

    private async Task WirtschaftlichkeitAnzeigenAsync()
    {
        AktiverBereich =
            "Wirtschaftlichkeit";

        AktuellerInhalt =
            _wirtschaftlichkeitViewModel;

        StatusText =
            "Wirtschaftlichkeit wurde ausgewählt.";

        await _wirtschaftlichkeitViewModel
            .LadenAsync();
    }

    private async Task KostenAnzeigenAsync()
    {
        AktiverBereich = "Kosten";
        AktuellerInhalt = _kostenViewModel;
        StatusText = "Kosten wurden ausgewählt.";

        await _kostenViewModel.LadenAsync();
    }

    private async Task ModernisierungsalternativenAnzeigenAsync()
    {
        AktiverBereich = "Modernisierungsalternativen";
        AktuellerInhalt = _modernisierungsalternativenViewModel;
        StatusText = "Modernisierungsalternativen wurden ausgewählt.";
        await _modernisierungsalternativenViewModel.LadenAsync();
    }

    private async Task FoerderungAnzeigenAsync()
    {
        AktiverBereich =
            "Förderung";

        AktuellerInhalt =
            _foerderungViewModel;

        StatusText =
            "Förderung wurde ausgewählt.";

        await _foerderungViewModel
            .LadenAsync();
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
