# B56-Gap-Analyse

**Stand:** 27. Juli 2026 (aktualisiert nach Paket-3- bis Paket-5-Implementierung)

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

Seit der ersten Gap-Analyse wurden die Pakete 3 bis 5 erfolgreich
implementiert:

- Snapshot-Schema- und Parser-Versionierung mit Legacy-Migration;
- fachlicher Snapshotlebenszyklus mit allen sieben Zuständen;
- Bestätigung, Verwerfung und Übernahme in das Projektmodell;
- Re-Import und Versionsvergleich anhand stabiler B56-Positionen;
- B56-Position und Präsenzkennzeichnung für Alternativen.

Der aktuelle Implementierungsstand deckt damit den durchgängigen
fachlichen B56-Importpfad vom Hochladen bis zur Projektmodellübernahme
einschließlich Folgeimport-Vergleich vollständig ab. `dotnet test`
bestätigt 95/95 Tests bestanden.

Offene Schwerpunkte für die nächste Ausbaustufe:

- bearbeitbarer vollständiger Projektstand (Kontakte, Standort,
  Kosten, Förderung, Wirtschaftlichkeit);
- persistierte Vergleichs- und Konfliktergebnisse;
- vollständiger End-to-End-Prozesstest über alle elf Abnahmeschritte;
- Wirtschaftlichkeit, Förderung und Berichtswesen.

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

- `B56SnapshotVersionen` enthält `AktuelleSchemaVersion = 1` und
  `AktuelleParserVersion = "1.1"` sowie `LegacyParserVersion = "legacy"`.
- `SnapshotSchemaVersion` und `ParserVersion` werden relational in
  `B56ImportEintraege` gespeichert.
- Bestandsdaten erhalten durch die Migration Standardwerte
  (`SnapshotSchemaVersion = 1`, `ParserVersion = "legacy"`).
- `EfB56ImportRegister` wirft `B56SnapshotFormatException` bei einer
  unbekannten Schema-Version; der Payload wird nicht lautlos falsch
  deserialisiert.
- `B56ImportEintrag` und `B56ImportEintragEntity` führen beide Felder.

Nachweise:

- `Kompass.Application/B56Import/B56SnapshotVersionen.cs`
- `Kompass.Application/B56Import/B56SnapshotFormatException.cs`
- `Kompass.Persistence/Data/Entities/B56ImportEintragEntity.cs`
- `Kompass.Persistence/Migrations/20260725075146_VersionB56Snapshots.cs`
- `Kompass.Persistence/Services/EfB56ImportRegister.cs`
- `Kompass.Tests/KompassDbContextMigrationTests.cs`
- `Kompass.Tests/EfB56ImportRegisterTests.cs`

### 3.8 Fachlicher Snapshotlebenszyklus

- `B56SnapshotStatus` bildet alle sieben geforderten Zustände ab:
  `TechnischGeprueft`, `MitWarnungen`, `Blockiert`, `FachlichBestaetigt`,
  `InProjektmodellUebernommen`, `Verworfen`.
- `B56SnapshotLebenszyklusService` implementiert `BestaetigenAsync` und
  `VerwerfenAsync` mit expliziten, geprüften Statusübergängen.
- Blockierte Snapshots können nicht bestätigt, aber verworfen werden.
- Bestätigungs- und Verwerfungszeitpunkt werden in `BestaetigtAm`
  beziehungsweise `VerworfenAm` gespeichert.
- `B56SnapshotLebenszyklusController` bildet Anwendungsergebnisse auf
  HTTP-Status ab.
- Migration `AddB56SnapshotLifecycle` fügt alle drei Felder rückwärts­
  kompatibel mit dem Standardstatus `TechnischGeprueft` hinzu.

Nachweise:

- `Kompass.Application/B56Import/B56SnapshotStatus.cs`
- `Kompass.Application/B56Import/B56SnapshotLebenszyklusService.cs`
- `Kompass.Api/B56Import/B56SnapshotLebenszyklusController.cs`
- `Kompass.Persistence/Migrations/20260725084054_AddB56SnapshotLifecycle.cs`
- `Kompass.Tests/B56SnapshotLebenszyklusServiceTests.cs`
- `Kompass.Tests/B56SnapshotLebenszyklusControllerTests.cs`

### 3.9 Übernahme in das Projektmodell

- `B56ProjektmodellUebernahmeService` überträgt Modernisierungs­
  alternativen und Bauteilreferenzen aus einem fachlich bestätigten
  Snapshot in das Projektmodell.
