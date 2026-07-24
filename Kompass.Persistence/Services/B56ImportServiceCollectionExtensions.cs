using Kompass.Application.B56Import;
using Kompass.Persistence.Services;
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
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionen =
            new B56ImportOptionen
            {
                DoppelteImporteZulassen = false,
                ArchivHashPruefen = true,
                ArchivBasisverzeichnis =
                    @"D:\KOMPASS\B56-Archiv",
                ErlaubteDateiendungen =
                [
                    ".xlsx",
                    ".xlsm"
                ],
                MaximaleDateigroesseBytes =
                    50L * 1024L * 1024L,
                ImportdateiArchivieren = true,
                HashBerechnen = true,
                VorhandeneArchivdateienUeberschreiben = false,
                ProjektUnterordnerErstellen = true,
                ZeitstempelImArchivPfad = true
            };

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
}
