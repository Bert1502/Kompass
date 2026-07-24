using Kompass.Application.B56Import;
using Kompass.Persistence.Services;

namespace Kompass.Tests.B56Import;

public sealed class B56TabellenImportServiceTests
{
    [Fact]
    public async Task Importiert_Bestand_Modernisierungen_und_Bauteil_U_Werte()
    {
        var arbeitsmappe =
            new B56Arbeitsmappe
            {
                Dateipfad =
                    "anonymisierte-b56-struktur.xlsm",
                Arbeitsblaetter =
                [
                    new B56Arbeitsblatt
                    {
                        Name =
                            "SCModernisierungen",
                        Zeilen =
                            ErzeugeReferenzzeilen()
                    }
                ]
            };

        var pipeline =
            new B56ImportPipeline(
                new B56TabellenImportService(
                    new B56TabellenFinder()));

        var ergebnis =
            await pipeline.ImportierenAsync(
                new B56ImportKontext
                {
                    ImportId =
                        Guid.NewGuid(),
                    ProjektId =
                        Guid.NewGuid(),
                    Projektname =
                        "Anonymisiertes Testprojekt",
                    Quelldatei =
                        arbeitsmappe.Dateipfad,
                    Archivdatei =
                        arbeitsmappe.Dateipfad,
                    SHA256 =
                        "0123456789abcdef",
                    Importzeitpunkt =
                        DateTimeOffset.UtcNow,
                    Arbeitsmappe =
                        arbeitsmappe
                });

        Assert.Equal(
            3,
            ergebnis.ErkannteTabellen);

        Assert.Equal(
            3,
            ergebnis.ImportierteTabellen);

        Assert.Equal(
            2,
            ergebnis.ImportierteBauteile);

        Assert.Equal(
            12,
            ergebnis.ImportierteKennwerte);

        Assert.Equal(
            2,
            ergebnis.ImportierteModernisierungsalternativen);

        Assert.Empty(
            ergebnis.Warnungen);

        Assert.Equal(
            ["AW01", "AF01"],
            ergebnis.Bauteile.Select(
                bauteil => bauteil.Bauteilcode));

        Assert.Equal(
            [0.24, 1.3],
            ergebnis.Bauteile.Select(
                bauteil => bauteil.UWert));

        Assert.Equal(
            [
                "Primärenergiebedarf Gebäude",
                "Endenergiebedarf Gebäude",
                "CO2-Emissionen Gebäude"
            ],
            ergebnis.Bestandskennwerte.Select(
                kennwert => kennwert.Name));

        Assert.Equal(
            ["Gesamtpaket", "Fenster"],
            ergebnis.Modernisierungsalternativen.Select(
                alternative => alternative.Bezeichnung));

        Assert.Equal(
            "Anonymisierte Gesamtmaßnahme",
            ergebnis.Modernisierungsalternativen[0]
                .Beschreibung);
    }

    private static IReadOnlyList<B56Zeile>
        ErzeugeReferenzzeilen()
    {
        return
        [
            Zeile(
                4,
                ("A", "Modernisierung in einem Zug")),
            Zeile(
                5,
                ("B", "Bezeichnung"),
                ("C", "Gesamtpaket")),
            Zeile(
                6,
                ("B", "Beschreibung"),
                ("C", "Anonymisierte Gesamtmaßnahme")),
            Zeile(
                8,
                ("B", "Primärenergiebedarf Gebäude"),
                ("C", "100.5")),
            Zeile(
                9,
                ("B", "Endenergiebedarf Gebäude"),
                ("C", "80.25")),
            Zeile(
                10,
                ("B", "CO2-Emissionen Gebäude"),
                ("C", "20.5")),
            Zeile(
                11,
                ("B", "Investitionskosten"),
                ("C", "50000")),
            Zeile(
                14,
                ("B", "Förderung gesamt"),
                ("C", "7500")),
            Zeile(
                28,
                ("A", "Modernisierung 1")),
            Zeile(
                29,
                ("B", "Bezeichnung"),
                ("C", "Fenster")),
            Zeile(
                30,
                ("B", "Beschreibung"),
                ("C", "Fenstertausch")),
            Zeile(
                31,
                ("B", "Investitionskosten"),
                ("C", "20000")),
            Zeile(
                37,
                ("B", "Primärenergiebedarf Gebäude"),
                ("C", "150")),
            Zeile(
                38,
                ("B", "Endenergiebedarf Gebäude"),
                ("C", "130")),
            Zeile(
                39,
                ("B", "CO2-Emissionen Gebäude"),
                ("C", "35")),
            Zeile(
                52,
                ("A", "Modernisierung 2")),
            Zeile(
                227,
                ("A", "Bestand")),
            Zeile(
                228,
                ("B", "Primärenergiebedarf Gebäude"),
                ("C", "200")),
            Zeile(
                229,
                ("B", "Endenergiebedarf Gebäude"),
                ("C", "180")),
            Zeile(
                230,
                ("B", "CO2-Emissionen Gebäude"),
                ("C", "50")),
            Zeile(
                245,
                ("A", "Tabelle U-Werte der Bauteile")),
            Zeile(
                247,
                ("B", "Bauteilcode"),
                ("C", "Bauteil"),
                ("D", "Nachbarseite"),
                ("E", "U-Wert")),
            Zeile(
                248,
                ("E", "[W/(m²K)]")),
            Zeile(
                249,
                ("B", "AW01"),
                ("C", "Außenwand"),
                ("D", "gegen Außenluft"),
                ("E", "0.24")),
            Zeile(
                250,
                ("B", "AF01"),
                ("C", "Fenster"),
                ("D", "Fenster(tür)"),
                ("E", "1.3"))
        ];
    }

    private static B56Zeile Zeile(
        int zeilennummer,
        params (string Spalte, string Wert)[] zellen)
    {
        return new B56Zeile
        {
            Zeilennummer =
                zeilennummer,
            Zellen =
                zellen.Select(
                        zelle =>
                            new B56Zelle
                            {
                                Adresse =
                                    $"{zelle.Spalte}{zeilennummer}",
                                Spalte =
                                    zelle.Spalte,
                                Zeile =
                                    zeilennummer,
                                Wert =
                                    zelle.Wert
                            })
                    .ToList()
        };
    }
}