- Nur Snapshots mit Status `FachlichBestaetigt` dürfen übernommen werden.
- Die erneute Übernahme desselben Snapshots ist idempotent.
- Herkunft wird als `QuellSnapshotId` am Projekt und an jeder
  übernommenen Alternative gespeichert.
- `ProjektmodellVersion` am Projekt steigt nach jeder Übernahme.
- Nach erfolgreicher Übernahme erhält der Snapshot den Status
  `InProjektmodellUebernommen`.
- `B56ProjektmodellController` bildet die Ergebnisse auf HTTP ab.
- Migration `AddB56ProjectModelOrigin` ergänzt `QuellSnapshotId` und
  `ProjektmodellVersion` rückwärtskompatibel.

Nachweise:

- `Kompass.Application/B56Import/IB56ProjektmodellUebernahmeService.cs`
- `Kompass.Persistence/Services/B56ProjektmodellUebernahmeService.cs`
- `Kompass.Api/B56Import/B56ProjektmodellController.cs`
- `Kompass.Persistence/Migrations/20260725085558_AddB56ProjectModelOrigin.cs`
- `Kompass.Tests/B56ProjektmodellUebernahmeServiceTests.cs`
- `Kompass.Tests/B56ProjektmodellControllerTests.cs`

### 3.10 Re-Import und Versionsvergleich

- `B56SnapshotVergleichService` vergleicht zwei Snapshots anhand
  Kennwertname, Bauteilcode und B56-Position (1–9).
- Hinzugefügte, geänderte und entfernte Kennwerte, Bauteile und
  Alternativen werden erkannt und im Ergebnis ausgewiesen.
- Bezeichnungsänderungen bei Alternativen werden als inhaltliche
  Änderung behandelt.
- `B56SnapshotVergleichController` stellt den Vergleich als
  HTTP-Endpunkt bereit.
- Der HTTP-End-to-End-Test prüft zweiten Import und Vergleich über
  echte HTTP-Serialisierung.

Nachweise:

- `Kompass.Application/B56Import/B56SnapshotVergleichService.cs`
- `Kompass.Api/B56Import/B56SnapshotVergleichController.cs`
- `Kompass.Tests/B56SnapshotVergleichServiceTests.cs` (12 Tests)
- `Kompass.Tests/B56ImportHttpEndToEndTests.cs`

### 3.11 B56-Position und Präsenzkennzeichnung für Alternativen

- `B56Position` wird beim Import aus `SCModernisierungen` extrahiert
  und als stabiler Schlüssel in `Modernisierungsalternativen` gespeichert.
- `IstImAktuellenB56SnapshotVorhanden` kennzeichnet Alternativen, die
  im letzten Snapshot nicht mehr belegt sind.
- Damit ist der ADR-0008-Grundsatz umgesetzt: Kosten, Kommentare und
  Historie bleiben erhalten; die Alternative wird nicht gelöscht.
- Migration `TrackB56AlternativePresence` ergänzt beide Spalten.

Nachweise:

- `Kompass.Persistence/Migrations/20260725101500_TrackB56AlternativePresence.cs`
- `Kompass.Application/B56Import/B56Modernisierungsalternative.cs`
- `Kompass.Tests/B56SnapshotVergleichServiceTests.cs`

## 4. Teilweise erfüllt

### 4.1 Unveränderlicher Snapshot

`B56ImportEintragEntity`, `FachdatenJson`, Schema-Version und
Lebenszyklusstatus bilden zusammen einen nachvollziehbaren,
versionierten Snapshot.

Noch nicht vollständig:

- Der explizite Begriff „Snapshot" taucht im Datenmodell und in der
  API erst als Pfadbestandteil auf; `B56ImportEintrag` verwendet noch
  „Import"-Terminologie.
- Eine fachlich explizite, monoton wachsende Snapshot-Nummer pro
  Projekt (unabhängig vom Zeitstempel) ist nicht implementiert.
- Die Behandlung eines beschädigten Payloads bei gültiger Schema-Version
  ist nicht gesondert getestet.

### 4.2 Warnungen und Validierungsergebnisse

Pipeline-Warnungen sind Bestandteil von `FachdatenJson` und werden im
Snapshot mit dem Lebenszyklus verbunden. Der Importstatus
`MitWarnungen` ermöglicht die Bestätigung trotz Warnungen; `Blockiert`
verhindert sie.

Noch offen:

- Strukturierte, maschinenlesbare Fehler- und Warnungscodes im
  Snapshot (derzeit freier Text im JSON);
