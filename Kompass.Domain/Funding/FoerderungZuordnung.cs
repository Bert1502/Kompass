using Kompass.Domain.Common;

namespace Kompass.Domain.Funding;

public sealed class FoerderungZuordnung : Entity
{
    private FoerderungZuordnung()
    {
    }

    public FoerderungZuordnung(
        Guid id,
        Guid modernisierungsalternativeId,
        Guid foerderprogrammId)
        : base(id)
    {
        if (modernisierungsalternativeId == Guid.Empty)
        {
            throw new DomainException(
                "Die Modernisierungsalternative muss angegeben werden.");
        }

        if (foerderprogrammId == Guid.Empty)
        {
            throw new DomainException(
                "Das Förderprogramm muss angegeben werden.");
        }

        ModernisierungsalternativeId = modernisierungsalternativeId;
        FoerderprogrammId = foerderprogrammId;
    }

    public Guid ModernisierungsalternativeId { get; private set; }

    public Guid FoerderprogrammId { get; private set; }
}
