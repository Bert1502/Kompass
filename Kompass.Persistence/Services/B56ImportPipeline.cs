using Kompass.Application.B56Import;

namespace Kompass.Persistence.Services;

/// <summary>
/// Führt den fachlichen Import der B56-Arbeitsmappe aus.
/// </summary>
public sealed class B56ImportPipeline : IB56ImportPipeline
{
    private readonly IB56TabellenFinder _tabellenFinder;

    public B56ImportPipeline(
        IB56TabellenFinder tabellenFinder)
    {
        _tabellenFinder = tabellenFinder;
    }

    public Task<B56ImportPipelineErgebnis> ImportierenAsync(
        B56ImportKontext kontext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kontext);

        var tabellen =
            _tabellenFinder.Finde(
                kontext.Arbeitsmappe);

        B56ImportPipelineErgebnis ergebnis =
            new()
            {
                ImportierteArbeitsblaetter =
                    kontext.Arbeitsmappe.Arbeitsblaetter.Count,

                ImportierteTabellen =
                    tabellen.Count,

                ImportierteBauteile = 0,

                ImportierteKennwerte = 0,

                ImportierteModernisierungsalternativen = 0
            };

        return Task.FromResult(ergebnis);
    }
}
