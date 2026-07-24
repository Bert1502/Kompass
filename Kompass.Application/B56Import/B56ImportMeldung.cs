namespace Kompass.Application.B56Import;

public sealed record B56ImportMeldung(
    B56Meldungstyp Typ,
    string Code,
    string Text);
