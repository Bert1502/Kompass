# KOMPASS – KI-Referenz

Verbindliche Wissensbasis für KI-Agenten, die KOMPASS analysieren, dokumentieren oder implementieren.

## Priorität
1. aktueller Quellcode und Tests
2. ADRs
3. System- und Fachregeln
4. Modul- und Workflowdokumente
5. historische Checkpoints

## Leitplanken
- B56 bleibt alleiniger energetischer Rechenkern.
- ThermCAD bleibt Fachprogramm für Wärmebrückenberechnung.
- Keine fachliche IFC-/gbXML-Auswertung in KOMPASS.
- Jeder B56-Import archiviert die Originaldatei unverändert.
- Jeder Import erzeugt einen unveränderlichen Snapshot.
- Das Projektmodell ist bearbeitbar und vom Snapshot getrennt.
- Varianten und Modernisierungsalternativen sind unterschiedliche B56-Begriffe.
- B56-Bauteilcodes bleiben unverändert.
- Freigegebene Stände werden revisioniert, nicht überschrieben.

## Struktur
- `docs/00_SYSTEM`
- `docs/01_DOMAIN`
- `docs/02_MODULES`
- `docs/03_WORKFLOWS`
- `docs/04_IMPLEMENTATION`
- `docs/05_REFERENCE`
- `docs/06_DECISIONS`
- `docs/99_AI`
- `source`
