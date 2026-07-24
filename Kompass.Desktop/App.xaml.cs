using Kompass.Desktop.Services;
using Kompass.Desktop.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Kompass.Desktop;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        KonfiguriereDienste(services);

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow =
            _serviceProvider.GetRequiredService<MainWindow>();

        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();

        base.OnExit(e);
    }

    private static void KonfiguriereDienste(
        IServiceCollection services)
    {
        var configuration =
            new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(
                    "appsettings.json",
                    optional: false,
                    reloadOnChange: false)
                .Build();

        var apiBasisadresse =
            configuration["Api:BaseAddress"];

        if (!Uri.TryCreate(
                apiBasisadresse,
                UriKind.Absolute,
                out var apiBasisUri))
        {
            throw new InvalidOperationException(
                "Die Konfiguration 'Api:BaseAddress' muss eine absolute URI enthalten.");
        }

        services.AddHttpClient<IProjektApiClient, ProjektApiClient>(
            client =>
            {
                client.BaseAddress = apiBasisUri;
            });

        services.AddHttpClient<IB56ImportApiClient, B56ImportApiClient>(
            client =>
            {
                client.BaseAddress = apiBasisUri;
            });

        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IDateiDialogService, DateiDialogService>();
        services.AddSingleton<IProjektNavigationService, ProjektNavigationService>();

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ProjektWorkspaceViewModel>();
        services.AddTransient<B56ImportViewModel>();

        services.AddTransient<MainWindow>();
        services.AddTransient<ProjektWindow>();
    }
}
