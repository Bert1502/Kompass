using Kompass.Domain.Economics;

namespace Kompass.Desktop.Models;

public sealed record WirtschaftlichkeitsberichtDto(
    WirtschaftlichkeitsberichtKopfDto Kopf,
    IReadOnlyList<WirtschaftlichkeitsberichtZeileDto> Alternativen);

public sealed record WirtschaftlichkeitsberichtKopfDto(
    Guid ProjektId,
    string ProjektName);

public sealed record WirtschaftlichkeitsberichtZeileDto(
    Guid AlternativeId,
    int? B56Position,
    string Bezeichnung,
    WirtschaftlichkeitsBasis Basis,
    decimal Investitionskosten,
    decimal Foerderung,
    int Betrachtungszeitraum,
    decimal Diskontsatz,
    decimal Inflationsrate,
    WirtschaftlichkeitsergebnisDto Ergebnis)
{
    public string DiskontsatzText =>
        $"{Diskontsatz:P1}";

    public string InflationsrateText =>
        $"{Inflationsrate:P1}";

    public string InvestitionskostenText =>
        $"{Investitionskosten:N0} €";

    public string FoerderungText =>
        $"{Foerderung:N0} €";
}

public sealed record WirtschaftlichkeitsergebnisDto(
    decimal Eigenanteil,
    decimal JaehrlicheEnergiekosteneinsparungJahr1,
    decimal KumulierteEnergiekosteneinsparung,
    decimal? AmortisationsdauerStatisch,
    decimal? AmortisationsdauerDynamisch,
    decimal Kapitalwert,
    decimal? KostenNutzenVerhaeltnis)
{
    public string EigenanteilText =>
        $"{Eigenanteil:N0} €";

    public string JaehrlicheEinsparungText =>
        $"{JaehrlicheEnergiekosteneinsparungJahr1:N0} €/a";

    public string KumulierteEinsparungText =>
        $"{KumulierteEnergiekosteneinsparung:N0} €";

    public string AmortisationStatischText =>
        AmortisationsdauerStatisch.HasValue
            ? $"{AmortisationsdauerStatisch.Value:N1} a"
            : "–";

    public string AmortisationDynamischText =>
        AmortisationsdauerDynamisch.HasValue
            ? $"{AmortisationsdauerDynamisch.Value:N0} a"
            : "–";

    public string KapitalwertText =>
        $"{Kapitalwert:N0} €";

    public string KostenNutzenText =>
        KostenNutzenVerhaeltnis.HasValue
            ? $"{KostenNutzenVerhaeltnis.Value:N2}"
            : "–";
}
