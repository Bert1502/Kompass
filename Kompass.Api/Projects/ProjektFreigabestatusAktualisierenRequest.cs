using Kompass.Domain.Projects;

namespace Kompass.Api.Projects;

public sealed class ProjektFreigabestatusAktualisierenRequest
{
    public Freigabestatus Freigabestatus { get; set; }
}
