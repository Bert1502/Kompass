namespace Kompass.Application.B56Import.Domain;

/// <summary>
/// Beschreibt den vollständigen Gebäudebestand eines B56-Projekts.
/// Varianten bauen auf diesem Bestand auf.
/// </summary>
public sealed class B56Bestand
{
    private readonly List<B56Bauteil> _bauteile = [];
    private readonly List<B56Fenster> _fenster = [];
    private readonly List<B56Anlage> _anlagen = [];

    public IReadOnlyCollection<B56Bauteil> Bauteile => _bauteile;

    public IReadOnlyCollection<B56Fenster> Fenster => _fenster;

    public IReadOnlyCollection<B56Anlage> Anlagen => _anlagen;

    public void BauteilHinzufuegen(B56Bauteil bauteil)
    {
        ArgumentNullException.ThrowIfNull(bauteil);
        _bauteile.Add(bauteil);
    }

    public void FensterHinzufuegen(B56Fenster fenster)
    {
        ArgumentNullException.ThrowIfNull(fenster);
        _fenster.Add(fenster);
    }

    public void AnlageHinzufuegen(B56Anlage anlage)
    {
        ArgumentNullException.ThrowIfNull(anlage);
        _anlagen.Add(anlage);
    }
}
