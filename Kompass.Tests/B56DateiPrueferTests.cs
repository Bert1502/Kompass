using Kompass.Application.B56Import;
using Kompass.Persistence.B56Import;
using Microsoft.Extensions.Options;

namespace Kompass.Tests.B56Import;

public sealed class B56DateiPrueferTests
{
    private readonly B56DateiPruefer _dateiPruefer =
        new(
            Options.Create(
                new B56ImportOptionen()));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fehlender_Dateipfad_wird_abgelehnt(
        string dateipfad)
    {
        var ergebnis =
            _dateiPruefer.Pruefen(dateipfad);

        Assert.False(ergebnis.IstGueltig);
        Assert.Equal(
            "B56-DATEIPFAD-FEHLT",
            ergebnis.Fehlercode);
    }

    [Fact]
    public void Nicht_vorhandene_Datei_wird_abgelehnt()
    {
        var dateipfad =
            Path.Combine(
                Path.GetTempPath(),
                $"nicht-vorhanden-{Guid.NewGuid():N}.xlsx");

        var ergebnis =
            _dateiPruefer.Pruefen(dateipfad);

        Assert.False(ergebnis.IstGueltig);
        Assert.Equal(
            "B56-DATEI-NICHT-GEFUNDEN",
            ergebnis.Fehlercode);
    }

    [Fact]
    public void Leere_Xlsx_Datei_wird_abgelehnt()
    {
        var dateipfad =
            Path.Combine(
                Path.GetTempPath(),
                $"leere-b56-{Guid.NewGuid():N}.xlsx");

        try
        {
            using (File.Create(dateipfad))
            {
            }

            var ergebnis =
                _dateiPruefer.Pruefen(dateipfad);

            Assert.False(ergebnis.IstGueltig);
            Assert.Equal(
                "B56-DATEI-LEER",
                ergebnis.Fehlercode);
        }
        finally
        {
            if (File.Exists(dateipfad))
            {
                File.Delete(dateipfad);
            }
        }
    }
}
