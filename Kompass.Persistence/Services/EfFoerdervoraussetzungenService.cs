using Kompass.Application.Funding;
using Kompass.Application.B56Import;
using Kompass.Domain.Funding;
using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Kompass.Persistence.Services;

public sealed class EfFoerdervoraussetzungenService : IFoerdervoraussetzungenService
{
    private readonly KompassDbContext _db;
    private readonly IB56ImportRegister? _importRegister;
    private readonly IB56ArbeitsmappenLeser? _arbeitsmappenLeser;

    public EfFoerdervoraussetzungenService(
        KompassDbContext db,
        IB56ImportRegister? importRegister = null,
        IB56ArbeitsmappenLeser? arbeitsmappenLeser = null)
    {
        _db = db;
        _importRegister = importRegister;
        _arbeitsmappenLeser = arbeitsmappenLeser;
    }

    public async Task<Foerdervoraussetzungen?> AbrufenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default)
    {
        var voraussetzungen = await _db.Foerdervoraussetzungen
            .SingleOrDefaultAsync(x => x.ProjektId == projektId, cancellationToken);

        if (voraussetzungen is not null &&
            voraussetzungen.Nettogrundflaeche.HasValue &&
            voraussetzungen.JahresPrimaerenergiebedarf.HasValue)
        {
            return voraussetzungen;
        }

        if (_importRegister is null)
        {
            return voraussetzungen;
        }

        var snapshotId = await _db.Projekte
            .Where(x => x.Id == projektId)
            .Select(x => x.QuellSnapshotId)
            .SingleOrDefaultAsync(cancellationToken);
        if (!snapshotId.HasValue)
        {
            return voraussetzungen;
        }

        var fachdaten = await _importRegister.FachdatenAbrufenAsync(
            projektId,
            snapshotId.Value,
            cancellationToken);
        var ngf = Kennwert(fachdaten?.Bestandskennwerte, "NGF");
        var primaerenergieGesamt = Kennwert(
            fachdaten?.Bestandskennwerte,
            "Prim\u00e4renergiebedarf Geb\u00e4ude",
            "Prim\u00e4renergiebedarf Bericht");

        if ((!ngf.HasValue || !primaerenergieGesamt.HasValue) &&
            _arbeitsmappenLeser is not null)
        {
            var snapshot = await _importRegister.NachIdSuchenAsync(
                projektId,
                snapshotId.Value,
                cancellationToken);
            if (snapshot is not null && File.Exists(snapshot.Archivdateipfad))
            {
                var arbeitsmappe = await _arbeitsmappenLeser.LesenAsync(
                    snapshot.Archivdateipfad,
                    cancellationToken);
                ngf ??= BenannterZahlenwert(arbeitsmappe, "AllgBezugFlach");
                primaerenergieGesamt ??= BenannterZahlenwert(
                    arbeitsmappe,
                    "bestand_primaerenergiebedarf");
            }
        }

        if (!ngf.HasValue && !primaerenergieGesamt.HasValue)
        {
            return voraussetzungen;
        }

        voraussetzungen ??= new Foerdervoraussetzungen(Guid.NewGuid(), projektId);
        if (_db.Entry(voraussetzungen).State == EntityState.Detached)
        {
            _db.Foerdervoraussetzungen.Add(voraussetzungen);
        }

        var qpBestand = ngf is > 0 && primaerenergieGesamt.HasValue
            ? Math.Round(primaerenergieGesamt.Value / ngf.Value, 3)
            : primaerenergieGesamt;
        voraussetzungen.B56BestandswerteUebernehmen(
            ngf ?? voraussetzungen.Nettogrundflaeche,
            qpBestand ?? voraussetzungen.JahresPrimaerenergiebedarf);
        await _db.SaveChangesAsync(cancellationToken);
        return voraussetzungen;
    }

    public async Task<Foerdervoraussetzungen?> SpeichernAsync(Guid projektId, FoerdervoraussetzungenEingabe e, CancellationToken cancellationToken = default)
    {
        if (!await _db.Projekte.AnyAsync(x => x.Id == projektId, cancellationToken)) return null;
        var v = await AbrufenAsync(projektId, cancellationToken);
        if (v is null) { v = new Foerdervoraussetzungen(Guid.NewGuid(), projektId); _db.Add(v); }
        v.Aktualisieren(e.Baujahr, e.Erstnutzung, e.Gebaeudeart, e.Nutzung, e.Wohneinheiten, e.Eigentuemart,
            e.Selbstnutzung, e.Vermietung, e.Denkmal, e.BesondersErhaltenswerteBausubstanz, e.Gemeinnuetzigkeit,
            e.WirtschaftlicheTaetigkeit, e.Vorsteuerabzug, e.ISfp, e.Energieausweis, e.Nachweise,
            e.QpReferenz, e.QpReferenzQuelle, e.WpbFachlichBestaetigt);
        await _db.SaveChangesAsync(cancellationToken);
        return v;
    }

    private static decimal? Kennwert(
        IReadOnlyList<B56Kennwert>? kennwerte,
        params string[] namen) =>
        kennwerte?
            .FirstOrDefault(k => namen.Any(n => string.Equals(k.Name, n, StringComparison.OrdinalIgnoreCase)))?
            .Wert is { } wert
            ? (decimal)wert
            : null;

    private static decimal? BenannterZahlenwert(B56Arbeitsmappe arbeitsmappe, string name) =>
        arbeitsmappe.BenannteZellwerte.TryGetValue(name, out var rohwert) &&
        decimal.TryParse(
            rohwert.Replace(',', '.'),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var wert)
            ? wert
            : null;
}
