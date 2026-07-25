# B56-Gap-Analyse

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

Der vorhandene B56-Pfad ist ein stabiler technischer Import mit
Archivierung, projektbezogener Duplikaterkennung, ausgewählter
fachlicher Zuordnung, Persistenz, API-Abfrage und Desktopanzeige.

Er ist noch nicht der in der Fachspezifikation beschriebene
bestätigungs- und versionsgesteuerte Snapshot-Prozess:

- `FachdatenJson` besitzt keine Schema- oder Parser-Version.
- Der Importstatus beschreibt nur den unmittelbaren technischen
  Aufruf, nicht den fachlichen Lebenszyklus.
- Eine fachliche Bestätigung ist nicht modelliert.
- Es gibt keinen expliziten Anwendungsfall zur Übernahme in das
  Projektmodell.
- Folgeimporte werden nicht verglichen.
- Benutzerergänzungen und Konflikte sind noch nicht mit ihrer Herkunft
  verbunden.

Die nächste funktionale Änderung sollte deshalb weiterhin die
Snapshot-Schema-Versionierung sein. Zusätzliche B56-Felder,
Wirtschaftlichkeit und Berichtswesen sollten diesem Fundament nicht
vorgezogen werden.

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

## 4. Teilweise erfüllt

### 4.1 Unveränderlicher Snapshot

`B56ImportEintragEntity` und `FachdatenJson` bilden technisch bereits
einen importbezogenen Datenstand. Es existiert kein Updatepfad für den
JSON-Inhalt.

Es fehlen jedoch:

- expliziter Snapshot-Begriff im Datenmodell;
- Snapshot-Schema-Version;
- Parser-Version;
- fachliche Snapshot-Version beziehungsweise Reihenfolge;
- persistierter Lebenszyklusstatus;
- persistierte Bestätigung und Verwerfung;
- klare Behandlung unbekannter Payload-Versionen.

Damit beruht die Unveränderlichkeit derzeit auf fehlenden
Änderungsoperationen, nicht auf einem ausdrücklich modellierten
Snapshot-Vertrag.

### 4.2 Warnungen und Validierungsergebnisse

Pipeline-Warnungen sind Bestandteil von `FachdatenJson` und werden
angezeigt. Technische Ablehnungen werden im unmittelbaren
`B56ImportErgebnis` zurückgegeben.

Es fehlen:

- dauerhaft gespeicherte technische Validierungsergebnisse;
- strukturierte Fehler- und Warnungscodes im Snapshot;
- Unterscheidung blockierender und nicht blockierender fachlicher
  Befunde;
- Statusübergang von „mit Warnungen“ zu „fachlich bestätigt“;
- Auditdaten zur bestätigenden Person und zum Zeitpunkt.

### 4.3 Modernisierungsalternativen

Bezeichnung, Beschreibung, Kennwerte und Bauteile werden aus einem
freigegebenen B56-Bereich importiert und dargestellt. Leere
Alternativenplätze werden übersprungen.

Es fehlen:

- explizite B56-Nummer beziehungsweise Position;
- Zuordnung zu einer B56-Variante oder einem Berechnungsstand;
- technische Begrenzung und Nachweis „bis zu neun je Variante“;
- stabile importierte Identität für spätere Vergleiche;
- eindeutige Trennung zwischen B56-Bezeichnung und ergänzender interner
  Bezeichnung;
- vollständige energetische Ergebnisse je Energieträger.

Die Desktopansicht verwendet an einzelnen Stellen noch den Begriff
„Variante“ für importierte Modernisierungsalternativen. Das widerspricht
der verbindlichen Terminologie.

### 4.4 Bauteile und Kennwerte

Bauteilcode, Bezeichnung, Nachbarseite, Fläche und U-Wert sind im
Importmodell vorhanden. Bestands- und Alternativenkennwerte können als
Name, Einheit und numerischer Wert gespeichert werden.

Es fehlen:

- freigegebene vollständige Feldlisten je B56-Exportblatt;
- stabile Identitäten für Vergleich und Herkunft;
- vollständige Gebäudegrunddaten und Bezugsgrößen;
- freigegebene Nutzungsprofile beziehungsweise Zonenübersicht;
- strukturierte Energieträger;
- fachliche Pflichtfeld- und Plausibilitätsregeln;
- Kennzeichnung, welche Werte blockierend fehlen dürfen.

