using Kompass.Application.B56Import;
<<<<<<< HEAD
using Kompass.Persistence.Data;
using Kompass.Persistence.Services;
using Kompass.Tests.Persistence;
using Microsoft.EntityFrameworkCore;
=======
>>>>>>> origin/main

namespace Kompass.Tests.B56Import;

public sealed class B56SnapshotVergleichServiceTests
{
<<<<<<< HEAD
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
=======
    [Fact]
    public async Task Gleiche_Snapshots_liefern_nur_unveraenderte_Eintraege()
    {
        var fachdaten =
            ErstelleFachdaten(
                primaerenergiebedarfBestand: 200,
                primaerenergiebedarfAlternative: 100,
                uWert: 0.24);

        var register =
            new ImportRegisterFake(
                fachdaten,
                fachdaten);

        var service =
            new B56SnapshotVergleichService(
                register);

        var altId = Guid.NewGuid();
        var neuId = Guid.NewGuid();

        var aktionsErgebnis =
            await service.VergleichenAsync(
                Guid.NewGuid(),
                altId,
                neuId);

        Assert.Equal(
            B56SnapshotVergleichStatus.Erfolgreich,
            aktionsErgebnis.Status);

        var ergebnis = aktionsErgebnis.Ergebnis!;

        Assert.All(
            ergebnis.Bestandskennwerte,
            k => Assert.Equal(
                B56VergleichsArt.Unveraendert,
                k.Art));

        Assert.All(
            ergebnis.Bauteile,
            b => Assert.Equal(
                B56VergleichsArt.Unveraendert,
                b.Art));

        Assert.All(
            ergebnis.Alternativen,
            a => Assert.Equal(
                B56VergleichsArt.Unveraendert,
                a.Art));
    }

    [Fact]
    public async Task Geaenderter_U_Wert_wird_als_Geaendert_erkannt()
    {
        var altFachdaten =
            ErstelleFachdaten(
                primaerenergiebedarfBestand: 200,
                primaerenergiebedarfAlternative: 100,
                uWert: 0.24);

        var neuFachdaten =
            ErstelleFachdaten(
                primaerenergiebedarfBestand: 200,
                primaerenergiebedarfAlternative: 100,
                uWert: 0.18);

        var register =
            new ImportRegisterFake(
                altFachdaten,
                neuFachdaten);

        var service =
            new B56SnapshotVergleichService(
                register);

        var ergebnis =
            (await service.VergleichenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid())).Ergebnis!;

        var bauteil =
            Assert.Single(
                ergebnis.Bauteile);

        Assert.Equal(
            B56VergleichsArt.Geaendert,
            bauteil.Art);
        Assert.Equal(0.24, bauteil.AlterUWert);
        Assert.Equal(0.18, bauteil.NeuerUWert);
    }

    [Fact]
    public async Task Geaenderter_Bestandskennwert_wird_als_Geaendert_erkannt()
    {
        var altFachdaten =
            ErstelleFachdaten(
                primaerenergiebedarfBestand: 200,
                primaerenergiebedarfAlternative: 100,
                uWert: 0.24);

        var neuFachdaten =
            ErstelleFachdaten(
                primaerenergiebedarfBestand: 180,
                primaerenergiebedarfAlternative: 100,
                uWert: 0.24);

        var register =
            new ImportRegisterFake(
                altFachdaten,
                neuFachdaten);

        var service =
            new B56SnapshotVergleichService(
                register);

        var ergebnis =
            (await service.VergleichenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid())).Ergebnis!;

        var kennwert =
            Assert.Single(
                ergebnis.Bestandskennwerte);

        Assert.Equal(
            B56VergleichsArt.Geaendert,
            kennwert.Art);
        Assert.Equal(200.0, kennwert.AlterWert);
        Assert.Equal(180.0, kennwert.NeuerWert);
    }

    [Fact]
    public async Task Neue_Alternative_wird_als_Hinzugefuegt_erkannt()
    {
        var altFachdaten =
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen = Array.Empty<B56Modernisierungsalternative>(),
                Bestandskennwerte = Array.Empty<B56Kennwert>(),
                Bauteile = Array.Empty<B56Bauteil>()
            };

        var neuFachdaten =
>>>>>>> origin/main
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen =
                [
                    new B56Modernisierungsalternative
                    {
                        Position = 1,
<<<<<<< HEAD
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
=======
                        Bezeichnung = "Fenstertausch"
                    }
                ],
                Bestandskennwerte = Array.Empty<B56Kennwert>(),
                Bauteile = Array.Empty<B56Bauteil>()
            };

        var register =
            new ImportRegisterFake(
                altFachdaten,
                neuFachdaten);

        var service =
            new B56SnapshotVergleichService(
                register);

        var ergebnis =
            (await service.VergleichenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid())).Ergebnis!;

        var alternative =
            Assert.Single(
                ergebnis.Alternativen);

        Assert.Equal(
            B56VergleichsArt.Hinzugefuegt,
            alternative.Art);
        Assert.Equal(1, alternative.Position);
        Assert.Null(alternative.AlteBezeichnung);
        Assert.Equal(
            "Fenstertausch",
>>>>>>> origin/main
            alternative.NeueBezeichnung);
    }

    [Fact]
