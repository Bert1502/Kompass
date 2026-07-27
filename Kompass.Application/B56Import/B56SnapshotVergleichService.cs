namespace Kompass.Application.B56Import;

public sealed class B56SnapshotVergleichService
    : IB56SnapshotVergleichService
{
    private readonly IB56ImportRegister _importRegister;

    public B56SnapshotVergleichService(
        IB56ImportRegister importRegister)
    {
        _importRegister = importRegister;
    }

    public async Task<B56SnapshotVergleichErgebnis> VergleichenAsync(
        Guid projektId,
        Guid vorgaengerSnapshotId,
        Guid nachfolgerSnapshotId,
        CancellationToken cancellationToken = default)
    {
        var vorgaengerFachdaten =
            await _importRegister.FachdatenAbrufenAsync(
                projektId,
                vorgaengerSnapshotId,
                cancellationToken);

        if (vorgaengerFachdaten is null)
        {
            return new B56SnapshotVergleichErgebnis(
                B56SnapshotVergleichStatus.NichtGefunden,
                null,
                $"Der Vorgänger-Snapshot '{vorgaengerSnapshotId}' wurde nicht gefunden.");
        }

        var nachfolgerFachdaten =
            await _importRegister.FachdatenAbrufenAsync(
                projektId,
                nachfolgerSnapshotId,
                cancellationToken);

        if (nachfolgerFachdaten is null)
        {
            return new B56SnapshotVergleichErgebnis(
                B56SnapshotVergleichStatus.NichtGefunden,
                null,
                $"Der Nachfolger-Snapshot '{nachfolgerSnapshotId}' wurde nicht gefunden.");
        }

        var vergleich = new B56SnapshotVergleich
        {
            ProjektId = projektId,
            VorgaengerSnapshotId = vorgaengerSnapshotId,
            NachfolgerSnapshotId = nachfolgerSnapshotId,
            BestandskennwertVergleiche =
                VergleicheKennwerte(
                    vorgaengerFachdaten.Bestandskennwerte,
                    nachfolgerFachdaten.Bestandskennwerte),
            AlternativVergleiche =
                VergleicheAlternativen(
                    vorgaengerFachdaten.Modernisierungsalternativen,
                    nachfolgerFachdaten.Modernisierungsalternativen),
            GesamtbauteilVergleiche =
                VergleicheBauteile(
                    vorgaengerFachdaten.Bauteile,
                    nachfolgerFachdaten.Bauteile)
        };

        return new B56SnapshotVergleichErgebnis(
            B56SnapshotVergleichStatus.Erfolgreich,
            vergleich,
            vergleich.HatAenderungen
                ? "Der Vergleich enthält Änderungen."
                : "Zwischen den beiden Snapshots wurden keine Änderungen festgestellt.");
    }

    private static IReadOnlyList<B56KennwertVergleich> VergleicheKennwerte(
        IReadOnlyList<B56Kennwert> alt,
        IReadOnlyList<B56Kennwert> neu)
    {
        var altNachName =
            alt.ToDictionary(
                k => k.Name,
                StringComparer.OrdinalIgnoreCase);

        var neuNachName =
            neu.ToDictionary(
                k => k.Name,
                StringComparer.OrdinalIgnoreCase);

        var alleNamen =
            altNachName.Keys
                .Union(
                    neuNachName.Keys,
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    n => n,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var ergebnisse =
            new List<B56KennwertVergleich>(
                alleNamen.Count);

        foreach (var name in alleNamen)
        {
            var hatAlt =
                altNachName.TryGetValue(
                    name,
                    out var altKennwert);

            var hatNeu =
                neuNachName.TryGetValue(
                    name,
                    out var neuKennwert);

            B56VergleichsAenderung aenderung;

            if (hatAlt && hatNeu)
            {
                aenderung =
                    Math.Abs(
                        altKennwert!.Wert -
                        neuKennwert!.Wert) < 1e-9
                        ? B56VergleichsAenderung.Unveraendert
                        : B56VergleichsAenderung.Geaendert;
            }
            else if (hatNeu)
            {
                aenderung = B56VergleichsAenderung.Hinzugefuegt;
            }
            else
            {
                aenderung = B56VergleichsAenderung.Entfernt;
            }

            ergebnisse.Add(
                new B56KennwertVergleich(
                    name,
                    altKennwert?.Einheit
                        ?? neuKennwert?.Einheit
                        ?? string.Empty,
                    hatAlt
                        ? altKennwert!.Wert
                        : null,
                    hatNeu
                        ? neuKennwert!.Wert
                        : null,
                    aenderung));
        }

        return ergebnisse;
    }

    private static IReadOnlyList<B56BauteilVergleich> VergleicheBauteile(
        IReadOnlyList<B56Bauteil> alt,
        IReadOnlyList<B56Bauteil> neu)
    {
        var altNachCode =
            alt.GroupBy(
                    b => b.Bauteilcode,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    StringComparer.OrdinalIgnoreCase);

        var neuNachCode =
            neu.GroupBy(
                    b => b.Bauteilcode,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    StringComparer.OrdinalIgnoreCase);

        var alleCodes =
            altNachCode.Keys
                .Union(
                    neuNachCode.Keys,
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    c => c,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        var ergebnisse =
            new List<B56BauteilVergleich>(
                alleCodes.Count);

        foreach (var code in alleCodes)
        {
            var hatAlt =
                altNachCode.TryGetValue(
                    code,
                    out var altBauteil);

            var hatNeu =
                neuNachCode.TryGetValue(
                    code,
                    out var neuBauteil);

            B56VergleichsAenderung aenderung;

            if (hatAlt && hatNeu)
            {
                aenderung =
                    Math.Abs(
                        altBauteil!.UWert -
                        neuBauteil!.UWert) < 1e-9 &&
                    Math.Abs(
                        altBauteil.Flaeche -
                        neuBauteil.Flaeche) < 1e-9
                        ? B56VergleichsAenderung.Unveraendert
                        : B56VergleichsAenderung.Geaendert;
            }
            else if (hatNeu)
            {
                aenderung = B56VergleichsAenderung.Hinzugefuegt;
            }
            else
            {
                aenderung = B56VergleichsAenderung.Entfernt;
            }

            ergebnisse.Add(
                new B56BauteilVergleich(
                    code,
                    altBauteil?.Bezeichnung
                        ?? neuBauteil?.Bezeichnung
                        ?? string.Empty,
                    hatAlt
                        ? altBauteil!.UWert
                        : null,
                    hatNeu
                        ? neuBauteil!.UWert
                        : null,
                    hatAlt
                        ? altBauteil!.Flaeche
                        : null,
                    hatNeu
                        ? neuBauteil!.Flaeche
                        : null,
                    aenderung));
        }

        return ergebnisse;
    }

    private static IReadOnlyList<B56AlternativeVergleich> VergleicheAlternativen(
        IReadOnlyList<B56Modernisierungsalternative> alt,
        IReadOnlyList<B56Modernisierungsalternative> neu)
    {
        var altNachPosition =
            alt.ToDictionary(
                a => a.Position);

        var neuNachPosition =
            neu.ToDictionary(
                a => a.Position);

        var allePositionen =
            altNachPosition.Keys
                .Union(neuNachPosition.Keys)
                .OrderBy(p => p)
                .ToList();

        var ergebnisse =
            new List<B56AlternativeVergleich>(
                allePositionen.Count);

        foreach (var position in allePositionen)
        {
            var hatAlt =
                altNachPosition.TryGetValue(
                    position,
                    out var altAlternative);

            var hatNeu =
                neuNachPosition.TryGetValue(
                    position,
                    out var neuAlternative);

            if (hatAlt && hatNeu)
            {
                var kennwertVergleiche =
                    VergleicheKennwerte(
                        altAlternative!.Kennwerte
                            .ToList(),
                        neuAlternative!.Kennwerte
                            .ToList());

                var bauteilVergleiche =
                    VergleicheBauteile(
                        altAlternative.Bauteile
                            .ToList(),
                        neuAlternative.Bauteile
                            .ToList());

                var istBezeichnungGeaendert =
                    !string.Equals(
                        altAlternative.Bezeichnung,
                        neuAlternative.Bezeichnung,
                        StringComparison.Ordinal);

                var hatInhaltlicheAenderungen =
                    istBezeichnungGeaendert ||
                    kennwertVergleiche.Any(
                        k => k.Aenderung !=
                            B56VergleichsAenderung.Unveraendert) ||
                    bauteilVergleiche.Any(
                        b => b.Aenderung !=
                            B56VergleichsAenderung.Unveraendert);

                ergebnisse.Add(
                    new B56AlternativeVergleich(
                        position,
                        altAlternative.Bezeichnung,
                        neuAlternative.Bezeichnung,
                        hatInhaltlicheAenderungen
                            ? B56VergleichsAenderung.Geaendert
                            : B56VergleichsAenderung.Unveraendert,
                        kennwertVergleiche,
                        bauteilVergleiche));
            }
            else if (hatNeu)
            {
                ergebnisse.Add(
                    new B56AlternativeVergleich(
                        position,
                        string.Empty,
                        neuAlternative!.Bezeichnung,
                        B56VergleichsAenderung.Hinzugefuegt,
                        Array.Empty<B56KennwertVergleich>(),
                        Array.Empty<B56BauteilVergleich>()));
            }
            else
            {
                ergebnisse.Add(
                    new B56AlternativeVergleich(
                        position,
                        altAlternative!.Bezeichnung,
                        string.Empty,
                        B56VergleichsAenderung.Entfernt,
                        Array.Empty<B56KennwertVergleich>(),
                        Array.Empty<B56BauteilVergleich>()));
            }
        }

        return ergebnisse;
    }
}
