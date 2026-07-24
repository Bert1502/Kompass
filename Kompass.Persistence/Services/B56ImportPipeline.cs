using Kompass.Application.B56Import;

namespace Kompass.Persistence.Services;

/// <summary>
/// Führt den fachlichen Import der B56-Arbeitsmappe aus.
/// </summary>
public sealed class B56ImportPipeline : IB56ImportPipeline
{
    private readonly IB56TabellenImportService _tabellenImportService;

    public B56ImportPipeline(
        IB56TabellenImportService tabellenImportService)
    {
        _tabellenImportService = tabellenImportService;
    }

    public async Task<B56ImportPipelineErgebnis> ImportierenAsync(
        B56ImportKontext kontext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kontext);

        var tabellenErgebnis =
            await _tabellenImportService.ImportierenAsync(
                kontext,
                cancellationToken);

        return new B56ImportPipelineErgebnis
        {
            ImportierteArbeitsblaetter =
                kontext.Arbeitsmappe.Arbeitsblaetter.Count,

            ErkannteTabellen =
                tabellenErgebnis.TabellenGesamt,

            ImportierteTabellen =
                tabellenErgebnis.ErfolgreichImportiert,

            ImportierteBauteile =
                tabellenErgebnis.Bauteile.Count,

            ImportierteKennwerte =
                tabellenErgebnis.Bestandskennwerte.Count +
                tabellenErgebnis.Modernisierungsalternativen.Sum(
                    alternative => alternative.Kennwerte.Count),

            ImportierteModernisierungsalternativen =
                tabellenErgebnis.Modernisierungsalternativen.Count,

            Bauteile =
                tabellenErgebnis.Bauteile,

            Bestandskennwerte =
                tabellenErgebnis.Bestandskennwerte,

            Modernisierungsalternativen =
                tabellenErgebnis.Modernisierungsalternativen,

            Warnungen =
                tabellenErgebnis.Warnungen
        };
    }
}