Diese Lücken können wegen der ausdrücklich offenen Feld- und
Mappingentscheidungen nicht eigenmächtig geschlossen werden.

### 4.5 Beziehung zwischen Projekt und Import

Projekt und Import teilen eine Projekt-ID. Die Importsuche ist dadurch
projektbezogen. Es gibt bewusst keine kaskadierende
Entity-Framework-Beziehung, sodass ein Snapshot bei normaler
Projektlöschung erhalten bleibt.

Offen bleibt:

- wie ein aufbewahrter Snapshot nach Projektlöschung zugänglich und
  verwaltet wird;
- ob Projektlöschung fachlich als Archivierung statt physischer
  Löschung modelliert werden muss;
- wie Berichte und spätere Herkunftsverweise erhalten bleiben;
- welche Datenschutz- und Aufbewahrungsregel physisches Löschen
  autorisiert.

Aktuell verlangt die Historien- und Detail-API ein vorhandenes Projekt.
Ein erhaltener Snapshot ist nach Projektlöschung daher zwar in der
Datenbank vorhanden, über den regulären API-Pfad aber nicht mehr
erreichbar.

### 4.6 Technische Konsistenz von Archiv und Datenbank

Die Importpipeline kompensiert viele Fehler durch Löschen der neu
erzeugten Archivdatei. Das reduziert verwaiste Dateien.

Es fehlen:

- dauerhafter Importzustand für abgebrochene Verarbeitung;
- Reconciliation-Prozess für Archiv und Datenbank;
- Wiederanlauf nach Prozessabbruch;
- definierter Umgang mit Fehlern beim kompensierenden Löschen;
- Backup-, Restore- und Recovery-Verfahren.

## 5. Nicht erfüllt

### 5.1 Snapshot-Schema- und Parser-Versionierung

Weder Anwendung noch Datenbank speichern eine Snapshot-Schema-Version
oder Parser-Version. `FachdatenJson` wird unmittelbar als aktueller
`B56ImportPipelineErgebnis`-Typ deserialisiert.

Folgen:

- zukünftige Modelländerungen können alte Payloads unlesbar machen;
- Legacy-Daten sind nicht ausdrücklich erkennbar;
- unbekannte zukünftige Versionen werden nicht kontrolliert
  abgewiesen;
- Parseränderungen sind nicht auditierbar.

### 5.2 Fachlicher Importlebenszyklus

Die vorhandenen Statuswerte `Erfolgreich`, `BereitsImportiert`,
`Abgelehnt` und `Fehlgeschlagen` beschreiben einen technischen
Aufruf.

Nicht modelliert sind die fachlich geforderten Zustände:

- hochgeladen;
- technisch geprüft;
- mit Warnungen;
- blockiert;
- fachlich bestätigt;
- in das Projektmodell übernommen;
- verworfen.

Es gibt keine zulässigen Statusübergänge und keine Auditierung dieser
Übergänge.

### 5.3 Bestätigung und Übernahme in das Projektmodell

Es existiert kein eigener Application-Use-Case für:

- Import prüfen und bestätigen;
- blockierende Befunde vor Bestätigung verhindern;
- initiales Projektmodell aus einem Snapshot erzeugen;
- Herkunft jedes übernommenen Werts speichern;
- wiederholbare beziehungsweise idempotente Übernahme;
- Verwerfung eines Snapshots.

Das vorhandene Projekt-Domänenmodell wird durch den Import nicht
befüllt.

### 5.4 Bearbeitbarer vollständiger Projektstand

Das Projektmodell enthält derzeit im Wesentlichen Name,
Modernisierungsalternativen, alternative Bauteile und Kostenpositionen.

Noch nicht modelliert sind unter anderem:

- Auftraggeber und Ansprechpartner;
- Standortdaten und Gebäudetyp;
- Bearbeitungs- und Freigabestatus;
- Herkunftsverweise auf Snapshots;
- Varianten beziehungsweise Berechnungsstände;
- Förderparameter;
- Energiepreise und Preissteigerungen;
- CO₂-Preisannahmen;
- reale Verbrauchsdaten;
- Berichtseinstellungen;
- nachvollziehbare abweichende Annahmen.

