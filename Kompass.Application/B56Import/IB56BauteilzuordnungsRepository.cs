namespace Kompass.Application.B56Import;

public interface IB56BauteilzuordnungsRepository
{
    IReadOnlyList<B56Bauteilzuordnung> Laden();
}
