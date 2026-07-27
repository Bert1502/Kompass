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

    public async Task<B56SnapshotVergleichAktionErgebnis> VergleichenAsync(
        Guid projektId,
        Guid altSnapshotId,
        Guid neuSnapshotId,
        CancellationToken cancellationToken = default)
    {
        var altFachdaten =
            await _importRegister.FachdatenAbrufenAsync(
                projektId,
                altSnapshotId,
                cancellationToken);

        var neuFachdaten =
            await _importRegister.FachdatenAbrufenAsync(
                projektId,
                neuSnapshotId,
                cancellationToken);

        if (altFachdaten is null || neuFachdaten is null)
        {
            return new B56SnapshotVergleichAktionErgebnis(
                B56SnapshotVergleichStatus.NichtGefunden,
                null,
                "Mindestens einer der angegebenen B56-Snapshots wurde nicht gefunden.");
        }

        var ergebnis =
            VergleicheSnapshots(
                altSnapshotId,
                neuSnapshotId,
                altFachdaten,
                neuFachdaten);

        return new B56SnapshotVergleichAktionErgebnis(
            B56SnapshotVergleichStatus.Erfolgreich,
            ergebnis,
            "Der Vergleich der B56-Snapshots wurde erfolgreich durchgeführt.");
    }

    private static B56SnapshotVergleichErgebnis VergleicheSnapshots(
        Guid altSnapshotId,
        Guid neuSnapshotId,
        B56ImportPipelineErgebnis alt,
        B56ImportPipelineErgebnis neu)
    {
        return new B56SnapshotVergleichErgebnis(
            altSnapshotId,
            neuSnapshotId,
            VergleicheAlternativen(alt, neu),
            VergleicheKennwerte(
                alt.Bestandskennwerte,
                neu.Bestandskennwerte),
            VergleicheBauteile(
                alt.Bauteile,
                neu.Bauteile));
    }

    private static IReadOnlyList<B56AlternativenVergleich> VergleicheAlternativen(
        B56ImportPipelineErgebnis alt,
        B56ImportPipelineErgebnis neu)
    {
        var altNachPosition =
            alt.Modernisierungsalternativen
                .Where(a => a.Position >= 1 && a.Position <= 9)
                .ToDictionary(a => a.Position);

        var neuNachPosition =
            neu.Modernisierungsalternativen
                .Where(a => a.Position >= 1 && a.Position <= 9)
                .ToDictionary(a => a.Position);

        var allePositionen =
            altNachPosition.Keys
                .Union(neuNachPosition.Keys)
                .OrderBy(p => p);

        var vergleiche = new List<B56AlternativenVergleich>();

        foreach (var position in allePositionen)
        {
            var altAlternative =
                altNachPosition.GetValueOrDefault(position);

            var neuAlternative =
                neuNachPosition.GetValueOrDefault(position);

            B56VergleichsArt art;

            if (altAlternative is null)
            {
                art = B56VergleichsArt.Hinzugefuegt;
            }
            else if (neuAlternative is null)
            {
                art = B56VergleichsArt.Entfernt;
            }
            else
            {
                art = B56VergleichsArt.Unveraendert;
            }

            var kennwerteVergleich =
                VergleicheKennwerte(
                    altAlternative?.Kennwerte ?? Array.Empty<B56Kennwert>(),
                    neuAlternative?.Kennwerte ?? Array.Empty<B56Kennwert>());

            var bauteilVergleich =
                VergleicheBauteile(
                    altAlternative?.Bauteile ?? Array.Empty<B56Bauteil>(),
                    neuAlternative?.Bauteile ?? Array.Empty<B56Bauteil>());

            if (art == B56VergleichsArt.Unveraendert &&
                (kennwerteVergleich.Any(
                     k => k.Art != B56VergleichsArt.Unveraendert) ||
                 bauteilVergleich.Any(
                     b => b.Art != B56VergleichsArt.Unveraendert) ||
                 altAlternative?.Bezeichnung !=
                 neuAlternative?.Bezeichnung))
            {
                art = B56VergleichsArt.Geaendert;
            }

            vergleiche.Add(
                new B56AlternativenVergleich(
                    position,
                    art,
                    altAlternative?.Bezeichnung,
                    neuAlternative?.Bezeichnung,
                    kennwerteVergleich,
                    bauteilVergleich));
        }

        return vergleiche;
    }

    private static IReadOnlyList<B56KennwertVergleich> VergleicheKennwerte(
        IEnumerable<B56Kennwert> altKennwerte,
        IEnumerable<B56Kennwert> neuKennwerte)
    {
        var altNachName =
            altKennwerte
                .GroupBy(k => k.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    StringComparer.OrdinalIgnoreCase);

        var neuNachName =
            neuKennwerte
                .GroupBy(k => k.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    StringComparer.OrdinalIgnoreCase);

        var alleNamen =
            altNachName.Keys
                .Union(neuNachName.Keys, StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase);

        var vergleiche = new List<B56KennwertVergleich>();

        foreach (var name in alleNamen)
        {
            var altKennwert =
                altNachName.GetValueOrDefault(name);

            var neuKennwert =
                neuNachName.GetValueOrDefault(name);

            var einheit =
                altKennwert?.Einheit ??
                neuKennwert?.Einheit ??
                string.Empty;

            B56VergleichsArt art;

            if (altKennwert is null)
            {
                art = B56VergleichsArt.Hinzugefuegt;
            }
            else if (neuKennwert is null)
            {
                art = B56VergleichsArt.Entfernt;
            }
            else if (!AreApproximatelyEqual(
                         altKennwert.Wert,
                         neuKennwert.Wert))
            {
                art = B56VergleichsArt.Geaendert;
            }
            else
            {
                art = B56VergleichsArt.Unveraendert;
            }

            vergleiche.Add(
                new B56KennwertVergleich(
                    name,
                    einheit,
                    art,
                    altKennwert?.Wert,
                    neuKennwert?.Wert));
        }

        return vergleiche;
    }

    private static IReadOnlyList<B56BauteilVergleich> VergleicheBauteile(
        IEnumerable<B56Bauteil> altBauteile,
        IEnumerable<B56Bauteil> neuBauteile)
    {
        var altNachCode =
            altBauteile
                .GroupBy(
                    b => b.Bauteilcode,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    StringComparer.OrdinalIgnoreCase);

        var neuNachCode =
            neuBauteile
                .GroupBy(
                    b => b.Bauteilcode,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    StringComparer.OrdinalIgnoreCase);

        var alleCodes =
            altNachCode.Keys
                .Union(neuNachCode.Keys, StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase);

        var vergleiche = new List<B56BauteilVergleich>();

        foreach (var code in alleCodes)
        {
            var altBauteil =
                altNachCode.GetValueOrDefault(code);

            var neuBauteil =
                neuNachCode.GetValueOrDefault(code);

            B56VergleichsArt art;

            if (altBauteil is null)
            {
                art = B56VergleichsArt.Hinzugefuegt;
            }
            else if (neuBauteil is null)
            {
                art = B56VergleichsArt.Entfernt;
            }
            else if (!AreApproximatelyEqual(
                         altBauteil.UWert,
                         neuBauteil.UWert) ||
                     !AreApproximatelyEqual(
                         altBauteil.Flaeche,
                         neuBauteil.Flaeche) ||
                     !string.Equals(
                         altBauteil.Bezeichnung,
                         neuBauteil.Bezeichnung,
                         StringComparison.OrdinalIgnoreCase))
            {
                art = B56VergleichsArt.Geaendert;
            }
            else
            {
                art = B56VergleichsArt.Unveraendert;
            }

            vergleiche.Add(
                new B56BauteilVergleich(
                    code,
                    art,
                    altBauteil?.Bezeichnung,
                    neuBauteil?.Bezeichnung,
                    altBauteil?.UWert,
                    neuBauteil?.UWert,
                    altBauteil?.Flaeche,
                    neuBauteil?.Flaeche));
        }

        return vergleiche;
    }

    private static bool AreApproximatelyEqual(
        double a,
        double b,
        double toleranz = 1e-9)
    {
        return Math.Abs(a - b) <= toleranz;
    }
}
