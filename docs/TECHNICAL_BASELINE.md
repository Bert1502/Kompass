# Technische Baseline

Stand: 24. Juli 2026

Repository: `Bert1502/Kompass`

Ausgangsbranch: `main`

Analysiertes Commit: `31a8d3d5f51331667e848a3df11e671ddb8ab4cb`

Lokale Umgebung: Windows, .NET SDK `9.0.316`

CI-Umgebung: `windows-latest`, .NET SDK `8.0.x`

## Zusammenfassung

Die Solution ist build- und testfähig. Restore, Debug-Build, Release-Build und beide Testläufe sind erfolgreich. Die Builds melden keine Compilerfehler und keine Warnungen.

| Prüfung | Ergebnis | Fehler | Warnungen |
| --- | --- | ---: | ---: |
| `dotnet restore` | Erfolgreich | 0 | 0 |
| `dotnet build --no-restore --configuration Debug` | Erfolgreich | 0 | 0 |
| `dotnet test --no-build --no-restore --configuration Debug` | 2 von 2 Tests bestanden | 0 | 0 |
| `dotnet build --no-restore --configuration Release` | Erfolgreich | 0 | 0 |
| `dotnet test --no-build --no-restore --configuration Release` | 2 von 2 Tests bestanden | 0 | 0 |

GitHub Actions führt Restore, Build, EF-Migrationsprüfung und Tests für Debug und Release aus. Der Build des analysierten `main`-Stands ist in beiden Konfigurationen erfolgreich.

## Solution und Projektstruktur

`Kompass.sln` enthält sechs Projekte und 112 C#-Quelldateien:

| Projekt | Verantwortung | Direkte Projektabhängigkeiten |
| --- | --- | --- |
| `Kompass.Domain` | Domänenmodell und fachliche Basistypen | keine |
| `Kompass.Application` | Anwendungsverträge, DTOs und B56-Anwendungsmodell | `Kompass.Domain` |
| `Kompass.Persistence` | EF Core, SQLite, OpenXML und technische Implementierungen | `Kompass.Application`, `Kompass.Domain` |
| `Kompass.Api` | ASP.NET-Core-Host und HTTP-Endpunkte | `Kompass.Application`, `Kompass.Persistence` |
| `Kompass.Desktop` | WPF-Client und Desktop-Composition-Root | `Kompass.Application` |
| `Kompass.Tests` | xUnit-Tests | `Kompass.Application`, `Kompass.Domain`, `Kompass.Persistence` |

Der Projektgraph folgt der vorgesehenen Schichtung. Domain besitzt keine äußeren Projektabhängigkeiten; Application hängt nur von Domain ab; Hosts und Persistence hängen nach innen.

## NuGet-Pakete

Die direkten produktiven Paketgruppen sind:

- Application: `Microsoft.Extensions.Options 8.0.2`
- Persistence: `DocumentFormat.OpenXml 3.5.1`, EF Core Design/SQLite/Tools `8.0.29`, `SQLitePCLRaw.bundle_e_sqlite3 3.0.4`, Options Configuration Extensions `8.0.0`
- API: Configuration Binder `8.0.2`, Options Configuration Extensions `8.0.0`, Swashbuckle `6.6.2`
- Desktop: Configuration JSON, Dependency Injection und HTTP `8.0.x`
- Tests: coverlet `10.0.1`, Microsoft.NET.Test.Sdk `18.8.1`, xUnit `2.9.3`, Runner `3.1.5`

Die früher gemischten `Microsoft.Extensions.*`-Generationen 8 und 10 sind auf .NET 8 ausgerichtet.

`dotnet list Kompass.sln package --vulnerable --include-transitive` meldet für alle sechs Projekte keine bekannten anfälligen Pakete. Der frühere High-Severity-Fund in `SQLitePCLRaw.lib.e_sqlite3 2.1.6` wurde durch den 3.x-Bundle-Zweig beseitigt.

Eine zentrale Paketverwaltung mit `Directory.Packages.props` existiert weiterhin nicht. Paketversionen werden in den einzelnen Projektdateien gepflegt.

## Entity Framework Core

Persistence besitzt die vollständige EF-Core-Design- und Migrationsverantwortung:

- `KompassDbContext`
- `KompassDbContextFactory`
- acht `IEntityTypeConfiguration<T>`-Klassen
- drei Migrationen
- `KompassDbContextModelSnapshot`
- `Microsoft.EntityFrameworkCore.Design` und Tools ausschließlich im Persistence-Projekt

Das Repository enthält ein lokales Tool-Manifest für `dotnet-ef 8.0.29`. Folgende Migrationen werden lokal und in CI in Debug und Release erkannt:

1. `20260718205341_InitialCreate`
2. `20260719104649_ProjektverwaltungErweitert`
3. `20260720073017_AddB56ImportRegister`

Die CI verwendet `--no-connect`. Sie validiert daher Tooling, Design-Time-Factory und Migrationsassembly, prüft aber nicht, ob Migrationen auf einer realen oder temporären Datenbank erfolgreich angewendet und zurückgelesen werden können.

## Dependency Injection und Konfiguration

Die B56-Komposition registriert die benötigten Verträge und wird durch einen Composition-Root-Test aufgelöst. Dazu gehören unter anderem Dateiprüfung, Archivierung, Hashing, Importregister und Bauteilzuordnungsrepository.

`B56ImportOptionen` werden aus der API-Konfiguration gebunden und beim Start validiert. Der Archivpfad ist relativ konfiguriert.

