using Kompass.Domain.Projects;

namespace Kompass.Api.Projects;

public sealed class ProjektdatenAktualisierenRequest
{
    public string? InterneBezeichnung { get; init; }

    public Bearbeitungsstatus Bearbeitungsstatus { get; init; }
        = Bearbeitungsstatus.InBearbeitung;
}