### 5.5 Re-Import und Versionsvergleich

Ein neuer Hash kann als weiterer Import gespeichert werden. Darüber
hinaus fehlen:

- fachliche Versionsnummer innerhalb des Projekts;
- Auswahl eines Vorgänger-Snapshots;
- Vergleich von Projektdaten, Varianten, Alternativen, Bauteilen,
  Flächen, U-Werten, Kennwerten und Energieträgern;
- Erkennung hinzugefügter und entfernter Datensätze;
- Konfliktmodell;
- feldweise Bestätigung;
- Schutz manueller Ergänzungen durch einen expliziten
  Synchronisationsprozess.

### 5.6 Vollständiger produktiver End-to-End-Prozess

Der technische Smoke-Test verwendet Controller und Services direkt.
Es fehlt ein vollständiger Test über echte HTTP-Serialisierung und den
persistierten fachlichen Ablauf:

1. Projekt anlegen;
2. Datei hochladen;
3. Snapshot anzeigen;
4. Warnungen prüfen;
5. Import bestätigen;
6. Projektmodell erzeugen;
7. Ergänzung speichern;
8. Projekt neu öffnen;
9. zweiten Snapshot importieren;
10. Unterschiede anzeigen;
11. Ergänzung unverändert nachweisen.

## 6. Notwendige Datenbankmigrationen

Die folgenden Migrationen ergeben sich unmittelbar aus ADR-0008 und
der Fachspezifikation. Sie sollten in getrennten, rückwärtskompatiblen
Arbeitspaketen umgesetzt werden.

### 6.1 Migration 1: Snapshot-Versionierung

`B56ImportEintraege` mindestens ergänzen um:

- `SnapshotSchemaVersion`, nicht nullable;
- `ParserVersion`, nicht nullable;
- eine projektbezogene fachliche Snapshot-Reihenfolge oder
  Snapshot-Version;
- einen persistierten Snapshot-/Lebenszyklusstatus;
- optional einen strukturierten Payload-Typ, falls
  `FachdatenJson` als Legacy-Name abgelöst wird.

Bestandsdaten müssen deterministisch als Legacy beziehungsweise
Schema-Version 1 markiert werden. Die Migration darf vorhandenes
`FachdatenJson` nicht umschreiben oder verlieren.

### 6.2 Migration 2: Bestätigung und Audit

Nach Spezifikation des Statusmodells werden mindestens benötigt:

- Bestätigungs- beziehungsweise Verwerfungszeitpunkt;
- verantwortliche Person oder technischer Akteur, soweit im lokalen
  Betriebsmodell verfügbar;
- Statushistorie oder separate Auditereignisse;
- unveränderlicher Verweis auf den bestätigten Snapshot.

Die genaue Akteurmodellierung ist vor einer Mehrbenutzerlösung noch
fachlich offen.

### 6.3 Migration 3: Herkunft im Projektmodell

Vor der ersten Snapshot-Übernahme sind Herkunftsangaben erforderlich:

- Quell-Snapshot-ID;
- Quellobjekt beziehungsweise stabiler Quellschlüssel;
- Originalwert oder belastbarer Verweis darauf;
- Projektmodellversion;
- Änderungs- und Übernahmezeitpunkt.

Ob diese Angaben pro Projektwert, pro Fachobjekt oder in einem
separaten Herkunftsmodell gespeichert werden, muss vor Umsetzung des
Übernahme-Use-Cases spezifiziert werden.

### 6.4 Migration 4: Vergleich und Konflikte

Für Re-Import werden voraussichtlich persistente Vergleichs- oder
Konfliktdaten benötigt:

- alter und neuer Snapshot;
- betroffener stabiler Fachschlüssel;
- alter Originalwert, neuer Originalwert und aktueller Arbeitswert;
- Konfliktstatus und Benutzerentscheidung;
- Zeitpunkt und Auditinformation.

Diese Migration darf erst nach Klärung der offenen Feldidentitäten und
Konfliktregeln entworfen werden.

### 6.5 Keine kaskadierende Löschung einführen

Eine neue relationale Verbindung zwischen Projekt und Snapshot darf
Snapshots nicht automatisch mit dem Projekt löschen. Die
Aufbewahrungsentscheidung ist offen, während ADR-0008 und der Workflow
die Nachweisbarkeit historischer Quellen verlangen.

