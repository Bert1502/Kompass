# Architekturreview KOMPASS

Stand: 25. Juli 2026
Analysierter Commit: `2840858e9612221d61bdb5c26122fa38686c1892`

## 1. Auftrag und Methode

Dieser Bericht bewertet den vollständigen aktuellen `main`-Stand des
Repositories. Es wurden keine Quellcode-, Projekt- oder
Konfigurationsdateien verändert.

Die Bestandsaufnahme umfasst:

- `dotnet restore`, Debug- und Release-Build;
- Debug- und Release-Testlauf;
- EF-Core-Migrationsprüfung;
- Solution-, Projekt- und Paketgraph;
- statische Sichtung von Domain, Application, Persistence, API und
  Desktop;
- Dependency-Injection-Komposition und Konfiguration;
- Testinventar und instrumentierte Code-Coverage;
- vorhandene Architektur-, Modul- und Entwicklungsdokumentation.

Die Coverage-Messung wurde außerhalb des Repositorys erzeugt. Sie
umfasst die von `Kompass.Tests` referenzierten und instrumentierten
Assemblies. `Kompass.Desktop` ist nicht Teil dieser Messung.

## 2. Management Summary

KOMPASS befindet sich technisch in einem stabilen, buildbaren Zustand.
Die ursprünglichen B56-Buildblocker sind beseitigt, der
projektbezogene B56-Import ist durchgängig implementiert und durch
einen End-to-End-Smoke-Test abgesichert. Die Solution besitzt eine
erkennbare Schichtenarchitektur mit korrekter Abhängigkeitsrichtung
von Domain über Application zu Infrastruktur und Präsentation.

Die höchste verbleibende Architekturrisiko ist keine
Compilerinstabilität, sondern eine fachliche Doppelspur:

1. `Kompass.Domain` enthält ein reiches, relational persistiertes
   Projektmodell mit Modernisierungsalternativen, Bauteilen und Kosten.
2. Der produktive B56-Import verwendet ein separates
   Application-Modell und persistiert seine Fachdaten als JSON im
   Importregister.

Zwischen beiden Modellen gibt es keine dokumentierte
Synchronisations- oder Überführungsgrenze. Damit ist derzeit unklar,
welches Modell langfristig die fachliche Wahrheit für
Modernisierungsalternativen darstellt.

Weitere Schwerpunkte sind fehlende Tests für das Domänenmodell, die
Projektverwaltung und den gesamten WPF-Client, mehrere sehr große
Klassen sowie veraltete oder zu knappe Architekturdokumentation.

## 3. Build- und Qualitätsstatus

| Prüfung | Ergebnis |
|---|---:|
| `dotnet restore Kompass.sln` | erfolgreich |
| Debug-Build | 0 Fehler, 0 Warnungen |
| Debug-Tests | 34/34 bestanden |
| Release-Build | 0 Fehler, 0 Warnungen |
| Release-Tests | 34/34 bestanden |
| EF Core: ausstehende Modelländerungen | keine |
| NuGet Audit | aktiv, keine Restore-Warnung |
| Warnungen als Fehler | repositoryweit aktiv |

Positive Build- und CI-Eigenschaften:

- Nullable Reference Types und Implicit Usings sind in allen Projekten
  aktiv.
- `Directory.Build.props` erzwingt `TreatWarningsAsErrors`.
- NuGet Audit läuft im Modus `all`; `NU1901` bis `NU1904` werden als
  Fehler behandelt.
- `dotnet-ef` ist als lokales Tool auf `8.0.29` festgelegt.
- GitHub Actions baut Debug und Release auf Windows, validiert
  Migrationen und führt die Tests aus.
- Die Workflow-Actions verwenden aktuelle Node-Laufzeiten und
  erzeugten zuletzt keine Annotationen.

Verbleibende Reproduzierbarkeitslücke:

- Es gibt kein `global.json`. CI fordert zwar `.NET 8.0.x` an, lokale
  Builds können aber mit unterschiedlichen installierten SDK-Patches
  laufen.

## 4. Solution und Projektstruktur

Die Solution enthält sechs Projekte:

