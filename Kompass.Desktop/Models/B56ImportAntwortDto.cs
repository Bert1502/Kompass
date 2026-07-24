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
    int ImportierteTabellen,
    int ImportierteBauteile,
    int ImportierteKennwerte,
    int ImportierteModernisierungsalternativen,
    IReadOnlyList<string> Warnungen);
