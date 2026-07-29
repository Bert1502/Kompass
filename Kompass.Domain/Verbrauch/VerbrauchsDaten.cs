using Kompass.Domain.Common;
using Kompass.Domain.Economics;

namespace Kompass.Domain.Verbrauch;

/// <summary>
/// Reale Verbrauchsdaten einer Abrechnungsperiode je Energieträger für ein Projekt.
/// Unterstützt Witterungsbereinigung, Flächenbezug, Vergleich mit B56-Wert,
/// Anpassungsfaktoren und dokumentierte Abweichungsursachen (Abschnitt 18).
/// </summary>
public sealed class VerbrauchsDaten : AggregateRoot
{
    public const int MaxBegruendungLaenge = 1000;

    private VerbrauchsDaten()
    {
    }

    public VerbrauchsDaten(
        Guid id,
        Guid projektId,
        DateOnly periodeVon,
        DateOnly periodeBis,
        Energietraeger energietraeger,
        decimal menge,
        decimal kosten)
        : base(id)
    {
        if (projektId == Guid.Empty)
        {
            throw new DomainException(
                "Verbrauchsdaten benötigen eine gültige Projekt-ID.");
        }

        if (periodeBis < periodeVon)
        {
            throw new DomainException(
                "Das Ende der Abrechnungsperiode darf nicht vor dem Beginn liegen.");
        }

        if (menge < 0)
        {
            throw new DomainException(
                "Die Verbrauchsmenge darf nicht negativ sein.");
        }

        if (kosten < 0)
        {
            throw new DomainException(
                "Die Kosten dürfen nicht negativ sein.");
        }

        ProjektId = projektId;
        PeriodeVon = periodeVon;
        PeriodeBis = periodeBis;
        Energietraeger = energietraeger;
        Menge = menge;
        Kosten = kosten;
    }

    public Guid ProjektId { get; private set; }

    /// <summary>Beginn der Abrechnungsperiode.</summary>
    public DateOnly PeriodeVon { get; private set; }

    /// <summary>Ende der Abrechnungsperiode.</summary>
    public DateOnly PeriodeBis { get; private set; }

    public Energietraeger Energietraeger { get; private set; }

    /// <summary>Verbrauchsmenge in kWh.</summary>
    public decimal Menge { get; private set; }

    /// <summary>Energiekosten in EUR.</summary>
    public decimal Kosten { get; private set; }

    /// <summary>
    /// Witterungsbereinigungsfaktor (z. B. 1,12 für 12 % kälter als Normjahr).
    /// Null bedeutet: keine Witterungsbereinigung angewendet.
    /// </summary>
    public decimal? WitterungsbereinigungsFaktor { get; private set; }

    /// <summary>Bezugsfläche in m². Null bedeutet: kein Flächenbezug.</summary>
    public decimal? Flaeche { get; private set; }

    /// <summary>
    /// Vergleichswert aus dem B56-Bilanz in kWh für denselben Energieträger.
    /// Null bedeutet: kein B56-Vergleichswert vorhanden.
    /// </summary>
    public decimal? B56VergleichsWert { get; private set; }

    /// <summary>
    /// Nachvollziehbarer Anpassungsfaktor, z. B. für Leerstand oder Sondernutzung.
    /// </summary>
    public decimal? AnpassungsFaktor { get; private set; }

    /// <summary>Begründung für den Anpassungsfaktor.</summary>
    public string? AnpassungsBegruendung { get; private set; }

    /// <summary>Dokumentierte Abweichungsursache vom B56-Vergleichswert.</summary>
    public string? Abweichungsursache { get; private set; }

    /// <summary>
    /// Witterungsbereinigte Menge in kWh, wenn ein Faktor hinterlegt ist.
    /// Andernfalls identisch mit <see cref="Menge"/>.
    /// </summary>
    public decimal WitterungsbereinigteMenge =>
        WitterungsbereinigungsFaktor.HasValue
            ? Menge * WitterungsbereinigungsFaktor.Value
            : Menge;

    /// <summary>
    /// Verbrauch je Flächeneinheit in kWh/m², wenn eine Fläche hinterlegt ist.
    /// </summary>
    public decimal? MengeJeFlaeche =>
        Flaeche.HasValue && Flaeche.Value > 0
            ? Menge / Flaeche.Value
            : null;

    public void Aktualisieren(
        DateOnly periodeVon,
        DateOnly periodeBis,
        Energietraeger energietraeger,
        decimal menge,
        decimal kosten,
        decimal? witterungsbereinigungsFaktor,
        decimal? flaeche,
        decimal? b56VergleichsWert,
        decimal? anpassungsFaktor,
        string? anpassungsBegruendung,
        string? abweichungsursache)
    {
        if (periodeBis < periodeVon)
        {
            throw new DomainException(
                "Das Ende der Abrechnungsperiode darf nicht vor dem Beginn liegen.");
        }

        if (menge < 0)
        {
            throw new DomainException(
                "Die Verbrauchsmenge darf nicht negativ sein.");
        }

        if (kosten < 0)
        {
            throw new DomainException(
                "Die Kosten dürfen nicht negativ sein.");
        }

        if (witterungsbereinigungsFaktor.HasValue && witterungsbereinigungsFaktor.Value <= 0)
        {
            throw new DomainException(
                "Der Witterungsbereinigungsfaktor muss größer als 0 sein.");
        }

        if (flaeche.HasValue && flaeche.Value <= 0)
        {
            throw new DomainException(
                "Die Bezugsfläche muss größer als 0 sein.");
        }

        if (anpassungsBegruendung is { Length: > MaxBegruendungLaenge })
        {
            throw new DomainException(
                $"Die Anpassungsbegründung darf höchstens {MaxBegruendungLaenge} Zeichen enthalten.");
        }

        if (abweichungsursache is { Length: > MaxBegruendungLaenge })
        {
            throw new DomainException(
                $"Die Abweichungsursache darf höchstens {MaxBegruendungLaenge} Zeichen enthalten.");
        }

        PeriodeVon = periodeVon;
        PeriodeBis = periodeBis;
        Energietraeger = energietraeger;
        Menge = menge;
        Kosten = kosten;
        WitterungsbereinigungsFaktor = witterungsbereinigungsFaktor;
        Flaeche = flaeche;
        B56VergleichsWert = b56VergleichsWert;
        AnpassungsFaktor = anpassungsFaktor;
        AnpassungsBegruendung = anpassungsBegruendung;
        Abweichungsursache = abweichungsursache;
    }
}
