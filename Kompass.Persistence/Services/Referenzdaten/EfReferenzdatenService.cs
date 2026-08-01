using Kompass.Application.Referenzdaten;
using Kompass.Domain.Common;
using Kompass.Domain.Referenzdaten;
using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Persistence.Services.Referenzdaten;

public sealed class EfReferenzdatenService : IReferenzdatenService
{
    private readonly KompassDbContext _dbContext;
    private readonly IReadOnlyList<IReferenzdatenProvider> _providers;

    public EfReferenzdatenService(
        KompassDbContext dbContext,
        IEnumerable<IReferenzdatenProvider> providers)
    {
        _dbContext = dbContext;
        _providers = providers.ToList();
    }

    public async Task<IReadOnlyList<Referenzdatensatz>> ListenAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Referenzdatensaetze
            .OrderBy(datensatz => datensatz.Parameterart)
            .ThenBy(datensatz => datensatz.Ebene)
            .ThenBy(datensatz => datensatz.GueltigAb)
            .ToListAsync(cancellationToken);
    }

    public async Task<Referenzdatensatz> SpeichernAsync(
        Referenzdatensatz datensatz,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(datensatz);

        var vorhandener = await _dbContext.Referenzdatensaetze
            .SingleOrDefaultAsync(
                eintrag => eintrag.Id == datensatz.Id,
                cancellationToken);

        if (vorhandener is null)
        {
            _dbContext.Referenzdatensaetze.Add(datensatz);
        }
        else
        {
            _dbContext.Referenzdatensaetze.Remove(vorhandener);
            _dbContext.Referenzdatensaetze.Add(datensatz);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return datensatz;
    }

    public async Task<ReferenzwertAufloesung?> WertAufloesenAsync(
        ReferenzwertAnfrage anfrage,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(anfrage);

        var stichtag = anfrage.Stichtag ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var kandidat = await SucheKandidatAsync(
            anfrage,
            stichtag,
            ReferenzdatenPrioritaet.ProjektspezifischFreigegeben,
            query => query.Where(datensatz =>
                anfrage.ProjektId.HasValue &&
                datensatz.Ebene == ReferenzdatenEbene.Projektspezifisch &&
                datensatz.ProjektId == anfrage.ProjektId &&
                datensatz.Datenstatus == ReferenzdatenStatus.Freigegeben),
            cancellationToken);

        kandidat ??= await SucheKandidatAsync(
            anfrage,
            stichtag,
            ReferenzdatenPrioritaet.UnternehmensspezifischFreigegeben,
            query => query.Where(datensatz =>
                anfrage.UnternehmenId.HasValue &&
                datensatz.Ebene == ReferenzdatenEbene.Unternehmensweit &&
                datensatz.UnternehmenId == anfrage.UnternehmenId &&
                datensatz.Datenstatus == ReferenzdatenStatus.Freigegeben),
            cancellationToken);

        kandidat ??= await SucheKandidatAsync(
            anfrage,
            stichtag,
            ReferenzdatenPrioritaet.OffiziellerReferenzwert,
            query => query.Where(datensatz =>
                datensatz.Ebene == ReferenzdatenEbene.Systemweit &&
                datensatz.Datenstatus == ReferenzdatenStatus.Freigegeben &&
                datensatz.Qualitaetsstatus == Qualitaetsstatus.OffizielleQuelle),
            cancellationToken);

        kandidat ??= await SucheKandidatAsync(
            anfrage,
            stichtag,
            ReferenzdatenPrioritaet.AllgemeinerReferenzwert,
            query => query.Where(datensatz =>
                datensatz.Ebene == ReferenzdatenEbene.Systemweit &&
                datensatz.Datenstatus == ReferenzdatenStatus.Freigegeben),
            cancellationToken);

        kandidat ??= await SucheKandidatAsync(
            anfrage,
            stichtag,
            ReferenzdatenPrioritaet.LokalerErsatzwert,
            query => query.Where(datensatz =>
                datensatz.Qualitaetsstatus == Qualitaetsstatus.Ersatzwert &&
                datensatz.Datenstatus != ReferenzdatenStatus.Zurueckgezogen),
            cancellationToken,
            allowExpiredFallback: true);

        return kandidat;
    }

    public async Task<ReferenzwertAbweichung> ProjektabweichungSetzenAsync(
        ProjektabweichungAnfrage anfrage,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(anfrage);

        var referenz = await WertAufloesenAsync(
            new ReferenzwertAnfrage(
                anfrage.Parameterart,
                ProjektId: null,
                UnternehmenId: null,
                Bezugsgroesse: anfrage.Bezugsgroesse,
                EnergietraegerOderKategorie: anfrage.EnergietraegerOderKategorie),
            cancellationToken);

        if (referenz is null)
        {
            throw new DomainException("Es wurde kein überschreibbarer Referenzwert gefunden.");
        }

        var projektdatensatz = new Referenzdatensatz(
            Guid.NewGuid(),
            referenz.Datensatz.FachlicheBezeichnung,
            referenz.Datensatz.Parameterart,
            anfrage.VerwendeterProjektwert,
            ReferenzdatenEbene.Projektspezifisch,
            quelle: "Projektabweichung",
            herausgeber: anfrage.Benutzer,
            quellenVerweis: "Interne Projektfreigabe",
            gueltigAb: DateOnly.FromDateTime(DateTime.UtcNow),
            gueltigBis: null,
            versionsstand: "1",
            datenstatus: ReferenzdatenStatus.Freigegeben,
            qualitaetsstatus: Qualitaetsstatus.ProjektspezifischeAnnahme,
            importart: ReferenzdatenImportart.ManuellePflege,
            letzteAktualisierungUtc: DateTimeOffset.UtcNow,
            einheit: referenz.Datensatz.Einheit,
            bezugsgroesse: anfrage.Bezugsgroesse ?? referenz.Datensatz.Bezugsgroesse,
            energietraegerOderKategorie: anfrage.EnergietraegerOderKategorie ?? referenz.Datensatz.EnergietraegerOderKategorie,
            projektId: anfrage.ProjektId);

        var abweichung = new ReferenzwertAbweichung(
            Guid.NewGuid(),
            anfrage.ProjektId,
            anfrage.Parameterart,
            referenz.Datensatz.Wert,
            anfrage.VerwendeterProjektwert,
            anfrage.Begruendung,
            anfrage.Benutzer,
            DateTimeOffset.UtcNow,
            referenz.Datensatz.Id,
            anfrage.Bezugsgroesse,
            anfrage.EnergietraegerOderKategorie);

        _dbContext.Referenzdatensaetze.Add(projektdatensatz);
        _dbContext.ReferenzwertAbweichungen.Add(abweichung);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return abweichung;
    }

    public async Task<IReadOnlyList<ReferenzwertAbweichung>> ProjektabweichungenListenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ReferenzwertAbweichungen
            .Where(eintrag => eintrag.ProjektId == projektId)
            .OrderByDescending(eintrag => eintrag.AenderungszeitpunktUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<ReferenzdatenSynchronisationsErgebnis> SynchronisierenAsync(
        CancellationToken cancellationToken = default)
    {
        var result = new List<ReferenzdatenProviderErgebnis>();
        var aktualisiert = 0;
        var fallback = false;

        foreach (var provider in _providers)
        {
            try
            {
                var eintraege = await provider.LadeReferenzdatenAsync(cancellationToken);

                foreach (var eintrag in eintraege)
                {
                    aktualisiert += await UpsertDatensatzAsync(eintrag, cancellationToken);
                }

                result.Add(new ReferenzdatenProviderErgebnis(provider.ProviderName, eintraege.Count, null));
            }
            catch (Exception ex)
            {
                fallback = await _dbContext.Referenzdatensaetze.AnyAsync(cancellationToken);
                result.Add(new ReferenzdatenProviderErgebnis(provider.ProviderName, 0, ex.Message));
            }
        }

        if (aktualisiert > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new ReferenzdatenSynchronisationsErgebnis(result, aktualisiert, fallback);
    }

    private async Task<int> UpsertDatensatzAsync(
        ReferenzdatenImportEintrag eintrag,
        CancellationToken cancellationToken)
    {
        var vorhanden = await _dbContext.Referenzdatensaetze
            .SingleOrDefaultAsync(
                datensatz =>
                    datensatz.Parameterart == eintrag.Parameterart &&
                    datensatz.Ebene == eintrag.Ebene &&
                    datensatz.ProjektId == eintrag.ProjektId &&
                    datensatz.UnternehmenId == eintrag.UnternehmenId &&
                    datensatz.Bezugsgroesse == eintrag.Bezugsgroesse &&
                    datensatz.EnergietraegerOderKategorie == eintrag.EnergietraegerOderKategorie &&
                    datensatz.GueltigAb == eintrag.GueltigAb &&
                    datensatz.Versionsstand == eintrag.Versionsstand,
                cancellationToken);

        var id = vorhanden?.Id ?? Guid.NewGuid();

        if (vorhanden is not null)
        {
            _dbContext.Referenzdatensaetze.Remove(vorhanden);
        }

        _dbContext.Referenzdatensaetze.Add(new Referenzdatensatz(
            id,
            eintrag.FachlicheBezeichnung,
            eintrag.Parameterart,
            eintrag.Wert,
            eintrag.Ebene,
            eintrag.Quelle,
            eintrag.Herausgeber,
            eintrag.QuellenVerweis,
            eintrag.GueltigAb,
            eintrag.GueltigBis,
            eintrag.Versionsstand,
            eintrag.Datenstatus,
            eintrag.Qualitaetsstatus,
            eintrag.Importart,
            eintrag.LetzteAktualisierungUtc,
            eintrag.Einheit,
            eintrag.Bezugsgroesse,
            eintrag.EnergietraegerOderKategorie,
            eintrag.Veroeffentlichungsdatum,
            eintrag.Abrufdatum,
            eintrag.ProjektId,
            eintrag.UnternehmenId));

        return 1;
    }

    private async Task<ReferenzwertAufloesung?> SucheKandidatAsync(
        ReferenzwertAnfrage anfrage,
        DateOnly stichtag,
        ReferenzdatenPrioritaet prioritaet,
        Func<IQueryable<Referenzdatensatz>, IQueryable<Referenzdatensatz>> filter,
        CancellationToken cancellationToken,
        bool allowExpiredFallback = false)
    {
        var query = BasisQuery(anfrage);
        query = filter(query);

        if (allowExpiredFallback)
        {
            query = query.Where(datensatz => datensatz.GueltigAb <= stichtag);
        }
        else
        {
            query = query.Where(datensatz =>
                datensatz.GueltigAb <= stichtag &&
                (!datensatz.GueltigBis.HasValue || datensatz.GueltigBis >= stichtag));
        }

        var datensatz = await query
            .OrderByDescending(eintrag => eintrag.GueltigAb)
            .FirstOrDefaultAsync(cancellationToken);

        return datensatz is null
            ? null
            : new ReferenzwertAufloesung(datensatz, prioritaet);
    }

    private IQueryable<Referenzdatensatz> BasisQuery(ReferenzwertAnfrage anfrage)
    {
        var query = _dbContext.Referenzdatensaetze
            .Where(datensatz => datensatz.Parameterart == anfrage.Parameterart);

        if (!string.IsNullOrWhiteSpace(anfrage.Bezugsgroesse))
        {
            query = query.Where(datensatz => datensatz.Bezugsgroesse == anfrage.Bezugsgroesse);
        }

        if (!string.IsNullOrWhiteSpace(anfrage.EnergietraegerOderKategorie))
        {
            query = query.Where(datensatz => datensatz.EnergietraegerOderKategorie == anfrage.EnergietraegerOderKategorie);
        }

        return query;
    }
}
