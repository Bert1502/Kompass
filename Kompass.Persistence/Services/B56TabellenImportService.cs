using System.Globalization;
using System.Text.RegularExpressions;
using Kompass.Application.B56Import;

namespace Kompass.Persistence.Services;

/// <summary>
/// Importiert die fachlich zugeordneten Bereiche einer
/// B56-Arbeitsmappe.
/// </summary>
public sealed partial class B56TabellenImportService
    : IB56TabellenImportService
{
    private const string Modernisierungsblatt =
        "SCModernisierungen";
    private const string EnergieberichtBlatt =
        "SCEnergiebericht";

    private static readonly IReadOnlySet<string> IgnorierteArbeitsblaetter =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SCNeubau",
            "SCEnergiebilanzNeubau",
            "SCNeubauberatungsbericht",
            "SCEnergiebilanz",
            "SCZonendaten",
            "SCModernisierungen"
        };

    private static readonly IReadOnlyList<string> Kennwertnamen =
    [
        "Primärenergiebedarf Gebäude",
        "Endenergiebedarf Gebäude",
        "CO2-Emissionen Gebäude"
    ];

    private static readonly IReadOnlyList<string>
        Bestandskennwertnamen =
        [
            "Primärenergiebedarf Gebäude",
            "Endenergiebedarf Gebäude",
            "CO2-Emissionen Gebäude"
        ];

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
        cancellationToken.ThrowIfCancellationRequested();

        var tabellen =
            _tabellenFinder.Analysieren(
                kontext.Arbeitsmappe);

        var modernisierungsblatt =
            kontext.Arbeitsmappe.ArbeitsblattSuchen(
                Modernisierungsblatt);

        if (modernisierungsblatt is null)
        {
            return Task.FromResult(
                new B56TabellenImportErgebnis
                {
                    TabellenGesamt =
                        tabellen.Count,
                    Warnungen =
                    [
                        $"Das erforderliche Arbeitsblatt " +
                        $"'{Modernisierungsblatt}' wurde nicht gefunden."
                    ]
                });
        }

        var bauteile =
            BauteileImportieren(
                modernisierungsblatt);

        var bestandskennwerte =
            BestandskennwerteImportieren(
                modernisierungsblatt)
                .ToList();

        var modernisierungsalternativen =
            ModernisierungsalternativenImportieren(
                modernisierungsblatt,
                cancellationToken)
                .ToList();

        var energieberichtblatt =
            kontext.Arbeitsmappe.ArbeitsblattSuchen(
                EnergieberichtBlatt);

        var importierteBerichtstabellen =
            BerichtskennwerteImportieren(
                energieberichtblatt,
                bestandskennwerte,
                modernisierungsalternativen);

        var warnungen =
            tabellen
                .Where(
                    tabelle =>
                        !IgnorierteArbeitsblaetter.Contains(
                            tabelle.Arbeitsblatt) &&
                        !IstZugeordneteBauteiltabelle(
                            tabelle) &&
                        !IstZugeordneteBerichtstabelle(
                            tabelle))
                .Select(
                    tabelle =>
                        $"Die erkannte Tabelle '{tabelle.Titel}' im Arbeitsblatt " +
                        $"'{tabelle.Arbeitsblatt}' wurde noch nicht fachlich zugeordnet.")
                .ToList();

        var importierteBereiche =
            (bauteile.Count > 0 ? 1 : 0) +
            (bestandskennwerte.Count > 0 ? 1 : 0) +
            (modernisierungsalternativen.Count > 0 ? 1 : 0) +
            importierteBerichtstabellen;

        return Task.FromResult(
            new B56TabellenImportErgebnis
            {
                TabellenGesamt =
                    tabellen.Count +
                    (bestandskennwerte.Count > 0 ? 1 : 0) +
                    (modernisierungsalternativen.Count > 0 ? 1 : 0),

                ErfolgreichImportiert =
                    importierteBereiche,

                Bauteile =
                    bauteile,

                Bestandskennwerte =
                    bestandskennwerte,

                Modernisierungsalternativen =
                    modernisierungsalternativen,

                Warnungen =
                    warnungen
            });
    }

    private static IReadOnlyList<B56Bauteil>
        BauteileImportieren(
            B56Arbeitsblatt arbeitsblatt)
    {
        var kopfzeile =
            arbeitsblatt.Zeilen.FirstOrDefault(
                zeile =>
                    Wert(zeile, "B") ==
                        "Bauteilcode" &&
                    Wert(zeile, "C") ==
                        "Bauteil" &&
                    Wert(zeile, "E") ==
                        "U-Wert");

        if (kopfzeile is null)
        {
            return [];
        }

        return arbeitsblatt.Zeilen
            .Where(
                zeile =>
                    zeile.Zeilennummer >
                    kopfzeile.Zeilennummer)
            .SkipWhile(
                zeile =>
                    string.IsNullOrWhiteSpace(
                        Wert(zeile, "B")))
            .TakeWhile(
                zeile =>
                    !string.IsNullOrWhiteSpace(
                        Wert(zeile, "B")))
            .Select(
                zeile =>
                {
                    var uWert =
                        Zahl(
                            Wert(zeile, "E"));

                    return uWert.HasValue
                        ? new B56Bauteil
                        {
                            Bauteilcode =
                                Wert(zeile, "B"),
                            Bezeichnung =
                                Wert(zeile, "C"),
                            Nachbarseite =
                                Wert(zeile, "D"),
                            UWert =
                                uWert.Value
                        }
                        : null;
                })
            .Where(
                bauteil =>
                    bauteil is not null)
            .Cast<B56Bauteil>()
            .ToList();
    }

    private static IReadOnlyList<B56Kennwert>
        BestandskennwerteImportieren(
            B56Arbeitsblatt arbeitsblatt)
    {
        var bestand =
            arbeitsblatt.Zeilen.FirstOrDefault(
                zeile =>
                    Wert(zeile, "A") ==
                    "Bestand");

        if (bestand is null)
        {
            return [];
        }

        var nachfolgendeZeilen =
            arbeitsblatt.Zeilen
                .Where(
                    zeile =>
                        zeile.Zeilennummer >
                        bestand.Zeilennummer)
                .Take(12)
                .ToList();

        return KennwerteImportieren(
            nachfolgendeZeilen,
            Bestandskennwertnamen);
    }

    private static IReadOnlyList<B56Modernisierungsalternative>
        ModernisierungsalternativenImportieren(
            B56Arbeitsblatt arbeitsblatt,
            CancellationToken cancellationToken)
    {
        var startzeilen =
            arbeitsblatt.Zeilen
                .Where(
                    zeile =>
                        IstModernisierungsstart(
                            Wert(zeile, "A")))
                .ToList();

        var ergebnis =
            new List<B56Modernisierungsalternative>();

        for (var index = 0;
             index < startzeilen.Count;
             index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var startzeile =
                startzeilen[index];

            var naechsteStartzeile =
                index + 1 < startzeilen.Count
                    ? startzeilen[index + 1].Zeilennummer
                    : int.MaxValue;

            var bestand =
                arbeitsblatt.Zeilen.FirstOrDefault(
                    zeile =>
                        zeile.Zeilennummer >
                            startzeile.Zeilennummer &&
                        Wert(zeile, "A") ==
                            "Bestand");

            var ende =
                Math.Min(
                    naechsteStartzeile,
                    bestand?.Zeilennummer ??
                        int.MaxValue);

            var abschnitt =
                arbeitsblatt.Zeilen
                    .Where(
                        zeile =>
                            zeile.Zeilennummer >
                                startzeile.Zeilennummer &&
                            zeile.Zeilennummer <
                                ende)
                    .ToList();

            var bezeichnung =
                Feldwert(
                    abschnitt,
                    "Bezeichnung");

            if (string.IsNullOrWhiteSpace(
                    bezeichnung))
            {
                continue;
            }

            var alternative =
                new B56Modernisierungsalternative
                {
                    Position =
                        index + 1,
                    Bezeichnung =
                        bezeichnung,
                    Beschreibung =
                        Feldwert(
                            abschnitt,
                            "Beschreibung")
                };

            foreach (var kennwert in
                     KennwerteImportieren(
                         abschnitt,
                         Kennwertnamen))
            {
                alternative.Kennwerte.Add(
                    kennwert);
            }

            ergebnis.Add(
                alternative);
        }

        return ergebnis;
    }

    private static IReadOnlyList<B56Kennwert>
        KennwerteImportieren(
            IReadOnlyList<B56Zeile> zeilen,
            IReadOnlyList<string> namen)
    {
        var ergebnis =
            new List<B56Kennwert>();

        foreach (var name in namen)
        {
            var wert =
                Zahl(
                    Feldwert(
                        zeilen,
                        name));

            if (!wert.HasValue)
            {
                continue;
            }

            ergebnis.Add(
                new B56Kennwert
                {
                    Name = name,
                    Wert = wert.Value
                });
        }

        return ergebnis;
    }

    private static int BerichtskennwerteImportieren(
        B56Arbeitsblatt? arbeitsblatt,
        IList<B56Kennwert> bestandskennwerte,
        IReadOnlyList<B56Modernisierungsalternative>
            modernisierungsalternativen)
    {
        if (arbeitsblatt is null)
        {
            return 0;
        }

        var konfigurationen =
            new[]
            {
                new BerichtskennwertKonfiguration(
                    "Reduktion des Endenergiebedarfs",
                    "Endenergiebedarf Bericht",
                    "Endenergieeinsparung gegenüber Bedarf",
                    "[kWh/a]"),
                new BerichtskennwertKonfiguration(
                    "Reduktion des Primäreneribedarfs",
                    "Primärenergiebedarf Bericht",
                    "Primärenergieeinsparung gegenüber Bedarf",
                    "[kWh/a]"),
                new BerichtskennwertKonfiguration(
                    "Reduktion CO2-Emission",
                    "CO2-Emission Bericht",
                    "CO2-Einsparung gegenüber Bedarf",
                    "[kg]")
            };

        var importierteAbschnitte = 0;

        foreach (var konfiguration in konfigurationen)
        {
            if (BerichtskennwerteImportieren(
                    arbeitsblatt,
                    konfiguration,
                    bestandskennwerte,
                    modernisierungsalternativen))
            {
                importierteAbschnitte++;
            }
        }

        return importierteAbschnitte;
    }

    private static bool BerichtskennwerteImportieren(
        B56Arbeitsblatt arbeitsblatt,
        BerichtskennwertKonfiguration konfiguration,
        IList<B56Kennwert> bestandskennwerte,
        IReadOnlyList<B56Modernisierungsalternative>
            modernisierungsalternativen)
    {
        var startzeile =
            arbeitsblatt.Zeilen.FirstOrDefault(
                zeile =>
                    Wert(zeile, "A") ==
                    konfiguration.Abschnittstitel);

        if (startzeile is null)
        {
            return false;
        }

        var abschnitt =
            arbeitsblatt.Zeilen
                .Where(
                    zeile =>
                        zeile.Zeilennummer >
                        startzeile.Zeilennummer)
                .TakeWhile(
                    zeile =>
                        string.IsNullOrWhiteSpace(
                            Wert(zeile, "A")))
                .ToList();

        var importiert = false;

        var bestandswert =
            abschnitt
                .Where(
                    zeile =>
                        string.Equals(
                            Wert(zeile, "T"),
                            "Bestand",
                            StringComparison.OrdinalIgnoreCase))
                .Select(
                    zeile =>
                        Zahl(
                            Wert(zeile, "U")))
                .FirstOrDefault(
                    wert => wert.HasValue);

        if (bestandswert.HasValue)
        {
            bestandskennwerte.Add(
                new B56Kennwert
                {
                    Name =
                        konfiguration.Bedarfsname,
                    Einheit =
                        konfiguration.Einheit,
                    Wert =
                        bestandswert.Value
                });

            importiert = true;
        }

        foreach (var zeile in abschnitt)
        {
            var position =
                AlternativePositionErmitteln(
                    Wert(zeile, "B"));

            if (!position.HasValue)
            {
                continue;
            }

            var alternative =
                modernisierungsalternativen
                    .SingleOrDefault(
                        kandidat =>
                            kandidat.Position ==
                            position.Value);

            if (alternative is null)
            {
                continue;
            }

            var bedarf =
                Zahl(
                    Wert(zeile, "U"));

            if (bedarf.HasValue)
            {
                alternative.Kennwerte.Add(
                    new B56Kennwert
                    {
                        Name =
                            konfiguration.Bedarfsname,
                        Einheit =
                            konfiguration.Einheit,
                        Wert =
                            bedarf.Value
                    });

                importiert = true;
            }

            var einsparung =
                Zahl(
                    Wert(zeile, "V"));

            if (einsparung.HasValue)
            {
                alternative.Kennwerte.Add(
                    new B56Kennwert
                    {
                        Name =
                            konfiguration.Einsparungsname,
                        Einheit =
                            konfiguration.Einheit,
                        Wert =
                            einsparung.Value
                    });

                importiert = true;
            }
        }

        return importiert;
    }

    private static string Feldwert(
        IReadOnlyList<B56Zeile> zeilen,
        string feldname)
    {
        var zeile =
            zeilen.FirstOrDefault(
                kandidat =>
                    string.Equals(
                        Wert(kandidat, "B"),
                        feldname,
                        StringComparison.OrdinalIgnoreCase));

        return zeile is null
            ? string.Empty
            : Wert(zeile, "C");
    }

    private static string Wert(
        B56Zeile zeile,
        string spalte)
    {
        return zeile.Zellen
            .FirstOrDefault(
                zelle =>
                    string.Equals(
                        zelle.Spalte,
                        spalte,
                        StringComparison.OrdinalIgnoreCase))
            ?.Wert
            .Trim()
            ?? string.Empty;
    }

    private static double? Zahl(
        string text)
    {
        return double.TryParse(
            text,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var wert)
            ? wert
            : null;
    }

    private static bool IstModernisierungsstart(
        string text)
    {
        return string.Equals(
                text,
                "Modernisierung in einem Zug",
                StringComparison.OrdinalIgnoreCase) ||
            ModernisierungsnameRegex()
                .IsMatch(text);
    }

    private static bool IstZugeordneteBauteiltabelle(
        B56Tabelle tabelle)
    {
        return string.Equals(
                tabelle.Arbeitsblatt,
                Modernisierungsblatt,
                StringComparison.OrdinalIgnoreCase) &&
            tabelle.Spalten.Any(
                spalte =>
                    string.Equals(
                        spalte,
                        "Bauteilcode",
                        StringComparison.OrdinalIgnoreCase)) &&
            tabelle.Spalten.Any(
                spalte =>
                    string.Equals(
                        spalte,
                        "U-Wert",
                        StringComparison.OrdinalIgnoreCase));
    }

    private static bool IstZugeordneteBerichtstabelle(
        B56Tabelle tabelle)
    {
        return string.Equals(
                   tabelle.Arbeitsblatt,
                   EnergieberichtBlatt,
                   StringComparison.OrdinalIgnoreCase) &&
               (tabelle.Spalten.Any(
                    spalte =>
                        string.Equals(
                            spalte,
                            "Bedarf",
                            StringComparison.OrdinalIgnoreCase)) ||
                tabelle.Spalten.Any(
                    spalte =>
                        string.Equals(
                            spalte,
                            "Emission",
                            StringComparison.OrdinalIgnoreCase))) &&
               tabelle.Spalten.Any(
                   spalte =>
                       string.Equals(
                           spalte,
                           "Einsparung gegenüber Bedarf",
                           StringComparison.OrdinalIgnoreCase));
    }

    private static int? AlternativePositionErmitteln(
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var treffer =
            BerichtsalternativeRegex()
                .Match(text);

        return treffer.Success &&
               int.TryParse(
                   treffer.Groups["position"].Value,
                   out var position)
            ? position
            : null;
    }

    private sealed record BerichtskennwertKonfiguration(
        string Abschnittstitel,
        string Bedarfsname,
        string Einsparungsname,
        string Einheit);

    [GeneratedRegex(
        @"^Modernisierung\s+\d+$",
        RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant)]
    private static partial Regex ModernisierungsnameRegex();

    [GeneratedRegex(
        @"^(?:Mod|MP)\s*(?<position>\d+)\b",
        RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant)]
    private static partial Regex BerichtsalternativeRegex();
}
