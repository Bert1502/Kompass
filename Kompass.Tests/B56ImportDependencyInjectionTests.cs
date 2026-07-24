using Kompass.Application.B56Import;
using Kompass.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kompass.Tests.B56Import;

public sealed class B56ImportDependencyInjectionTests
{
    [Fact]
    public void Alle_B56_Dienste_koennen_aufgeloest_werden()
    {
        var configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:KompassDatabase"] =
                            "Data Source=:memory:",
                        ["B56Import:ArchivBasisverzeichnis"] =
                            "TestArchiv",
                        ["B56Import:ErlaubteDateiendungen:0"] =
                            ".xlsx",
                        ["B56Import:MaximaleDateigroesseBytes"] =
                            "1024"
                    })
                .Build();

        var services = new ServiceCollection();

        services.AddPersistence(configuration);
        services.AddB56Import(configuration);

        using var serviceProvider =
            services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });

        using var scope =
            serviceProvider.CreateScope();

        var scopedProvider =
            scope.ServiceProvider;

        Assert.IsType<B56ImportService>(
            scopedProvider.GetRequiredService<IB56ImportService>());

        Assert.NotNull(
            scopedProvider.GetRequiredService<IB56ImportRegister>());

        Assert.NotNull(
            scopedProvider.GetRequiredService<IB56ImportPipeline>());

        Assert.NotNull(
            scopedProvider.GetRequiredService<IB56TabellenImportService>());

        Assert.NotNull(
            scopedProvider.GetRequiredService<IB56DateiPruefer>());

        Assert.NotNull(
            scopedProvider.GetRequiredService<
                IB56BauteilzuordnungsRepository>());

        var optionen =
            scopedProvider.GetRequiredService<B56ImportOptionen>();

        Assert.Equal(
            "TestArchiv",
            optionen.ArchivBasisverzeichnis);

        Assert.Equal(
            [".xlsx"],
            optionen.ErlaubteDateiendungen);

        Assert.Equal(
            1024,
            optionen.MaximaleDateigroesseBytes);
    }
}
