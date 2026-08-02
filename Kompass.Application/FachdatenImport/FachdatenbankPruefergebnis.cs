namespace Kompass.Application.FachdatenImport;

public sealed record FachdatenbankPruefergebnis(
    string Dateiname,
    string SchemaVersion,
    IReadOnlyDictionary<string, int> Tabellenzeilen,
    IReadOnlyList<string> Fehler,
    IReadOnlyList<string> Warnungen)
{
    public bool IstGueltig => Fehler.Count == 0;
}

public sealed record FachdatenimportErgebnis(
    IReadOnlyList<FachdatenbankPruefergebnis> Datenbanken,
    int AngelegteStammdaten,
    int AngelegteKategorien,
    int AngelegteMassnahmen,
    bool DryRun)
{
    public bool IstGueltig => Datenbanken.All(x => x.IstGueltig);
}
