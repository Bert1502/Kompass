# B56-Gap-Analyse

**Version:** 2.0  
**Stand:** 27. Juli 2026  
**Vorgängeranalyse:** Version 1.0 (vor Phasen 1–5.1)

## 1. Auftrag und Bewertungsgrundlage

Diese Analyse vergleicht den aktuellen Quellcode mit:

- `FUNCTIONAL_SPECIFICATION.md`, Version 1.0 vom 25. Juli 2026;
- `docs/modules/WORKFLOW_B56_IMPORT.md`;
- `docs/adr/ADR-0008-B56-SNAPSHOT-UND-PROJEKTMODELL.md`;
- `docs/CODEX.md`.

Bewertet wird ausschließlich der vorhandene Stand. Dieses
Arbeitspaket enthält keine funktionalen Quellcodeänderungen und trifft
keine neuen fachlichen Entscheidungen.

Bewertungsstufen:

- **Erfüllt:** Die Anforderung ist im aktuellen Umfang implementiert
  und automatisiert nachgewiesen.
- **Teilweise erfüllt:** Technische Grundlagen sind vorhanden, aber
  mindestens ein fachlich notwendiger Bestandteil fehlt.
- **Nicht erfüllt:** Es gibt keinen vollständigen Anwendungsfall oder
  kein belastbares Datenmodell für die Anforderung.

## 2. Zusammenfassung

Seit der ersten Gap-Analyse (Version 1.0) wurden die in Abschnitt 9
der Vorgängeranalyse benannten prioritären Arbeitspakete P1 vollständig
umgesetzt. Der B56-Pfad deckt jetzt das vollständige Snapshot-Fundament
gemäß ADR-0008 ab:

- Schema- und Parser-Versionierung mit Legacy-Unterstützung;
- fachlicher Lebenszyklus mit allen geforderten Zuständen und
  geprüften Statusübergängen;
- fachliche Bestätigung und Verwerfung über eigene API-Endpunkte;
- idempotente Übernahme in das Projektmodell;
- Vergleich zweier Snapshots nach stabilen Fachschlüsseln (Position,
  Kennwertname, Bauteilcode);
- vollständiger HTTP-End-to-End-Test;
- korrekte verbindliche Terminologie im Desktop;
- Grundlagentypen für die Wirtschaftlichkeitsberechnung.

Derzeit offen sind:

- vollständiger bearbeitbarer Projektstand (Auftraggeber,
  Standortdaten, Förderparameter, reale Verbräuche);
- feldweise Konfliktlösung beim Re-Import;
- Wirtschaftlichkeitsberechnungsservice (Domänenmodell vorhanden);
- Förderung, Berichtswesen, Wissensdatenbank, Wärmebrückenmanagement.

Das nächste Arbeitspaket gemäß Fachspezifikation Abschnitt 24 ist die
Wirtschaftlichkeitsberechnung, gefolgt von Förderung und
Berichtswesen.

## 3. Bereits erfüllt

### 3.1 Projektbezogener technischer Import

- API und Desktop übergeben eine konkrete Projekt-ID.
- Die API prüft, ob das Projekt existiert.
- Importhistorie und Detailabfragen sind nach Projekt-ID begrenzt.
- Der Importregister-Index unterstützt projektbezogene Abfragen.
- Gleiche Hashes in unterschiedlichen Projekten bleiben getrennt.

Nachweise:

- `Kompass.Api/B56Import/B56ImportController.cs`
- `Kompass.Persistence/Services/EfB56ImportRegister.cs`
- `Kompass.Tests/ProjektB56ImportBeziehungTests.cs`
- `Kompass.Tests/B56ImportControllerTests.cs`

### 3.2 Unterstützte Dateien und technische Vorprüfung

- `.xlsx` und `.xlsm` sind zugelassen.
- Pfad, Existenz, Dateigröße und leerer Inhalt werden geprüft.
- Eine OpenXML-/ZIP-Signatur wird geprüft.
- Zugriffs- und Sperrfehler werden behandelt.
- Die maximale Dateigröße ist konfigurierbar.