- Auditdaten zur bestätigenden Person – bewusst offen bis zur
  Entscheidung über das Rollenmodell;
- dauerhaft gespeicherte Liste der einzelnen blockierenden Befunde
  getrennt vom Pipeline-Ergebnis.

### 4.3 Modernisierungsalternativen

Bezeichnung, Beschreibung, Kennwerte, Bauteile und B56-Position werden
importiert und dargestellt. Die B56-Position dient als stabiler
Vergleichsschlüssel.

Noch offen:

- Zuordnung zu einer B56-Variante oder einem Berechnungsstand (fachlich
  noch nicht freigegeben);
- technisch erzwungene Begrenzung auf neun Alternativen je Variante;
- eindeutige Trennung zwischen B56-Bezeichnung und ergänzender interner
  Bezeichnung im Projektmodell;
- vollständige energetische Ergebnisse je Energieträger.

Die Desktopansicht verwendet an einzelnen Stellen noch den Begriff
„Variante" für importierte Modernisierungsalternativen. Das widerspricht
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

### 5.1 Bearbeitbarer vollständiger Projektstand

Das Projektmodell enthält derzeit Name, Modernisierungsalternativen,
alternative Bauteile, Kostenpositionen und Herkunftsreferenz.

Noch nicht modelliert sind unter anderem:

- Auftraggeber und Ansprechpartner;
- Standortdaten und Gebäudetyp;
- Bearbeitungs- und Freigabestatus;
- Förderparameter;
- Energiepreise und Preissteigerungen;
- CO₂-Preisannahmen;
- reale Verbrauchsdaten;
- Berichtseinstellungen;
- nachvollziehbare abweichende Annahmen.

### 5.2 Persistierte Vergleichs- und Konfliktergebnisse

Der Snapshot-Vergleich wird berechnet, aber nicht dauerhaft
gespeichert. Es fehlen:

- persistierte Vergleichsergebnisse für spätere Auswertung;
- Konfliktmodell für feldweise Bestätigung;
- explizite Synchronisations-Use-Case (nach fachlicher Spezifikation);
- Schutzregel, die verhindert, dass manuelle Ergänzungen automatisch
  durch Snapshot-Werte überschrieben werden.

### 5.3 Vollständiger produktiver End-to-End-Prozess

HTTP-End-to-End-Tests prüfen Upload, Historie, Details und
Folgeimport-Vergleich. Noch nicht durch einen vollständigen Test
abgedeckt ist der Ablauf aus `FUNCTIONAL_SPECIFICATION.md`
Abschnitt 21:

5. Import bestätigen (HTTP);
6. Projektmodell erzeugen (HTTP);
7. Ergänzung speichern;
8. Projekt schließen und neu öffnen;
9. zweiten Snapshot importieren (HTTP) ← vorhanden;
10. Unterschiede anzeigen (HTTP) ← vorhanden;
11. Ergänzung unverändert nachweisen.

## 6. Datenbankmigrationen – Überblick

### 6.1 Abgeschlossene Migrationen

| Migration | Inhalt | Status |
|-----------|--------|--------|
| `20260725075146_VersionB56Snapshots` | `SnapshotSchemaVersion`, `ParserVersion` | ✅ umgesetzt |
| `20260725084054_AddB56SnapshotLifecycle` | `SnapshotStatus`, `BestaetigtAm`, `VerworfenAm` | ✅ umgesetzt |
| `20260725085558_AddB56ProjectModelOrigin` | `QuellSnapshotId`, `ProjektmodellVersion` | ✅ umgesetzt |
| `20260725101500_TrackB56AlternativePresence` | `B56Position`, `IstImAktuellenB56SnapshotVorhanden` | ✅ umgesetzt |

### 6.2 Ausstehende Migrationen

**Migration: Persistente Vergleichs- und Konfliktergebnisse**

Für spätere feldweise Bestätigung und Konfliktlösung werden voraussichtlich
benötigt:

- Tabelle oder JSON-Spalte für persistierte Vergleichsergebnisse;
- betroffener stabiler Fachschlüssel (B56Position, Kennwertname,
  Bauteilcode);
- alter Originalwert, neuer Originalwert und aktueller Arbeitswert;
- Konfliktstatus und Benutzerentscheidung;
- Zeitpunkt und Auditinformation.

Diese Migration darf erst nach Klärung der offenen Feldidentitäten und
Konfliktregeln entworfen werden.

