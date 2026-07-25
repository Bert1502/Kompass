# ADR-0008 – B56-Import-Snapshot und Projektmodell

## Status

Akzeptiert

## Kontext

KOMPASS besitzt ein B56-Importmodell und ein bearbeitbares Projekt-Domänenmodell. Ohne klare Grenze besteht das Risiko konkurrierender fachlicher Wahrheiten.

## Entscheidung

Jeder B56-Import erzeugt einen unveränderlichen, versionierten Snapshot.

Der Snapshot bewahrt:

- Originaldatei,
- Dateihash,
- Importzeitpunkt,
- Parser- und Schema-Version,
- importierte B56-Daten,
- Warnungen und Validierungsergebnisse.

Das KOMPASS-Projektmodell ist eine separate, bearbeitbare Arbeitskopie.

Es enthält bestätigte Übernahmen aus Snapshots sowie manuelle Ergänzungen wie Kosten, Förderparameter, reale Verbräuche, Annahmen und Kommentare.

Ein erneuter Import erzeugt einen neuen Snapshot. Er überschreibt weder ältere Snapshots noch das Projektmodell automatisch.

Änderungen werden verglichen und nach fachlicher Bestätigung übernommen.

## Konsequenzen

- Importierte B56-Werte bleiben nachweisbar.
- Manuelle Ergänzungen bleiben erhalten.
- Historische Berechnungsstände können verglichen werden.
- Snapshot-Payloads benötigen eine Schema-Version.
- Für die Übernahme in das Projektmodell ist ein eigener Anwendungsfall erforderlich.
- Wirtschaftlichkeit und Berichtswesen arbeiten auf dem Projektmodell und referenzieren die zugrunde liegenden Snapshots.
