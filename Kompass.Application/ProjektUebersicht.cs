using Kompass.Domain.Projects;

namespace Kompass.Application.Projects;

public sealed record ProjektUebersicht(
    Guid Id,
    string Name,
    int AnzahlAlternativen,
    Guid? QuellSnapshotId = null,
    int ProjektmodellVersion = 0,
    string? InterneBezeichnung = null,
    Bearbeitungsstatus Bearbeitungsstatus = Bearbeitungsstatus.InBearbeitung,
    string? Auftraggeber = null,
    string? Ansprechpartner = null,
    string? Strasse = null,
    string? Ort = null,
    string? Postleitzahl = null,
    string? Gebaeudeart = null);
