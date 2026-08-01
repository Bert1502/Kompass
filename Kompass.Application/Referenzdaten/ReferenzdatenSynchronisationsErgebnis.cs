namespace Kompass.Application.Referenzdaten;

public sealed record ReferenzdatenSynchronisationsErgebnis(
    IReadOnlyList<ReferenzdatenProviderErgebnis> ProviderErgebnisse,
    int AktualisierteDatensaetze,
    bool LokalerFallbackVerwendet);

public sealed record ReferenzdatenProviderErgebnis(
    string ProviderName,
    int ImportierteDatensaetze,
    string? Fehler);