**Hinweis:** Eine kaskadierende Löschung zwischen Projekt und Snapshot
darf nicht eingeführt werden. Snapshots müssen nach Projektlöschung
für die Nachweisbarkeit erhalten bleiben.

## 7. Notwendige Tests

### 7.1 Bereits implementierte Tests (Übersicht)

| Testdatei | Inhalt | Anzahl |
|-----------|--------|--------|
| `B56DateiPrueferTests.cs` | Dateiprüfung | 7 |
| `B56ImportControllerTests.cs` | API-Controller | 9 |
| `B56ImportDependencyInjectionTests.cs` | DI-Komposition | 1 |
| `B56ImportEndToEndSmokeTests.cs` | Smoke-Test | 1 |
| `B56ImportHttpEndToEndTests.cs` | HTTP E2E (Import + Vergleich) | 2 |
| `B56ImportServiceIntegrationTests.cs` | Import-Pipeline | 3 |
| `B56ProjektmodellControllerTests.cs` | Übernahme-Controller | 2 |
| `B56ProjektmodellUebernahmeServiceTests.cs` | Übernahme-Service | 2 |
| `B56SnapshotLebenszyklusControllerTests.cs` | Lebenszyklus-Controller | 1 |
| `B56SnapshotLebenszyklusServiceTests.cs` | Lebenszyklus-Service | 4 |
| `B56SnapshotVergleichServiceTests.cs` | Vergleich (alle Fälle) | 12 |
| `B56TabellenImportServiceTests.cs` | Tabellenimport | 2 |
| `EfB56ImportRegisterTests.cs` | EF-Register inkl. Versionen | 3 |
| `KompassDbContextMigrationTests.cs` | Migrationen | 2 |
| `OpenXmlB56ArbeitsmappenLeserTests.cs` | OpenXML-Leser | 2 |
| `ProjektB56ImportBeziehungTests.cs` | Projekt-Import-Beziehung | 2 |
| `ProjektDomainTests.cs` | Domain-Invarianten | 8 |
| `ProjektServiceTests.cs` | Projektservice | 6 |
| `ProjekteControllerTests.cs` | Projekte-API | 12 |
| `Sha256HashServiceTests.cs` | Hash-Service | 2 |
| `B56ArchivServiceTests.cs` | Archivservice | 2 |
| **Gesamt** | | **95** |

### 7.2 Noch fehlende Tests

- vollständiger HTTP-End-to-End-Test der elf Abnahmeschritte aus
  `FUNCTIONAL_SPECIFICATION.md` Abschnitt 21 (Schritte 5–8 und 11
  fehlen noch);
- beschädigter Payload bei gültiger Schema-Version erzeugt definierten
  Fehler;
- Benutzerergänzung bleibt nach Folgeimport unverändert (Schutz
  manueller Daten);
- Snapshot nach Projektlöschung erreichbar halten (sobald Use-Case
  entschieden);
- Persistiertes Vergleichsergebnis (sobald Datenmodell entschieden).

## 8. Risiken (aktualisiert)

### R1 – Alte Snapshots werden durch Modelländerungen unlesbar

**Priorität: erledigt/mitigiert.**
`B56SnapshotVersionen`, `B56SnapshotFormatException` und die Migration
`VersionB56Snapshots` setzen eine explizite Versionsgrenze.

**Restrisiko:** Der `FachdatenJson`-Payload selbst besitzt noch keine
interne Feldversionierung. Neue Felder im `B56ImportPipelineErgebnis`
müssen abwärtskompatibel hinzugefügt werden.

### R2 – Zwei fachliche Wahrheiten entstehen

**Priorität: mitigiert.**
Der explizite Übernahme-Use-Case (`B56ProjektmodellUebernahmeService`)
und der Lebenszyklusstatus stellen sicher, dass das Projektmodell nur
aus fachlich bestätigten Snapshots befüllt wird.

**Restrisiko:** Der bearbeitbare Projektstand ist noch nicht vollständig
modelliert. Bis dahin gibt es keinen Konflikterkennung für manuelle
Ergänzungen.

### R3 – Benutzeränderungen werden bei Re-Import überschrieben

**Priorität: mitigiert.**
Re-Import erzeugt nur einen neuen Snapshot. Die Synchronisation in das
Projektmodell bleibt einem separaten, fachlich noch zu spezifizierenden
Use-Case vorbehalten. `IstImAktuellenB56SnapshotVorhanden` schützt
vorhandene Alternativen.