Nachweise:

- `Kompass.Persistence/B56Import/B56DateiPruefer.cs`
- `Kompass.Tests/B56DateiPrueferTests.cs`

### 3.3 Hash, Archiv und Duplikaterkennung

- SHA-256 wird vor der fachlichen Verarbeitung gebildet.
- Die Originaldatei wird projektbezogen archiviert.
- Archivziele werden nicht überschrieben.
- Der Archivinhalt kann durch erneute Hashbildung geprüft werden.
- Ein identischer Hash wird innerhalb eines Projekts erkannt.
- Bei einem Verarbeitungsfehler wird die neu erzeugte Archivkopie
  bestmöglich entfernt.

Nachweise:

- `Kompass.Application/B56Import/B56ImportService.cs`
- `Kompass.Persistence/Services/B56ArchivService.cs`
- `Kompass.Persistence/B56Import/Sha256HashService.cs`
- `Kompass.Tests/B56ArchivServiceTests.cs`
- `Kompass.Tests/Sha256HashServiceTests.cs`
- `Kompass.Tests/B56ImportServiceIntegrationTests.cs`

### 3.4 Technische Arbeitsmappenverarbeitung

- Arbeitsblätter, Zeilen, Zellen und wesentliche OpenXML-Zelltypen
  werden gelesen.
- Tabellarische Bereiche werden erkannt.
- Bekannte Bereiche aus `SCModernisierungen` werden fachlich
  zugeordnet.
- Unbekannte Bereiche werden nicht fachlich erfunden, sondern als
  Warnungen gemeldet.

Nachweise:

- `Kompass.Persistence/Services/OpenXmlB56ArbeitsmappenLeser.cs`
- `Kompass.Persistence/Services/B56TabellenFinder.cs`
- `Kompass.Persistence/Services/B56TabellenImportService.cs`
- `Kompass.Tests/OpenXmlB56ArbeitsmappenLeserTests.cs`
- `Kompass.Tests/B56TabellenImportServiceTests.cs`

### 3.5 Persistenz, Historie und Anzeige des erreichten Importumfangs

- Importmetadaten werden relational gespeichert.
- Das fachliche Pipeline-Ergebnis wird als JSON gespeichert.
- API-Endpunkte liefern Historie und Details.
- Interne Archivpfade werden nicht in den API-Antworten offengelegt.
- Der Desktop zeigt Bestandskennwerte, Bauteile,
  Modernisierungsalternativen und Warnungen.
- Historische Detailergebnisse können erneut geladen werden, solange
  das Projekt über die API erreichbar ist.

Nachweise:

- `Kompass.Persistence/Data/Entities/B56ImportEintragEntity.cs`
- `Kompass.Persistence/Services/EfB56ImportRegister.cs`
- `Kompass.Api/B56Import/B56ImportController.cs`
- `Kompass.Desktop/ViewModels/B56ImportViewModel.cs`
- `Kompass.Desktop/Views/B56ImportView.xaml`

### 3.6 Projektverwaltung und grundlegende Domain-Invarianten

- Projekte können angelegt, gelesen, umbenannt und gelöscht werden.
- Projektnamen werden bereinigt und validiert.
- Namenskonflikte werden erkannt.
- Modernisierungsalternativen können dem Projekt-Aggregat zugeordnet
  werden.
- Das Verhalten ist durch Domain- und SQLite-Tests abgesichert.

Nachweise:

- `Kompass.Domain/Projects/Projekt.cs`
- `Kompass.Persistence/Services/ProjektService.cs`
- `Kompass.Api/Projects/ProjekteController.cs`
- `Kompass.Tests/ProjektDomainTests.cs`
- `Kompass.Tests/ProjektServiceTests.cs`

### 3.7 Snapshot-Schema- und Parser-Versionierung

_Vormals nicht erfüllt (Version 1.0, Abschnitt 5.1)._

- `B56SnapshotVersionen` definiert konstante Schema- und
  Parser-Versionswerte.
