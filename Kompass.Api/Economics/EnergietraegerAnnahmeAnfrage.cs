using Kompass.Domain.Economics;

namespace Kompass.Api.Economics;

public sealed class EnergietraegerAnnahmeAnfrage
{
    public Energietraeger Energietraeger { get; init; }

    /// <summary>Energiepreis in €/kWh.</summary>
    public decimal Preis { get; init; }

    /// <summary>Jährliche Preissteigerungsrate als Dezimalzahl (z. B. 0,03 = 3 %).</summary>
    public decimal Preissteigerungsrate { get; init; }

    /// <summary>CO₂-Faktor in kg CO₂/kWh.</summary>
    public decimal Co2Faktor { get; init; }

    /// <summary>CO₂-Preis in €/t CO₂.</summary>
    public decimal Co2Preis { get; init; }

    /// <summary>Jährliche CO₂-Preissteigerungsrate als Dezimalzahl.</summary>
    public decimal Co2Preissteigerungsrate { get; init; }

    /// <summary>Endenergiebedarf im Ist-Zustand in kWh/a.</summary>
    public decimal EndenergieIstZustand { get; init; }

    /// <summary>Endenergiebedarf nach Umsetzung der Alternative in kWh/a.</summary>
    public decimal EndenergieAlternative { get; init; }
}
