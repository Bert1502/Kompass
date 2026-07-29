using Kompass.Domain.Economics;
using Kompass.Domain.Funding;
using Kompass.Domain.Projects;
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

    public DbSet<Wirtschaftlichkeitsannahmen> Wirtschaftlichkeitsannahmen
        => Set<Wirtschaftlichkeitsannahmen>();

    public DbSet<Foerderprogramm> Foerderprogramme
        => Set<Foerderprogramm>();

    public DbSet<FoerderungZuordnung> FoerderungZuordnungen
        => Set<FoerderungZuordnung>();

    public DbSet<Waermebruecke> Waermebruecken
        => Set<Waermebruecke>();

    public DbSet<VerbrauchsDaten> VerbrauchsDaten
        => Set<VerbrauchsDaten>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(KompassDbContext).Assembly);
    }
}