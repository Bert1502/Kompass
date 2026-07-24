using Kompass.Desktop.Services;
using Kompass.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
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
        services.AddSingleton(
            new HttpClient
            {
                BaseAddress =
                    new Uri("https://localhost:7275/")
            });
            services.AddSingleton<IProjektApiClient, ProjektApiClient>();
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