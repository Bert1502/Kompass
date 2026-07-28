# B56-Gap-Analyse

**Stand:** 28. Juli 2026

## 1. Grundlage

Verglichen wurde der aktuelle Code mit:

- `FUNCTIONAL_SPECIFICATION.md`
- `docs/modules/WORKFLOW_B56_IMPORT.md`
- `docs/adr/ADR-0008-B56-SNAPSHOT-UND-PROJEKTMODELL.md`
- `docs/CODEX.md`

Diese Analyse beschreibt nur den Ist-Stand. Es wurden keine funktionalen Quellcodeänderungen vorgenommen.

Bewertung:

- **already fulfilled** = fachlich umgesetzt und im Code nachweisbar
- **partially fulfilled** = Kern vorhanden, aber fachlich noch lückenhaft
- **not fulfilled** = aktuell nicht als vollständiger Anwendungsfall vorhanden

## 2. Kurzfazit

Der B56-Kern ist weitgehend umgesetzt:

- Datei-Prüfung, Archivierung, Hashing und Importregister sind vorhanden.
- Snapshot-Versionierung, Lebenszyklus und Projektmodell-Übernahme sind vorhanden.
- Re-Import-Vergleich über stabile B56-Positionen ist vorhanden.
- Die Präsenzkennzeichnung für entfernte Alternativen ist vorhanden.

`dotnet restore`, `dotnet build` und `dotnet test` laufen erfolgreich; aktuell bestehen **152/152 Tests**.

Für den nächsten Ausbauschritt bleibt vor allem der vollständige, bearbeitbare Projektstand plus der durchgehende End-to-End-Nachweis offen.

## 3. Already fulfilled

### 3.1 Technischer Importpfad

- Projektbezogene Importverarbeitung ist umgesetzt.
- Die API bindet Import, Historie und Detailabrufe an eine Projekt-ID.
- Duplikaterkennung per Hash ist projektbezogen.
- Archivierung der Originaldatei ist vorhanden.

Nachweise:

- `Kompass.Api/B56Import/B56ImportController.cs`
- `Kompass.Persistence/Services/EfB56ImportRegister.cs`
- `Kompass.Persistence/Services/B56ArchivService.cs`
- `Kompass.Application/B56Import/B56ImportService.cs`
- `Kompass.Tests/B56ImportControllerTests.cs`
- `Kompass.Tests/B56ImportServiceIntegrationTests.cs`

### 3.2 Technische Vorprüfung der Datei

- `.xlsx` und `.xlsm` werden geprüft.
- Pfad, Existenz, Größe, Signatur und Sperrfehler werden behandelt.

Nachweise:

- `Kompass.Persistence/B56Import/B56DateiPruefer.cs`
- `Kompass.Tests/B56DateiPrueferTests.cs`

### 3.3 Snapshot-Versionierung

- `SnapshotSchemaVersion` und `ParserVersion` sind persistent vorhanden.
- Die Legacy-Migration setzt Standardwerte.
- Ungültige Schema-Versionen lösen eine fachliche Ausnahme aus.

Nachweise:

- `Kompass.Application/B56Import/B56SnapshotVersionen.cs`
- `Kompass.Persistence/Migrations/20260725075146_VersionB56Snapshots.cs`
- `Kompass.Persistence/Services/EfB56ImportRegister.cs`

### 3.4 Fachlicher Snapshot-Lebenszyklus

- Statusmodell und Statuswechsel sind implementiert.
- Bestätigung und Verwerfung sind getrennte Use Cases.
- Zeitpunkte werden gespeichert.

Nachweise:

- `Kompass.Application/B56Import/B56SnapshotStatus.cs`
- `Kompass.Application/B56Import/B56SnapshotLebenszyklusService.cs`
- `Kompass.Api/B56Import/B56SnapshotLebenszyklusController.cs`
- `Kompass.Persistence/Migrations/20260725084054_AddB56SnapshotLifecycle.cs`

### 3.5 Projektmodell-Übernahme

- Ein fachlich bestätigter Snapshot kann in das Projektmodell übernommen werden.
- Herkunft wird an Projekt und Alternativen gespeichert.
- Die Übernahme ist idempotent für denselben Snapshot.

Nachweise:

- `Kompass.Application/B56Import/IB56ProjektmodellUebernahmeService.cs`
- `Kompass.Persistence/Services/B56ProjektmodellUebernahmeService.cs`
- `Kompass.Api/B56Import/B56ProjektmodellController.cs`
- `Kompass.Persistence/Migrations/20260725085558_AddB56ProjectModelOrigin.cs`

### 3.6 Vergleich und stabile Zuordnung

- Snapshot-Vergleich nutzt Kennwertname, Bauteilcode und B56-Position.
- Alternative-Bezeichnungsänderungen gelten als Änderung.
- Entfernte Alternativen werden als nicht mehr vorhanden markiert statt gelöscht.

Nachweise:

- `Kompass.Application/B56Import/B56SnapshotVergleichService.cs`
- `Kompass.Persistence/Migrations/20260725101500_TrackB56AlternativePresence.cs`
- `Kompass.Domain/Projects/Projekt.cs`

