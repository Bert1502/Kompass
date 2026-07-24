using Kompass.Application.B56Import;
using Kompass.Persistence.Data;
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

        Assert.Equal(
            neuererEintrag.ImportId,
            nachHash?.ImportId);

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

        Assert.Equal(
            "Fenster",
            Assert.Single(
                    fachdaten.Modernisierungsalternativen)
                .Bezeichnung);

        Assert.Null(
            fremdeFachdaten);
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
                    "Investitionskosten",
                Wert =
                    20000
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
}
