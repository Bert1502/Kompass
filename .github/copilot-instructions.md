# KOMPASS – Repository-Anweisungen

Diese Datei ist die einzige repositoryweite Anweisungsquelle für GitHub Copilot und andere Agenten. Fachdetails stehen in den unten genannten kanonischen Dokumenten; historische Reviews und Baselines sind keine Anweisungen.

## Produkt und Systemgrenzen

- KOMPASS unterstützt Energieberatungsprojekte und ergänzt externe Fachprogramme.
- Solar-Computer B56 ist der einzige energetische Rechenkern. Keine eigene DIN-V-18599-Bilanzierung implementieren.
- KOMPASS stuft importierte B56-Ergebnisse anhand versionierter Regelwerte selbst ein. Den B56-Wert `BEG_ZIEL` nur als getrennten Gegenkontrollwert behandeln.
- ThermCAD bleibt das Fachprogramm für Wärmebrückenberechnung. Keine eigenen Psi-, Chi- oder fRsi-Berechnungen implementieren.
- Keine fachliche IFC-/gbXML-Auswertung, keine Geometrieauswertung und keinen IFC-Parser implementieren. Externe Modellreferenzen dürfen nur nach ausdrücklicher fachlicher Freigabe gespeichert werden.
- Keine Förder-, Norm- oder Parserregeln erfinden. Bei fachlicher Unklarheit anhalten und eine konkrete Entscheidung anfordern.

## Datenhoheit und Begriffe

- Originalimporte und Import-Snapshots sind unveränderlich. Herkunft, Hash, Archivbezug und Schema-/Parser-Version erhalten.
- Das Projektmodell ist bearbeitbar und vom Snapshot getrennt. Ein Reimport erzeugt einen neuen Stand und überschreibt Benutzeränderungen nicht automatisch.
- Historische Payloads nur mit expliziter Versions-, Migrations- und Kompatibilitätsstrategie ändern.
- Freigegebene Berichte und andere freigegebene Stände revisionieren, nicht überschreiben.
- `Variante` bezeichnet einen B56-Plan- oder Berechnungsstand. `Modernisierungsalternative` bezeichnet eine Maßnahme innerhalb einer Variante. Begriffe nicht gleichsetzen.
- B56-Bauteilcodes niemals still verändern. Unbekannte Werte nicht schätzen oder durch stille Fallbacks ersetzen.
- Kosten und Förderung nicht aus B56 importieren; sie gehören zum bearbeitbaren KOMPASS-Projektmodell.
- Fachsprache im Domainmodell bevorzugt deutsch; `Nutzungsprofil` statt eines allgemeinen Begriffs `Zone` verwenden.
- C#-Konventionen beibehalten: Interfaces beginnen mit `I`, asynchrone Methoden enden mit `Async`, `CancellationToken` ist der letzte Parameter.

## Architektur

- Abhängigkeiten nach innen halten: `Desktop/API -> Application -> Domain`; Persistence implementiert Application-Abstraktionen und darf Domain und Application referenzieren.
- Domain darf EF Core, HTTP, Dateisystem, WPF und andere Infrastruktur nicht kennen.
- Fachlogik gehört nicht in Controller, ViewModels oder technische Adapter.
- Domain-Entitäten nicht als HTTP-Verträge verwenden. Vertragsänderungen versionieren und durch Serialisierungs-/Kompatibilitätstests absichern.
- Bestehende Architektur nur im ausdrücklich beauftragten Umfang ändern. Große Komponenten erst nach Charakterisierungstests zerlegen.

## Vorgehen bei Änderungen

1. Betroffenen Code, Tests, ADRs und die fachliche Spezifikation lesen.
2. Den aktuellen Code- und Teststand als Implementierungsnachweis verwenden. Widersprüche zu Dokumenten melden, nicht still zugunsten eines historischen Dokuments entscheiden.
3. Änderungen klein und fachlich zusammenhängend halten. Parserfelder nur nach fachlicher Feldfreigabe ergänzen.
4. Passende Tests ergänzen. Migrationen, gespeicherte Payloads und HTTP-Verträge besonders auf Rückwärtskompatibilität prüfen.
5. Mindestens `dotnet test Kompass.sln --configuration Release` ausführen. Bei Build-, Projekt- oder CI-Änderungen zusätzlich die betroffenen Debug- und CI-Schritte prüfen.
6. Keine Commits, Pushes oder Pull Requests erstellen, sofern dies nicht ausdrücklich beauftragt wurde.

## Dokumentenpriorität

Bei Widersprüchen gilt für Implementierungsfragen:

1. aktueller Code und ausführbare Tests für den nachweislich implementierten Stand;
2. akzeptierte ADRs unter `docs/adr/` für Architekturentscheidungen;
3. `FUNCTIONAL_SPECIFICATION.md` für freigegebene Fachanforderungen;
4. aktuelle Modul- und Workflowdokumente unter `docs/`;
5. `docs/knowledge/` als erklärendes, nicht automatisch verbindliches Wissen;
6. datierte Reviews, Baselines und `Docs2/` nur als historische oder ergänzende Referenz.

Wichtige Einstiegsdokumente:

- `FUNCTIONAL_SPECIFICATION.md`
- `docs/adr/ADR-0008-B56-SNAPSHOT-UND-PROJEKTMODELL.md`
- `docs/modules/B56.md`
- `docs/modules/WORKFLOW_B56_IMPORT.md`