- `SnapshotSchemaVersion` und `ParserVersion` werden beim Import
  gespeichert (relational, nicht nur im JSON).
- Legacy-Einträge erhalten beim Lesen die Kennung `legacy`.
- Unbekannte Versionen werden kontrolliert behandelt.
- EF-Core-Migrationen sichern den Schema-Übergang.

Nachweise:

- `Kompass.Application/B56Import/B56SnapshotVersionen.cs`
- `Kompass.Persistence/Data/Entities/B56ImportEintragEntity.cs`
- `Kompass.Persistence/Migrations/20260725075146_VersionB56Snapshots.cs`
- `Kompass.Tests/EfB56ImportRegisterTests.cs`

### 3.8 Fachlicher Importlebenszyklus

_Vormals nicht erfüllt (Version 1.0, Abschnitt 5.2)._

- `B56SnapshotStatus` umfasst alle fachlich geforderten Zustände:
  `TechnischGeprueft`, `MitWarnungen`, `Blockiert`,
  `FachlichBestaetigt`, `InProjektmodellUebernommen`, `Verworfen`.
- `B56SnapshotLebenszyklusService` prüft und erzwingt zulässige
  Statusübergänge.
- Bestätigungs- und Verwerfungszeitpunkte werden persistiert.
- API-Endpunkte `POST .../bestaetigen` und `POST .../verwerfen`
  exponieren den Lebenszyklus.
- Das Verhalten ist durch Unit- und HTTP-Tests abgesichert.

Nachweise:

- `Kompass.Application/B56Import/B56SnapshotStatus.cs`
- `Kompass.Application/B56Import/B56SnapshotLebenszyklusService.cs`
- `Kompass.Api/B56Import/B56SnapshotLebenszyklusController.cs`
- `Kompass.Persistence/Migrations/20260725084054_AddB56SnapshotLifecycle.cs`
- `Kompass.Tests/B56SnapshotLebenszyklusServiceTests.cs`
- `Kompass.Tests/B56SnapshotLebenszyklusControllerTests.cs`

### 3.9 Fachliche Bestätigung und Übernahme in das Projektmodell

_Vormals nicht erfüllt (Version 1.0, Abschnitt 5.3)._

- `B56ProjektmodellUebernahmeService` prüft den Snapshot-Status und
  ist idempotent.
- Nur fachlich bestätigte Snapshots dürfen übernommen werden.
- Alternativen und Bauteile werden aus dem Snapshot in das
  Projektmodell übertragen.
- `QuellSnapshotId` und `ProjektmodellVersion` werden am
  Projekt-Aggregat gespeichert.
- Ein bereits übernommener Snapshot kann ohne Fehler erneut aufgerufen
  werden.
- API-Endpunkt `POST .../in-projektmodell-uebernehmen` exponiert den
  Use-Case.

Nachweise:

- `Kompass.Application/B56Import/IB56ProjektmodellUebernahmeService.cs`
- `Kompass.Persistence/Services/B56ProjektmodellUebernahmeService.cs`
- `Kompass.Api/B56Import/B56ProjektmodellController.cs`
- `Kompass.Persistence/Migrations/20260725085558_AddB56ProjectModelOrigin.cs`
- `Kompass.Tests/B56ProjektmodellUebernahmeServiceTests.cs`

### 3.10 Re-Import und Snapshot-Vergleich

_Vormals nicht erfüllt (Version 1.0, Abschnitt 5.5)._

- `B56SnapshotVergleichService` vergleicht zwei Snapshots anhand
  stabiler Fachschlüssel:
  - Modernisierungsalternativen → B56-Position (1–9);
  - Bestandskennwerte → Kennwertname;
  - Bauteile → Bauteilcode.
- Jedes Element erhält einen Vergleichsstatus:
  `Unveraendert | Hinzugefuegt | Entfernt | Geaendert`.
- `B56Position` und `IstImAktuellenB56SnapshotVorhanden` sind am
  `Modernisierungsalternative`-Objekt modelliert.
