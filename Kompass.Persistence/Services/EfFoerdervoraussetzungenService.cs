using Kompass.Application.Funding;
using Kompass.Domain.Funding;
using Kompass.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Persistence.Services;

public sealed class EfFoerdervoraussetzungenService : IFoerdervoraussetzungenService
{
    private readonly KompassDbContext _db;
    public EfFoerdervoraussetzungenService(KompassDbContext db) => _db = db;

    public Task<Foerdervoraussetzungen?> AbrufenAsync(Guid projektId, CancellationToken cancellationToken = default) =>
        _db.Foerdervoraussetzungen.SingleOrDefaultAsync(x => x.ProjektId == projektId, cancellationToken);

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
}