## 7. Notwendige Tests

### 7.1 Für die unmittelbar nächste Snapshot-Versionierung

- Migration einer bestehenden Datenbank mit `FachdatenJson`;
- Legacy-Eintrag wird als Version 1 gelesen;
- aktueller Snapshot-Roundtrip mit expliziter Schema-Version;
- Parser-Version wird beim Import gespeichert und zurückgegeben;
- unbekannte Schema-Version wird kontrolliert und ohne Datenverlust
  behandelt;
- beschädigter Payload erzeugt einen definierten Fehler;
- neue Imports erhalten eine monotone projektbezogene Version;
- parallele Imports erzeugen keine doppelte Version;
- vorhandene Hash-Duplikaterkennung bleibt wirksam;
- Migration entfernt oder verändert keine Archiv- und Hashdaten.

### 7.2 Für Bestätigung und Projektübernahme

- blockierender Befund verhindert Bestätigung;
- Warnungen erlauben Bestätigung, bleiben aber sichtbar;
- Bestätigung wird auditierbar gespeichert;
- verworfener Snapshot kann nicht übernommen werden;
- Erstübernahme erzeugt ein Projektmodell mit Herkunft;
- wiederholte Übernahme ist idempotent;
- B56-Originalwerte bleiben unverändert;
- Benutzerergänzungen werden getrennt gespeichert;
- gelöschtes Projekt löscht keinen Snapshot.

### 7.3 Für Re-Import und Vergleich

- gleicher Hash wird als Duplikat behandelt;
- neuer Hash erzeugt einen neuen Snapshot;
- hinzugefügte, entfernte und geänderte Objekte werden erkannt;
- bis zu neun Alternativen je Variante werden stabil identifiziert;
- Umbenennung wird gemäß noch festzulegender Regel behandelt;
- manueller Arbeitswert wird nicht automatisch überschrieben;
- bestätigte und abgelehnte Konfliktentscheidungen sind
  nachvollziehbar.

### 7.4 Für API und Desktop

- echte HTTP-Tests für Upload, Historie, Details und Fehlerantworten;
- JSON-Kompatibilität zwischen API und Desktop;
- Anzeige von Schema-, Parser- und Snapshot-Version;
- Anzeige blockierender Fehler getrennt von Warnungen;
- Bestätigungs- und Verwerfungsaktionen;
- Anzeige von Originalwert, Arbeitswert und Abweichung;
- Timeout, ungültiges JSON und unbekannte Version;
- vollständiger End-to-End-Test des verbindlichen Anwenderablaufs.

## 8. Risiken

### R1 – Alte Snapshots werden durch Modelländerungen unlesbar

**Priorität: hoch.** Die direkte Deserialisierung von `FachdatenJson`
in den aktuellen Typ besitzt keine Versionsgrenze.

**Maßnahme:** Snapshot-Schema-Versionierung und Legacy-Leser vor jeder
Erweiterung des Payloadmodells.

### R2 – Zwei fachliche Wahrheiten entstehen

**Priorität: hoch.** Importmodell und Projektmodell existieren bereits,
aber ein expliziter Übernahme- und Herkunftsprozess fehlt.

**Maßnahme:** Keine direkte Befüllung des Projektmodells außerhalb
eines bestätigten Application-Use-Cases.

### R3 – Benutzeränderungen werden bei Re-Import überschrieben

**Priorität: hoch.** Ohne Herkunfts- und Konfliktmodell wäre ein
automatisches Update nicht sicher.

**Maßnahme:** Re-Import zunächst nur als neuen Snapshot speichern;
Synchronisation erst nach spezifizierter Konfliktentscheidung.

### R4 – Erhaltene Snapshots sind nach Projektlöschung unerreichbar

**Priorität: hoch.** Die Daten bleiben erhalten, die reguläre API
verweigert aber Historie und Details für ein nicht mehr vorhandenes
Projekt.

**Maßnahme:** Vor Ausbau der Projektlöschung eine Archivierungs-,
Aufbewahrungs- und Zugriffslösung entscheiden. Bis dahin keine
kaskadierende Löschung ergänzen.