| Projekt | Verantwortung | Produktionsdateien | Direkte Projektabhängigkeiten |
|---|---|---:|---|
| `Kompass.Domain` | Aggregate, Entitäten, Invarianten | 11 C# | keine |
| `Kompass.Application` | Anwendungsfälle, Ports, B56-Transfermodell | 38 C# | Domain |
| `Kompass.Persistence` | EF Core, SQLite, OpenXML, Archiv/Hashing | 34 C# | Application, Domain |
| `Kompass.Api` | HTTP-Endpunkte und Composition Root | 7 C# | Application, Persistence |
| `Kompass.Desktop` | WPF/MVVM und API-Clients | 26 C#, 4 XAML | Application |
| `Kompass.Tests` | Unit-, Integrations- und Smoke-Tests | 11 C# | API, Application, Domain, Persistence |

Die grundlegende Abhängigkeitsrichtung ist sinnvoll:

```text
Domain
  ↑
Application
  ↑             ↑
Persistence     Desktop
  ↑
API
```

`Kompass.Api` darf als Composition Root die Infrastruktur referenzieren.
`Kompass.Desktop` kommuniziert zur Laufzeit per HTTP mit der API und
referenziert Application derzeit zusätzlich für gemeinsame B56-Enums.

## 5. NuGet- und Technologieinventar

Wesentliche direkte Pakete:

- API: Swashbuckle `6.6.2`, Configuration Binder/Options `8.0.x`;
- Application: Microsoft.Extensions.Options `8.0.2`;
- Persistence: EF Core SQLite/Design/Tools `8.0.29`,
  DocumentFormat.OpenXml `3.5.1`,
  SQLitePCLRaw.bundle_e_sqlite3 `3.0.4`;
- Desktop: Configuration, DI und HttpClient `8.0.1`;
- Tests: xUnit `2.9.3`, Runner `3.1.5`,
  Microsoft.NET.Test.Sdk `18.8.1`, coverlet `10.0.1`.

Bewertung:

- `Kompass.Domain` ist frei von externen Paketen.
- Infrastrukturpakete sind überwiegend korrekt in Persistence
  konzentriert.
- Microsoft.Extensions-Pakete sind über mehrere Patchstände verteilt
  (`8.0.0`, `8.0.1`, `8.0.2`). Das ist aktuell kompatibel, sollte aber
  zentral verwaltet werden, bevor weitere Projekte entstehen.
- Es gibt noch kein Central Package Management
  (`Directory.Packages.props`).

## 6. Domänenmodell

### 6.1 Projekt-Aggregat

`Projekt` ist ein Aggregate Root mit:

- Name und Namensvalidierung;
- Sammlung von `Modernisierungsalternative`;
- Schutz gegen doppelte Alternativen anhand der ID.

Eine `Modernisierungsalternative` enthält:

- Bezeichnung und Kurztext;
- `AlternativeBauteil`;
- `Kostenposition`;
- berechnete Gesamtkosten als Summe der Kostenpositionen.

`AlternativeBauteil` referenziert einen B56-`Bauteilcode`.
`Kostenposition` schützt Bezeichnung und nichtnegative Beträge.

### 6.2 B56-Domäne

Im Domain-Projekt existieren:

- `Bauteilcode`;
- das Aggregate Root `B56ImportDatei`;
- `B56ImportZeile`.

Daneben existiert in Application ein separates B56-Importmodell:

- Arbeitsmappe, Arbeitsblatt, Zeile und Zelle;
- Bestandkennwerte und importierte Bauteile;
- importierte Modernisierungsalternativen;
- Pipeline- und Importergebnisse.

### 6.3 Persistenz

EF Core modelliert das Projekt-Aggregat relational. Der
`KompassDbContext` exponiert `Projekte` und `B56ImportEintraege`; weitere
Domain-Typen werden über die Aggregate und
`ApplyConfigurationsFromAssembly` eingebunden.

Vier Migrationen sind vorhanden:

1. `20260718205341_InitialCreate`
2. `20260719104649_ProjektverwaltungErweitert`
3. `20260720073017_AddB56ImportRegister`
4. `20260724184936_PersistB56DomainResults`

Die importierten B56-Fachdaten werden im Importregister als
`FachdatenJson` gespeichert. Metadaten und Projekt-/Hash-Indizes sind
relational abgebildet.

