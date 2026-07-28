using Kompass.Application.B56Import;
using Kompass.Persistence.Data;
using Kompass.Persistence.Services;
using Kompass.Tests.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests.B56Import;

public sealed class B56SnapshotVergleichServiceTests
{
    // ─── Kein Snapshot gefunden ───────────────────────────────────────────────

    [Fact]
    public async Task Vorgaenger_nicht_gefunden_liefert_NichtGefunden()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service = ErzeugeService(testdatenbank.Context);

        var projektId = Guid.NewGuid();
        var nachfolgerId = Guid.NewGuid();

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            nachfolgerId,
            ErstelleFachdaten("A", 100));

        var ergebnis =
            await service.VergleichenAsync(
                projektId,
                Guid.NewGuid(),
                nachfolgerId);

        Assert.Equal(
            B56SnapshotVergleichStatus.NichtGefunden,
            ergebnis.Status);
    }

    [Fact]
    public async Task Nachfolger_nicht_gefunden_liefert_NichtGefunden()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service = ErzeugeService(testdatenbank.Context);

        var projektId = Guid.NewGuid();
        var vorgaengerId = Guid.NewGuid();

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            vorgaengerId,
            ErstelleFachdaten("A", 100));

        var ergebnis =
            await service.VergleichenAsync(
                projektId,
                vorgaengerId,
                Guid.NewGuid());

        Assert.Equal(
            B56SnapshotVergleichStatus.NichtGefunden,
            ergebnis.Status);
    }

    // ─── Identische Snapshots ─────────────────────────────────────────────────

    [Fact]
    public async Task Identische_Snapshots_liefern_nur_unveraenderte_Eintraege()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service = ErzeugeService(testdatenbank.Context);

        var projektId = Guid.NewGuid();
        var vorgaengerId = Guid.NewGuid();
        var nachfolgerId = Guid.NewGuid();

        var fachdaten = ErstelleFachdaten(
            "Primärenergie",
            200,
            bauteilcode: "AW01",
            alternativePosition: 1,
            alternativeBezeichnung: "Fenster",
            alternativeKennwertName: "Investition",
            alternativeKennwertWert: 10000);

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            vorgaengerId,
            fachdaten);

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            nachfolgerId,
            fachdaten);

        var ergebnis =
            await service.VergleichenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        Assert.Equal(
            B56SnapshotVergleichStatus.Erfolgreich,
            ergebnis.Status);

        Assert.NotNull(ergebnis.Vergleich);
        Assert.False(ergebnis.Vergleich.HatAenderungen);

        Assert.All(
            ergebnis.Vergleich.BestandskennwertVergleiche,
            k => Assert.Equal(
                B56VergleichsAenderung.Unveraendert,
                k.Aenderung));

        Assert.All(
            ergebnis.Vergleich.AlternativVergleiche,
            a => Assert.Equal(
                B56VergleichsAenderung.Unveraendert,
                a.Aenderung));

        Assert.All(
            ergebnis.Vergleich.GesamtbauteilVergleiche,
            b => Assert.Equal(
                B56VergleichsAenderung.Unveraendert,
                b.Aenderung));
    }

    // ─── Bestandskennwerte ────────────────────────────────────────────────────

    [Fact]
    public async Task Neuer_Kennwert_im_Nachfolger_wird_als_Hinzugefuegt_erkannt()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service = ErzeugeService(testdatenbank.Context);

        var projektId = Guid.NewGuid();
        var vorgaengerId = Guid.NewGuid();
        var nachfolgerId = Guid.NewGuid();

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            vorgaengerId,
            new B56ImportPipelineErgebnis
            {
                Bestandskennwerte =
                [
                    new B56Kennwert
                    {
                        Name = "Primärenergie",
                        Wert = 200
                    }
                ]
            });

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            nachfolgerId,
            new B56ImportPipelineErgebnis
            {
                Bestandskennwerte =
                [
                    new B56Kennwert
                    {
                        Name = "Primärenergie",
                        Wert = 200
                    },
                    new B56Kennwert
                    {
                        Name = "Endenergie",
                        Wert = 150
                    }
                ]
            });

        var ergebnis =
            await service.VergleichenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        Assert.Equal(
            B56SnapshotVergleichStatus.Erfolgreich,
            ergebnis.Status);

        Assert.True(ergebnis.Vergleich!.HatAenderungen);

        var neuerKennwert =
            Assert.Single(
                ergebnis.Vergleich.BestandskennwertVergleiche,
                k => k.Name == "Endenergie");

        Assert.Equal(
            B56VergleichsAenderung.Hinzugefuegt,
            neuerKennwert.Aenderung);
        Assert.Null(neuerKennwert.AlterWert);
        Assert.Equal(150, neuerKennwert.NeuerWert);
    }

    [Fact]
    public async Task Fehlender_Kennwert_im_Nachfolger_wird_als_Entfernt_erkannt()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service = ErzeugeService(testdatenbank.Context);

        var projektId = Guid.NewGuid();
        var vorgaengerId = Guid.NewGuid();
        var nachfolgerId = Guid.NewGuid();

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            vorgaengerId,
            new B56ImportPipelineErgebnis
            {
                Bestandskennwerte =
                [
                    new B56Kennwert
                    {
                        Name = "Primärenergie",
                        Wert = 200
                    },
                    new B56Kennwert
                    {
                        Name = "Endenergie",
                        Wert = 150
                    }
                ]
            });

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            nachfolgerId,
            new B56ImportPipelineErgebnis
            {
                Bestandskennwerte =
                [
                    new B56Kennwert
                    {
                        Name = "Primärenergie",
                        Wert = 200
                    }
                ]
            });

        var ergebnis =
            await service.VergleichenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        var entfernterKennwert =
            Assert.Single(
                ergebnis.Vergleich!.BestandskennwertVergleiche,
                k => k.Name == "Endenergie");

        Assert.Equal(
            B56VergleichsAenderung.Entfernt,
            entfernterKennwert.Aenderung);
        Assert.Equal(150, entfernterKennwert.AlterWert);
        Assert.Null(entfernterKennwert.NeuerWert);
    }

    [Fact]
    public async Task Geaenderter_Kennwertwert_wird_als_Geaendert_erkannt()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service = ErzeugeService(testdatenbank.Context);

        var projektId = Guid.NewGuid();
        var vorgaengerId = Guid.NewGuid();
        var nachfolgerId = Guid.NewGuid();

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            vorgaengerId,
            new B56ImportPipelineErgebnis
            {
                Bestandskennwerte =
                [
                    new B56Kennwert { Name = "Primärenergie", Wert = 200 }
                ]
            });

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            nachfolgerId,
            new B56ImportPipelineErgebnis
            {
                Bestandskennwerte =
                [
                    new B56Kennwert { Name = "Primärenergie", Wert = 180 }
                ]
            });

        var ergebnis =
            await service.VergleichenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        var geaendert =
            Assert.Single(
                ergebnis.Vergleich!.BestandskennwertVergleiche);

        Assert.Equal(
            B56VergleichsAenderung.Geaendert,
            geaendert.Aenderung);
        Assert.Equal(200, geaendert.AlterWert);
        Assert.Equal(180, geaendert.NeuerWert);
    }

    // ─── Bauteile ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Geaenderter_UWert_eines_Bauteils_wird_erkannt()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service = ErzeugeService(testdatenbank.Context);

        var projektId = Guid.NewGuid();
        var vorgaengerId = Guid.NewGuid();
        var nachfolgerId = Guid.NewGuid();

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            vorgaengerId,
            new B56ImportPipelineErgebnis
            {
                Bauteile =
                [
                    new B56Bauteil
                    {
                        Bauteilcode = "AW01",
                        Bezeichnung = "Außenwand",
                        UWert = 0.24
                    }
                ]
            });

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            nachfolgerId,
            new B56ImportPipelineErgebnis
            {
                Bauteile =
                [
                    new B56Bauteil
                    {
                        Bauteilcode = "AW01",
                        Bezeichnung = "Außenwand",
                        UWert = 0.18
                    }
                ]
            });

        var ergebnis =
            await service.VergleichenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        var geaendert =
            Assert.Single(
                ergebnis.Vergleich!.GesamtbauteilVergleiche,
                b => b.Bauteilcode == "AW01");

        Assert.Equal(
            B56VergleichsAenderung.Geaendert,
            geaendert.Aenderung);
        Assert.Equal(0.24, geaendert.AlterUWert);
        Assert.Equal(0.18, geaendert.NeuerUWert);
    }

    [Fact]
    public async Task Neues_Bauteil_im_Nachfolger_wird_als_Hinzugefuegt_erkannt()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service = ErzeugeService(testdatenbank.Context);

        var projektId = Guid.NewGuid();
        var vorgaengerId = Guid.NewGuid();
        var nachfolgerId = Guid.NewGuid();

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            vorgaengerId,
            new B56ImportPipelineErgebnis
            {
                Bauteile =
                [
                    new B56Bauteil { Bauteilcode = "AW01" }
                ]
            });

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            nachfolgerId,
            new B56ImportPipelineErgebnis
            {
                Bauteile =
                [
                    new B56Bauteil { Bauteilcode = "AW01" },
                    new B56Bauteil { Bauteilcode = "DA01" }
                ]
            });

        var ergebnis =
            await service.VergleichenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        var neues =
            Assert.Single(
                ergebnis.Vergleich!.GesamtbauteilVergleiche,
                b => b.Bauteilcode == "DA01");

        Assert.Equal(
            B56VergleichsAenderung.Hinzugefuegt,
            neues.Aenderung);
    }

    // ─── Alternativen ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Neue_Alternative_im_Nachfolger_wird_als_Hinzugefuegt_erkannt()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service = ErzeugeService(testdatenbank.Context);

        var projektId = Guid.NewGuid();
        var vorgaengerId = Guid.NewGuid();
        var nachfolgerId = Guid.NewGuid();

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            vorgaengerId,
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen =
                [
                    new B56Modernisierungsalternative
                    {
                        Position = 1,
                        Bezeichnung = "Fenster"
                    }
                ]
            });

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            nachfolgerId,
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen =
                [
                    new B56Modernisierungsalternative
                    {
                        Position = 1,
                        Bezeichnung = "Fenster"
                    },
                    new B56Modernisierungsalternative
                    {
                        Position = 2,
                        Bezeichnung = "Dach"
                    }
                ]
            });

        var ergebnis =
            await service.VergleichenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        var neueAlternative =
            Assert.Single(
                ergebnis.Vergleich!.AlternativVergleiche,
                a => a.B56Position == 2);

        Assert.Equal(
            B56VergleichsAenderung.Hinzugefuegt,
            neueAlternative.Aenderung);
        Assert.Equal(
            "Dach",
            neueAlternative.NeueBezeichnung);
    }

    [Fact]
    public async Task Entfernte_Alternative_im_Nachfolger_wird_als_Entfernt_erkannt()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service = ErzeugeService(testdatenbank.Context);

        var projektId = Guid.NewGuid();
        var vorgaengerId = Guid.NewGuid();
        var nachfolgerId = Guid.NewGuid();

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            vorgaengerId,
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen =
                [
                    new B56Modernisierungsalternative
                    {
                        Position = 1,
                        Bezeichnung = "Fenster"
                    },
                    new B56Modernisierungsalternative
                    {
                        Position = 2,
                        Bezeichnung = "Dach"
                    }
                ]
            });

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            nachfolgerId,
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen =
                [
                    new B56Modernisierungsalternative
                    {
                        Position = 1,
                        Bezeichnung = "Fenster"
                    }
                ]
            });

        var ergebnis =
            await service.VergleichenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        var entfernte =
            Assert.Single(
                ergebnis.Vergleich!.AlternativVergleiche,
                a => a.B56Position == 2);

        Assert.Equal(
            B56VergleichsAenderung.Entfernt,
            entfernte.Aenderung);
        Assert.Equal(
            "Dach",
            entfernte.AlteBezeichnung);
    }

    [Fact]
    public async Task Geaenderte_Bezeichnung_einer_Alternative_wird_als_Geaendert_erkannt()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service = ErzeugeService(testdatenbank.Context);

        var projektId = Guid.NewGuid();
        var vorgaengerId = Guid.NewGuid();
        var nachfolgerId = Guid.NewGuid();

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            vorgaengerId,
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen =
                [
                    new B56Modernisierungsalternative
                    {
                        Position = 1,
                        Bezeichnung = "Alt-Bezeichnung"
                    }
                ]
            });

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            nachfolgerId,
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen =
                [
                    new B56Modernisierungsalternative
                    {
                        Position = 1,
                        Bezeichnung = "Neu-Bezeichnung"
                    }
                ]
            });

        var ergebnis =
            await service.VergleichenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        var alternative =
            Assert.Single(
                ergebnis.Vergleich!.AlternativVergleiche);

        Assert.Equal(
            B56VergleichsAenderung.Geaendert,
            alternative.Aenderung);
        Assert.Equal(
            "Alt-Bezeichnung",
            alternative.AlteBezeichnung);
        Assert.Equal(
            "Neu-Bezeichnung",
            alternative.NeueBezeichnung);
    }

    [Fact]
    public async Task Geaenderter_Kennwert_in_Alternative_propagiert_als_Geaendert()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service = ErzeugeService(testdatenbank.Context);

        var projektId = Guid.NewGuid();
        var vorgaengerId = Guid.NewGuid();
        var nachfolgerId = Guid.NewGuid();

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            vorgaengerId,
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen =
                [
                    new B56Modernisierungsalternative
                    {
                        Position = 1,
                        Bezeichnung = "Fenster",
                        Kennwerte =
                        {
                            new B56Kennwert
                            {
                                Name = "Investition",
                                Wert = 10000
                            }
                        }
                    }
                ]
            });

        await SpeichereSnapshot(
            testdatenbank.Context,
            projektId,
            nachfolgerId,
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen =
                [
                    new B56Modernisierungsalternative
                    {
                        Position = 1,
                        Bezeichnung = "Fenster",
                        Kennwerte =
                        {
                            new B56Kennwert
                            {
                                Name = "Investition",
                                Wert = 12000
                            }
                        }
                    }
                ]
            });

        var ergebnis =
            await service.VergleichenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        var alternative =
            Assert.Single(
                ergebnis.Vergleich!.AlternativVergleiche);

        Assert.Equal(
            B56VergleichsAenderung.Geaendert,
            alternative.Aenderung);

        var kennwert =
            Assert.Single(
                alternative.KennwertVergleiche);

        Assert.Equal(
            B56VergleichsAenderung.Geaendert,
            kennwert.Aenderung);
        Assert.Equal(10000, kennwert.AlterWert);
        Assert.Equal(12000, kennwert.NeuerWert);
    }

    // ─── Hilfsmethoden ────────────────────────────────────────────────────────

    private static B56SnapshotVergleichService ErzeugeService(
        KompassDbContext context)
    {
        return new B56SnapshotVergleichService(
            new EfB56ImportRegister(context));
    }

    private static async Task SpeichereSnapshot(
        KompassDbContext context,
        Guid projektId,
        Guid importId,
        B56ImportPipelineErgebnis fachdaten)
    {
        var register =
            new EfB56ImportRegister(context);

        await register.EintragMitFachdatenSpeichernAsync(
            new B56ImportEintrag
            {
                ImportId = importId,
                ProjektId = projektId,
                Projektname = "Testprojekt",
                Originaldateiname = "b56.xlsx",
                Archivdateipfad = "archiv/b56.xlsx",
                // Deterministischer 64-Zeichen-Hash: Wiederholung eines aus
                // der Import-ID abgeleiteten Buchstabens, damit verschiedene
                // Import-IDs unterschiedliche Hashes erzeugen.
                Sha256 = new string(
                    (char)('a' + (importId.ToByteArray()[0] % 26)),
                    64),
                DateigroesseBytes = 1024,
                ImportiertAm = DateTimeOffset.UtcNow,
                Dateiendung = ".xlsx"
            },
            fachdaten);
    }

    private static B56ImportPipelineErgebnis ErstelleFachdaten(
        string kennwertName,
        double kennwertWert,
        string bauteilcode = "AW01",
        int alternativePosition = 0,
        string alternativeBezeichnung = "",
        string alternativeKennwertName = "",
        double alternativeKennwertWert = 0)
    {
        var alternativen =
            new List<B56Modernisierungsalternative>();

        if (alternativePosition > 0)
        {
            var alternative =
                new B56Modernisierungsalternative
                {
                    Position = alternativePosition,
                    Bezeichnung = alternativeBezeichnung
                };

            if (!string.IsNullOrWhiteSpace(alternativeKennwertName))
            {
                alternative.Kennwerte.Add(
                    new B56Kennwert
                    {
                        Name = alternativeKennwertName,
                        Wert = alternativeKennwertWert
                    });
            }

            alternativen.Add(alternative);
        }

        return new B56ImportPipelineErgebnis
        {
            Bestandskennwerte =
            [
                new B56Kennwert
                {
                    Name = kennwertName,
                    Wert = kennwertWert
                }
            ],
            Bauteile =
            [
                new B56Bauteil
                {
                    Bauteilcode = bauteilcode,
                    Bezeichnung = "Außenwand",
                    UWert = 0.24
                }
            ],
            Modernisierungsalternativen = alternativen
        };
    }
}
