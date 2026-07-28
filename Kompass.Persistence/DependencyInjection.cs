using Kompass.Application.Economics;
using Kompass.Application.Projects;
using Kompass.Persistence.Data;
using Kompass.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kompass.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("KompassDatabase")
            ?? throw new InvalidOperationException(
                "Die Datenbankverbindung 'KompassDatabase' wurde nicht gefunden.");

        services.AddDbContext<KompassDbContext>(
            options =>
            {
                options.UseSqlite(connectionString);

                options.EnableDetailedErrors();
            });

        services.AddScoped<IProjektService, ProjektService>();

        services.AddScoped<
            IWirtschaftlichkeitsannahmenRepository,
            EfWirtschaftlichkeitsannahmenRepository>();

        services.AddSingleton<WirtschaftlichkeitsberechnungsService>(sp =>
            new WirtschaftlichkeitsberechnungsService(
                sp.GetRequiredService<TimeProvider>()));

        return services;
    }
}