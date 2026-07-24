using Kompass.Application.B56Import;

namespace Kompass.Persistence.Services;

/// <summary>
/// Importiert sämtliche erkannte Tabellen der B56-Arbeitsmappe.
/// </summary>
public sealed class B56TabellenImportService
    : IB56TabellenImportService
{
    private readonly IB56TabellenFinder _tabellenFinder;

    public B56TabellenImportService(
        IB56TabellenFinder tabellenFinder)
    {
        ArgumentNullException.ThrowIfNull(tabellenFinder);

        _tabellenFinder = tabellenFinder;
    }

    public Task<B56TabellenImportErgebnis> ImportierenAsync(
        B56ImportKontext kontext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kontext);

        var tabellen =
            _tabellenFinder.Analysieren(
                kontext.Arbeitsmappe);

        cancellationToken.ThrowIfCancellationRequested();

        var warnungen =
            tabellen
                .Select(
                    tabelle =>
                        $"Die erkannte Tabelle '{tabelle.Titel}' im Arbeitsblatt " +
                        $"'{tabelle.Arbeitsblatt}' wurde noch nicht fachlich zugeordnet.")
                .ToList();

        return Task.FromResult(
            new B56TabellenImportErgebnis
            {
                TabellenGesamt =
                    tabellen.Count,

                ErfolgreichImportiert =
                    0,

                Warnungen =
                    warnungen
            });
    }
}
