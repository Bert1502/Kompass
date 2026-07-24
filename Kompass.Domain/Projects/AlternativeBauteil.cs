using Kompass.Domain.B56;
using Kompass.Domain.Common;

namespace Kompass.Domain.Projects;

public sealed class AlternativeBauteil : Entity
{
private AlternativeBauteil()
{
    Bauteilcode = null!;
    Bemerkung = string.Empty;
}

    public AlternativeBauteil(
        Guid id,
        Bauteilcode bauteilcode,
        string bemerkung = "")
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(bauteilcode);

        Bauteilcode = bauteilcode;
        Bemerkung = bemerkung?.Trim() ?? string.Empty;
    }

    public Bauteilcode Bauteilcode { get; private set; }

    public string Bemerkung { get; private set; }

    public void BemerkungAendern(string bemerkung)
    {
        Bemerkung = bemerkung?.Trim() ?? string.Empty;
    }
}