# ADR-0009 – Fachdatenkataloge in der zentralen KOMPASS-Datenbank

**Status:** Akzeptiert
**Datum:** 2. August 2026

## Kontext

Für KOMPASS liegen sechs getrennte SQLite-Schemata für Regelwerke, Förderung, Wirtschaftlichkeit, Maßnahmen, Materialien und Projekte vor. Förderung, Wirtschaftlichkeit, Projekte und B56 sind im Repository bereits als Domainmodelle und EF-Core-Persistenz umgesetzt. Die gelieferten Schemata enthalten außerdem datenbankübergreifende Referenz-IDs, die SQLite nicht mit Fremdschlüsseln absichern kann.

## Entscheidung

KOMPASS behält eine einzige produktive, durch EF Core migrierte `kompass.db`.

- Regelwerke, Maßnahmen und Materialien werden als typisierte Katalogmodule ergänzt.
- Förderung und Wirtschaftlichkeit erweitern ihre bestehenden Module; es entstehen keine parallelen Modelle.
- Das gelieferte Projektschema wird nicht übernommen. Fehlende Projektbezüge werden am bestehenden Projektmodell ergänzt.
- Quellen werden zentral als `Fachdatenquelle` gespeichert.
- Katalogobjekte besitzen stabile fachliche Codes, Version, Gültigkeit und Freigabestatus.
- Freigegebene Versionen werden nicht überschrieben.
- Projekte referenzieren konkrete Versionen und werden nicht automatisch aktualisiert.
- Externe SQLite-Dateien werden nur durch einen schema-validierenden, protokollierten Import gelesen. Ein Dry Run verändert keine Daten.
- Seedwerte ohne Quelle werden höchstens als Entwurf importiert.

## Systemgrenzen

Regelwerk und Materialkatalog dokumentieren und strukturieren Fachwerte. Sie führen keine B56-, B02-, ThermCAD-, U-Wert-, Feuchte- oder IFC-/gbXML-Berechnung aus.

## Folgen

- Fachliche Beziehungen können als echte Fremdschlüssel abgebildet werden.
- Backup, Migration und Recovery bleiben auf eine Datenbank beschränkt.
- Die gelieferten sechs Dateien sind Importquellen und Schemaentwürfe, keine Laufzeitdatenbanken.
- Ein Import benötigt ein dauerhaftes Quell-ID-zu-Ziel-ID-Mapping und muss idempotent sein.
