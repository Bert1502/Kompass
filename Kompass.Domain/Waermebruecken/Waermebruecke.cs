using Kompass.Domain.Common;

namespace Kompass.Domain.Waermebruecken;

public sealed class Waermebruecke : AggregateRoot
{
    public const int MaxInterneNummerLaenge = 20;
    public const int MaxBezeichnungLaenge = 200;
    public const int MaxTextLaenge = 500;
    public const int MaxAnmerkungLaenge = 2000;

    private Waermebruecke()
    {
        InterneNummer = string.Empty;
        Bezeichnung = string.Empty;
    }

    public Waermebruecke(
        Guid id,
        Guid projektId,
        string interneNummer,
        string bezeichnung,
        WaermebrueckeTyp typ)
        : base(id)
    {
        if (projektId == Guid.Empty)
        {
            throw new DomainException(
                "Eine Wärmebrücke benötigt eine gültige Projekt-ID.");
        }

        ProjektId = projektId;
        InterneNummer = ValidiereKurztext(
            interneNummer,
            MaxInterneNummerLaenge,
            "Die interne Nummer der Wärmebrücke darf nicht leer sein.",
            $"Die interne Nummer darf höchstens {MaxInterneNummerLaenge} Zeichen enthalten.");
        Bezeichnung = ValidiereKurztext(
            bezeichnung,
            MaxBezeichnungLaenge,
            "Die Bezeichnung der Wärmebrücke darf nicht leer sein.",
            $"Die Bezeichnung darf höchstens {MaxBezeichnungLaenge} Zeichen enthalten.");
        Typ = typ;
    }

    public Guid ProjektId { get; private set; }

    public string InterneNummer { get; private set; }

    public string Bezeichnung { get; private set; }

    public string? Lage { get; private set; }

    public string? Planreferenz { get; private set; }

    public string? Detailreferenz { get; private set; }

    public string? Fremdnummer { get; private set; }

    public decimal? Laenge { get; private set; }

    public WaermebrueckeTyp Typ { get; private set; }

    public WaermebrueckeStatus Status { get; private set; }
        = WaermebrueckeStatus.Offen;

    public GleichwertigkeitStatus GleichwertigkeitStatus { get; private set; }
        = GleichwertigkeitStatus.NichtBewertet;

    public string? Beiblatt2Referenz { get; private set; }

    public string? ThermCadReferenz { get; private set; }

    public decimal? PsiWert { get; private set; }

    public decimal? FRsi { get; private set; }

    public string? Pruefanmerkung { get; private set; }

    public string? Berichtsdarstellung { get; private set; }

    public void DatenAktualisieren(
        string interneNummer,
        string bezeichnung,
        WaermebrueckeTyp typ,
        WaermebrueckeStatus status,
        GleichwertigkeitStatus gleichwertigkeitStatus,
        string? lage = null,
        string? planreferenz = null,
        string? detailreferenz = null,
        string? fremdnummer = null,
        decimal? laenge = null,
        string? beiblatt2Referenz = null,
        string? thermCadReferenz = null,
        decimal? psiWert = null,
        decimal? fRsi = null,
        string? pruefanmerkung = null,
        string? berichtsdarstellung = null)
    {
        InterneNummer = ValidiereKurztext(
            interneNummer,
            MaxInterneNummerLaenge,
            "Die interne Nummer der Wärmebrücke darf nicht leer sein.",
            $"Die interne Nummer darf höchstens {MaxInterneNummerLaenge} Zeichen enthalten.");
        Bezeichnung = ValidiereKurztext(
            bezeichnung,
            MaxBezeichnungLaenge,
            "Die Bezeichnung der Wärmebrücke darf nicht leer sein.",
            $"Die Bezeichnung darf höchstens {MaxBezeichnungLaenge} Zeichen enthalten.");
        Typ = typ;
        Status = status;
        GleichwertigkeitStatus = gleichwertigkeitStatus;
        Lage = BegrenzeOptionalenText(lage, MaxTextLaenge, "Lage");
        Planreferenz = BegrenzeOptionalenText(planreferenz, MaxTextLaenge, "Planreferenz");
        Detailreferenz = BegrenzeOptionalenText(detailreferenz, MaxTextLaenge, "Detailreferenz");
        Fremdnummer = BegrenzeOptionalenText(fremdnummer, MaxTextLaenge, "Fremdnummer");
        Beiblatt2Referenz = BegrenzeOptionalenText(beiblatt2Referenz, MaxTextLaenge, "Beiblatt-2-Referenz");
        ThermCadReferenz = BegrenzeOptionalenText(thermCadReferenz, MaxTextLaenge, "ThermCAD-Referenz");
        Pruefanmerkung = BegrenzeOptionalenText(pruefanmerkung, MaxAnmerkungLaenge, "Prüfanmerkung");
        Berichtsdarstellung = BegrenzeOptionalenText(berichtsdarstellung, MaxAnmerkungLaenge, "Berichtsdarstellung");

        if (laenge is < 0)
        {
            throw new DomainException(
                "Die Länge einer Wärmebrücke darf nicht negativ sein.");
        }

        Laenge = laenge;
        PsiWert = psiWert;
        FRsi = fRsi;
    }

    private static string ValidiereKurztext(
        string wert,
        int maxLaenge,
        string leerFehler,
        string laengeFehler)
    {
        if (string.IsNullOrWhiteSpace(wert))
        {
            throw new DomainException(leerFehler);
        }

        var bereinigt = wert.Trim();

        if (bereinigt.Length > maxLaenge)
        {
            throw new DomainException(laengeFehler);
        }

        return bereinigt;
    }

    private static string? BegrenzeOptionalenText(
        string? wert,
        int maxLaenge,
        string feldname)
    {
        if (wert is null)
        {
            return null;
        }

        var bereinigt = wert.Trim();

        if (bereinigt.Length > maxLaenge)
        {
            throw new DomainException(
                $"'{feldname}' darf höchstens {maxLaenge} Zeichen enthalten.");
        }

        return bereinigt.Length == 0 ? null : bereinigt;
    }
}