**Restrisiko:** Sobald der Synchronisations-Use-Case implementiert wird,
ist eine feldweise Konfliktlösung erforderlich.

### R4 – Erhaltene Snapshots sind nach Projektlöschung unerreichbar

**Priorität: hoch.** Unverändert: Die reguläre API verweigert Zugriff
auf Snapshots eines nicht mehr vorhandenen Projekts.

**Maßnahme:** Vor Ausbau der Projektlöschung eine Archivierungs-,
Aufbewahrungs- und Zugriffslösung entscheiden. Keine kaskadierende
Löschung einführen.

### R5 – Alternativen können über Versionen nicht stabil verglichen werden

**Priorität: mitigiert.**
B56-Position ist als stabiler Schlüssel implementiert und wird im
Vergleich genutzt.

**Restrisiko:** Die Behandlung bei Variantenwechsel oder
Positionsneuordnung ist fachlich noch nicht spezifiziert.

### R6 – Statusbegriffe vermischen Technik und Fachlichkeit

**Priorität: erledigt.**
Technisches Aufruf­ergebnis (`B56ImportErgebnis`) und persistierter
Snapshot-Lebenszyklus (`B56SnapshotStatus`) sind explizit getrennt.

### R7 – Archiv und Datenbank driften auseinander

**Priorität: mittel.** Unverändert: Kompensation bei Fehler ist
vorhanden, aber kein Reconciliation-Prozess für Prozessabbrüche oder
Datenträgerfehler.

**Maßnahme:** Reconciliation- und Recovery-Verfahren nach dem nächsten
funktionalen Ausbaupunkt ergänzen.

### R8 – Fachlich nicht freigegebene Felder werden voreilig erfunden

**Priorität: mittel.** Unverändert: Feldlisten, Bauteilcode-Mapping und
weitere B56-Exportbereiche sind ausdrücklich offen.

**Maßnahme:** Unbekannte Bereiche weiter als Warnung behandeln.

### R9 – Dokumentpfade und Terminologie sind inkonsistent

**Priorität: niedrig.**

- `FUNCTIONAL_SPECIFICATION.md` liegt im Repository-Stamm, nicht unter
  `docs/`.
- Die Desktopansicht bezeichnet Modernisierungsalternativen teilweise
  als „Variante" beziehungsweise „Modernisierungsvariante".

**Maßnahme:** In einem getrennten Terminologiepaket bereinigen.

## 9. Priorisierte nächste Arbeitspakete

### P1 – Vollständiger erster Anwenderprozess (Paket 6)

Der gesamte fachliche Ablauf gemäß `FUNCTIONAL_SPECIFICATION.md`
Abschnitt 21 muss in einem einzigen durchgängigen HTTP-End-to-End-Test
nachgewiesen werden:

1. Projekt anlegen.
2. Datei importieren.
3. Snapshot bestätigen.
4. Projektmodell erzeugen.
5. Ergänzbare Projektdaten bearbeiten und speichern.
6. Projekt schließen und wieder öffnen.
7. Zweiten Snapshot importieren.
8. Unterschiede anzeigen.
9. Manuelle Ergänzung unverändert nachweisen.

Dazu sind mindestens einfache Felder für ergänzbare Projektdaten
(z. B. interne Bezeichnung oder Bearbeitungsstatus) zu modellieren,
damit Schritt 5 prüfbar ist.

### P2 – Wirtschaftlichkeit

Erst nach Abschluss von Paket 6 gemäß `FUNCTIONAL_SPECIFICATION.md`
Abschnitt 14.

### P3 – Förderung

Nach Wirtschaftlichkeit gemäß `FUNCTIONAL_SPECIFICATION.md`
Abschnitt 15.

### P4 – Berichtswesen

Nach Förderung gemäß `FUNCTIONAL_SPECIFICATION.md` Abschnitt 17.

### P5 – Wärmebrückenmanagement

Gemäß Gesamtprozess und `FUNCTIONAL_SPECIFICATION.md` Abschnitt 16.

## 10. Abgrenzung

Diese Analyse autorisiert nicht:

- eigene energetische Berechnungen;
- IFC- oder gbXML-Auswertung;
- editierbare B56-Originalwerte;
- automatische Snapshot-Überschreibung;
- automatische Übernahme in das Projektmodell außerhalb des bestätigten
  Use-Cases;
- freie Interpretation unbekannter B56-Felder;
- neue Förder-, Wirtschafts- oder Berichtsregeln.

Die in der Fachspezifikation als offen markierten Punkte bleiben offen.
