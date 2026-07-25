using Kompass.Application.B56Import;
using Kompass.Persistence.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kompass.Persistence;

/// <summary>
/// Registriert sämtliche Dienste des B56-Importmoduls.
/// </summary>
public static class B56ImportServiceCollectionExtensions
{
    /// <summary>
    /// Fügt die Dienste für Prüfung, Einlesen, Verarbeitung,
    /// Archivierung und Registrierung von B56-Dateien hinzu.
    /// </summary>
    /// <param name="services">
    /// Die Dienstesammlung der Anwendung.
    /// </param>
    /// <returns>
    /// Die ergänzte Dienstesammlung.
    /// </returns>
    public static IServiceCollection AddB56Import(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var optionen =
            configuration
                .GetSection("B56Import")
                .Get<B56ImportOptionen>()
            ?? new B56ImportOptionen();

        Validiere(optionen);

        services.AddSingleton(optionen);
        services.AddSingleton(Options.Create(optionen));

        services.AddScoped<
            IB56ImportRegister,
            EfB56ImportRegister>();

services.AddScoped<
    IB56ImportPipeline,
    B56ImportPipeline>();

        services.AddScoped<
            IB56ImportService,
            B56ImportService>();

        services.AddScoped<
            IB56SnapshotLebenszyklusService,
            B56SnapshotLebenszyklusService>();

        services.AddSingleton(
            TimeProvider.System);

        services.AddSingleton<
            IB56DateiPruefer,
            B56Import.B56DateiPruefer>();

            services.AddScoped<
    IB56TabellenImportService,
    B56TabellenImportService>();

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
            IB56BauteilzuordnungsRepository,
            B56Import.JsonB56BauteilzuordnungsRepository>();

        services.AddSingleton<
            IB56HashService,
            B56Import.Sha256HashService>();

        services.AddSingleton<
            IB56ArchivService,
            B56ArchivService>();

        return services;
    }

    private static void Validiere(
        B56ImportOptionen optionen)
    {
        if (string.IsNullOrWhiteSpace(
                optionen.ArchivBasisverzeichnis))
        {
            throw new InvalidOperationException(
                "B56Import:ArchivBasisverzeichnis darf nicht leer sein.");
        }

        if (optionen.ErlaubteDateiendungen is null ||
            optionen.ErlaubteDateiendungen.Length == 0)
        {
            throw new InvalidOperationException(
                "B56Import:ErlaubteDateiendungen muss mindestens einen Eintrag enthalten.");
        }

        if (optionen.MaximaleDateigroesseBytes <= 0)
        {
            throw new InvalidOperationException(
                "B56Import:MaximaleDateigroesseBytes muss größer als null sein.");
        }
    }
}
