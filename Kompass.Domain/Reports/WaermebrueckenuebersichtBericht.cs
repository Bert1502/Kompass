using Kompass.Domain.Waermebruecken;

namespace Kompass.Domain.Reports;

/// <summary>
/// Bericht "Wärmebrückenübersicht" gemäß Fachspezifikation Abschnitt 17.
/// Listet alle Wärmebrücken des Projekts mit ihren Prüfangaben.
/// </summary>
public sealed record WaermebrueckenuebersichtBericht(
    Berichtskopf Kopf,
    IReadOnlyList<Waermebruecke> Waermebruecken);
