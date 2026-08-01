namespace Kompass.Api.Referenzdaten;

public sealed record ReferenzwertAufloesungResponse(
    Guid DatensatzId,
    string Parameterart,
    string Wert,
    string? Einheit,
    string Prioritaet,
    string Quelle,
    string Herausgeber,
    DateOnly GueltigAb,
    DateOnly? GueltigBis);
