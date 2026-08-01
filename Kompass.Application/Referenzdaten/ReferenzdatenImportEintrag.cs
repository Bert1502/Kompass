using Kompass.Domain.Referenzdaten;

namespace Kompass.Application.Referenzdaten;

public sealed record ReferenzdatenImportEintrag(
    string FachlicheBezeichnung,
    string Parameterart,
    string Wert,
    ReferenzdatenEbene Ebene,
    string Quelle,
    string Herausgeber,
    string QuellenVerweis,
    DateOnly GueltigAb,
    DateOnly? GueltigBis,
    string Versionsstand,
    ReferenzdatenStatus Datenstatus,
    Qualitaetsstatus Qualitaetsstatus,
    ReferenzdatenImportart Importart,
    DateTimeOffset LetzteAktualisierungUtc,
    string? Einheit = null,
    string? Bezugsgroesse = null,
    string? EnergietraegerOderKategorie = null,
    DateOnly? Veroeffentlichungsdatum = null,
    DateOnly? Abrufdatum = null,
    Guid? ProjektId = null,
    Guid? UnternehmenId = null);
