using Kompass.Application.B56Import;
using Kompass.Persistence.Data;
using Kompass.Persistence.Data.Entities;
using Kompass.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests.Persistence;

public sealed class EfB56ImportRegisterTests
{
    [Fact]
    public async Task Abfragen_sortieren_DateTimeOffset_mit_SQLite_korrekt()
    {
        await using var verbindung =
            new SqliteConnection(
                "Data Source=:memory:");

        await verbindung.OpenAsync();

        var options =
            new DbContextOptionsBuilder<KompassDbContext>()
                .UseSqlite(verbindung)
                .Options;

        await using var context =
            new KompassDbContext(options);

        await context.Database.MigrateAsync();

        var register =
            new EfB56ImportRegister(context);

        var projektId =
            Guid.NewGuid();

        var aeltererEintrag =
            ErzeugeEintrag(
                projektId,
                DateTimeOffset.Parse(
                    "2026-07-23T10:00:00+02:00"));

        var neuererEintrag =
            ErzeugeEintrag(
                projektId,
                DateTimeOffset.Parse(
                    "2026-07-24T10:00:00+02:00"));

        await register.EintragSpeichernAsync(
            aeltererEintrag);

        await register.EintragMitFachdatenSpeichernAsync(
            neuererEintrag,
            ErzeugeFachdaten());

        var nachHash =
            await register.NachHashSuchenAsync(
                projektId,
                aeltererEintrag.Sha256);

        var alle =
            await register.AlleFuerProjektAbrufenAsync(
                projektId);

        var fachdaten =
            await register.FachdatenAbrufenAsync(
                projektId,
                neuererEintrag.ImportId);

        var fremdeFachdaten =
            await register.FachdatenAbrufenAsync(
                Guid.NewGuid(),
                neuererEintrag.ImportId);

        var bestaetigungszeitpunkt =
            DateTimeOffset.Parse(
                "2026-07-25T09:00:00Z");

        await register.LebenszyklusSpeichernAsync(
            neuererEintrag with
            {
                SnapshotStatus =
                    B56SnapshotStatus.FachlichBestaetigt,
                BestaetigtAm =
                    bestaetigungszeitpunkt
            });

        var bestaetigterSnapshot =
            await register.NachIdSuchenAsync(
                projektId,
                neuererEintrag.ImportId);

        Assert.Equal(
            neuererEintrag.ImportId,
            nachHash?.ImportId);

        Assert.All(
            alle,
            eintrag =>
            {
                Assert.Equal(
                    B56SnapshotVersionen.AktuelleSchemaVersion,
                    eintrag.SnapshotSchemaVersion);
                Assert.Equal(
                    B56SnapshotVersionen.AktuelleParserVersion,
                    eintrag.ParserVersion);
                Assert.Equal(
                    B56SnapshotStatus.TechnischGeprueft,
                    eintrag.SnapshotStatus);
            });

        Assert.Equal(
            [
                neuererEintrag.ImportId,
                aeltererEintrag.ImportId
            ],
            alle.Select(x => x.ImportId));

        Assert.NotNull(
            fachdaten);

        Assert.Equal(
            "AW01",
            Assert.Single(
                    fachdaten.Bauteile)
                .Bauteilcode);

        var alternative =
            Assert.Single(
                fachdaten.Modernisierungsalternativen);

        Assert.Equal(
            "Fenster",
            alternative.Bezeichnung);
        Assert.Equal(
            1,
            alternative.Position);

        Assert.Null(
            fremdeFachdaten);

        Assert.Equal(
            B56SnapshotStatus.FachlichBestaetigt,
            bestaetigterSnapshot?.SnapshotStatus);
        Assert.Equal(
            bestaetigungszeitpunkt,
            bestaetigterSnapshot?.BestaetigtAm);
    }

    [Fact]
    public async Task Unbekannte_Schemaversion_wird_kontrolliert_abgelehnt()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var entity =
            ErzeugeSnapshotEntity(
                snapshotSchemaVersion: 999,
                fachdatenJson: "{}");

        testdatenbank.Context.B56ImportEintraege.Add(
            entity);

        await testdatenbank.Context.SaveChangesAsync();

        var register =
            new EfB56ImportRegister(
                testdatenbank.Context);

        var exception =
            await Assert.ThrowsAsync<B56SnapshotFormatException>(
                () => register.FachdatenAbrufenAsync(
                    entity.ProjektId,
                    entity.ImportId));

