using Kompass.Domain.Common;

namespace Kompass.Domain.Economics;

/// <summary>
/// Annahmen für einen Energieträger innerhalb einer Wirtschaftlichkeitsberechnung.
/// </summary>
public sealed class EnergietraegerAnnahme : Entity
{
    private EnergietraegerAnnahme()
    {
    }

    public EnergietraegerAnnahme(
        Guid id,
        Energietraeger energietraeger,
        decimal preisProKwh,
        decimal jaehrlicherPreisanstiegProzent)
        : base(id)
    {
        if (preisProKwh < 0)
        {
            throw new DomainException(
                "Der Energiepreis darf nicht negativ sein.");
        }

        if (jaehrlicherPreisanstiegProzent < -100)
        {
            throw new DomainException(
                "Der jährliche Preisanstieg darf nicht unter -100 % liegen.");
        }

        Energietraeger = energietraeger;
        PreisProKwh = preisProKwh;
        JaehrlicherPreisanstiegProzent = jaehrlicherPreisanstiegProzent;
    }

    public Energietraeger Energietraeger { get; private set; }

    /// <summary>Energiepreis in EUR/kWh.</summary>
    public decimal PreisProKwh { get; private set; }

    /// <summary>Jährliche Preissteigerung in Prozent.</summary>
    public decimal JaehrlicherPreisanstiegProzent { get; private set; }

    public void PreisAendern(
        decimal preisProKwh,
        decimal jaehrlicherPreisanstiegProzent)
    {
        if (preisProKwh < 0)
        {
            throw new DomainException(
                "Der Energiepreis darf nicht negativ sein.");
        }

        if (jaehrlicherPreisanstiegProzent < -100)
        {
            throw new DomainException(
                "Der jährliche Preisanstieg darf nicht unter -100 % liegen.");
        }

        PreisProKwh = preisProKwh;
        JaehrlicherPreisanstiegProzent = jaehrlicherPreisanstiegProzent;
    }
}
