using Kompass.Domain.Common;

namespace Kompass.Domain.B56;

public sealed class Bauteilcode : Entity
{
    private Bauteilcode()
    {
    }

    public Bauteilcode(
        Guid id,
        string code,
        string bezeichnung)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException(
                "Der B56-Bauteilcode darf nicht leer sein.");
        }

        Code = code.Trim();
        Bezeichnung = bezeichnung?.Trim() ?? string.Empty;
    }

    public string Code { get; private set; } = string.Empty;

    public string Bezeichnung { get; private set; } = string.Empty;

    public void BezeichnungAendern(string bezeichnung)
    {
        Bezeichnung = bezeichnung?.Trim() ?? string.Empty;
    }

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Bezeichnung)
            ? Code
            : $"{Code} – {Bezeichnung}";
    }
}