- Alternativen, die in einem neuen Snapshot fehlen, bleiben im
  Projektmodell erhalten und werden als nicht mehr vorhanden
  gekennzeichnet.
- API-Endpunkt `GET .../vergleich?altSnapshotId&neuSnapshotId`
  exponiert das Ergebnis.

Nachweise:

- `Kompass.Application/B56Import/B56SnapshotVergleich.cs`
- `Kompass.Application/B56Import/B56SnapshotVergleichService.cs`
- `Kompass.Api/B56Import/B56SnapshotVergleichController.cs`
- `Kompass.Persistence/Migrations/20260725101500_TrackB56AlternativePresence.cs`
- `Kompass.Tests/B56SnapshotVergleichServiceTests.cs`

### 3.11 Vollständiger HTTP-End-to-End-Test

_Vormals nicht erfüllt (Version 1.0, Abschnitt 5.6)._

Der End-to-End-Test `B56ImportHttpEndToEndTests` deckt über echte
HTTP-Serialisierung den vollständigen Lebenszyklus ab:

1. Projekt anlegen;
2. Datei hochladen;
3. fachlich bestätigen;
4. in das Projektmodell übernehmen (idempotenter Zweitufruf);
5. zweiten Snapshot importieren (geänderter U-Wert);
6. Vergleich mit `Geaendert`-Nachweis für `AW01`.

Nachweise:

- `Kompass.Tests/B56ImportHttpEndToEndTests.cs`

### 3.12 Verbindliche Terminologie im Desktop

_Vormals nicht erfüllt (Version 1.0, Abschnitt 4.3 und R9)._

Alle zuvor abweichenden Beschriftungen in der Desktopansicht wurden
durch den verbindlichen Begriff `Modernisierungsalternative` ersetzt.

Nachweise:

- `Kompass.Desktop/Views/B56ImportView.xaml`

### 3.13 Wirtschaftlichkeits-Domänenmodell

_Neu seit Version 1.0, Phase 5.1._

Grundlagentypen für die spätere Wirtschaftlichkeitsberechnung sind im
Domänenmodell verankert:

- `Wirtschaftlichkeitsannahmen` (Aggregat): Betrachtungszeitraum,
  Diskontsatz, Inflation, CO₂-Pfad, Wartung, Nutzungsdauer,
  Restwert, Energieträgerannahmen-Sammlung.
- `EnergietraegerAnnahme` (Entity): Preis pro kWh und jährliche
  Preissteigerung je Energieträger.
- `Wirtschaftlichkeitsergebnis` (unveränderlicher Record): Kapitalwert,
  statische Amortisation, Kosten-Nutzen-Verhältnis, Restwert, Basis
  (bilanziert oder praktisch).
- `Energietraeger` (Enum): Erdgas, Heizöl, Fernwärme, Strom,
  Holzpellets, Holzhackschnitzel, Wärmepumpe, Sonstige.

Die Typen sind validiert und durch Unit-Tests abgesichert. Ein
Berechnungsservice und Persistenz sind noch nicht vorhanden.

Nachweise:

- `Kompass.Domain/Economics/Wirtschaftlichkeitsannahmen.cs`
- `Kompass.Domain/Economics/EnergietraegerAnnahme.cs`
- `Kompass.Domain/Economics/Wirtschaftlichkeitsergebnis.cs`
- `Kompass.Domain/Economics/Energietraeger.cs`
- `Kompass.Tests/WirtschaftlichkeitsannahmenTests.cs`

## 4. Teilweise erfüllt

### 4.1 Unveränderlicher Snapshot

`B56ImportEintragEntity`, `FachdatenJson`, Schema-Version und
Parser-Version bilden einen persistierten, identifizierbaren
Snapshot. Es existiert kein Updatepfad für den JSON-Inhalt.

Weiterhin nicht modelliert:

- strukturierte Fehler- und Warnungscodes im Snapshot (derzeit nur
  JSON-Payload);
