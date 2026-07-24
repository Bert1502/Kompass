using Kompass.Application.B56Import;

namespace Kompass.Persistence.Services;

public sealed class B56BauteilcodeParser
    : IB56BauteilcodeParser
{
    private readonly IReadOnlyList<B56Bauteilregel> _regeln;

    public B56BauteilcodeParser(
        IB56BauteilregelRepository repository)
    {
        _regeln = repository
            .Laden()
            .OrderByDescending(r => r.Prioritaet)
            .ToList();
    }

    public B56Bauteilcode Parsen(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new() { IstGueltig = false };

        foreach (var regel in _regeln)
        {
            var vergleich =
                regel.GrossKleinschreibungBeachten
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

            if (text.Contains(regel.Suchbegriff, vergleich))
            {
                return new()
                {
                    Originaltext = text,
                    Bezeichnung = text,
                    Kategorie = regel.Kategorie,
                    Code = regel.Suchbegriff,
                    IstGueltig = true
                };
            }
        }

        return new()
        {
            Originaltext = text,
            Bezeichnung = text,
            Kategorie = "Unbekannt",
            IstGueltig = false
        };
    }
}