Der Desktop:

- lädt seine API-Basisadresse aus `Kompass.Desktop/appsettings.json`
- validiert die Adresse als absolute URI
- kopiert die Konfiguration in Debug- und Release-Ausgaben
- registriert `IProjektApiClient` als typisierten Client über `IHttpClientFactory`

Damit bestehen die früher dokumentierten DI-Lücken und fest codierten C#-Konfigurationswerte nicht mehr.

## Namespaces und Schichtengrenzen

Die früher vertauschten B56-Dateiinhalte, konkurrierenden Serviceimplementierungen und falschen Assembly-Namespaces wurden konsolidiert. Interfaces liegen in Application, technische Implementierungen in Persistence.

Verbleibende kleinere Inkonsistenzen:

- `B56DateiPruefer.cs` und `Sha256HashService.cs` liegen unter `Kompass.Persistence/Services`, verwenden aber den Namespace `Kompass.Persistence.B56Import`.
- Persistence verwendet parallel die Namespacegruppen `Kompass.Persistence`, `Kompass.Persistence.Services` und `Kompass.Persistence.B56Import`. Das ist buildtechnisch korrekt, aber noch nicht vollständig nach fachlichen Features geordnet.

Es wurden keine Projektabhängigkeiten gefunden, die den vorgesehenen Schichtengraphen umkehren.

## Compilerfehler und Warnungen

Aktueller Stand:

- Compilerfehler: **0**
- Compilerwarnungen Debug: **0**
- Compilerwarnungen Release: **0**

Die CI erzwingt derzeit noch keine allgemeine `TreatWarningsAsErrors`-Regel. Der warnungsfreie Zustand ist daher gemessen, aber nicht als Repository-Policy abgesichert.

## Tote und doppelte Typen

Die früher eindeutig toten oder doppelten B56-Implementierungen, Controller und Beispieltypen wurden entfernt. Eine statische Deklarationssuche findet keine verbleibenden konkurrierenden öffentlichen Implementierungen mit identischem Typnamen.

`Result` und `Result<T>` teilen absichtlich denselben Basistypnamen und sind keine doppelte Implementierung.

Aktuell wurden keine sicher toten Klassen oder Interfaces identifiziert. Diese Aussage ist wegen Reflection, XAML, EF-Konventionen und der sehr geringen Testabdeckung vorsichtig zu bewerten. Automatisierte Dead-Code- oder Architekturanalysen existieren nicht.

## Wichtigste verbleibende technische Schulden

1. **Sehr geringe Testabdeckung:** Es existieren nur zwei Tests für 112 C#-Dateien. API-Endpunkte, Desktop-Client, EF-Persistenz, Migrationen und wesentliche B56-Fehlerpfade sind nicht automatisiert abgedeckt.
2. **Keine reale EF-Integrationsprüfung:** Die CI erkennt Migrationen, wendet sie aber nicht auf eine temporäre SQLite-Datenbank an.
3. **Nicht vollständig reproduzierbare SDK-Auswahl:** CI verwendet .NET 8, lokal wurde .NET 9 verwendet; ein `global.json` fehlt.
4. **Dezentrale Paketversionen:** Es gibt kein `Directory.Packages.props`, wodurch gemeinsame Paketfamilien manuell synchron gehalten werden müssen.
5. **Keine erzwungene Warnungs- und Architekturpolicy:** `TreatWarningsAsErrors` und automatisierte Schichtentests fehlen.
6. **Restliche Namespace-/Ordnerabweichungen:** Teile der B56-Persistence sind noch nicht konsistent nach Namespace und Pfad gegliedert.

## Priorisierte Maßnahmen

### P1 – Verhalten durch Tests absichern

1. Integrationstest erstellen, der alle EF-Migrationen auf einer temporären SQLite-Datenbank anwendet.
2. Tests für B56-Import-Erfolg, Duplikaterkennung, ungültige Dateien und Archivfehler ergänzen.
3. API-Integrationstests für Projekt-CRUD und B56-Endpunkte ergänzen.
4. `ProjektApiClient` mit kontrollierten HTTP-Antworten testen.

### P2 – Build und Abhängigkeiten reproduzierbar machen

1. Unterstützte SDK-Version festlegen und über `global.json` pinnen.
2. Gemeinsame Paketversionen optional in `Directory.Packages.props` zentralisieren.
3. Warnungen schrittweise als Fehler behandeln.
4. Vulnerability-Scan als expliziten CI-Schritt ergänzen.

### P3 – Architekturpflege

1. Verbleibende Persistence-Dateipfade und Namespaces konsistent nach Feature oder technischer Schicht ordnen.
2. Architekturtests für zulässige Projekt- und Namespace-Abhängigkeiten ergänzen.
3. Dead-Code-Erkennung oder statische Analyse in CI etablieren.

## Baseline-Fazit

Die ursprünglichen P0-Buildblocker sind beseitigt. Die Solution ist in Debug und Release fehler- und warnungsfrei, alle vorhandenen Tests bestehen, die EF-Migrationen werden in CI erkannt und der Paketgraph enthält keine bekannten Sicherheitsfunde.

Der aktuelle Stand ist technisch stabiler, aber noch nicht ausreichend durch Tests abgesichert. Die höchste Priorität liegt deshalb nicht mehr auf Buildreparaturen, sondern auf EF-, B56-, API- und Desktop-Integrationstests sowie einer reproduzierbaren SDK- und Buildpolicy.
