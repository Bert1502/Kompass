using Kompass.Desktop.Models;
using Kompass.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Kompass.Desktop.Services;

public sealed class ProjektNavigationService
    : IProjektNavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public ProjektNavigationService(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void ProjektOeffnen(
        ProjektUebersichtDto projekt)
    {
        ArgumentNullException.ThrowIfNull(projekt);

        var fenster =
            _serviceProvider
                .GetRequiredService<ProjektWindow>();

        var viewModel =
            (ProjektWorkspaceViewModel)fenster.DataContext;

        viewModel.ProjektLaden(projekt);

        fenster.Show();
    }
}
