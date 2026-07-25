using Kompass.Api.B56Import;
using Kompass.Application.B56Import;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Tests.B56Import;

public sealed class B56SnapshotLebenszyklusControllerTests
{
    [Theory]
    [InlineData(
        B56SnapshotAktionStatus.Erfolgreich,
        typeof(OkObjectResult))]
    [InlineData(
        B56SnapshotAktionStatus.NichtGefunden,
        typeof(NotFoundObjectResult))]
    [InlineData(
        B56SnapshotAktionStatus.NichtZulaessig,
        typeof(ConflictObjectResult))]
    public async Task Bestaetigen_bildet_Anwendungsergebnis_auf_HTTP_ab(
        B56SnapshotAktionStatus status,
        Type erwarteterErgebnistyp)
    {
        var snapshot =
            status == B56SnapshotAktionStatus.NichtGefunden
                ? null
                : new B56ImportEintrag
                {
                    ImportId = Guid.NewGuid(),
                    ProjektId = Guid.NewGuid(),
                    SnapshotStatus =
                        B56SnapshotStatus.FachlichBestaetigt,
                    BestaetigtAm =
                        DateTimeOffset.Parse(
                            "2026-07-25T09:00:00Z")
                };

        var controller =
            new B56SnapshotLebenszyklusController(
                new LebenszyklusServiceFake(
                    new B56SnapshotAktionErgebnis(
                        status,
                        snapshot,
                        "Testnachricht")));

        var ergebnis =
            await controller.BestaetigenAsync(
                snapshot?.ProjektId ?? Guid.NewGuid(),
                snapshot?.ImportId ?? Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType(
            erwarteterErgebnistyp,
            ergebnis.Result);
    }

    private sealed class LebenszyklusServiceFake(
        B56SnapshotAktionErgebnis ergebnis)
        : IB56SnapshotLebenszyklusService
    {
        public Task<B56SnapshotAktionErgebnis> BestaetigenAsync(
            Guid projektId,
            Guid importId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ergebnis);
        }

        public Task<B56SnapshotAktionErgebnis> VerwerfenAsync(
            Guid projektId,
            Guid importId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ergebnis);
        }
    }
}
