# ADR-0008: B56-Snapshot und Projektmodell

## Status

Angenommen

## Datum

25. Juli 2026

## Kontext

B56 ist der einzige energetische Rechenkern von KOMPASS. KOMPASS
importiert B56-Ergebnisse, archiviert die Quelldatei und stellt die
importierten Fachwerte für die weitere Projektbearbeitung bereit.
KOMPASS führt keine eigenen Berechnungen nach DIN V 18599 durch.

Im aktuellen System existieren zwei fachliche Sichten:

1. Der B56-Import liefert einen projektbezogenen, zeitlich bestimmten
   Stand aus einer konkreten B56-Datei.
2. Das KOMPASS-Projekt enthält den bearbeitbaren Arbeitsstand für
   Beratung, Variantenbildung, Kosten, Auswahlentscheidungen und
   Berichtswesen.

Ohne eine ausdrückliche Regel zur Datenhoheit besteht die Gefahr, dass
ein erneuter Import Benutzeränderungen überschreibt, historische
B56-Werte nachträglich verändert werden oder Import- und Projektmodell
als konkurrierende Wahrheiten auseinanderlaufen.

Diese ADR definiert die fachlichen Rollen beider Modelle, den
Lebenszyklus eines Imports und die Regeln für erneute Importe,
Versionierung und Synchronisation. Sie beschreibt die verbindliche
Zielentscheidung. Die technische Umsetzung erfolgt in separaten
Arbeitspaketen.

## Entscheidung

KOMPASS führt zwei bewusst getrennte Datenbereiche:

- Der **B56-Import-Snapshot** ist die unveränderliche Quelle für alle
  aus einer konkreten B56-Datei übernommenen Daten.
- Das **Projektmodell** ist die bearbeitbare Arbeitskopie für die
  laufende Projektbearbeitung.

Beide Bereiche sind über eine nachvollziehbare Herkunftsbeziehung
verbunden. Sie werden nicht durch implizite oder automatische
Synchronisation gleichgesetzt.

## 1. Fachliche Datenhoheit

### 1.1 Importierte B56-Werte

Für einen Wert, der aus B56 stammt, ist der zugehörige
Import-Snapshot die maßgebliche Quelle für:

- den unveränderten Originalwert;
- Einheit und fachliche Bezeichnung;
- Herkunftsdatei und Datei-Hash;
- Importzeitpunkt;
- Parser- und Schema-Version;
- Warnungen und Erkennungsstatus;
- den Zusammenhang mit Bestand oder Modernisierungsalternative.

Ein Snapshotwert wird nach erfolgreicher Speicherung nicht verändert.
Eine Korrektur erfolgt durch einen neuen Import-Snapshot, nicht durch
Bearbeitung des bestehenden Snapshots.

### 1.2 Projektarbeitsstand

Für den aktuellen Beratungs- und Bearbeitungsstand ist das
Projektmodell maßgeblich. Es darf insbesondere enthalten:

- ausgewählte und verworfene Modernisierungsalternativen;
- projektbezogene Benennungen und Beschreibungen;
- Benutzerkommentare und Bearbeitungsstatus;
- Kostenpositionen und fachlich zulässige manuelle Ergänzungen;
- Zuordnungen zu Berichten und Planständen;
- bewusst übernommene Werte aus einem ausgewählten Snapshot;
- dokumentierte manuelle Abweichungen von importierten Werten.

Das Projektmodell ist bearbeitbar. Eine Bearbeitung des Projektmodells
verändert niemals den zugrunde liegenden Import-Snapshot.

### 1.3 Energetische Werte

Energetische Ergebnisse stammen ausschließlich aus B56.

KOMPASS darf:

- B56-Werte übernehmen;
- Originalwert und Arbeitswert gegenüberstellen;
- einen manuellen Arbeitswert als ausdrücklich gekennzeichnete
  Abweichung speichern;
- Unterschiede und Herkunft anzeigen.

KOMPASS darf nicht:

- einen B56-Snapshot rückwirkend ändern;
- aus Projektänderungen neue DIN-V-18599-Ergebnisse berechnen;
- eine manuelle Abweichung als unverändertes B56-Ergebnis ausgeben;
- fehlende energetische Werte selbst herleiten.

## 2. Identität und Inhalt eines Snapshots

