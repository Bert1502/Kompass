namespace Kompass.Application.B56Import;

public sealed class B56ImportErgebnis
{
    private readonly List<B56ImportMeldung> _meldungen = [];

    public B56ImportStatus Status { get; private init; }

    public Guid ProjektId { get; private init; }

    public string Quelldateipfad { get; private init; } = string.Empty;

    public B56ImportEintrag? ImportEintrag { get; private init; }

    public IReadOnlyList<B56ImportMeldung> Meldungen => _meldungen;

    public bool IstErfolgreich =>
        Status == B56ImportStatus.Erfolgreich;

    private B56ImportErgebnis()
    {
    }

    public static B56ImportErgebnis Erfolgreich(
        B56ImportEintrag eintrag,
        string quelldateipfad)
    {
        ArgumentNullException.ThrowIfNull(eintrag);

        return new B56ImportErgebnis
        {
            Status = B56ImportStatus.Erfolgreich,
            ProjektId = eintrag.ProjektId,
            Quelldateipfad = quelldateipfad,
            ImportEintrag = eintrag
        };
    }

    public static B56ImportErgebnis BereitsImportiert(
        B56ImportEintrag eintrag,
        string quelldateipfad)
    {
        ArgumentNullException.ThrowIfNull(eintrag);

        var ergebnis = new B56ImportErgebnis
        {
            Status = B56ImportStatus.BereitsImportiert,
            ProjektId = eintrag.ProjektId,
            Quelldateipfad = quelldateipfad,
            ImportEintrag = eintrag
        };

        ergebnis.MeldungHinzufuegen(
            B56Meldungstyp.Information,
            "B56-BEREITS-IMPORTIERT",
            "Diese Datei wurde bereits importiert.");

        return ergebnis;
    }

    public static B56ImportErgebnis Abgelehnt(
        Guid projektId,
        string quelldateipfad,
        string code,
        string text)
    {
        var ergebnis = new B56ImportErgebnis
        {
            Status = B56ImportStatus.Abgelehnt,
            ProjektId = projektId,
            Quelldateipfad = quelldateipfad
        };

        ergebnis.MeldungHinzufuegen(
            B56Meldungstyp.Fehler,
            code,
            text);

        return ergebnis;
    }

    public static B56ImportErgebnis Fehlgeschlagen(
        Guid projektId,
        string quelldateipfad,
        string text)
    {
        var ergebnis = new B56ImportErgebnis
        {
            Status = B56ImportStatus.Fehlgeschlagen,
            ProjektId = projektId,
            Quelldateipfad = quelldateipfad
        };

        ergebnis.MeldungHinzufuegen(
            B56Meldungstyp.Fehler,
            "B56-FEHLER",
            text);

        return ergebnis;
    }

    public void MeldungHinzufuegen(
        B56Meldungstyp typ,
        string code,
        string text)
    {
        _meldungen.Add(
            new B56ImportMeldung(
                typ,
                code,
                text));
    }
}
