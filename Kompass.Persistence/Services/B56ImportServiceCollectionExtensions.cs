using Kompass.Application.B56Import;
using Kompass.Persistence.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kompass.Persistence;

public static class B56ImportServiceCollectionExtensions
{
    public static IServiceCollection AddB56Import(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(
            new B56ImportOptionen
            {
                DoppelteImporteZulassen = false,
                ArchivHashPruefen = true
            });

        services.AddScoped<
            IB56ImportRegister,
            EfB56ImportRegister>();

        services.AddScoped<
            IB56ImportService,
            B56ImportService>();

        services.AddSingleton<
            IB56ArbeitsmappenLeser,
            OpenXmlB56ArbeitsmappenLeser>();

        services.AddSingleton<
            IB56TabellenFinder,
            B56TabellenFinder>();

        services.AddSingleton<
            IB56BauteilcodeParser,
            B56BauteilcodeParser>();

        services.AddSingleton<
            IB56BauteilregelRepository,
            StandardBauteilregelRepository>();

        services.AddSingleton<
            IB56BauteilcodeParser,
            B56BauteilcodeParser>();

        return services;
    }
}