## 4. Partially fulfilled

### 4.1 Unveränderlicher Snapshot

Der Snapshot ist technisch versioniert und nachvollziehbar gespeichert. Fachlich offen bleiben aber:

- der explizite, durchgängige Snapshot-Begriff im Domänenmodell;
- eine monotone Snapshot-Nummer pro Projekt;
- ein separater Nachweis für beschädigte Payloads bei gültiger Schema-Version.

### 4.2 Warnungen und Validierung

Warnungen sind im Pipeline-Ergebnis vorhanden und beeinflussen den Status.

Offen bleiben:

- strukturierte Warn- und Fehlercodes;
- persistierte Einzelbefunde außerhalb des JSON-Payloads;
- Auditdaten zur bestätigenden Person.

### 4.3 Modernisierungsalternativen

Position, Bezeichnung, Beschreibung, Bauteile und Kennwerte werden verarbeitet.

Offen bleiben:

- fachlich freigegebene Begrenzung und Vollständigkeit je Exportblatt;
- eindeutige Trennung zwischen importierter Bezeichnung und interner Projektbezeichnung;
- vollständige energetische Ergebnisse je Energieträger.

### 4.4 Projekt-Modell

Das Projektmodell trägt bereits Herkunft und Versionierung, bleibt aber fachlich schmal.

Offen bleiben:

- Standort, Ansprechpartner, Förderparameter, Energiepreise, Verbrauchsdaten;
- Berichtseinstellungen;
- nachvollziehbare abweichende Annahmen und Freigabeinformationen.

### 4.5 Projektlöschung und Zugriff auf Snapshots

Snapshots bleiben in der Datenbank erhalten, sind aber über die reguläre Projekt-API an das existierende Projekt gekoppelt.

Offen bleibt:

- fachliche Regel für Archivierung statt physischer Löschung;
- Zugriff nach Projektlöschung;
- Aufbewahrung und Recovery.

## 5. Not fulfilled

### 5.1 Persistierte Vergleichs- und Konfliktergebnisse

Der Vergleich wird berechnet, aber nicht dauerhaft gespeichert.

Fehlt:

- persistiertes Vergleichsmodell;
- feldweise Konfliktlösung;
- Nutzerentscheidung pro Konflikt;
- Schutz gegen automatische Überschreibung manueller Daten.

### 5.2 Vollständiger erster End-to-End-Prozess

Die bestehenden Tests decken Import, Vergleich und Übernahme gut ab, aber nicht den kompletten fachlichen Ablauf aus Abschnitt 21 der Spezifikation.

Fehlen insbesondere:

- vollständiger HTTP-Durchlauf mit bestätigtem Snapshot;
- Bearbeiten und Speichern ergänzbarer Projektdaten;
- Wiederöffnen des Projekts;
- Nachweis, dass Ergänzungen nach Folgeimport unverändert bleiben.

## 6. Needed DB migrations

Bereits umgesetzt:

- `20260725075146_VersionB56Snapshots`
- `20260725084054_AddB56SnapshotLifecycle`
- `20260725085558_AddB56ProjectModelOrigin`
- `20260725101500_TrackB56AlternativePresence`

Voraussichtlich nötig für die nächste Ausbaustufe:

- persistente Vergleichs- und Konfliktergebnisse;
- feldweise Übernahme-/Synchronisationsdaten;
- Auditdaten für Freigaben und Konfliktentscheidungen;
- optional spätere Erweiterungen für den vollständigen Projektstand.

## 7. Needed tests

Vorhanden:

- Datei-/Import-/Archivtests
- Snapshot-Versionierung und Lebenszyklus
- Übernahme ins Projektmodell
- Vergleichstests
- HTTP-End-to-End- und Smoke-Tests
- **Gesamt: 152 Tests**

Noch sinnvoll:

- vollständiger HTTP-E2E-Test für den gesamten Abschnitt 21;
- Test für beschädigten Payload bei gültiger Schema-Version;
- Test, dass manuelle Ergänzungen nach Folgeimport erhalten bleiben;
- Tests für persistierte Vergleichs-/Konfliktmodelle nach Einführung.

## 8. Risks

- **R1 – Zwei Wahrheiten im Projektmodell:** noch riskant, solange manuelle Projektfelder nicht vollständig modelliert sind.
- **R2 – Alte Snapshots werden nach Modelländerungen schwer lesbar:** aktuell durch Schema-Versionierung gemildert, aber Payload-Feldversionierung bleibt offen.
- **R3 – Benutzeränderungen bei Re-Import:** technisch abgefangen durch Snapshot-/Projektmodell-Trennung, fachlich aber noch nicht vollständig abgesichert.
- **R4 – Snapshot-Zugriff nach Projektlöschung:** fachliche Entscheidung fehlt.
- **R5 – Archiv und Datenbank driften auseinander:** Recovery/Reconciliation fehlt.

## 9. Nächster sinnvoller Schritt

Das nächste Arbeitspaket ist der vollständige erste Anwenderprozess mit bearbeitbaren Projektdaten und durchgehendem HTTP-End-to-End-Nachweis.