<<<<<<< HEAD
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
=======
    public async Task Entfernte_Alternative_wird_als_Entfernt_erkannt()
    {
        var altFachdaten =
>>>>>>> origin/main
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen =
                [
                    new B56Modernisierungsalternative
                    {
<<<<<<< HEAD
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

=======
                        Position = 2,
                        Bezeichnung = "Dachsanierung"
                    }
                ],
                Bestandskennwerte = Array.Empty<B56Kennwert>(),
                Bauteile = Array.Empty<B56Bauteil>()
            };

        var neuFachdaten =
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen = Array.Empty<B56Modernisierungsalternative>(),
                Bestandskennwerte = Array.Empty<B56Kennwert>(),
                Bauteile = Array.Empty<B56Bauteil>()
            };

        var register =
            new ImportRegisterFake(
                altFachdaten,
                neuFachdaten);

        var service =
            new B56SnapshotVergleichService(
                register);

        var ergebnis =
            (await service.VergleichenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid())).Ergebnis!;

        var alternative =
            Assert.Single(
                ergebnis.Alternativen);

        Assert.Equal(
            B56VergleichsArt.Entfernt,
            alternative.Art);
        Assert.Equal(
            "Dachsanierung",
            alternative.AlteBezeichnung);
        Assert.Null(alternative.NeueBezeichnung);
    }

    [Fact]
    public async Task Fehlender_Snapshot_liefert_NichtGefunden()
    {
        var register =
            new ImportRegisterFake(null, null);

        var service =
            new B56SnapshotVergleichService(
                register);

        var aktionsErgebnis =
            await service.VergleichenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid());

        Assert.Equal(
            B56SnapshotVergleichStatus.NichtGefunden,
            aktionsErgebnis.Status);
        Assert.Null(aktionsErgebnis.Ergebnis);
    }

    [Fact]
    public async Task Snapshot_IDs_werden_korrekt_uebernommen()
    {
        var fachdaten =
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen = Array.Empty<B56Modernisierungsalternative>(),
                Bestandskennwerte = Array.Empty<B56Kennwert>(),
                Bauteile = Array.Empty<B56Bauteil>()
            };

        var register =
            new ImportRegisterFake(fachdaten, fachdaten);

        var service =
            new B56SnapshotVergleichService(register);

        var projektId = Guid.NewGuid();
        var altId = Guid.NewGuid();
        var neuId = Guid.NewGuid();

        var aktionsErgebnis =
            await service.VergleichenAsync(projektId, altId, neuId);

        Assert.Equal(altId, aktionsErgebnis.Ergebnis!.AltSnapshotId);
        Assert.Equal(neuId, aktionsErgebnis.Ergebnis!.NeuSnapshotId);
    }

    private static B56ImportPipelineErgebnis ErstelleFachdaten(
        double primaerenergiebedarfBestand,
        double primaerenergiebedarfAlternative,
        double uWert)
    {
>>>>>>> origin/main
        return new B56ImportPipelineErgebnis
        {
            Bestandskennwerte =
            [
                new B56Kennwert
                {
<<<<<<< HEAD
                    Name = kennwertName,
                    Wert = kennwertWert
=======
                    Name = "Primärenergiebedarf Gebäude",
                    Einheit = "kWh/(m²a)",
                    Wert = primaerenergiebedarfBestand
                }
            ],
            Modernisierungsalternativen =
            [
                new B56Modernisierungsalternative
                {
                    Position = 1,
                    Bezeichnung = "Gesamtpaket",
                    Kennwerte =
                    [
                        new B56Kennwert
                        {
                            Name = "Primärenergiebedarf Gebäude",
                            Einheit = "kWh/(m²a)",
                            Wert = primaerenergiebedarfAlternative
                        }
                    ]
>>>>>>> origin/main
                }
            ],
            Bauteile =
            [
                new B56Bauteil
                {
<<<<<<< HEAD
                    Bauteilcode = bauteilcode,
                    Bezeichnung = "Außenwand",
                    UWert = 0.24
                }
            ],
            Modernisierungsalternativen = alternativen
        };
    }
=======
                    Bauteilcode = "AW01",
                    Bezeichnung = "Außenwand",
                    UWert = uWert
                }
            ]
        };
    }

    private sealed class ImportRegisterFake(
        B56ImportPipelineErgebnis? altFachdaten,
        B56ImportPipelineErgebnis? neuFachdaten) : IB56ImportRegister
    {
        private int _abrufIndex;

        public Task<B56ImportPipelineErgebnis?> FachdatenAbrufenAsync(
            Guid projektId,
            Guid importId,
            CancellationToken cancellationToken = default)
        {
            var ergebnis =
                _abrufIndex == 0
                    ? altFachdaten
                    : neuFachdaten;

            _abrufIndex++;

            return Task.FromResult(ergebnis);
        }

        public Task<B56ImportEintrag?> NachHashSuchenAsync(
            Guid projektId,
            string sha256,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<B56ImportEintrag?> NachIdSuchenAsync(
            Guid projektId,
            Guid importId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task EintragSpeichernAsync(
            B56ImportEintrag eintrag,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task EintragMitFachdatenSpeichernAsync(
            B56ImportEintrag eintrag,
            B56ImportPipelineErgebnis fachdaten,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task LebenszyklusSpeichernAsync(
            B56ImportEintrag eintrag,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<B56ImportEintrag>> AlleFuerProjektAbrufenAsync(
            Guid projektId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
>>>>>>> origin/main
}
