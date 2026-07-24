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

        await register.EintragSpeichernAsync(
            neuererEintrag);

        var nachHash =
            await register.NachHashSuchenAsync(
                projektId,
                aeltererEintrag.Sha256);

        var alle =
            await register.AlleFuerProjektAbrufenAsync(
                projektId);

        Assert.Equal(
            neuererEintrag.ImportId,
            nachHash?.ImportId);

        Assert.Equal(
            [
                neuererEintrag.ImportId,
                aeltererEintrag.ImportId
            ],
            alle.Select(x => x.ImportId));
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