- Audit-Trail zur bestätigenden Person oder dem technischen Akteur
  (Zeitpunkt ist vorhanden, Akteur fehlt);
- Reconciliation-Prozess nach Prozessabbruch zwischen Archiv und
  Datenbank;
- klare Recovery-Verfahren für Backup und Restore.

### 4.2 Warnungen und Validierungsergebnisse

Pipeline-Warnungen sind Bestandteil von `FachdatenJson` und werden
über die API und den Desktop angezeigt. Technische Ablehnungen
werden im unmittelbaren `B56ImportErgebnis` zurückgegeben.

Weiterhin fehlend:

- dauerhaft gespeicherte strukturierte Validierungscodes außerhalb
  des JSON-Payloads;
- explizite Unterscheidung blockierender und nicht blockierender
  Befunde in separaten Feldern (der Status `Blockiert` ist modelliert,
  aber nicht automatisch gesetzt);
- Auditdaten zur bestätigenden Person.

### 4.3 Modernisierungsalternativen

Bezeichnung, Beschreibung, Kennwerte, Bauteile, B56-Position und
`IstImAktuellenB56SnapshotVorhanden` sind vorhanden. Die
Desktopterminologie ist korrigiert.

Weiterhin fehlend:

- eindeutige Zuordnung zu einer B56-Variante oder einem
  Berechnungsstand (offener Punkt gemäß Spezifikation);
- technischer Nachweis der Begrenzung auf neun Alternativen je
  Variante in der Domain-Schicht;
- eindeutige Trennung zwischen B56-Bezeichnung und ergänzender
  interner Bezeichnung im Projektmodell;
- vollständige energetische Ergebnisse je Energieträger
  (abhängig von freigegebenen Feldlisten).

### 4.4 Bauteile und Kennwerte

Bauteilcode, Bezeichnung, Fläche und U-Wert sind im Importmodell
vorhanden. Vergleich nach Bauteilcode zwischen zwei Snapshots ist
implementiert.

Weiterhin fehlend:

- freigegebene vollständige Feldlisten je B56-Exportblatt (offener
  Punkt in der Fachspezifikation);
- vollständige Gebäudegrunddaten und Bezugsgrößen;
- freigegebene Nutzungsprofile beziehungsweise Zonenübersicht;
- fachliche Pflichtfeld- und Plausibilitätsregeln.

Diese Lücken können wegen der ausdrücklich offenen Feld- und
Mappingentscheidungen nicht eigenmächtig geschlossen werden.

### 4.5 Beziehung zwischen Projekt und Import

Projekt und Import teilen eine Projekt-ID. Die Importsuche ist
projektbezogen. Es gibt bewusst keine kaskadierende
Entity-Framework-Beziehung.

Offen bleibt:

- wie ein aufbewahrter Snapshot nach Projektlöschung zugänglich und
  verwaltet wird;
- ob Projektlöschung fachlich als Archivierung statt physischer
  Löschung modelliert werden muss;
- welche Datenschutz- und Aufbewahrungsregel physisches Löschen
  autorisiert.

### 4.6 Technische Konsistenz von Archiv und Datenbank

Die Importpipeline kompensiert viele Fehler durch Löschen der neu
erzeugten Archivdatei. Das reduziert verwaiste Dateien.

Weiterhin fehlend:

- dauerhafter Importzustand für abgebrochene Verarbeitung;
- Reconciliation-Prozess für Archiv und Datenbank;
- definierter Umgang mit Fehlern beim kompensierenden Löschen;
- Backup-, Restore- und Recovery-Verfahren.

### 4.7 Wirtschaftlichkeit

Das Domänenmodell (`Wirtschaftlichkeitsannahmen`, `EnergietraegerAnnahme`,
`Wirtschaftlichkeitsergebnis`, `Energietraeger`) ist vorhanden und
validiert.

Weiterhin fehlend:

- Persistenz für `Wirtschaftlichkeitsannahmen` und
  `EnergietraegerAnnahme` (keine EF-Core-Konfiguration,
  keine Migration);
