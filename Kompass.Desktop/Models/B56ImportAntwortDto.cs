using Kompass.Application.B56Import;

namespace Kompass.Desktop.Models;

public sealed record B56ImportAntwortDto(
    B56ImportStatus Status,
    Guid ProjektId,
    Guid? ImportId,
    string? Originaldateiname,
    string? Sha256,
    long? DateigroesseBytes,
    DateTimeOffset? ImportiertAm,
    B56ImportPipelineAntwortDto? Pipeline,
    IReadOnlyList<B56ImportMeldungDto> Meldungen);

public sealed record B56ImportMeldungDto(
    B56Meldungstyp Typ,
    string Code,
    string Text);

public sealed record B56ImportPipelineAntwortDto(
    int ImportierteArbeitsblaetter,
    int ErkannteTabellen,
    int ImportierteTabellen,
    int ImportierteBauteile,
    int ImportierteKennwerte,
    int ImportierteModernisierungsalternativen,
    IReadOnlyList<B56BauteilDto> Bauteile,
    IReadOnlyList<B56KennwertDto> Bestandskennwerte,
    IReadOnlyList<B56ModernisierungsalternativeDto>
        Modernisierungsalternativen,
    IReadOnlyList<string> Warnungen);

public sealed record B56BauteilDto(
    string Bauteilcode,
    string Bezeichnung,
    string Nachbarseite,
    double Flaeche,
    double UWert,
    double? TransmissionAnteil = null,
    double? Flaechenanteil = null);

public sealed record B56KennwertDto(
    string Name,
    string Einheit,
    double Wert);

public sealed record B56ModernisierungsalternativeDto(
    string Bezeichnung,
    string Beschreibung,
    IReadOnlyList<B56BauteilDto> Bauteile,
    IReadOnlyList<B56KennwertDto> Kennwerte,
    int Position = 0);

public sealed record B56ImportHistorieDto(
    Guid ImportId,
    Guid ProjektId,
    string Originaldateiname,
    string Sha256,
    long DateigroesseBytes,
    DateTimeOffset ImportiertAm,
    string Dateiendung,
    int SnapshotSchemaVersion,
    string ParserVersion,
    B56SnapshotStatus SnapshotStatus,
    DateTimeOffset? BestaetigtAm,
    DateTimeOffset? VerworfenAm)
{
    public string ImportiertAmText =>
        ImportiertAm
            .ToLocalTime()
            .ToString("g");

    public string DateigroesseText =>
        FormatiereDateigroesse(
            DateigroesseBytes);

    public string KurzHash =>
        Sha256.Length > 12
            ? Sha256[..12]
            : Sha256;

    private static string FormatiereDateigroesse(
        long bytes)
    {
        const double kilobyte = 1024;
        const double megabyte = kilobyte * 1024;

        if (bytes >= megabyte)
        {
            return $"{bytes / megabyte:N2} MB";
        }

        if (bytes >= kilobyte)
        {
            return $"{bytes / kilobyte:N1} KB";
        }

        return $"{bytes} Byte";
    }
}

public sealed record B56SnapshotAktionAntwortDto(
    B56SnapshotAktionStatus Status,
    Guid? ImportId,
    B56SnapshotStatus? SnapshotStatus,
    DateTimeOffset? BestaetigtAm,
    DateTimeOffset? VerworfenAm,
    string Nachricht);

public sealed record B56ProjektmodellUebernahmeAntwortDto(
    B56ProjektmodellUebernahmeStatus Status,
    Guid ProjektId,
    Guid ImportId,
    int ProjektmodellVersion,
    int UebernommeneAlternativen,
    string Nachricht);
