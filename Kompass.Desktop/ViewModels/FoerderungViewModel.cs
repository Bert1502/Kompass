using Kompass.Desktop.Models;
using Kompass.Desktop.Mvvm;
using Kompass.Desktop.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Kompass.Domain.Funding;

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

        AllePruefanforderungen = new ObservableCollection<FoerderanforderungDto>();

        LadenCommand =
            new AsyncRelayCommand(
                LadenAsync,
                () => _projektId != Guid.Empty);
        SpeichernCommand = new AsyncRelayCommand(SpeichernAsync, () => _projektId != Guid.Empty);
    }

    public ObservableCollection<FoerderuebersichtAlternativeDto> Alternativen { get; }

    public ObservableCollection<FoerderprogrammKatalogDto> Foerderprogramme { get; }
    public ObservableCollection<FoerderanforderungDto> AllePruefanforderungen { get; }

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
        AllePruefanforderungen.Clear();

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
                    alternative.Pruefanforderungen = ErzeugePruefanforderungen(alternative);
                    foreach (var anforderung in alternative.Pruefanforderungen.Where(
                                 kandidat => !AllePruefanforderungen.Any(vorhanden =>
                                     vorhanden.Programmkennung == kandidat.Programmkennung &&
                                     vorhanden.Bereich == kandidat.Bereich &&
                                     vorhanden.Anforderung == kandidat.Anforderung)))
                    {
                        AllePruefanforderungen.Add(anforderung);
                    }
                    Alternativen.Add(alternative);
                }
            }

            StatusText = Alternativen.Count == 0
                ? "Keine Modernisierungsalternativen im Projektmodell vorhanden. Bitte den bestätigten B56-Snapshot in das Projektmodell übernehmen."
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

    private IReadOnlyList<FoerderanforderungDto> ErzeugePruefanforderungen(
        FoerderuebersichtAlternativeDto alternative)
    {
        var ergebnis = new List<FoerderanforderungDto>();
        foreach (var kurz in alternative.ZugeordneteProgramme)
        {
            var programm = Foerderprogramme.FirstOrDefault(p => p.Id == kurz.Id);
            if (programm is null)
            {
                ergebnis.Add(new(kurz.Programmkennung, "Programmdaten",
                    "Die vollständigen Programmanforderungen konnten nicht geladen werden.",
                    "Förderprogrammkatalog", "Daten fehlen"));
                continue;
            }

            Hinzufuegen("Gültigkeit",
                $"Programmzeitraum {programm.GueltigAb:d} bis {(programm.GueltigBis?.ToString("d") ?? "unbefristet")}",
                programm.Quellenstand, "Automatisch geprüft");
            Hinzufuegen("Zielgruppe", programm.Zielgruppe,
                "Projekt- und Fördervoraussetzungen", "Kontrollieren");
            Hinzufuegen("Fördergegenstand", programm.Foerdergegenstand,
                "Modernisierungsalternative und Kostenpositionen", "Kontrollieren");
            Hinzufuegen("Technische Mindestanforderungen", programm.TechnischeMindestanforderungen,
                "Technischer Projektnachweis / Fachplanung", Nachweisstatus("Technischer Projektnachweis"));

            if (programm.Foerdergegenstand.Contains("Hülle", StringComparison.OrdinalIgnoreCase) ||
                programm.TechnischeMindestanforderungen.Contains("U-Wert", StringComparison.OrdinalIgnoreCase))
            {
                Hinzufuegen("Bauteile / U-Werte",
                    "U-Wert-Anforderungen für jedes sanierte Bauteil einschließlich Bestands- und Zielwert nachweisen.",
                    "Manuelle Bauteilliste, Fachplanung oder technischer Projektnachweis; B56 enthält diese Zielwerte nicht zuverlässig.",
                    "Manuell zu prüfen");
            }

            if (programm.Foerdergegenstand.Contains("Anlage", StringComparison.OrdinalIgnoreCase) ||
                programm.Foerdergegenstand.Contains("Heiz", StringComparison.OrdinalIgnoreCase) ||
                programm.TechnischeMindestanforderungen.Contains("Anlage", StringComparison.OrdinalIgnoreCase))
            {
                Hinzufuegen("Anlagentechnik",
                    "Programmspezifische Anforderungen an Erzeuger, Verteilung, Übergabe, Regelung und hydraulischen Abgleich nachweisen.",
                    "Fachunternehmererklärung / technischer Projektnachweis; B56 nur ergänzende Bilanzquelle.",
                    "Manuell zu prüfen");
            }

            foreach (var regel in programm.Foerderquoten ?? [])
                Hinzufuegen("Förderquote", $"{regel.Bezeichnung}: {regel.Quote:P0} auf {regel.Bezugsbasis}",
                    regel.Beschreibung ?? programm.Quellenstand, "In Berechnung berücksichtigt");
            foreach (var regel in programm.Hoechstbetraege ?? [])
                Hinzufuegen("Höchstbetrag", $"{regel.Bezeichnung}: {regel.Betrag:N2} {regel.Waehrung} {regel.Bezugsbasis}",
                    regel.Beschreibung ?? programm.Quellenstand, "In Berechnung berücksichtigt");
            if ((programm.Hoechstbetraege?.Count ?? 0) == 0 &&
                programm.Programmkennung.Contains("BEG", StringComparison.OrdinalIgnoreCase) &&
                programm.Programmkennung.Contains("EM", StringComparison.OrdinalIgnoreCase))
            {
                Hinzufuegen("Höchstbetrag",
                    "Sonstige Effizienzmaßnahmen: WG 30.000 € je Wohneinheit und Kalenderjahr, mit iSFP 60.000 €; NWG 500 € je m² NGF und Kalenderjahr. Anteilige Begrenzung bei Teilmaßnahmen beachten.",
                    "BEG-FAQ / BEG-EM-Richtlinie, Stand Juli 2026",
                    "In Berechnung berücksichtigt");
            }
            foreach (var regel in programm.Kumulierbarkeitsregeln ?? [])
                Hinzufuegen("Kumulierbarkeit", $"{regel.Bezeichnung}: {regel.Status} – {regel.Beschreibung}",
                    programm.Quellenstand, regel.Status == KumulierbarkeitStatus.Unbestimmt ? "Manuell zu prüfen" : "Kontrollieren");
            foreach (var regel in programm.Pflichtnachweisregeln ?? [])
                Hinzufuegen("Nachweis", $"{regel.Bezeichnung} ({regel.Zeitpunkt}): {regel.Beschreibung}",
                    "Antrags-/Abschlussunterlagen", Nachweisstatus(regel.Bezeichnung));
            foreach (var regel in programm.Gueltigkeitsregeln ?? [])
                Hinzufuegen("Gültigkeitsbedingung", $"{regel.Bezeichnung}: {regel.Beschreibung ?? regel.Bezug.ToString()}",
                    programm.Quellenstand, "Automatisch geprüft");

            void Hinzufuegen(string bereich, string anforderung, string quelle, string status) =>
                ergebnis.Add(new(programm.Programmkennung, bereich, anforderung, quelle, status));
        }

        return ergebnis;

        string Nachweisstatus(string bezeichnung) =>
            !string.IsNullOrWhiteSpace(Voraussetzungen.Nachweise) &&
            Voraussetzungen.Nachweise.Contains(bezeichnung, StringComparison.OrdinalIgnoreCase)
                ? "Als vorhanden angegeben"
                : "Offen / zu kontrollieren";
    }
}
