# ADR-0010 – Parallele Effizienzstandard-Einstufung

## Status

Akzeptiert am 3. August 2026.

## Entscheidung

KOMPASS bilanziert das Gebäude nicht erneut. Es stuft die von B56
berechneten energetischen Ergebnisse künftig anhand versionierter
Regelwerte selbstständig ein.

Der B56-Wert `BEG_ZIEL` aus `SCModernisierungen!C7` wird unverändert
und mit Herkunftsnachweis importiert. Er ist ausschließlich ein
Gegenkontrollwert und darf nicht als von KOMPASS ermitteltes Ergebnis
ausgegeben oder stillschweigend übernommen werden.

Der B56-Kontrollwert und die spätere KOMPASS-Einstufung bleiben im
Snapshotmodell getrennt. Eine Abweichung muss sichtbar ausgewiesen
werden und darf nicht automatisch aufgelöst werden.

## Konsequenzen

- Snapshot-Schema 2 enthält den optionalen B56-Kontrollwert mit
  Originaltext, Feldname, Arbeitsblatt, Zelladresse und Import-ID.
- Schema-1-Snapshots bleiben lesbar; ihnen fehlt der Kontrollwert.
- Die KOMPASS-Einstufung benötigt einen eigenen Anwendungsfall mit
  versioniertem Regelstand und nachvollziehbaren Eingangswerten.
- Kosten- und Förderwerte werden nicht aus B56 übernommen.
