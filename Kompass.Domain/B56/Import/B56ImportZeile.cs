using Kompass.Domain.Common;

namespace Kompass.Domain.B56.Import;

public sealed class B56ImportZeile : Entity
{
    private B56ImportZeile()
    {
        Bauteilcode = string.Empty;
        Bezeichnung = string.Empty;
    }

    public B56ImportZeile(
        Guid id,
        string bauteilcode,
        string bezeichnung)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(bauteilcode))
        {
            throw new DomainException(
                "Bauteilcode darf nicht leer sein.");
        }

        if (string.IsNullOrWhiteSpace(bezeichnung))
        {
            throw new DomainException(
                "Bezeichnung darf nicht leer sein.");
        }

        Bauteilcode = bauteilcode.Trim();
        Bezeichnung = bezeichnung.Trim();
    }

    public string Bauteilcode { get; private set; }

    public string Bezeichnung { get; private set; }
}