## 7. Erkannte Architekturverstöße und Risiken

### A1 – Zwei konkurrierende fachliche Modelle

**Priorität: hoch**

Das Domain-Projekt modelliert Modernisierungsalternativen, Bauteile und
Kosten relational. Der produktive B56-Pfad erzeugt dagegen
`Kompass.Application.B56Import.B56Modernisierungsalternative` und
speichert sie als JSON. Die Domain-Typen `Modernisierungsalternative`,
`AlternativeBauteil`, `Kostenposition`, `B56ImportDatei` und
`B56ImportZeile` werden vom aktuellen Importpfad nicht aufgebaut.

Folgen:

- unklare fachliche Source of Truth;
- doppelte Begriffe und Mappingaufwand;
- Gefahr divergierender Validierungsregeln;
- Wirtschaftlichkeits- und Berichtsmodule können versehentlich auf
  unterschiedliche Datenmodelle zugreifen.

Vor einer Änderung ist eine explizite Architekturentscheidung nötig:
Import-Snapshot als unveränderliches Quellartefakt, Überführung in das
Projekt-Aggregat oder bewusstes Nebeneinander mit klaren Grenzen.

### A2 – Anwendungsorchestrierung in Persistence

**Priorität: mittel**

`B56ImportPipeline` und `B56TabellenImportService` liegen in
Persistence, obwohl sie fachliche Importorchestrierung,
Tabellenbedeutungen und Ergebnisbildung enthalten. OpenXML-Lesen,
Dateisystem und EF gehören eindeutig in Persistence; die Zuordnung
„welche B56-Tabelle bedeutet was“ ist dagegen Anwendungs- bzw.
Domänenlogik.

Folgen:

- fachliche Regeln sind mit Infrastrukturcode vermischt;
- Parserregeln lassen sich schwerer unabhängig testen und ersetzen;
- Persistence wächst zum faktischen B56-Modul statt Adapter zu bleiben.

### A3 – Manuell duplizierte API-Verträge

**Priorität: mittel**

API-Antworttypen und Desktop-DTOs bilden denselben B56-Vertrag separat
ab. Änderungen an Listen, Nullable-Semantik oder Feldnamen werden nur
durch Laufzeittests entdeckt.

Empfehlung: Vertrag explizit versionieren und entweder generierten
Clientcode aus OpenAPI verwenden oder ein bewusstes, präsentationsfreies
Contracts-Projekt einführen. Keine Domain-Entitäten als HTTP-Vertrag
verwenden.

### A4 – Überladene Klassen und Dateien

**Priorität: mittel**

Besonders große Dateien:

- `B56ImportViewModel.cs`: 661 Zeilen;
- `B56TabellenImportService.cs`: 426 Zeilen;
- `MainWindowViewModel.cs`: 420 Zeilen;
- `B56ImportController.cs`: 418 Zeilen;
- `B56ImportView.xaml`: 324 Zeilen;
- `ProjektApiClient.cs`: 284 Zeilen;
- `OpenXmlB56ArbeitsmappenLeser.cs`: 240 Zeilen.

Der B56-Controller enthält Endpunkte, Upload-Dateimanagement und alle
Antwortmodelle. Das B56-ViewModel verwaltet Dateiauswahl, Upload,
Historie, Detailabruf, Anzeigezustand und Befehle. Der Tabellenimport
vereint Abschnittserkennung, Feldzuordnung, Zahlenkonvertierung und
Warnungen.

Das erschwert gezielte Tests und erhöht das Risiko großer,
konfliktanfälliger Änderungen.

### A5 – Automatische Migration beim API-Start

**Priorität: mittel, abhängig vom Betriebsmodell**

`Program.cs` führt bei jedem Start `Database.MigrateAsync()` aus. Für
eine lokale Einzelplatzanwendung ist das pragmatisch. Bei mehreren
Instanzen, kontrollierten Releases oder produktiven Datenbanken fehlen
jedoch:

- explizite Backup-/Rollback-Strategie;
- Koordination konkurrierender Starts;
- getrennte Deployment-Berechtigungen;
- dokumentierte Recovery bei abgebrochener Migration.

### A6 – Dateisystem und Datenbank sind nicht atomar

**Priorität: mittel**

