using Kompass.Desktop.Models;

namespace Kompass.Desktop.Services;

public interface IProjektNavigationService
{
    void ProjektOeffnen(
        ProjektUebersichtDto projekt);
}
