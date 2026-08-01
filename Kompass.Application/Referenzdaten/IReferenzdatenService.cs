using Kompass.Domain.Referenzdaten;

namespace Kompass.Application.Referenzdaten;

public interface IReferenzdatenService
{
    Task<IReadOnlyList<Referenzdatensatz>> ListenAsync(
        CancellationToken cancellationToken = default);

    Task<Referenzdatensatz> SpeichernAsync(
        Referenzdatensatz datensatz,
        CancellationToken cancellationToken = default);

    Task<ReferenzwertAufloesung?> WertAufloesenAsync(
        ReferenzwertAnfrage anfrage,
        CancellationToken cancellationToken = default);

    Task<ReferenzwertAbweichung> ProjektabweichungSetzenAsync(
        ProjektabweichungAnfrage anfrage,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReferenzwertAbweichung>> ProjektabweichungenListenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default);

    Task<ReferenzdatenSynchronisationsErgebnis> SynchronisierenAsync(
        CancellationToken cancellationToken = default);
}
