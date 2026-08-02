namespace Kompass.Application.FachdatenImport;

public interface IFachdatenbankImportService
{
    Task<FachdatenimportErgebnis> PruefenAsync(string verzeichnis, CancellationToken cancellationToken = default);
    Task<FachdatenimportErgebnis> ImportierenAsync(string verzeichnis, CancellationToken cancellationToken = default);
}