        Assert.Equal(
            entity.ImportId,
            exception.ImportId);
        Assert.Contains(
            "999",
            exception.Message);
    }

    [Fact]
    public async Task Schema_Eins_Snapshot_bleibt_nach_Erweiterung_lesbar()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var entity =
            ErzeugeSnapshotEntity(
                snapshotSchemaVersion: 1,
                fachdatenJson: "{}");

        testdatenbank.Context.B56ImportEintraege.Add(entity);
        await testdatenbank.Context.SaveChangesAsync();

        var register =
            new EfB56ImportRegister(testdatenbank.Context);

        var fachdaten =
            await register.FachdatenAbrufenAsync(
                entity.ProjektId,
                entity.ImportId);

        Assert.NotNull(fachdaten);
        Assert.Null(fachdaten.EffizienzstandardKontrollwert);
    }

    [Fact]
    public async Task Beschaedigte_Fachdaten_werden_kontrolliert_abgelehnt()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var entity =
            ErzeugeSnapshotEntity(
                B56SnapshotVersionen.AktuelleSchemaVersion,
                "{ungueltig");

        testdatenbank.Context.B56ImportEintraege.Add(
            entity);

        await testdatenbank.Context.SaveChangesAsync();

        var register =
            new EfB56ImportRegister(
                testdatenbank.Context);

        var exception =
            await Assert.ThrowsAsync<B56SnapshotFormatException>(
                () => register.FachdatenAbrufenAsync(
                    entity.ProjektId,
                    entity.ImportId));

        Assert.Equal(
            entity.ImportId,
            exception.ImportId);
        Assert.IsType<System.Text.Json.JsonException>(
            exception.InnerException);
    }

    [Fact]
    public async Task Vergleichsergebnis_kann_persistiert_und_gelesen_werden()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var register =
            new EfB56ImportRegister(
                testdatenbank.Context);

        var vergleich =
            new B56SnapshotVergleich
            {
                ProjektId = Guid.NewGuid(),
                VorgaengerSnapshotId = Guid.NewGuid(),
                NachfolgerSnapshotId = Guid.NewGuid(),
                BestandskennwertVergleiche =
                [
                    new B56KennwertVergleich(
                        "Primärenergiebedarf",
                        "kWh/(m²a)",
                        200,
                        180,
                        B56VergleichsAenderung.Geaendert)
                ],
                Konflikte =
                [
                    new B56Vergleichskonflikt(
                        "Bestandskennwert",
                        "Primärenergiebedarf",
                        "Wert",
                        B56VergleichsAenderung.Geaendert)
                ]
            };

        await register.VergleichSpeichernAsync(
            vergleich);

        var gespeichert =
            await register.VergleichAbrufenAsync(
                vergleich.ProjektId,
                vergleich.VorgaengerSnapshotId,
                vergleich.NachfolgerSnapshotId);

        Assert.NotNull(gespeichert);
        Assert.True(
            gespeichert!.HatAenderungen);
        Assert.Single(
            gespeichert.Konflikte);
    }

    private static B56ImportPipelineErgebnis ErzeugeFachdaten()
    {
        var alternative =
            new B56Modernisierungsalternative
            {
                Bezeichnung =
                    "Fenster",
                Beschreibung =
                    "Fenstertausch"
            };

        alternative.Kennwerte.Add(
            new B56Kennwert
            {
                Name =
                    "Primärenergiebedarf Gebäude",
                Wert =
                    150
            });

        return new B56ImportPipelineErgebnis
        {
            ImportierteArbeitsblaetter = 1,
            ErkannteTabellen = 3,
            ImportierteTabellen = 3,
            ImportierteBauteile = 1,
            ImportierteKennwerte = 2,
            ImportierteModernisierungsalternativen = 1,
            Bauteile =
            [
                new B56Bauteil
                {
                    Bauteilcode =
                        "AW01",
                    Bezeichnung =
                        "Außenwand",
                    Nachbarseite =
                        "gegen Außenluft",
                    UWert =
                        0.24
                }
            ],
            Bestandskennwerte =
            [
                new B56Kennwert
                {
                    Name =
                        "Primärenergiebedarf Gebäude",
                    Wert =
                        200
                }
            ],
            Modernisierungsalternativen =
            [
                alternative
            ]
        };
    }

    private static B56ImportEintrag ErzeugeEintrag(
        Guid projektId,
        DateTimeOffset importiertAm)
    {
        return new B56ImportEintrag
        {
            ImportId = Guid.NewGuid(),
            ProjektId = projektId,
            Projektname = "Testprojekt",
            Originaldateiname = "b56.xlsx",
            Archivdateipfad = "archiv/b56.xlsx",
            Sha256 = "0123456789abcdef",
            DateigroesseBytes = 4,
            ImportiertAm = importiertAm,
            Dateiendung = ".xlsx"
        };
    }

    private static B56ImportEintragEntity ErzeugeSnapshotEntity(
        int snapshotSchemaVersion,
        string fachdatenJson)
    {
        return new B56ImportEintragEntity
        {
            ImportId = Guid.NewGuid(),
            ProjektId = Guid.NewGuid(),
            Projektname = "Testprojekt",
            Originaldateiname = "b56.xlsx",
            Archivdateipfad = "archiv/b56.xlsx",
            Sha256 = new string('a', 64),
            DateigroesseBytes = 4,
            ImportiertAm = DateTimeOffset.UtcNow,
            Dateiendung = ".xlsx",
            FachdatenJson = fachdatenJson,
            SnapshotSchemaVersion = snapshotSchemaVersion,
            ParserVersion =
                B56SnapshotVersionen.AktuelleParserVersion
        };
    }
}