Jeder erfolgreiche Import erzeugt einen logisch eigenständigen
Snapshot mit mindestens:

- stabiler `SnapshotId` beziehungsweise `ImportId`;
- `ProjektId`;
- fortlaufender fachlicher Snapshot-Version innerhalb des Projekts;
- SHA-256-Hash der Quelldatei;
- Originaldateiname und archivierter Quelldatei;
- Importzeitpunkt;
- Snapshot-Schema-Version;
- Parser-Version;
- B56-Quellversion, soweit aus der Datei erkennbar;
- unverändertem fachlichem Importergebnis;
- Importwarnungen und Diagnoseinformationen;
- Status des Imports.

Die Snapshot-Version beschreibt die Reihenfolge erfolgreicher,
fachlich verfügbarer Importe eines Projekts. Sie ist nicht mit der
Schema-Version zu verwechseln:

- **Snapshot-Version**: fachlicher Stand 1, 2, 3, … eines Projekts;
- **Schema-Version**: technisches Format der gespeicherten
  Snapshot-Payload;
- **Parser-Version**: Version der Zuordnungslogik, die den Snapshot
  erzeugt hat;
- **B56-Quellversion**: Version des externen B56-Formats, soweit
  bekannt.

## 3. Lebenszyklus eines B56-Imports

### 3.1 Zustände

Ein Import durchläuft fachlich folgende Zustände:

1. **Empfangen**  
   Die Datei wurde projektbezogen entgegengenommen, ist aber noch
   nicht fachlich verwertbar.

2. **Validiert**  
   Dateiendung, Dateigröße, OpenXML-Struktur und Projektbezug sind
   gültig. Der Datei-Hash wurde bestimmt.

3. **Archiviert**  
   Die unveränderte Quelldatei wurde projektbezogen und
   überschreibgeschützt abgelegt.

4. **Analysiert**  
   Arbeitsblätter und bekannte B56-Bereiche wurden gelesen.
   Unbekannte oder nicht freigegebene Bereiche erzeugen Warnungen und
   werden nicht fachlich erfunden.

5. **Persistiert**  
   Metadaten, Snapshot-Payload, Versionen, Warnungen und
   Herkunftsinformationen wurden gemeinsam gespeichert.

6. **Verfügbar**  
   Der Snapshot kann angezeigt, verglichen und ausdrücklich in das
   Projektmodell übernommen werden.

Alternative Endzustände:

- **Abgelehnt**: Die Datei erfüllt die Eingangsregeln nicht; es entsteht
  kein fachlicher Snapshot.
- **Fehlgeschlagen**: Verarbeitung oder Speicherung wurde nicht
  erfolgreich abgeschlossen; ein unvollständiger Stand darf nicht als
  verfügbar erscheinen.

### 3.2 Sichtbarkeit

Nur ein vollständig persistierter Snapshot erhält den Zustand
„Verfügbar“. Zwischenstände dürfen weder in der regulären
Importhistorie noch als Synchronisationsquelle angeboten werden.

Fehlerhafte oder abgelehnte Versuche dürfen als technische
Diagnoseereignisse protokolliert werden, sind aber keine fachlichen
Snapshot-Versionen.

### 3.3 Unveränderlichkeit

Nach Erreichen des Zustands „Verfügbar“ sind unveränderlich:

- archivierte Quelldatei;
- Datei-Hash;
- fachliche Snapshot-Payload;
- Importwarnungen;
- Schema-, Parser- und Quellversion;
- Importzeitpunkt;
- fachliche Snapshot-Version.

Zulässige nachträgliche Änderungen betreffen nur
Verwaltungsmetadaten, beispielsweise eine Kennzeichnung als
„überholt“ oder „nicht mehr zur Synchronisation empfohlen“. Auch diese
Änderungen müssen den Snapshotinhalt unberührt lassen.

## 4. Regeln für erneute Importe

### 4.1 Gleicher Hash im gleichen Projekt

Der gleiche Datei-Hash im gleichen Projekt ist standardmäßig
idempotent:

- Es wird kein neuer Snapshot erzeugt.
- KOMPASS verweist auf den bereits vorhandenen Snapshot.
- Projektarbeitsdaten werden nicht verändert.

