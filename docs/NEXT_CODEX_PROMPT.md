Du arbeitest im Repository Bert1502/KOMPASS.

Lies vor jeder Änderung vollständig:

- docs/FUNCTIONAL_SPECIFICATION.md
- docs/modules/WORKFLOW_B56_IMPORT.md
- docs/adr/ADR-0008-B56-SNAPSHOT-UND-PROJEKTMODELL.md
- CODEX.md beziehungsweise docs/CODEX.md

Diese Dokumente sind für die fachliche Umsetzung verbindlich.

Nächster Auftrag:

1. Vergleiche den aktuellen Code mit ADR-0008 und der B56-Fachspezifikation.
2. Erstelle unter docs/reviews/B56_GAP_ANALYSIS.md eine konkrete Gap-Analyse.
3. Liste:
   - bereits erfüllt,
   - teilweise erfüllt,
   - nicht erfüllt,
   - notwendige Datenbankmigrationen,
   - notwendige Tests,
   - Risiken.
4. Nimm in diesem Arbeitspaket keine funktionalen Quellcodeänderungen vor.
5. Aktualisiere keine fachliche Entscheidung eigenmächtig.
6. Führe dotnet restore, dotnet build und dotnet test aus.
7. Erstelle einen Commit und Pull Request ausschließlich für Dokumentation und Gap-Analyse.
