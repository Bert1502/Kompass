namespace Kompass.Application.B56Import;

public interface IB56BauteilregelRepository
{
    IReadOnlyList<B56Bauteilregel> Laden();
}
