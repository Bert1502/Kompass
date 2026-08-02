using Kompass.Domain.Common;

namespace Kompass.Domain.Fachdaten;

public sealed class Fachdatenquelle : AggregateRoot
{
    private Fachdatenquelle()
    {
        FachlicheId = string.Empty;
        Name = string.Empty;
        Quellenart = string.Empty;
    }

    public Fachdatenquelle(Guid id, string fachlicheId, string name, string quellenart)
        : base(id)
    {
        FachlicheId = Pflichtwert(fachlicheId, nameof(fachlicheId));
        Name = Pflichtwert(name, nameof(name));
        Quellenart = Pflichtwert(quellenart, nameof(quellenart));
    }

    public string FachlicheId { get; private set; }
    public string Name { get; private set; }
    public string Quellenart { get; private set; }
    public string? Referenz { get; private set; }
    public DateOnly? GueltigAb { get; private set; }
    public DateOnly? GueltigBis { get; private set; }
    public DateOnly? AbgerufenAm { get; private set; }
    public string? PruefsummeSha256 { get; private set; }
    public string? Notizen { get; private set; }

    public void Beschreiben(string? referenz, DateOnly? gueltigAb, DateOnly? gueltigBis, DateOnly? abgerufenAm, string? pruefsummeSha256, string? notizen)
    {
        if (gueltigAb.HasValue && gueltigBis.HasValue && gueltigBis < gueltigAb)
        {
            throw new DomainException("GueltigBis darf nicht vor GueltigAb liegen.");
        }

        Referenz = Optional(referenz);
        GueltigAb = gueltigAb;
        GueltigBis = gueltigBis;
        AbgerufenAm = abgerufenAm;
        PruefsummeSha256 = Optional(pruefsummeSha256);
        Notizen = Optional(notizen);
    }

    private static string Pflichtwert(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new DomainException($"{name} ist erforderlich.") : value.Trim();

    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
