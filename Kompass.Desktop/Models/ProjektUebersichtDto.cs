namespace Kompass.Desktop.Models;

public sealed record ProjektUebersichtDto(
    Guid Id,
    string Name,
    int AnzahlAlternativen,
    Guid? QuellSnapshotId = null,
    int ProjektmodellVersion = 0,
    string? InterneBezeichnung = null,
    int Bearbeitungsstatus = 0,
    string? Auftraggeber = null,
    string? Ansprechpartner = null,
    string? Strasse = null,
    string? Ort = null,
    string? Postleitzahl = null,
    string? Gebaeudeart = null);
