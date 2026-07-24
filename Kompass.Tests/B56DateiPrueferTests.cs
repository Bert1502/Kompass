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

    [Fact]
    public void Nicht_unterstuetzte_Dateiendung_wird_abgelehnt()
    {
        var ergebnis =
            PruefeTemporareDatei(
                ".txt",
                [1]);

        Assert.False(ergebnis.IstGueltig);
        Assert.Equal(
            "B56-DATEIFORMAT-NICHT-UNTERSTUETZT",
            ergebnis.Fehlercode);
    }

    [Fact]
    public void Zu_grosse_Datei_wird_abgelehnt()
    {
        var ergebnis =
            PruefeTemporareDatei(
                ".xlsx",
                [1, 2, 3, 4, 5],
                maximaleDateigroesseBytes: 4);

        Assert.False(ergebnis.IstGueltig);
        Assert.Equal(
            "B56-DATEI-ZU-GROSS",
            ergebnis.Fehlercode);
    }

    [Fact]
    public void Ungueltige_OpenXml_Signatur_wird_abgelehnt()
    {
        var ergebnis =
            PruefeTemporareDatei(
                ".xlsx",
                [1, 2, 3, 4]);

        Assert.False(ergebnis.IstGueltig);
        Assert.Equal(
            "B56-DATEI-KEINE-GUELTIGE-EXCEL-DATEI",
            ergebnis.Fehlercode);
    }

    [Fact]
    public void OpenXml_Zip_Signatur_wird_akzeptiert()
    {
        var ergebnis =
            PruefeTemporareDatei(
                ".XLSM",
                [0x50, 0x4B, 0x03, 0x04]);

        Assert.True(ergebnis.IstGueltig);
        Assert.Equal(
            ".xlsm",
            ergebnis.Dateiendung);
        Assert.Equal(
            4,
            ergebnis.DateigroesseBytes);
    }

    private static B56DateiPruefung PruefeTemporareDatei(
        string dateiendung,
        byte[] inhalt,
        long maximaleDateigroesseBytes = 1024)
    {
        var dateipfad =
            Path.Combine(
                Path.GetTempPath(),
                $"b56-pruefung-{Guid.NewGuid():N}{dateiendung}");

        try
        {
            File.WriteAllBytes(
                dateipfad,
                inhalt);

            var dateiPruefer =
                new B56DateiPruefer(
                    Options.Create(
                        new B56ImportOptionen
                        {
                            MaximaleDateigroesseBytes =
                                maximaleDateigroesseBytes
                        }));

            return dateiPruefer.Pruefen(
                dateipfad);
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
