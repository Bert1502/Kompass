namespace Kompass.Application.B56Import;

public interface IB56TabellenFinder
{
    IReadOnlyList<B56Tabelle> Analysieren(
        B56Arbeitsmappe arbeitsmappe);
}