Ein technisch erzwungener erneuter Import derselben Datei ist nur für
Diagnose, Parsermigration oder administrative Wiederherstellung
zulässig. Er muss:

- ausdrücklich angefordert werden;
- einen Grund dokumentieren;
- eine neue Snapshot-ID erhalten;
- die verwendete Parser- und Schema-Version speichern;
- als Re-Import desselben Quellartefakts erkennbar sein.

### 4.2 Neuer Hash im gleichen Projekt

Eine geänderte B56-Datei erzeugt nach erfolgreichem Import einen neuen
Snapshot mit neuer fachlicher Snapshot-Version.

Der neue Snapshot:

- ersetzt oder überschreibt keinen älteren Snapshot;
- wird zunächst nur als neuer verfügbarer Quellstand registriert;
- verändert das Projektmodell nicht automatisch;
- kann mit dem aktuell verwendeten Snapshot verglichen werden.

### 4.3 Gleicher Hash in unterschiedlichen Projekten

Snapshots sind projektbezogen. Der gleiche Dateiinhalt darf in
unterschiedlichen Projekten jeweils einen eigenen Projektbezug
besitzen. Eine projektübergreifende Verknüpfung oder Wiederverwendung
darf keine Daten oder Archivpfade zwischen Projekten offenlegen.

### 4.4 Reihenfolge

Die fachliche Snapshot-Version richtet sich nach dem erfolgreichen
Persistieren, nicht nach Dateiname, Dateidatum oder Uploadbeginn.

Parallelimporte müssen so koordiniert werden, dass:

- keine doppelte Versionsnummer entsteht;
- die Hash-Idempotenz erhalten bleibt;
- nur vollständig gespeicherte Snapshots sichtbar werden.

## 5. Versionierungsregeln

### 5.1 Snapshot-Schema

Jede gespeicherte Snapshot-Payload trägt eine explizite
Schema-Version. Eine Payload ohne erkennbare Version wird als
Legacy-Version behandelt und darf nicht stillschweigend als aktuelle
Version interpretiert werden.

Für jede unterstützte Schema-Version gilt:

- Deserialisierung ist automatisiert getestet;
- die fachliche Bedeutung der Felder ist dokumentiert;
- ein Upgradepfad oder eine klare Nur-Lese-Strategie ist definiert;
- unbekannte neuere Versionen werden kontrolliert abgelehnt.

### 5.2 Parser-Version

Die Parser-Version dokumentiert, mit welcher Zuordnungslogik ein
Snapshot erzeugt wurde.

Eine neue Parser-Version verändert bestehende Snapshots nicht. Wenn
eine Datei mit neuer Parser-Version erneut ausgewertet werden soll,
entsteht ein neuer Snapshot beziehungsweise ein ausdrücklich
gekennzeichneter Re-Import.

### 5.3 Projektmodell-Version

Das Projektmodell besitzt eine eigene Änderungs- beziehungsweise
Concurrency-Version. Sie verhindert, dass eine Synchronisation
zwischenzeitliche Benutzeränderungen unbemerkt überschreibt.

Snapshot-Version und Projektmodell-Version sind unabhängig:

- Ein neuer Snapshot erhöht nicht automatisch die Projektmodell-Version.
- Eine Projektbearbeitung erzeugt keinen neuen B56-Snapshot.
- Eine bestätigte Synchronisation ändert das Projektmodell und
  protokolliert den verwendeten Snapshot.

## 6. Synchronisation in das Projektmodell

### 6.1 Grundsatz

Synchronisation ist ein ausdrücklicher Anwendungsfall. Ein erfolgreicher
Import allein synchronisiert nichts.

Der Benutzer beziehungsweise ein autorisierter Prozess wählt:

- den Quell-Snapshot;
- die zu übernehmenden Bereiche;
- die betroffenen Projektalternativen;
- die Behandlung erkannter Konflikte.

### 6.2 Erstübernahme

Wenn ein Projektbereich noch keine Arbeitskopie besitzt, darf er aus
einem ausgewählten Snapshot initialisiert werden.

Für jedes übernommene Element werden mindestens gespeichert:

- Quell-Snapshot-ID;
- fachlicher Schlüssel beziehungsweise Quellpfad;
- Originalwert;
- übernommener Arbeitswert;
- Übernahmezeitpunkt;
- ausführender Benutzer oder Prozess;
- Projektmodell-Version nach der Übernahme.

