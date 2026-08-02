# Fachdatenkataloge

## Zweck

KOMPASS integriert externe Fachdatenschemata für Regelwerke, Förderung, Wirtschaftlichkeit, Maßnahmen, Materialien und Projekte in die bestehende zentrale EF-Core-Datenbank. Die sechs gelieferten SQLite-Dateien sind Importquellen, keine zusätzlichen Laufzeitdatenbanken.

Verbindliche Architekturentscheidung: [`ADR-0009-FACHDATENKATALOGE.md`](../adr/ADR-0009-FACHDATENKATALOGE.md).

## Implementierter Stand

- zentrale `Fachdatenquelle` mit Quellenart, Referenz, Gültigkeit, Abrufdatum und SHA-256;
- gemeinsamer Fachdatenstatus von Entwurf bis Archivierung;
- versioniertes Regelwerk mit typisierten numerischen oder textlichen Anforderungen;
- versionierter Maßnahmenkatalog und Kategorien;
- Materialkatalog und Kategorien als erweiterbare Grundlage;
- Fördergeber und strukturierte Fördertatbestände im bestehenden Funding-Modul;
- wirtschaftliche Zeitreihen mit eindeutigen Stichtagswerten;
- konkrete Projektmaßnahmen mit Referenz auf Maßnahmenkatalog und optionaler B56-Modernisierungsalternative;
- EF-Migration `AddFachdatenkataloge`;
- read-only Schema- und Integritätsprüfung der sechs SQLite-Quellen;
- idempotenter Import der sicheren vorbereiteten Stammdaten, Kategorien und Maßnahmen als Entwurf;
- API-Endpunkte für Dry Run und Import.

## Konfiguration

Ein unveränderlicher Referenz-Seed wird versionsbezogen unter `data/fachdatenbanken/v1.0.0/` gepflegt. Für den Betrieb wird immer eine vollständige Kopie in ein lokales, beschreibbares Verzeichnis außerhalb des Repositorys verwendet; die eingecheckten Referenzdateien werden nie zur Laufzeit verändert.

Das lokale Quellverzeichnis wird außerhalb des Repositorys konfiguriert:

```json
{
  "Fachdatenbanken": {
    "Verzeichnis": "D:\\Fachdatenbanken"
  }
}
```

Es wird kein benutzerspezifischer absoluter Pfad eingecheckt.

## API

- `GET /api/fachdatenbanken/pruefen`: prüft Dateien, Schema-Version, Tabellen und SQLite-Integrität; keine Datenänderung.
- `POST /api/fachdatenbanken/importieren`: wiederholt die Prüfung und importiert ausschließlich unterstützte Daten idempotent.

Fehlende Dateien, abweichende Schema-Versionen oder fehlende Tabellen blockieren den Import.

## Importierte Daten aus Schema 1.0.0

- Fördergeber;
- Maßnahmenkategorien;
- Maßnahmenkatalogeinträge;
- Materialkategorien.

Die gelieferten Regelwerke, Förderprogramme, Wirtschaftlichkeitswerte und Materialien sind leer und erzeugen deshalb keine erfundenen Datensätze. Maßnahmen ohne Quelle bleiben Entwurf.

## Nicht übernommen

- das konkurrierende Projektschema aus `06_ProjektDB.sqlite`;
- datenbankübergreifende Referenz-IDs ohne validierte Zielbeziehung;
- separate `data_source`, `db_info` oder `schema_history`-Tabellen je Fachgebiet;
- automatische Projektaktualisierung;
- eigene B56-, B02-, ThermCAD-, U-Wert-, Feuchte- oder IFC-/gbXML-Berechnung.

## Weiterer Ausbau

Noch leere Quelltabellen können später über zusätzliche, typisierte Importadapter erschlossen werden. Jede Erweiterung benötigt Quellpflicht, Versionsmapping, Domainvalidierung und Tests; ein generischer Schlüssel-Wert-Import ist nicht zulässig.
