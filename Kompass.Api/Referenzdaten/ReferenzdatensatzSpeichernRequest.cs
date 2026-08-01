using Kompass.Domain.Referenzdaten;

namespace Kompass.Api.Referenzdaten;

public sealed class ReferenzdatensatzSpeichernRequest
{
    public string FachlicheBezeichnung { get; set; } = string.Empty;

    public string Parameterart { get; set; } = string.Empty;

    public string Wert { get; set; } = string.Empty;

    public string? Einheit { get; set; }

    public string? Bezugsgroesse { get; set; }

    public string? EnergietraegerOderKategorie { get; set; }

    public ReferenzdatenEbene Ebene { get; set; } = ReferenzdatenEbene.Systemweit;

    public Guid? ProjektId { get; set; }

    public Guid? UnternehmenId { get; set; }

    public string Quelle { get; set; } = string.Empty;

    public string Herausgeber { get; set; } = string.Empty;

    public string QuellenVerweis { get; set; } = string.Empty;

    public DateOnly? Veroeffentlichungsdatum { get; set; }

    public DateOnly? Abrufdatum { get; set; }

    public DateOnly GueltigAb { get; set; }

    public DateOnly? GueltigBis { get; set; }

    public string Versionsstand { get; set; } = "1";

    public ReferenzdatenStatus Datenstatus { get; set; } = ReferenzdatenStatus.Freigegeben;

    public Qualitaetsstatus Qualitaetsstatus { get; set; } = Qualitaetsstatus.NichtVerifiziert;

    public ReferenzdatenImportart Importart { get; set; } = ReferenzdatenImportart.ManuellePflege;
}