- Berechnungsservice (bilanziert und praktisch);
- Zuordnung von `Wirtschaftlichkeitsannahmen` zu einer
  `Modernisierungsalternative` im Projektmodell;
- API-Endpunkte für Wirtschaftlichkeitsannahmen und -ergebnisse;
- Desktop-Ansicht.

## 5. Nicht erfüllt

### 5.1 Bearbeitbarer vollständiger Projektstand

Das Projektmodell enthält derzeit Name, `QuellSnapshotId`,
`ProjektmodellVersion`, Modernisierungsalternativen und
Kostenpositionen.

Noch nicht modelliert:

- Auftraggeber und Ansprechpartner;
- Standortdaten und Gebäudetyp;
- Bearbeitungs- und Freigabestatus;
- Förderparameter;
- reale Verbrauchsdaten;
- Berichtseinstellungen;
- nachvollziehbare abweichende Annahmen mit Herkunftsnachweis;
- Varianten beziehungsweise Berechnungsstände als eigenständige
  Entitäten.

### 5.2 Feldweise Bestätigung bei Re-Import

Der Vergleich zweier Snapshots ist implementiert. Was noch fehlt:

- feldweises Konfliktmodell (welches Feld aus Alt- oder Neustand
  übernehmen);
- explizite Benutzerentscheidung pro Konfliktfeld;
- Schutz manueller Ergänzungen gegen automatische Überschreibung;
- persistierte Konfliktentscheidungen mit Zeitstempel.

Diese Punkte setzen fachlich freigegebene Feldidentitäten voraus
(Abschnitt 23.4–5 der Fachspezifikation).

### 5.3 Wirtschaftlichkeitsberechnungsservice

Das Domänenmodell ist vorhanden (Abschnitt 3.13). Fehlend:

- Berechnungsservice für bilanzierte Wirtschaftlichkeit;
- Berechnungsservice für praktische Wirtschaftlichkeit;
- Zuordnungsmodell zwischen Alternative und Annahmen;
- Persistenz, API und Desktop.

### 5.4 Förderung

Kein Datenmodell, kein Service, keine API.

Vorgesehene Bereiche gemäß Fachspezifikation: BEG EM, KfW, EFRE,
KNN. Dieses Modul ist ausdrücklich nach der Wirtschaftlichkeit
priorisiert.

### 5.5 Berichtswesen

Kein Datenmodell, kein Service, keine API.

Vorgesehene Ausgaben: Energieberatungsbericht,
Wirtschaftlichkeitsbericht, Förderübersicht, Executive Summary,
Vergleich, Wärmebrückenübersicht, Prüferunterlagen, Präsentationen.

### 5.6 Wärmebrückenmanagement

Kein Datenmodell, kein Service, keine API.

Gemäß Fachspezifikation Abschnitt 16 sind Fall A (Markierung im
Plan) und Fall B (vorhandene Architekturdetails) mit dem Fachobjekt
Wärmebrücke zu implementieren.

### 5.7 Wissensdatenbank

Kein Datenmodell, kein Service, keine API.

Referenzdaten gemäß Fachspezifikation Abschnitt 19: Nutzungsdauern,
Wartungsansätze, Energiepreise, CO₂-Faktoren, U-Wert-Anforderungen,
Förderkriterien.

## 6. Notwendige Datenbankmigrationen

Die folgenden Migrationen ergeben sich aus dem noch offenen Ausbau
gemäß ADR-0008 und der Fachspezifikation. Bereits durchgeführte
Migrationen sind als erledigt gekennzeichnet.

### 6.1 ✅ Migration 1: Snapshot-Versionierung (erledigt)

`SnapshotSchemaVersion` und `ParserVersion` wurden mit
`20260725075146_VersionB56Snapshots` ergänzt.

### 6.2 ✅ Migration 2: Snapshot-Lebenszyklus (erledigt)

`SnapshotStatus`, `BestaetigtAm` und `VerworfenAm` wurden mit
`20260725084054_AddB56SnapshotLifecycle` ergänzt.

