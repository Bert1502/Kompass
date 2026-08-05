using Kompass.Application.B56Import;
using Kompass.Domain.Economics;
using Kompass.Domain.Funding;
using Kompass.Domain.Fachdaten;
using Kompass.Domain.Massnahmen;
using Kompass.Domain.Materialien;
using Kompass.Domain.Projects;
using Kompass.Domain.Referenzdaten;
using Kompass.Domain.Regelwerke;
using Kompass.Domain.Verbrauch;
using Kompass.Domain.Waermebruecken;
using Kompass.Persistence.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Persistence.Data;

public sealed class KompassDbContext : DbContext
{
    public KompassDbContext(
        DbContextOptions<KompassDbContext> options)
        : base(options)
    {
    }

    public DbSet<Projekt> Projekte
        => Set<Projekt>();

    public DbSet<B56ImportEintragEntity> B56ImportEintraege
        => Set<B56ImportEintragEntity>();

    public DbSet<B56SnapshotVergleichEntity> B56SnapshotVergleiche
        => Set<B56SnapshotVergleichEntity>();

    public DbSet<B56KonfliktEintragEntity> B56KonfliktEintraege
        => Set<B56KonfliktEintragEntity>();

    public DbSet<Wirtschaftlichkeitsannahmen> Wirtschaftlichkeitsannahmen
        => Set<Wirtschaftlichkeitsannahmen>();

    public DbSet<Foerderprogramm> Foerderprogramme
        => Set<Foerderprogramm>();

    public DbSet<FoerderungZuordnung> FoerderungZuordnungen
        => Set<FoerderungZuordnung>();

    public DbSet<Foerdervoraussetzungen> Foerdervoraussetzungen
        => Set<Foerdervoraussetzungen>();

    public DbSet<Waermebruecke> Waermebruecken
        => Set<Waermebruecke>();

    public DbSet<VerbrauchsDaten> VerbrauchsDaten
        => Set<VerbrauchsDaten>();

    public DbSet<Referenzdatensatz> Referenzdatensaetze
        => Set<Referenzdatensatz>();

    public DbSet<ReferenzwertAbweichung> ReferenzwertAbweichungen
        => Set<ReferenzwertAbweichung>();

    public DbSet<Fachdatenquelle> Fachdatenquellen => Set<Fachdatenquelle>();
    public DbSet<Regelwerk> Regelwerke => Set<Regelwerk>();
    public DbSet<Regelwerksanforderung> Regelwerksanforderungen => Set<Regelwerksanforderung>();
    public DbSet<Massnahmenkategorie> Massnahmenkategorien => Set<Massnahmenkategorie>();
    public DbSet<Massnahmenkatalogeintrag> Massnahmenkatalogeintraege => Set<Massnahmenkatalogeintrag>();
    public DbSet<Materialkategorie> Materialkategorien => Set<Materialkategorie>();
    public DbSet<Material> Materialien => Set<Material>();
    public DbSet<Projektmassnahme> Projektmassnahmen => Set<Projektmassnahme>();
    public DbSet<Foerdergeber> Foerdergeber => Set<Foerdergeber>();
    public DbSet<Foerdertatbestand> Foerdertatbestaende => Set<Foerdertatbestand>();
    public DbSet<WirtschaftlicheZeitreihe> WirtschaftlicheZeitreihen => Set<WirtschaftlicheZeitreihe>();
    public DbSet<WirtschaftlicherZeitwert> WirtschaftlicheZeitwerte => Set<WirtschaftlicherZeitwert>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(KompassDbContext).Assembly);
    }
}
