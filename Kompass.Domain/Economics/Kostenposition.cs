using Kompass.Domain.Common;

namespace Kompass.Domain.Economics;

public sealed class Kostenposition : Entity
{
    private Kostenposition()
    {
        Bezeichnung = string.Empty;
    }

    public Kostenposition(
        Guid id,
        string bezeichnung,
        decimal betrag,
        Kostenart kostenart)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(bezeichnung))
        {
            throw new DomainException(
                "Die Bezeichnung darf nicht leer sein.");
        }

        if (betrag < 0)
        {
            throw new DomainException(
                "Der Betrag darf nicht negativ sein.");
        }

        Bezeichnung = bezeichnung.Trim();
        Betrag = betrag;
        Kostenart = kostenart;
    }

    public string Bezeichnung { get; private set; }

    public decimal Betrag { get; private set; }

    public Kostenart Kostenart { get; private set; }

    public void BetragAendern(
        decimal betrag)
    {
        if (betrag < 0)
        {
            throw new DomainException(
                "Der Betrag darf nicht negativ sein.");
        }

        Betrag = betrag;
    }

    public void KostenartAendern(
        Kostenart kostenart)
    {
        Kostenart = kostenart;
    }
}