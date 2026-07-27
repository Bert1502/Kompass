using Kompass.Application.B56Import;

namespace Kompass.Tests.B56Import;

public sealed class B56SnapshotVergleichServiceTests
{
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
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen =
                [
                    new B56Modernisierungsalternative
                    {
                        Position = 1,
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
            alternative.NeueBezeichnung);
    }

    [Fact]
    public async Task Entfernte_Alternative_wird_als_Entfernt_erkannt()
    {
        var altFachdaten =
            new B56ImportPipelineErgebnis
            {
                Modernisierungsalternativen =
                [
                    new B56Modernisierungsalternative
                    {
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
        return new B56ImportPipelineErgebnis
        {
            Bestandskennwerte =
            [
                new B56Kennwert
                {
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
                }
            ],
            Bauteile =
            [
                new B56Bauteil
                {
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
}
