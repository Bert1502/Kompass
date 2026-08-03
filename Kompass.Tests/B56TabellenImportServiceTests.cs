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
            9,
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
            ["[kWh/a]", "[kWh/a]", "[kg]"],
            ergebnis.Bestandskennwerte.Select(
                kennwert => kennwert.Einheit));

        Assert.Equal(
            ["Gesamtpaket", "Fenster"],
            ergebnis.Modernisierungsalternativen.Select(
                alternative => alternative.Bezeichnung));

        Assert.Equal(
            "Anonymisierte Gesamtmaßnahme",
            ergebnis.Modernisierungsalternativen[0]
                .Beschreibung);

        Assert.Equal(
            "EG 55",
            ergebnis.EffizienzstandardKontrollwert?.Originaltext);
    }

    [Fact]
    public async Task Importiert_Bauteilflaechen_aus_der_Energiebilanz()
    {
        var service =
            new B56TabellenImportService(
                new B56TabellenFinder());

        var ergebnis =
            await service.ImportierenAsync(
                new B56ImportKontext
                {
                    ImportId = Guid.NewGuid(),
                    ProjektId = Guid.NewGuid(),
                    Projektname = "Testprojekt",
                    Quelldatei = "test.xlsm",
                    Archivdatei = "test.xlsm",
                    SHA256 = "0123456789abcdef",
                    Importzeitpunkt = DateTimeOffset.UtcNow,
                    Arbeitsmappe = new B56Arbeitsmappe
                    {
                        Dateipfad = "test.xlsm",
                        Arbeitsblaetter =
                        [
                            new B56Arbeitsblatt
                            {
                                Name = "SCModernisierungen",
                                Zeilen =
                                [
                                    Zeile(1, ("A", "Bestand")),
                                    Zeile(2, ("B", "Bauteilcode"), ("C", "Bauteil"), ("D", "Nachbarseite"), ("E", "U-Wert")),
                                    Zeile(3, ("B", "AW01"), ("C", "Außenwand"), ("D", "gegen Außenluft"), ("E", "0.24")),
                                    Zeile(4, ("B", "AF01"), ("C", "Fenster"), ("D", "Fenster(tür)"), ("E", "1.3"))
                                ]
                            },
                            new B56Arbeitsblatt
                            {
                                Name = "SCEnergiebilanz",
                                Zeilen =
                                [
                                    Zeile(5, ("B", "Codierung"), ("C", "Bezeichnung"), ("D", "Fläche"), ("E", "U-Wert")),
                                    Zeile(6, ("B", "AW01"), ("C", "Abweichende Bezeichnung"), ("D", "359.24"), ("E", "0.24")),
                                    Zeile(7, ("B", "AF01"), ("C", "Fenster"), ("D", "33.78"), ("E", "1.3"))
                                ]
                            }
                        ]
                    }
                });

        Assert.Equal(
            [359.24d, 33.78d],
            ergebnis.Bauteile.Select(bauteil => bauteil.Flaeche));
    }

    [Fact]
    public async Task Importiert_Beg_Ziel_unveraendert_als_Gegenkontrolle()
    {
        var importId = Guid.NewGuid();
        var service =
            new B56TabellenImportService(
                new B56TabellenFinder());

        var ergebnis =
            await service.ImportierenAsync(
                new B56ImportKontext
                {
                    ImportId = importId,
                    ProjektId = Guid.NewGuid(),
                    Projektname = "Testprojekt",
                    Quelldatei = "test.xlsm",
                    Archivdatei = "test.xlsm",
                    SHA256 = "0123456789abcdef",
                    Importzeitpunkt = DateTimeOffset.UtcNow,
                    Arbeitsmappe = new B56Arbeitsmappe
                    {
                        Dateipfad = "test.xlsm",
                        Arbeitsblaetter =
                        [
                            new B56Arbeitsblatt
                            {
                                Name = "SCModernisierungen",
                                Zeilen =
                                [
                                    Zeile(7, ("C", "EG 55"))
                                ]
                            }
                        ]
                    }
                });

        var kontrollwert =
            Assert.IsType<B56EffizienzstandardKontrollwert>(
                ergebnis.EffizienzstandardKontrollwert);

        Assert.Equal(importId, kontrollwert.ImportId);
        Assert.Equal("BEG_ZIEL", kontrollwert.Feldname);
        Assert.Equal("EG 55", kontrollwert.Originaltext);
        Assert.Equal("SCModernisierungen", kontrollwert.Arbeitsblatt);
        Assert.Equal("C7", kontrollwert.Zelladresse);
    }

    [Fact]
    public async Task Importiert_Neun_Modernisierungsalternativen()
    {
        var zeilen =
            Enumerable.Range(
                    1,
                    9)
                .SelectMany(
                    nummer =>
                    {
                        var startzeile =
                            nummer * 20;

                        return new[]
                        {
                            Zeile(
                                startzeile,
                                ("A", $"Modernisierung {nummer}")),
                            Zeile(
                                startzeile + 1,
                                ("B", "Bezeichnung"),
                                ("C", $"Alternative {nummer}")),
                            Zeile(
                                startzeile + 2,
                                ("B", "Primärenergiebedarf Gebäude"),
                                ("C", $"{200 - nummer}"))
                        };
                    })
                .Append(
                    Zeile(
                        220,
                        ("A", "Bestand")))
                .ToList();

        var service =
            new B56TabellenImportService(
                new B56TabellenFinder());

        var ergebnis =
            await service.ImportierenAsync(
                new B56ImportKontext
                {
                    ImportId =
                        Guid.NewGuid(),
                    ProjektId =
                        Guid.NewGuid(),
                    Projektname =
                        "Testprojekt mit neun Alternativen",
                    Quelldatei =
                        "neun-modernisierungsalternativen.xlsm",
                    Archivdatei =
                        "neun-modernisierungsalternativen.xlsm",
                    SHA256 =
                        "0123456789abcdef",
                    Importzeitpunkt =
                        DateTimeOffset.UtcNow,
                    Arbeitsmappe =
                        new B56Arbeitsmappe
                        {
                            Dateipfad =
                                "neun-modernisierungsalternativen.xlsm",
                            Arbeitsblaetter =
                            [
                                new B56Arbeitsblatt
                                {
                                    Name =
                                        "SCModernisierungen",
                                    Zeilen =
                                        zeilen
                                }
                            ]
                        }
                });

        Assert.Equal(
            9,
            ergebnis.Modernisierungsalternativen.Count);

        Assert.Equal(
            Enumerable.Range(
                    1,
                    9)
                .Select(
                    nummer => $"Alternative {nummer}"),
            ergebnis.Modernisierungsalternativen.Select(
                alternative => alternative.Bezeichnung));

        Assert.Equal(
            Enumerable.Range(
                1,
                9),
            ergebnis.Modernisierungsalternativen.Select(
                alternative => alternative.Position));
    }

    [Fact]
    public async Task Kennwert_mit_numerischen_Spaltenwerten_erzeugt_keine_Warnung()
    {
        // Rows like "Primärenergie | 3860.393 | 22351.231" in SCModernisierungen
        // are data rows, not table headers – they must not generate spurious warnings.
        var zeilen =
            new List<B56Zeile>
            {
                Zeile(1, ("A", "Modernisierung in einem Zug")),
                Zeile(2, ("B", "Bezeichnung"), ("C", "Gesamtpaket")),
                Zeile(3, ("B", "Primärenergiebedarf Gebäude"), ("C", "100.5")),
                Zeile(4, ("B", "Endenergiebedarf Gebäude"), ("C", "80.25")),
                // Row that looks like "Primärenergie | 3860.393 | 22351.231":
                // keyword in first cell, numeric values in subsequent cells.
                Zeile(5, ("A", "Primärenergie"), ("B", "3860.393"), ("C", "22351.231")),
                Zeile(6, ("A", "Endenergie"), ("B", "1234.5"), ("C", "6789.0")),
                Zeile(10, ("A", "Bestand")),
                Zeile(11, ("B", "Primärenergiebedarf Gebäude"), ("C", "200")),
                Zeile(20, ("A", "Tabelle U-Werte der Bauteile")),
                Zeile(22, ("B", "Bauteilcode"), ("C", "Bauteil"), ("D", "Nachbarseite"), ("E", "U-Wert")),
                Zeile(23, ("B", "AW01"), ("C", "Außenwand"), ("D", "gegen Außenluft"), ("E", "0.24"))
            };

        var service =
            new B56TabellenImportService(
                new B56TabellenFinder());

        var ergebnis =
            await service.ImportierenAsync(
                new B56ImportKontext
                {
                    ImportId = Guid.NewGuid(),
                    ProjektId = Guid.NewGuid(),
                    Projektname = "Testprojekt",
                    Quelldatei = "test.xlsm",
                    Archivdatei = "test.xlsm",
                    SHA256 = "0123456789abcdef",
                    Importzeitpunkt = DateTimeOffset.UtcNow,
                    Arbeitsmappe = new B56Arbeitsmappe
                    {
                        Dateipfad = "test.xlsm",
                        Arbeitsblaetter =
                        [
                            new B56Arbeitsblatt
                            {
                                Name = "SCModernisierungen",
                                Zeilen = zeilen
                            }
                        ]
                    }
                });

        Assert.Empty(ergebnis.Warnungen);
        Assert.Single(ergebnis.Bauteile);
    }

    [Fact]
    public async Task Importiert_Berichtskennwerte_aus_SCEnergiebericht_je_Position()
    {
        var service =
            new B56TabellenImportService(
                new B56TabellenFinder());

        var ergebnis =
            await service.ImportierenAsync(
                new B56ImportKontext
                {
                    ImportId = Guid.NewGuid(),
                    ProjektId = Guid.NewGuid(),
                    Projektname = "Testprojekt",
                    Quelldatei = "test.xlsm",
                    Archivdatei = "test.xlsm",
                    SHA256 = "0123456789abcdef",
                    Importzeitpunkt = DateTimeOffset.UtcNow,
                    Arbeitsmappe = new B56Arbeitsmappe
                    {
                        Dateipfad = "test.xlsm",
                        Arbeitsblaetter =
                        [
                            new B56Arbeitsblatt
                            {
                                Name = "SCModernisierungen",
                                Zeilen = ErzeugeReferenzzeilen()
                            },
                            new B56Arbeitsblatt
                            {
                                Name = "SCEnergiebericht",
                                Zeilen = ErzeugeEnergieberichtZeilen()
                            }
                        ]
                    }
                });

        Assert.Contains(
            ergebnis.Bestandskennwerte,
            kennwert =>
                kennwert.Name == "Endenergiebedarf Bericht" &&
                kennwert.Wert == 393298d &&
                kennwert.Einheit == "[kWh/a]");

        Assert.Contains(
            ergebnis.Bestandskennwerte,
            kennwert =>
                kennwert.Name == "CO2-Emission Bericht" &&
                kennwert.Wert == 90207d &&
                kennwert.Einheit == "[kg]");

        var alternative1 =
            Assert.Single(
                ergebnis.Modernisierungsalternativen,
                alternative => alternative.Position == 1);

        Assert.Contains(
            alternative1.Kennwerte,
            kennwert =>
                kennwert.Name == "Endenergiebedarf Bericht" &&
                kennwert.Wert == 190644d);

        Assert.Contains(
            alternative1.Kennwerte,
            kennwert =>
                kennwert.Name == "Endenergieeinsparung gegenüber Bedarf" &&
                kennwert.Wert == 202653d);

        Assert.Contains(
            alternative1.Kennwerte,
            kennwert =>
                kennwert.Name == "CO2-Einsparung gegenüber Bedarf" &&
                kennwert.Wert == 316498d);

        var alternative2 =
            Assert.Single(
                ergebnis.Modernisierungsalternativen,
                alternative => alternative.Position == 2);

        Assert.Contains(
            alternative2.Kennwerte,
            kennwert =>
                kennwert.Name == "Endenergiebedarf Bericht" &&
                kennwert.Wert == 393298d);

        Assert.Contains(
            alternative2.Kennwerte,
            kennwert =>
                kennwert.Name == "CO2-Emission Bericht" &&
                kennwert.Wert == 90207d);

        Assert.DoesNotContain(
            ergebnis.Warnungen,
            warnung =>
                warnung.Contains(
                    "SCEnergiebericht",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("SCNeubau")]
    [InlineData("SCEnergiebilanzNeubau")]
    [InlineData("SCNeubauberatungsbericht")]
    [InlineData("SCEnergiebilanz")]
    [InlineData("SCZonendaten")]
    [InlineData("SCModernisierungen")]
    public async Task Tabellen_in_ignorierten_Arbeitsblaettern_erzeugen_keine_Warnung(
        string arbeitsblattName)
    {
        // Tables detected in worksheets that are not part of the
        // fachlich relevant import scope must be silently ignored.
        var zeilen =
            new List<B56Zeile>
            {
                Zeile(1,
                    ("A", "Bauteil"),
                    ("B", "Fläche"),
                    ("C", "U-Wert"),
                    ("D", "Transmission")),
                Zeile(2,
                    ("A", "AW01"),
                    ("B", "42.5"),
                    ("C", "0.24"),
                    ("D", "10.2"))
            };

        var service =
            new B56TabellenImportService(
                new B56TabellenFinder());

        var ergebnis =
            await service.ImportierenAsync(
                new B56ImportKontext
                {
                    ImportId = Guid.NewGuid(),
                    ProjektId = Guid.NewGuid(),
                    Projektname = "Testprojekt",
                    Quelldatei = "test.xlsm",
                    Archivdatei = "test.xlsm",
                    SHA256 = "0123456789abcdef",
                    Importzeitpunkt = DateTimeOffset.UtcNow,
                    Arbeitsmappe = new B56Arbeitsmappe
                    {
                        Dateipfad = "test.xlsm",
                        Arbeitsblaetter =
                        [
                            new B56Arbeitsblatt
                            {
                                Name = arbeitsblattName,
                                Zeilen = zeilen
                            },
                            new B56Arbeitsblatt
                            {
                                Name = "SCModernisierungen",
                                Zeilen = []
                            }
                        ]
                    }
                });

        Assert.Empty(ergebnis.Warnungen);
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
                7,
                ("C", "EG 55")),
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

    private static IReadOnlyList<B56Zeile>
        ErzeugeEnergieberichtZeilen()
    {
        return
        [
            Zeile(
                115,
                ("A", "Reduktion des Endenergiebedarfs")),
            Zeile(
                116,
                ("U", "Bedarf"),
                ("V", "Einsparung gegenüber Bedarf"),
                ("W", "Referenz")),
            Zeile(
                118,
                ("B", "Mod2: Fenster"),
                ("T", "Bestand"),
                ("U", "393298"),
                ("V", "---")),
            Zeile(
                119,
                ("B", "Mod1: Gesamtpaket"),
                ("T", "Mod2"),
                ("U", "190644"),
                ("V", "202653")),
            Zeile(
                120,
                ("B", "Mod9: Unbekannt"),
                ("T", "Mod1"),
                ("U", "420743"),
                ("V", "-27445")),
            Zeile(
                130,
                ("A", "Reduktion des Primäreneribedarfs")),
            Zeile(
                131,
                ("U", "Bedarf"),
                ("V", "Einsparung gegenüber Bedarf"),
                ("W", "Referenz")),
            Zeile(
                133,
                ("B", "Mod2: Fenster"),
                ("T", "Bestand"),
                ("U", "401921"),
                ("V", "---")),
            Zeile(
                134,
                ("B", "Mod1: Gesamtpaket"),
                ("T", "Mod2"),
                ("U", "272654"),
                ("V", "120644")),
            Zeile(
                135,
                ("B", "Mod9: Unbekannt"),
                ("T", "Mod1"),
                ("U", "433882"),
                ("V", "-40584")),
            Zeile(
                145,
                ("A", "Reduktion CO2-Emission")),
            Zeile(
                146,
                ("U", "Emission"),
                ("V", "Einsparung gegenüber Bedarf"),
                ("W", "Referenz")),
            Zeile(
                148,
                ("B", "Mod2: Fenster"),
                ("T", "Bestand"),
                ("U", "90207"),
                ("V", "---")),
            Zeile(
                149,
                ("B", "Mod1: Gesamtpaket"),
                ("T", "Mod2"),
                ("U", "76800"),
                ("V", "316498")),
            Zeile(
                150,
                ("B", "Mod9: Unbekannt"),
                ("T", "Mod1"),
                ("U", "98166"),
                ("V", "295132"))
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
