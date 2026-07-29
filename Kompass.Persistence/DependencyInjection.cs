using Kompass.Application.Economics;
using Kompass.Application.Funding;
using Kompass.Application.Projects;
using Kompass.Application.Reports;
using Kompass.Application.Waermebruecken;
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

        services.AddScoped<IWirtschaftlichkeitsService, EfWirtschaftlichkeitsService>();

        services.AddScoped<IKostenpositionService, EfKostenpositionService>();

        services.AddScoped<IFoerderprogrammService, EfFoerderprogrammService>();

        services.AddScoped<IAlternativeFoerderungService, EfAlternativeFoerderungService>();

        services.AddScoped<IWaermebrueckeService, EfWaermebrueckeService>();

        services.AddScoped<IBerichtsService, BerichtsService>();

        return services;
    }
}