Der Import archiviert zuerst eine Datei und speichert danach den
Registereintrag. Fehler werden kompensiert, indem die Archivdatei
gelöscht wird. Ein Prozessabsturz oder Stromausfall zwischen beiden
Schritten kann dennoch eine verwaiste Datei hinterlassen.

Empfehlung: Wiederanlauf-/Reconciliation-Prozess dokumentieren und
testen; nicht versuchen, Dateisystem und SQLite in eine scheinbar
globale Transaktion zu zwingen.

### A7 – JSON-Persistenz ohne sichtbare Schema-Version

**Priorität: mittel**

`FachdatenJson` ist einfach und bewahrt den Import-Snapshot, bietet aber
keine erkennbare Payload-Version. DTO-Umbenennungen oder geänderte
Serialisierung können historische Importe unlesbar machen.

Empfehlung: Schema-/Formatversion zusammen mit dem Snapshot speichern,
Kompatibilitätstest mit historischen Payloads ergänzen und
Migrationsstrategie definieren.

### A8 – Namespace- und Formatinkonsistenzen

**Priorität: niedrig**

Persistence verwendet parallel:

- `Kompass.Persistence`;
- `Kompass.Persistence.Services`;
- `Kompass.Persistence.B56Import`.

Einzelne B56-Implementierungen sind zwischen `B56Import` und `Services`
verteilt. Die DI-Datei enthält zudem sichtbar inkonsistente Einrückung.
Das beeinflusst den Build nicht, erschwert aber Navigation und
Ownership.

## 8. Technische Schulden

1. Kein explizit dokumentiertes Zielmodell für importierte
   Modernisierungsalternativen.
2. B56-Fachdaten als unversionierter JSON-Blob.
3. Keine zentrale Paketversionsverwaltung.
4. Kein `global.json` für lokale SDK-Reproduzierbarkeit.
5. Große Controller-, Parser-, Client- und ViewModel-Klassen.
6. Desktop- und API-Verträge werden manuell synchron gehalten.
7. Projekt-CRUD und B56-Import verwenden unterschiedliche
   Persistenzformen für fachlich zusammengehörige Daten.
8. Keine dokumentierte Betriebsstrategie für Archivbereinigung,
   Datenbankbackup und Migrationen.
9. Keine Authentifizierung oder Autorisierung an der API. Das kann für
   ausschließlich lokalen Betrieb akzeptabel sein, muss vor Netzwerk-
   oder Mehrbenutzerbetrieb neu bewertet werden.
10. Keine automatisierten Architekturtests, welche verbotene
    Projekt- oder Namespace-Abhängigkeiten verhindern.

## 9. Teststatus und fehlende Tests

### 9.1 Gemessener Stand

- 34 Tests bestehen in Debug und Release.
- 31 Testmethoden verwenden `[Fact]` oder `[Theory]`; Theories erzeugen
  mehrere Testfälle.
- Instrumentierte Zeilenabdeckung: **70,95 %**.
- Instrumentierte Branch-Abdeckung: **55 %**.
- Abdeckung nach Assembly:

| Assembly | Zeilen | Branches |
|---|---:|---:|
| `Kompass.Api` | 56,38 % | 73,68 % |
| `Kompass.Application` | 79,39 % | 46,87 % |
| `Kompass.Domain` | 0 % | 0 % |
| `Kompass.Persistence` | 77,36 % | 65,55 % |
| `Kompass.Desktop` | nicht instrumentiert | nicht instrumentiert |

Die gute Gesamtrate wird stark durch den B56-Pfad getragen. Sie darf
nicht als gleichmäßige Abdeckung der Anwendung interpretiert werden.

### 9.2 Kritische Testlücken

**P1**

- Keine Domain-Tests für `Projekt`,
  `Modernisierungsalternative`, `AlternativeBauteil`,
  `Kostenposition`, Entity-Gleichheit und Invarianten.
- Keine Tests für `ProjektService` und keinen End-to-End-Test des
  Projekt-CRUD.
- Keine Tests für `ProjekteController`.
- Keine Tests für WPF-ViewModels, Befehlszustände, Auswahlwechsel,
  Fehleranzeigen oder API-Ausfälle.
- Keine Tests für Desktop-API-Clients und JSON-Vertragskompatibilität.

