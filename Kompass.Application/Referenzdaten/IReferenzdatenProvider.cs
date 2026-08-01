namespace Kompass.Application.Referenzdaten;

public interface IReferenzdatenProvider
{
    string ProviderName { get; }

    Task<IReadOnlyList<ReferenzdatenImportEintrag>> LadeReferenzdatenAsync(
        CancellationToken cancellationToken = default);
}