### 6.3 ✅ Migration 3: Herkunft im Projektmodell (erledigt)

`QuellSnapshotId` und `ProjektmodellVersion` am Projekt-Aggregat sowie
`QuellSnapshotId` an der Modernisierungsalternative wurden mit
`20260725085558_AddB56ProjectModelOrigin` ergänzt.

### 6.4 ✅ Migration 4: B56-Position und Vorhandensein (erledigt)

`B56Position` und `IstImAktuellenB56SnapshotVorhanden` an der
Modernisierungsalternative wurden mit
`20260725101500_TrackB56AlternativePresence` ergänzt.

### 6.5 Migration 5: Wirtschaftlichkeitsannahmen-Persistenz

Für das nächste Arbeitspaket werden benötigt:

- Tabelle `Wirtschaftlichkeitsannahmen` mit den Feldern des
  Aggregats;
- Tabelle `EnergietraegerAnnahmen` als Detailtabelle;
- Fremdschlüssel zur `Modernisierungsalternative`.

### 6.6 Migration 6: Audit-Trail für Bestätigung

Sobald Mehrbenutzerbetrieb spezifiziert ist:

- Akteur (Person oder technischer Akteur) für Bestätigung und
  Verwerfung;
- optional separate Ereignistabelle für Statushistorie.

Die genaue Modellierung ist fachlich noch offen.

### 6.7 Migration 7: Feldweise Konfliktentscheidungen

Erst nach Klärung stabiler Feldidentitäten und Konfliktregeln:

- alter und neuer Snapshot-Verweis;
- betroffener stabiler Fachschlüssel;
- Konfliktstatus und Benutzerentscheidung;
- Zeitstempel.

### 6.8 Kein Einführen kaskadierender Löschung

Eine neue relationale Verbindung zwischen Projekt und Snapshot darf
Snapshots nicht automatisch mit dem Projekt löschen. Die
Aufbewahrungsentscheidung ist offen.

## 7. Notwendige Tests

### 7.1 ✅ Snapshot-Versionierung (erledigt)

Abgedeckt durch `EfB56ImportRegisterTests` und
`B56ImportHttpEndToEndTests`.

### 7.2 ✅ Bestätigung und Projektübernahme (erledigt)

Abgedeckt durch `B56SnapshotLebenszyklusServiceTests`,
`B56SnapshotLebenszyklusControllerTests`,
`B56ProjektmodellUebernahmeServiceTests` und
`B56ImportHttpEndToEndTests`.

### 7.3 ✅ Re-Import und Vergleich (erledigt)

Abgedeckt durch `B56SnapshotVergleichServiceTests` und
`B56ImportHttpEndToEndTests`.

### 7.4 Für Wirtschaftlichkeit

- `Wirtschaftlichkeitsannahmen`-Persistenz (Roundtrip mit
  `EnergietraegerAnnahme`);
- bilanzierte Berechnung mit Testdaten;
- praktische Berechnung mit realen Verbrauchswerten;
- Zuordnung von Annahmen zu einer Modernisierungsalternative;
- API-Endpunkte für Lesen, Anlegen und Ändern von Annahmen;
- Änderung von Annahmen erzeugt ein neues Ergebnis, überschreibt
  das alte nicht.

### 7.5 Für Re-Import-Konfliktlösung

- manuell ergänzter Wert bleibt nach erneutem Import erhalten;
- bestätigte Konfliktentscheidung wird persistiert;
- abgelehnte Konfliktentscheidung ist nachvollziehbar.

### 7.6 Für Desktop

- Anzeige von Schema-, Parser- und Snapshot-Version;
- Anzeige blockierender Fehler getrennt von Warnungen;
- Anzeige von Wirtschaftlichkeitsergebnissen.

## 8. Risiken

### R1 – Wirtschaftlichkeitsannahmen ohne Persistenz

**Priorität: hoch.** Das Domänenmodell ist vorhanden, aber
Annahmen können noch nicht gespeichert werden. Berechnungen sind
daher nicht persistierbar.

