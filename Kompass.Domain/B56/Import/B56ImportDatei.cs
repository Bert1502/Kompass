using Kompass.Domain.Common;

namespace Kompass.Domain.B56.Import;

public sealed class B56ImportDatei : AggregateRoot
{
    private readonly List<B56ImportZeile> _zeilen = new();

    private B56ImportDatei()
    {
        Dateiname = string.Empty;
    }

    public B56ImportDatei(
        Guid id,
        string dateiname)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(dateiname))
        {
            throw new DomainException(
                "Dateiname darf nicht leer sein.");
        }

        Dateiname = dateiname.Trim();
        ImportZeitpunkt = DateTime.UtcNow;
    }

    public string Dateiname { get; private set; }

    public DateTime ImportZeitpunkt { get; private set; }

    public IReadOnlyCollection<B56ImportZeile> Zeilen =>
        _zeilen.AsReadOnly();

    public void ZeileHinzufuegen(
        B56ImportZeile zeile)
    {
        ArgumentNullException.ThrowIfNull(zeile);

        _zeilen.Add(zeile);
    }
}