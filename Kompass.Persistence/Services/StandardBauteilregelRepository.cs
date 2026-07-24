using Kompass.Application.B56Import;

namespace Kompass.Persistence.Services;

public sealed class StandardBauteilregelRepository
    : IB56BauteilregelRepository
{
    private static readonly IReadOnlyList<B56Bauteilregel> Regeln =
    [
        new()
        {
            Suchbegriff = "Fenster",
            Kategorie = "Fenster",
            Prioritaet = 100
        },
        new()
        {
            Suchbegriff = "Außenwand",
            Kategorie = "Außenwand",
            Prioritaet = 100
        },
        new()
        {
            Suchbegriff = "Dach",
            Kategorie = "Dach",
            Prioritaet = 100
        },
        new()
        {
            Suchbegriff = "Kellerdecke",
            Kategorie = "Kellerdecke",
            Prioritaet = 90
        },
        new()
        {
            Suchbegriff = "Bodenplatte",
            Kategorie = "Bodenplatte",
            Prioritaet = 90
        },
        new()
        {
            Suchbegriff = "Tür",
            Kategorie = "Tür",
            Prioritaet = 80
        }
    ];

    public IReadOnlyList<B56Bauteilregel> Laden()
        => Regeln;
}