### R5 – Alternativen können über Versionen nicht stabil verglichen werden

**Priorität: hoch.** Importierte Alternativen besitzen weder Position
noch stabile Identität und keine explizite Variante.

**Maßnahme:** B56-Position, Variante und stabilen Vergleichsschlüssel
erst nach fachlicher Bestätigung modellieren.

### R6 – Statusbegriffe vermischen Technik und Fachlichkeit

**Priorität: mittel.** Der aktuelle technische Ergebnisstatus kann
leicht mit dem geforderten fachlichen Snapshotstatus verwechselt
werden.

**Maßnahme:** Technisches Aufrufergebnis und persistierten
Snapshot-Lebenszyklus getrennt benennen und modellieren.

### R7 – Archiv und Datenbank driften auseinander

**Priorität: mittel.** Kompensation ist vorhanden, aber nicht gegen
Prozessabbruch, Datenträgerfehler oder fehlgeschlagenes Cleanup
abgesichert.

**Maßnahme:** Reconciliation und Betriebs-/Recovery-Verfahren nach dem
Snapshot-Fundament ergänzen.

### R8 – Fachlich nicht freigegebene Felder werden voreilig erfunden

**Priorität: mittel.** Feldlisten, Projektidentifikation,
Bauteilcode-Mapping und Umbenennungslogik sind ausdrücklich offen.

**Maßnahme:** Unbekannte Bereiche weiter als Warnung behandeln und
keine freie Zuordnung implementieren.

### R9 – Dokumentpfade und Terminologie sind inkonsistent

**Priorität: niedrig.**

- Der Folgeauftrag verweist auf
  `docs/FUNCTIONAL_SPECIFICATION.md`; die Datei liegt aktuell im
  Repository-Stamm als `FUNCTIONAL_SPECIFICATION.md`.
- Die Desktopansicht bezeichnet Modernisierungsalternativen teilweise
  als „Variante“ beziehungsweise „Modernisierungsvariante“.

**Maßnahme:** In einem getrennten Dokumentations-/Terminologiepaket
vereinheitlichen, ohne fachliches Verhalten zu ändern.

## 9. Priorisierte nächste Arbeitspakete

### P1 – Snapshot-Schema-Versionierung

Als nächstes freigegeben und technisch notwendig:

1. Versionskonstanten und versionierten Snapshot-Vertrag definieren.
2. Schema- und Parser-Version relational speichern.
3. Bestandsdaten als Legacy-/Version-1-Daten lesbar halten.
4. EF-Core-Migration und Kompatibilitätstests ergänzen.
5. Unbekannte oder beschädigte Versionen kontrolliert behandeln.

Dieses Paket darf noch keine automatische Projektübernahme enthalten.

### P1 – Echter HTTP-End-to-End-Test

Danach Upload, Historie und Details über einen echten Testserver
prüfen. Der Test muss Serialisierung, Datenbank und Archiv umfassen.

### P1 – Fachlicher Lebenszyklus und Bestätigung

Persistierten Snapshotstatus, blockierende Befunde, Warnungen,
Bestätigung und Verwerfung als getrennten Application-Use-Case
implementieren.

### P1 – Projektübernahme mit Herkunft

Erst nach Bestätigung einen expliziten, idempotenten
Übernahme-Use-Case entwickeln. Originalwerte und bearbeitbare
Ergänzungen müssen sichtbar getrennt bleiben.

### P2 – Re-Import und Versionsvergleich

Nach Klärung stabiler Fachschlüssel und Konfliktregeln neue Snapshots
vergleichen. Keine automatische Überschreibung des Projektmodells.

### P3 – Zusätzliche B56-Bereiche

Erst nach fachlicher Freigabe der Feldlisten, Pflichtregeln und
Mappings weitere Exportbereiche zuordnen.

## 10. Abgrenzung

Diese Analyse autorisiert nicht:

- eigene energetische Berechnungen;
- IFC- oder gbXML-Auswertung;
- editierbare B56-Originalwerte;
- automatische Snapshot-Überschreibung;
- automatische Übernahme in das Projektmodell;
- freie Interpretation unbekannter B56-Felder;
- neue Förder-, Wirtschafts- oder Berichtsregeln.

Die in der Fachspezifikation als offen markierten Punkte bleiben offen.
