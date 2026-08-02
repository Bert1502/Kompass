using Kompass.Domain.Fachdaten;
using Kompass.Domain.Massnahmen;
using Kompass.Domain.Regelwerke;

namespace Kompass.Tests;

public sealed class FachdatenkatalogDomainTests
{
    [Fact]
    public void Regelwerk_ohne_Quelle_kann_nicht_freigegeben_werden()
    {
        var regelwerk = new Regelwerk(Guid.NewGuid(), "GEG", 1, "Gebäudeenergiegesetz", "2026", new DateOnly(2026, 1, 1));
        Assert.Throws<Kompass.Domain.Common.DomainException>(() => regelwerk.Freigeben());
    }

    [Fact]
    public void Numerische_Anforderung_benoetigt_Operator_und_Einheit()
    {
        Assert.Throws<Kompass.Domain.Common.DomainException>(() =>
            new Regelwerksanforderung(Guid.NewGuid(), "A-1", "GRENZWERT", "U-Wert", new DateOnly(2026, 1, 1), grenzwert: 0.24m));
    }

    [Fact]
    public void Massnahmenkatalogeintrag_startet_als_Entwurf()
    {
        var eintrag = new Massnahmenkatalogeintrag(Guid.NewGuid(), "M-AW", 1, "Außenwanddämmung", Guid.NewGuid(), "m²", new DateOnly(2026, 1, 1));
        Assert.Equal(FachdatenStatus.Entwurf, eintrag.Status);
    }
}
