# KOMPASS-Fachdatenbanken – Referenz-Seed v1.0.0

Stand: 2. August 2026

Dieses Verzeichnis enthält den unveränderlichen, geprüften Referenz-Seed für den KOMPASS-Fachdatenimport. Die SQLite-Dateien sind Importquellen und Schemaentwürfe; sie sind keine produktiven Laufzeitdatenbanken.

## Dateien

- `01_RegelwerkDB.sqlite`: GEG, 8 Bauteiltypen und 11 Anforderungen aus Anlage 7.
- `02_FoerderDB.sqlite`: BEG EM und KfW 261 mit Fördertatbeständen, Konditionen, Boni und Nachweisen.
- `03_WirtschaftlichkeitsDB.sqlite`: amtliche Haushaltsstrom- und Erdgaspreise für Deutschland, 2. Halbjahr 2025.
- `04_MassnahmenDB.sqlite`: 12 Maßnahmen, 2 Maßnahmenpakete sowie Bauteil- und Nachweiszuordnungen.
- `05_MaterialDB.sqlite`: 10 generische Materialstämme auf Grundlage der ÖKOBAUDAT-Kategorien.
- `06_ProjektDB.sqlite`: Schema ohne fiktive Projekte; Projektdaten entstehen ausschließlich aus realen KOMPASS-Prozessen.

## Verwendung

1. Den kompletten Ordner in ein lokales, beschreibbares Verzeichnis außerhalb des Repositorys kopieren.
2. Dieses Verzeichnis über `Fachdatenbanken:Verzeichnis` konfigurieren.
3. Zuerst `GET /api/fachdatenbanken/pruefen` ausführen.
4. Erst nach erfolgreicher Prüfung `POST /api/fachdatenbanken/importieren` aufrufen.

Die Dateien in diesem Repository werden nicht zur Laufzeit beschrieben. Eine fachliche Aktualisierung erzeugt einen neuen Versionsordner; veröffentlichte Seeds werden nicht überschrieben.

## Primärquellen

- Gebäudeenergiegesetz: https://www.gesetze-im-internet.de/geg/
- BAFA, BEG-Einzelmaßnahmen: https://www.bafa.de/DE/Energie/Effiziente_Gebaeude/effiziente_gebaeude_node.html
- KfW, Wohngebäude – Kredit 261: https://www.kfw.de/inlandsfoerderung/Privatpersonen/Bestehende-Immobilie/Foerderprodukte/Bundesfoerderung-fuer-effiziente-Gebaeude-Wohngebaeude-Kredit-(261)/
- Destatis, Erdgas- und Stromdurchschnittspreise: https://www.destatis.de/DE/Presse/Pressemitteilungen/2026/03/PD26_111_61243.html
- ÖKOBAUDAT: https://www.oekobaudat.de/

## Freigabegrenzen

- Förderprogramme stehen unter Haushaltsvorbehalt. Vor jeder Projektbewertung sind Gültigkeit und Konditionen erneut anhand der amtlichen Quelle zu prüfen.
- KOMPASS führt keine eigene energetische Berechnung durch; B56, B02 und ThermCAD bleiben zuständige Fachprogramme.
- Materialstämme enthalten keine Wärmeleitfähigkeiten, EPD- oder Modulwerte ohne eindeutige Datensatz-ID und funktionale Einheit.
- Wirtschaftliche Werte sind amtliche Ist-Durchschnittspreise, keine Prognosen.
- Die Daten ersetzen keine fachliche, rechtliche oder förderrechtliche Einzelfallprüfung.

## Technische Prüfung

Alle sechs Dateien wurden mit `PRAGMA integrity_check` und `PRAGMA foreign_key_check` geprüft. Es bestehen keine Integritäts- oder Fremdschlüsselfehler.
