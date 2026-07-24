namespace Kompass.Application.B56Import;

public interface IB56DateiPruefer
{
    B56DateiPruefung Pruefen(
        string dateipfad);
}