### 6.3 Folgeimport

Bei einem neueren Snapshot wird ein Vergleich zwischen:

1. dem Originalwert des zuletzt synchronisierten Snapshots,
2. dem aktuellen bearbeiteten Projektwert und
3. dem Wert des neuen Snapshots

erstellt.

Diese Dreifachsicht bestimmt die Konfliktbehandlung:

- **Nur B56 geändert**: Übernahme darf vorgeschlagen werden.
- **Nur Projekt geändert**: Projektänderung bleibt bestehen.
- **Beide gleich geändert**: Übernahme ist konfliktfrei.
- **B56 und Projekt unterschiedlich geändert**: expliziter Konflikt;
  keine automatische Übernahme.
- **In B56 entfernt**: keine automatische Löschung im Projekt.
- **In B56 neu**: als neue übernehmbare Position anbieten.

### 6.4 Schutz von Benutzeränderungen

Benutzeränderungen werden nie stillschweigend überschrieben.

Eine bestätigte Ersetzung muss:

- den bisherigen Projektwert historisieren oder auditierbar machen;
- den neuen Quell-Snapshot referenzieren;
- die Entscheidung und gegebenenfalls eine Begründung speichern;
- atomar gegen die erwartete Projektmodell-Version erfolgen.

### 6.5 Teilweise Synchronisation

Eine Synchronisation darf auf fachlich geschlossene Bereiche begrenzt
werden, beispielsweise:

- Bestandskennwerte;
- ausgewählte Modernisierungsalternative;
- Bauteile einer Alternative;
- Kosten- oder Förderwerte, sofern fachlich freigegeben.

Teilübernahmen dürfen keine inkonsistenten Aggregate erzeugen.
Abhängige Felder werden entweder gemeinsam übernommen oder die
Übernahme wird abgelehnt.

### 6.6 Wiederholbarkeit

Die erneute Anwendung derselben Synchronisationsentscheidung auf
dieselbe Projektmodell-Version ist idempotent. Sie erzeugt keine
doppelten Alternativen, Bauteile oder Kostenpositionen.

## 7. Konflikte und Herkunft

Jeder aus einem Snapshot übernommene Projektwert besitzt eine
Herkunftskennzeichnung:

- `B56` für unverändert übernommene Werte;
- `Manuell` für rein manuelle Werte;
- `Abweichend` für einen manuell geänderten ehemaligen B56-Wert.

Bei `Abweichend` bleiben Originalwert und Quell-Snapshot nachvollziehbar.
Die Benutzeroberfläche und spätere Berichte müssen B56-Originalwert und
abweichenden Arbeitswert eindeutig unterscheiden.

Ein Konflikt ist ein fachlicher Zustand und kein technischer Fehler.
Er bleibt offen, bis eine ausdrückliche Entscheidung getroffen wurde.

## 8. Löschen, Aufbewahrung und Überholt-Markierung

Snapshots werden im normalen Projektworkflow nicht gelöscht oder
überschrieben.

Ein Snapshot darf als „überholt“ markiert werden. Dadurch:

- bleibt er lesbar und auditierbar;
- bleibt seine Quelldatei erhalten;
- wird er nicht mehr als bevorzugte Synchronisationsquelle angeboten;
- bleiben vorhandene Herkunftsverweise gültig.

Physisches Löschen ist ausschließlich über eine gesonderte
Aufbewahrungs- und Datenschutzregel zulässig. Vor dem Löschen muss
geprüft werden, ob Projektwerte, Berichte oder Auditdaten den Snapshot
referenzieren.

## 9. Fehler- und Wiederanlaufregeln

Ein Import ist nur dann erfolgreich, wenn Archiv und persistierter
Snapshot konsistent verfügbar sind.

Für Abbrüche zwischen Dateisystem und Datenbank gilt:

- unvollständige Snapshots werden nicht sichtbar;
- verwaiste Archivdateien werden durch einen
  Reconciliation-/Bereinigungsprozess erkannt;
- ein Wiederholungsversuch respektiert Hash-Idempotenz und
  Versionsregeln;
- technische Fehler verändern das Projektmodell nicht.

