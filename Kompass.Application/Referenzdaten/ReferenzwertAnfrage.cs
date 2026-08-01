namespace Kompass.Application.Referenzdaten;

public sealed record ReferenzwertAnfrage(
    string Parameterart,
    DateOnly? Stichtag = null,
    Guid? ProjektId = null,
    Guid? UnternehmenId = null,
    string? Bezugsgroesse = null,
    string? EnergietraegerOderKategorie = null);
