using Kompass.Application.Funding;
using Kompass.Domain.Funding;
using Kompass.Domain.Projects;
using Kompass.Persistence.Data;
using Kompass.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests;

public sealed class FoerdervoraussetzungenTests
{
    [Theory]
    [InlineData(400, 100, WpbPruefstatus.RechnerischErfuellt)]
    [InlineData(399, 100, WpbPruefstatus.RechnerischNichtErfuellt)]
    [InlineData(400, null, WpbPruefstatus.Unvollstaendig)]
    public void Wpb_Vorschlag_verwendet_Schwelle_vier(int qp, int? qpRef, WpbPruefstatus erwartet)
    {
        var v = new Foerdervoraussetzungen(Guid.NewGuid(), Guid.NewGuid());
        v.B56BestandswerteUebernehmen(1000, qp);
        v.Aktualisieren(1970, new DateOnly(1970, 1, 1), FoerderGebaeudeart.Nichtwohngebaeude,
            FoerderNutzung.Selbstgenutzt, null, Antragstellerart.Kommune, true, false, false, false,
            false, false, false, false, true, "Energieausweis", qpRef, qpRef.HasValue ? "B56-Referenzgebäude" : null, false);

        Assert.Equal(erwartet, v.WpbRechnerischerVorschlag);
        if (qpRef.HasValue) Assert.Equal((decimal)qp / qpRef.Value, v.WpbVerhaeltnis);
    }

    [Fact]
    public async Task Service_speichert_manuelle_Angaben_getrennt_von_B56_Werten()
    {
        await using var verbindung = new SqliteConnection("Data Source=:memory:");
        await verbindung.OpenAsync();
        var optionen = new DbContextOptionsBuilder<KompassDbContext>().UseSqlite(verbindung).Options;
        await using var db = new KompassDbContext(optionen);
        await db.Database.MigrateAsync();
        var projekt = new Projekt(Guid.NewGuid(), "Schule");
        db.Projekte.Add(projekt);
        await db.SaveChangesAsync();
        var service = new EfFoerdervoraussetzungenService(db);

        var wert = await service.SpeichernAsync(projekt.Id, new FoerdervoraussetzungenEingabe(
            1965, new DateOnly(1966, 1, 1), FoerderGebaeudeart.Nichtwohngebaeude, FoerderNutzung.Gemischt,
            null, Antragstellerart.Kommune, true, true, false, true, false, false, true, true, true,
            "Energieausweis; Fachunternehmererklärung", 100m, "GEG-Nachweis", true));

        Assert.NotNull(wert);
        Assert.Equal(1965, wert.Baujahr);
        Assert.Equal(100m, wert.QpReferenz);
        Assert.Null(wert.Nettogrundflaeche);
    }
}
