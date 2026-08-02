namespace Kompass.Desktop.Models;

public sealed record FachdatenbankPruefergebnisDto(
    string Dateiname,
    string SchemaVersion,
    IReadOnlyDictionary<string, int> Zeilen,
    IReadOnlyList<string> Fehler,
    IReadOnlyList<string> Warnungen)
{
    public bool IstGueltig => Fehler.Count == 0;
}

public sealed record FachdatenimportErgebnisDto(
    IReadOnlyList<FachdatenbankPruefergebnisDto> Datenbanken,
    int AngelegteStammdaten,
    int AngelegteKategorien,
    int AngelegteMassnahmen,
    bool DryRun)
{
    public bool IstGueltig => Datenbanken.All(x => x.IstGueltig);
    public int AngelegteDatensaetze => AngelegteStammdaten + AngelegteKategorien + AngelegteMassnahmen;
}