**P2**

- Der B56-Smoke-Test ruft den Controller direkt auf; Routing,
  Multipart-Model-Binding, Middleware, HTTPS-Verhalten und echte
  JSON-Serialisierung werden nicht über einen Testserver geprüft.
- Keine Parallelitäts-/Race-Tests für zwei identische gleichzeitige
  Importe desselben Projekts.
- Keine Kompatibilitätstests für bereits gespeicherte
  `FachdatenJson`-Versionen.
- Keine Tests für abgebrochene Prozesse zwischen Archivierung und
  Registerpersistenz.
- Keine Upgrade-Tests von einer realistischen älteren Datenbank mit
  Daten; geprüft wird nur Migration auf eine leere Datenbank.
- Keine Negativtests für beschädigtes oder unerwartetes gespeichertes
  JSON.
- Keine Tests für maximale Zahl und Anzeige aller neun Alternativen im
  WPF-Binding; der Parser selbst ist abgedeckt.

**P3**

- Keine automatisierten Architekturregeln, etwa:
  Domain darf keine Infrastruktur referenzieren,
  Application darf Persistence nicht referenzieren,
  Desktop darf keine Persistence-Typen verwenden.
- Keine definierte Coverage-Schwelle in CI.
- Keine Last-/Volumentests für große XLSM-Dateien bis zum konfigurierten
  50-MiB-Limit.

## 10. Dokumentationslücken

### Veraltet

- `docs/TECHNICAL_BASELINE.md` beschreibt noch eine sehr geringe
  Testabdeckung und behauptet, `TreatWarningsAsErrors` sei nicht
  repositoryweit aktiv. Beides ist überholt.
- `docs/modules/B56.md` nennt 33 statt aktuell 34 Tests.
- Der technische Stand verteilt sich über Baseline und B56-Modulseite,
  ohne klaren Aktualisierungsprozess.

### Zu knapp oder fehlend

- `docs/architecture/SYSTEM_OVERVIEW.md` besteht im Wesentlichen aus
  einer einzeiligen Schichtenliste; Komponenten, Laufzeitkommunikation,
  Datenflüsse und Deployment fehlen.
- Kein dokumentierter Projekt-/Assembly-Abhängigkeitsgraph.
- Kein Datenmodell für Projekt, Importregister, Archiv und
  B56-Snapshot.
- Kein ADR zur Trennung oder Zusammenführung von Domain-Projektmodell
  und B56-Importmodell.
- Kein ADR zur JSON-Snapshot-Persistenz und deren Versionierung.
- Kein API-Vertrag bzw. Versionierungskonzept außerhalb von Swagger.
- Kein Betriebs-/Recovery-Handbuch für SQLite, Archiv, Backup,
  Migration und verwaiste Dateien.
- Keine Teststrategie mit Testpyramide, Testdatenregeln und
  Coverage-Zielen.
- Keine Sicherheits- und Datenschutzannahmen für hochgeladene
  Projektdaten.
- ThermCAD, Wirtschaftlichkeit und Berichtswesen sind nur als
  Platzhalter dokumentiert.
- Das Developer Handbook führt B56 weiterhin als „offenes Kapitel“,
  obwohl ein umfangreicher Implementierungsstand vorliegt.

## 11. Priorisierte Empfehlungen

### P1 – Fachliche Datenhoheit entscheiden

Vor dem Ausbau von Wirtschaftlichkeit oder Berichtswesen ein ADR
erstellen:

1. Ist `FachdatenJson` ein unveränderlicher B56-Quellsnapshot?
2. Welche Daten werden in das `Projekt`-Aggregat übernommen?
3. Wann und durch welchen Anwendungsfall erfolgt die Übernahme?
4. Wie werden erneute Importe, Versionen und Benutzeränderungen
   behandelt?

Ohne diese Entscheidung wächst die fachliche Doppelmodellierung weiter.

### P1 – Domain und Projektverwaltung absichern

- Unit-Tests für alle Domain-Invarianten;
- Integrations-/API-Tests für Projekt anlegen, umbenennen, lesen und
  löschen;
- Beziehung zwischen Projekt und B56-Import explizit testen;
- erst danach neue fachliche Module auf das Projektmodell aufsetzen.