Eine Synchronisation ist atomar innerhalb des Projektmodells. Scheitert
sie, bleibt die vorherige Projektmodell-Version gültig.

## 10. Auditierbarkeit

Folgende Ereignisse müssen nachvollziehbar sein:

- erfolgreicher, abgelehnter und fehlgeschlagener Import;
- erzwungener Re-Import;
- Auswahl eines Snapshots zur Erstübernahme;
- Vergleich mit einem Folgeimport;
- bestätigte, abgelehnte oder teilweise Synchronisation;
- Konfliktentscheidung;
- manuelle Abweichung von einem B56-Wert;
- Überholt-Markierung und zulässige Löschung.

Ein Auditereignis enthält mindestens Zeitpunkt, Projekt, Snapshot,
Projektmodell-Version, Aktion und ausführenden Benutzer oder Prozess.

## 11. Verbindliche Invarianten

1. Ein verfügbarer Snapshot wird niemals inhaltlich verändert.
2. Ein neuer Import überschreibt niemals automatisch das Projektmodell.
3. Benutzeränderungen werden niemals stillschweigend überschrieben.
4. Jeder übernommene B56-Wert bleibt auf Snapshot und Originalwert
   zurückführbar.
5. KOMPASS berechnet keine energetischen B56-Ergebnisse neu.
6. Ein Snapshot ist nur nach vollständiger Archivierung und Persistenz
   verfügbar.
7. Gleicher Hash im gleichen Projekt ist standardmäßig idempotent.
8. Schema-, Parser-, Snapshot- und Projektmodell-Version sind getrennte
   Konzepte.
9. Synchronisationen prüfen die erwartete Projektmodell-Version.
10. Projektübergreifende Daten- oder Archivpfadlecks sind unzulässig.

## 12. Konsequenzen

### Positive Konsequenzen

- B56-Originaldaten bleiben beweis- und auditierbar.
- Projektbearbeitung ist möglich, ohne Quelldaten zu verfälschen.
- Folgeimporte können kontrolliert verglichen werden.
- Benutzeränderungen sind vor automatischem Überschreiben geschützt.
- Wirtschaftlichkeit und Berichtswesen erhalten eine definierte
  Arbeitsgrundlage.
- Historische Berichte können auf den verwendeten Snapshot
  zurückgeführt werden.

### Negative Konsequenzen

- Snapshot- und Projektmodell müssen getrennt gespeichert und
  versioniert werden.
- Synchronisation benötigt Herkunftsmetadaten, Konfliktmodell und
  Auditierung.
- Folgeimporte führen nicht automatisch zum aktuellsten Projektstand;
  eine fachliche Entscheidung ist erforderlich.
- JSON-Snapshots benötigen eine langfristige
  Kompatibilitätsstrategie.
- Benutzeroberfläche und API müssen Original-, Arbeits- und
  Konfliktwerte unterscheiden.

## 13. Technische Umsetzungsschritte

Diese ADR autorisiert keine unmittelbare Quellcodeänderung. Die
Umsetzung wird in getrennten Arbeitspaketen geplant:

1. Snapshot-Schema-Version und fachliche Snapshot-Version modellieren.
2. Bestehende `FachdatenJson`-Einträge als Legacy-/Version-1-Snapshots
   lesbar halten.
3. Herkunftsmetadaten und Projektmodell-Version definieren.
4. Vergleichs- und Konfliktmodell spezifizieren.
5. Erstübernahme und Folge-Synchronisation als explizite
   Application-Use-Cases implementieren.
6. Auditereignisse und Reconciliation-Prozess ergänzen.
7. API- und Desktopdarstellung für Originalwert, Arbeitswert,
   Abweichung und Konflikt entwickeln.
8. Migrations-, Kompatibilitäts-, Parallelitäts- und
   End-to-End-Tests ergänzen.

## 14. Nicht Bestandteil dieser Entscheidung

- fachliche Zuordnung weiterer B56-Exportbereiche ohne gesonderte
  Feldfreigabe;
- eigene DIN-V-18599-Berechnungen;
- Wärmebrückenberechnung außerhalb von ThermCAD;
- IFC-Auswertung;
- konkrete Aufbewahrungsfristen;
- konkrete Benutzer- und Rollenverwaltung;
- sofortige Änderung bestehender Datenbank- oder API-Verträge.
