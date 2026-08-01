namespace Kompass.Domain.Reports;

/// <summary>
/// Kennzeichnet die Art eines KOMPASS-Berichts gemäß Abschnitt 17 der Fachspezifikation.
/// </summary>
public enum Berichtstyp
{
    Alternativenvergleich,
    Foerderuebersicht,
    Waermebrueckenuebersicht,
    Wirtschaftlichkeitsbericht,
    Energieberatungsbericht,
    Verbrauchsvergleich,
}
