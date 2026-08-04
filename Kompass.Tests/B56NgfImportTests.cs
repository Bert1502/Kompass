using Kompass.Application.B56Import;
using Kompass.Persistence.Services;

namespace Kompass.Tests;

public sealed class B56NgfImportTests
{
    [Fact]
    public async Task Nettogrundflaeche_wird_als_Ngf_Bestandskennwert_importiert()
    {
        var mappe = new B56Arbeitsmappe
        {
            Arbeitsblaetter =
            [
                new B56Arbeitsblatt
                {
                    Name = "SCModernisierungen",
                    Zeilen =
                    [
                        Zeile(1, ("A", "Bestand")),
                        Zeile(2, ("B", "Nettogrundfl\u00e4che"), ("C", "1250.5"))
                    ]
                }
            ]
        };
        var service = new B56TabellenImportService(new B56TabellenFinder());

        var ergebnis = await service.ImportierenAsync(new B56ImportKontext
        {
            Arbeitsmappe = mappe,
            ImportId = Guid.NewGuid(),
            ProjektId = Guid.NewGuid()
        });

        var ngf = Assert.Single(ergebnis.Bestandskennwerte, x => x.Name == "NGF");
        Assert.Equal(1250.5, ngf.Wert);
        Assert.Equal("[m\u00b2]", ngf.Einheit);
    }

    private static B56Zeile Zeile(int nummer, params (string Spalte, string Wert)[] werte) => new()
    {
        Zeilennummer = nummer,
        Zellen = werte.Select(x => new B56Zelle { Spalte = x.Spalte, Wert = x.Wert }).ToArray()
    };
}
