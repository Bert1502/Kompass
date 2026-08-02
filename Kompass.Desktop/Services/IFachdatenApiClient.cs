using Kompass.Desktop.Models;

namespace Kompass.Desktop.Services;

public interface IFachdatenApiClient
{
    Task<FachdatenimportErgebnisDto> PruefenAsync(CancellationToken cancellationToken = default);
    Task<FachdatenimportErgebnisDto> ImportierenAsync(CancellationToken cancellationToken = default);
}
