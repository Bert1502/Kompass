namespace Kompass.Application.B56Import;

public sealed record B56ImportAnfrage(
    Guid ProjektId,
    string Projektname,
    string Quelldateipfad);
