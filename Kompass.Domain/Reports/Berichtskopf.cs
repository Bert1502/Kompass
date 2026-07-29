using Kompass.Domain.Projects;

namespace Kompass.Domain.Reports;

/// <summary>
/// Gemeinsamer Kopfteil aller KOMPASS-Berichte.
/// Enthält Projektstand, Datenquelle, Berechnungsdatum und den Berichtstyp.
/// </summary>
public sealed record Berichtskopf(
    Guid ProjektId,
    string ProjektName,
    string? InterneBezeichnung,
    Bearbeitungsstatus Bearbeitungsstatus,
    Guid? QuellSnapshotId,
    DateTimeOffset ErstelltAm,
    Berichtstyp Berichtstyp);