**Maßnahme:** Persistenz als nächsten Schritt im
Wirtschaftlichkeitspaket umsetzen.

### R2 – Audit-Trail unvollständig

**Priorität: mittel.** Bestätigungs- und Verwerfungszeitpunkte
werden gespeichert. Die bestätigende Person oder der technische
Akteur fehlt.

**Maßnahme:** Akteur-Modellierung nach Klärung des
Mehrbenutzerbetriebskonzepts ergänzen.

### R3 – Erhaltene Snapshots sind nach Projektlöschung unerreichbar

**Priorität: hoch.** Die Daten bleiben erhalten, die reguläre API
verweigert aber Historie und Details für ein nicht mehr vorhandenes
Projekt.

**Maßnahme:** Vor Ausbau der Projektlöschung eine Archivierungs-,
Aufbewahrungs- und Zugriffslösung entscheiden. Bis dahin keine
kaskadierende Löschung ergänzen.

### R4 – Feldweise Konfliktlösung ohne Datenmodell

**Priorität: hoch.** Der Vergleich zweier Snapshots ist
implementiert. Eine feldweise Konfliktentscheidung und ihr
persistiertes Ergebnis fehlen.

**Maßnahme:** Erst nach fachlicher Klärung der Konfliktregeln
(Fachspezifikation Abschnitt 23.4–5) implementieren.

### R5 – Archiv und Datenbank können auseinanderdriften

**Priorität: mittel.** Kompensation ist vorhanden, aber nicht gegen
Prozessabbruch, Datenträgerfehler oder fehlgeschlagenes Cleanup
abgesichert.

**Maßnahme:** Reconciliation und Recovery-Verfahren nach dem
Wirtschaftlichkeitspaket ergänzen.

### R6 – Fachlich nicht freigegebene Felder werden voreilig erfunden

**Priorität: mittel.** Feldlisten, vollständige Gebäudegrunddaten
und Bauteilcode-Mapping sind ausdrücklich offen.

**Maßnahme:** Unbekannte Bereiche weiter als Warnung behandeln und
keine freie Zuordnung implementieren.

## 9. Priorisierte nächste Arbeitspakete

### P1 – Wirtschaftlichkeit (nächstes freigegebenes Paket)

Gemäß Fachspezifikation Abschnitt 24.7:

1. EF-Core-Konfiguration und Migration für
   `Wirtschaftlichkeitsannahmen` und `EnergietraegerAnnahme`.
2. Zuordnung zu einer `Modernisierungsalternative`.
3. Berechnungsservice für bilanzierte Wirtschaftlichkeit.
4. Berechnungsservice für praktische Wirtschaftlichkeit.
5. API-Endpunkte und Unit-Tests.

### P2 – Förderung

Gemäß Fachspezifikation Abschnitt 24.8: BEG EM, KfW, EFRE, KNN.
Erst nach fachlicher Freigabe des Förderdatenmodells.

### P3 – Berichtswesen

Gemäß Fachspezifikation Abschnitt 24.9.

### P4 – Vollständiger Projektstand

Auftraggeber, Standortdaten, Bearbeitungsstatus, reale Verbräuche,
Berichtseinstellungen und abweichende Annahmen ergänzen.

### P5 – Feldweise Konfliktlösung beim Re-Import

Erst nach fachlicher Klärung stabiler Feldidentitäten und
Konfliktregeln (Fachspezifikation Abschnitt 23.4–5).

### P6 – Wärmebrückenmanagement

Gemäß Fachspezifikation Abschnitt 24.10.

## 10. Abgrenzung

Diese Analyse autorisiert nicht:

- eigene energetische Berechnungen;
- IFC- oder gbXML-Auswertung;
- editierbare B56-Originalwerte;
- automatische Snapshot-Überschreibung;
- freie Interpretation unbekannter B56-Felder;
- neue Förder- oder Berichtsregeln ohne fachliche Freigabe.

Die in der Fachspezifikation als offen markierten Punkte bleiben offen.
