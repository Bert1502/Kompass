using Kompass.Domain.Projects;
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

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(KompassDbContext).Assembly);
    }
}