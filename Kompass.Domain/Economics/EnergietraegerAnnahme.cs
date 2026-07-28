using Kompass.Domain.Common;

namespace Kompass.Domain.Economics;

<<<<<<< HEAD
/// <summary>
/// Annahmen für einen Energieträger innerhalb einer Wirtschaftlichkeitsberechnung.
/// </summary>
=======
>>>>>>> origin/main
public sealed class EnergietraegerAnnahme : Entity
{
    private EnergietraegerAnnahme()
    {
    }

    public EnergietraegerAnnahme(
        Guid id,
        Energietraeger energietraeger,
<<<<<<< HEAD
        decimal preisProKwh,
        decimal jaehrlicherPreisanstiegProzent)
        : base(id)
    {
        if (preisProKwh < 0)
=======
        decimal preis,
        decimal preissteigerungsrate,
        decimal co2Faktor,
        decimal co2Preis,
        decimal co2Preissteigerungsrate,
        decimal endenergieIstZustand,
        decimal endenergieAlternative)
        : base(id)
    {
        if (preis < 0)
>>>>>>> origin/main
        {
            throw new DomainException(
                "Der Energiepreis darf nicht negativ sein.");
        }

<<<<<<< HEAD
        if (jaehrlicherPreisanstiegProzent < -100)
        {
            throw new DomainException(
                "Der jährliche Preisanstieg darf nicht unter -100 % liegen.");
        }

        Energietraeger = energietraeger;
        PreisProKwh = preisProKwh;
        JaehrlicherPreisanstiegProzent = jaehrlicherPreisanstiegProzent;
=======
        if (preissteigerungsrate < 0 || preissteigerungsrate > 1)
        {
            throw new DomainException(
                "Die Preissteigerungsrate muss zwischen 0 und 1 liegen.");
        }

        if (co2Faktor < 0)
        {
            throw new DomainException(
                "Der CO₂-Faktor darf nicht negativ sein.");
        }

        if (co2Preis < 0)
        {
            throw new DomainException(
                "Der CO₂-Preis darf nicht negativ sein.");
        }

        if (co2Preissteigerungsrate < 0 || co2Preissteigerungsrate > 1)
        {
            throw new DomainException(
                "Die CO₂-Preissteigerungsrate muss zwischen 0 und 1 liegen.");
        }

        if (endenergieIstZustand < 0)
        {
            throw new DomainException(
                "Der Endenergiebedarf im Ist-Zustand darf nicht negativ sein.");
        }

        if (endenergieAlternative < 0)
        {
            throw new DomainException(
                "Der Endenergiebedarf der Alternative darf nicht negativ sein.");
        }

        Energietraeger = energietraeger;
        Preis = preis;
        Preissteigerungsrate = preissteigerungsrate;
        Co2Faktor = co2Faktor;
        Co2Preis = co2Preis;
        Co2Preissteigerungsrate = co2Preissteigerungsrate;
        EndenergieIstZustand = endenergieIstZustand;
        EndenergieAlternative = endenergieAlternative;
>>>>>>> origin/main
    }

    public Energietraeger Energietraeger { get; private set; }

<<<<<<< HEAD
    /// <summary>Energiepreis in EUR/kWh.</summary>
    public decimal PreisProKwh { get; private set; }

    /// <summary>Jährliche Preissteigerung in Prozent.</summary>
    public decimal JaehrlicherPreisanstiegProzent { get; private set; }

    public void PreisAendern(
        decimal preisProKwh,
        decimal jaehrlicherPreisanstiegProzent)
    {
        if (preisProKwh < 0)
=======
    /// <summary>Energiepreis in €/kWh.</summary>
    public decimal Preis { get; private set; }

    /// <summary>Jährliche Preissteigerungsrate als Dezimalzahl (z. B. 0,03 = 3 %).</summary>
    public decimal Preissteigerungsrate { get; private set; }

    /// <summary>CO₂-Faktor in kg CO₂/kWh.</summary>
    public decimal Co2Faktor { get; private set; }

    /// <summary>CO₂-Preis in €/t CO₂.</summary>
    public decimal Co2Preis { get; private set; }

    /// <summary>Jährliche CO₂-Preissteigerungsrate als Dezimalzahl.</summary>
    public decimal Co2Preissteigerungsrate { get; private set; }

    /// <summary>Endenergiebedarf im Ist-Zustand in kWh/a.</summary>
    public decimal EndenergieIstZustand { get; private set; }

    /// <summary>Endenergiebedarf nach Umsetzung der Alternative in kWh/a.</summary>
    public decimal EndenergieAlternative { get; private set; }

    /// <summary>Endenergieeinsparung in kWh/a.</summary>
    public decimal Einsparung => EndenergieIstZustand - EndenergieAlternative;

    public void Aktualisieren(
        decimal preis,
        decimal preissteigerungsrate,
        decimal co2Faktor,
        decimal co2Preis,
        decimal co2Preissteigerungsrate,
        decimal endenergieIstZustand,
        decimal endenergieAlternative)
    {
        if (preis < 0)
>>>>>>> origin/main
        {
            throw new DomainException(
                "Der Energiepreis darf nicht negativ sein.");
        }

<<<<<<< HEAD
        if (jaehrlicherPreisanstiegProzent < -100)
        {
            throw new DomainException(
                "Der jährliche Preisanstieg darf nicht unter -100 % liegen.");
        }

        PreisProKwh = preisProKwh;
        JaehrlicherPreisanstiegProzent = jaehrlicherPreisanstiegProzent;
=======
        if (preissteigerungsrate < 0 || preissteigerungsrate > 1)
        {
            throw new DomainException(
                "Die Preissteigerungsrate muss zwischen 0 und 1 liegen.");
        }

        if (co2Faktor < 0)
        {
            throw new DomainException(
                "Der CO₂-Faktor darf nicht negativ sein.");
        }

        if (co2Preis < 0)
        {
            throw new DomainException(
                "Der CO₂-Preis darf nicht negativ sein.");
        }

        if (co2Preissteigerungsrate < 0 || co2Preissteigerungsrate > 1)
        {
            throw new DomainException(
                "Die CO₂-Preissteigerungsrate muss zwischen 0 und 1 liegen.");
        }

        if (endenergieIstZustand < 0)
        {
            throw new DomainException(
                "Der Endenergiebedarf im Ist-Zustand darf nicht negativ sein.");
        }

        if (endenergieAlternative < 0)
        {
            throw new DomainException(
                "Der Endenergiebedarf der Alternative darf nicht negativ sein.");
        }

        Preis = preis;
        Preissteigerungsrate = preissteigerungsrate;
        Co2Faktor = co2Faktor;
        Co2Preis = co2Preis;
        Co2Preissteigerungsrate = co2Preissteigerungsrate;
        EndenergieIstZustand = endenergieIstZustand;
        EndenergieAlternative = endenergieAlternative;
>>>>>>> origin/main
    }
}
