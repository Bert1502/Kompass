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
    IReadOnlyList<string> Warnungen);

public sealed record B56ImportHistorieDto(
    Guid ImportId,
    Guid ProjektId,
    string Originaldateiname,
    string Sha256,
    long DateigroesseBytes,
    DateTimeOffset ImportiertAm,
    string Dateiendung)
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
