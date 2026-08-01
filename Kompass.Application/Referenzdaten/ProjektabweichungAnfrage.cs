namespace Kompass.Application.Referenzdaten;

public sealed record ProjektabweichungAnfrage(
    Guid ProjektId,
    string Parameterart,
    string VerwendeterProjektwert,
    string Begruendung,
    string Benutzer,
    string? Bezugsgroesse = null,
    string? EnergietraegerOderKategorie = null);