### P1 – Historische B56-Snapshots versionieren

- Payload-Version in `B56ImportEintragEntity` ergänzen;
- Roundtrip-Test für die aktuelle Version;
- Fixture-Test für mindestens eine ältere Version;
- Verhalten bei unbekannter oder beschädigter Version definieren.

Diese Änderung betrifft Persistenz und Vertrag und sollte als eigenes
Migrationspaket umgesetzt werden.

### P2 – Echte HTTP- und Desktop-Vertragstests

- API mit Testserver über echte HTTP-Aufrufe prüfen;
- Upload, Historie und Details inklusive JSON serialisieren;
- Desktop-Client gegen diese Verträge testen;
- Fehlerfälle wie 404, 500, Timeout und ungültiges JSON abdecken.

### P2 – Große B56-Komponenten zerlegen

Ohne Verhaltensänderung:

- Abschnittserkennung, Kennwertparser und Bauteiltabellenparser aus
  `B56TabellenImportService` extrahieren;
- HTTP-Antwortmodelle aus dem Controller verschieben;
- Dateiauswahl/Upload und Ergebnis-/Historienanzeige in getrennte
  Desktop-ViewModels oder Services aufteilen;
- API-Clients nach Ressource und Verantwortung gliedern.

Jede Zerlegung benötigt Charakterisierungstests vor der Änderung.

### P2 – Build reproduzierbarer machen

- `global.json` mit dokumentierter Roll-forward-Policy;
- `Directory.Packages.props` für zentrale Paketversionen;
- optional deterministische Builds und SourceLink prüfen;
- Coverage-Bericht und sinnvolle, schrittweise Schwellen in CI
  aufnehmen.

### P2 – Betriebsfähigkeit dokumentieren

- Backup und Restore für `kompass.db` und B56-Archiv;
- Reihenfolge und Konsistenz beider Speicher;
- Recovery verwaister Archivdateien;
- Verhalten bei fehlendem Schreibzugriff und vollem Datenträger;
- Migrations- und Rollbackprozess.

### P3 – Architekturdokumentation konsolidieren

- Systemkontext, Container und Komponenten dokumentieren;
- Projektgraph und Datenfluss „Desktop → API → Import → Archiv/SQLite“
  darstellen;
- Baseline als historische Momentaufnahme kennzeichnen oder
  aktualisieren;
- Modulstatus und Testzahlen automatisiert oder releasebezogen pflegen.

### P3 – Erst danach weitere B56-Bereiche zuordnen

Energiebilanz, Energiebericht, Zonen und Neubau dürfen erst nach
fachlicher Feldfreigabe importiert werden. KOMPASS darf dabei keine
eigenen DIN-V-18599-Ergebnisse berechnen. ThermCAD bleibt für
Wärmebrückennachweise zuständig; IFC bleibt außerhalb des Umfangs.

## 12. Empfohlene Reihenfolge der nächsten Arbeitspakete

1. ADR „B56-Snapshot und Projekt-Domänenmodell“.
2. Domain- und Projekt-CRUD-Testpaket.
3. Versionierung von `FachdatenJson` mit Migration und
   Kompatibilitätstests.
4. Echter HTTP-Vertragstest plus Desktop-Clienttests.
5. Zerlegung der großen B56-/Desktop-Komponenten.
6. SDK- und Paketversionszentralisierung.
7. Betriebs-/Recovery-Dokumentation.
8. Erst nach fachlicher Freigabe: zusätzliche B56-Exportbereiche.

## 13. Schlussbewertung

KOMPASS ist aktuell buildstabil und der freigegebene B56-Importpfad ist
für den erreichten Umfang gut abgesichert. Die nächste Entwicklungsphase
sollte nicht mit weiteren Parserfeldern beginnen, sondern die
fachliche Datenhoheit zwischen Import-Snapshot und Projekt-Domäne
klären. Parallel müssen Domain, Projektverwaltung und Desktop aus der
gegenwärtigen Testblindstelle geholt werden.

Es bestehen keine P0-Buildblocker. Die wichtigsten Risiken sind
strukturell und beherrschbar, sofern die Datenmodellentscheidung vor
dem Ausbau von Wirtschaftlichkeit und Berichtswesen getroffen wird.
