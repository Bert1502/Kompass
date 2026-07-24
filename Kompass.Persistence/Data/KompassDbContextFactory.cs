using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kompass.Persistence.Data;

public sealed class KompassDbContextFactory
    : IDesignTimeDbContextFactory<KompassDbContext>
{
    public KompassDbContext CreateDbContext(
        string[] args)
    {
        var projektverzeichnis =
            Directory.GetCurrentDirectory();

        var datenbankpfad =
            Path.Combine(
                projektverzeichnis,
                "kompass.db");

        var optionsBuilder =
            new DbContextOptionsBuilder<KompassDbContext>();

        optionsBuilder.UseSqlite(
            $"Data Source={datenbankpfad}");

        return new KompassDbContext(
            optionsBuilder.Options);
    }
}