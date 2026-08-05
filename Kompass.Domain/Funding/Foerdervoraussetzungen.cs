using Kompass.Domain.Common;

namespace Kompass.Domain.Funding;

public enum FoerderGebaeudeart { Wohngebaeude, Nichtwohngebaeude }
public enum FoerderNutzung { Selbstgenutzt, Vermietet, Gemischt }
public enum Antragstellerart { Privatperson, Wohnungseigentuemergemeinschaft, Unternehmen, Kommune, Gemeinnuetzig, Sonstige }
public enum WpbPruefstatus { Unvollstaendig, RechnerischNichtErfuellt, RechnerischErfuellt }

public sealed class Foerdervoraussetzungen : AggregateRoot
{
    private Foerdervoraussetzungen() { QpReferenzQuelle = string.Empty; Nachweise = string.Empty; }

    public Foerdervoraussetzungen(Guid id, Guid projektId) : base(id)
    {
        if (projektId == Guid.Empty) throw new DomainException("Das Projekt muss angegeben werden.");
        ProjektId = projektId;
        QpReferenzQuelle = string.Empty;
        Nachweise = string.Empty;
    }

    public Guid ProjektId { get; private set; }
    public int? Baujahr { get; private set; }
    public DateOnly? Erstnutzung { get; private set; }
    public FoerderGebaeudeart? Gebaeudeart { get; private set; }
    public FoerderNutzung? Nutzung { get; private set; }
    public int? Wohneinheiten { get; private set; }
    public Antragstellerart? Eigentuemart { get; private set; }
    public bool? Selbstnutzung { get; private set; }
    public bool? Vermietung { get; private set; }
    public bool? Denkmal { get; private set; }
    public bool? BesondersErhaltenswerteBausubstanz { get; private set; }
    public bool? Gemeinnuetzigkeit { get; private set; }
    public bool? WirtschaftlicheTaetigkeit { get; private set; }
    public bool? Vorsteuerabzug { get; private set; }
    public bool? ISfp { get; private set; }
    public bool? Energieausweis { get; private set; }
    public string Nachweise { get; private set; }
    public decimal? Nettogrundflaeche { get; private set; }
    public decimal? JahresPrimaerenergiebedarf { get; private set; }
    public decimal? QpReferenz { get; private set; }
    public string QpReferenzQuelle { get; private set; }
    public bool? WpbFachlichBestaetigt { get; private set; }

    public decimal? WpbVerhaeltnis => Gebaeudeart == FoerderGebaeudeart.Nichtwohngebaeude &&
        JahresPrimaerenergiebedarf.HasValue && QpReferenz is > 0
            ? Math.Round(JahresPrimaerenergiebedarf.Value / QpReferenz.Value, 3)
            : null;

    public WpbPruefstatus WpbRechnerischerVorschlag => WpbVerhaeltnis switch
    {
        null => WpbPruefstatus.Unvollstaendig,
        >= 4m => WpbPruefstatus.RechnerischErfuellt,
        _ => WpbPruefstatus.RechnerischNichtErfuellt
    };

    public void Aktualisieren(int? baujahr, DateOnly? erstnutzung, FoerderGebaeudeart? gebaeudeart,
        FoerderNutzung? nutzung, int? wohneinheiten, Antragstellerart? eigentuemart,
        bool? selbstnutzung, bool? vermietung, bool? denkmal, bool? besondersErhaltenswerteBausubstanz,
        bool? gemeinnuetzigkeit, bool? wirtschaftlicheTaetigkeit, bool? vorsteuerabzug, bool? iSfp,
        bool? energieausweis, string? nachweise, decimal? qpReferenz, string? qpReferenzQuelle,
        bool? wpbFachlichBestaetigt)
    {
        if (baujahr is < 1000 or > 3000) throw new DomainException("Das Baujahr ist ungültig.");
        if (wohneinheiten is < 0) throw new DomainException("Die Zahl der Wohneinheiten darf nicht negativ sein.");
        if (qpReferenz is < 0) throw new DomainException("Qp,Ref darf nicht negativ sein.");
        if (qpReferenz.HasValue && string.IsNullOrWhiteSpace(qpReferenzQuelle))
            throw new DomainException("Für Qp,Ref muss eine Quelle angegeben werden.");

        Baujahr = baujahr; Erstnutzung = erstnutzung; Gebaeudeart = gebaeudeart; Nutzung = nutzung;
        Wohneinheiten = wohneinheiten; Eigentuemart = eigentuemart; Selbstnutzung = selbstnutzung;
        Vermietung = vermietung; Denkmal = denkmal; BesondersErhaltenswerteBausubstanz = besondersErhaltenswerteBausubstanz;
        Gemeinnuetzigkeit = gemeinnuetzigkeit; WirtschaftlicheTaetigkeit = wirtschaftlicheTaetigkeit;
        Vorsteuerabzug = vorsteuerabzug; ISfp = iSfp; Energieausweis = energieausweis;
        Nachweise = nachweise?.Trim() ?? string.Empty; QpReferenz = qpReferenz;
        QpReferenzQuelle = qpReferenzQuelle?.Trim() ?? string.Empty; WpbFachlichBestaetigt = wpbFachlichBestaetigt;
    }

    public void B56BestandswerteUebernehmen(decimal? nettogrundflaeche, decimal? jahresPrimaerenergiebedarf)
    {
        if (nettogrundflaeche is < 0 || jahresPrimaerenergiebedarf is < 0)
            throw new DomainException("B56-Bestandswerte dürfen nicht negativ sein.");
        Nettogrundflaeche = nettogrundflaeche;
        JahresPrimaerenergiebedarf = jahresPrimaerenergiebedarf;
    }
}
