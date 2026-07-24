# Technische Baseline

Stand: 24. Juli 2026

Repository: `Bert1502/Kompass`

Ausgangsbranch: `main`

Analysiertes Commit: `9596d374dbbc2a4d95c7e0ebafde2e749ac77287`

Umgebung: Windows, .NET SDK `9.0.316`, MSBuild `17.14.43`

## Zusammenfassung

Der NuGet-Restore ist erfolgreich. Die Solution ist derzeit nicht buildfähig. `dotnet build` und `dotnet test` scheitern beim Kompilieren von `Kompass.Application`; dadurch werden keine Tests ausgeführt.

| Prüfung | Ergebnis | Fehler | Warnungen |
| --- | --- | ---: | ---: |
| `dotnet restore Kompass.sln` | Erfolgreich, 6 Projekte wiederhergestellt | 0 | 0 |
| `dotnet build Kompass.sln --no-restore --verbosity minimal` | Fehlgeschlagen in `Kompass.Application` | 9 | 0 |
| `dotnet test Kompass.sln --no-restore --verbosity minimal` | Fehlgeschlagen beim Build; keine Tests ausgeführt | 9 Compilerfehler | 0 gemeldet |

Die Warnungszahl ist nur für den bis zum Fehler erreichten Buildstand belastbar. Nach Behebung der Compilerfehler muss der vollständige Build erneut auf Warnungen geprüft werden.

## Solution und Projektstruktur

`Kompass.sln` enthält sechs .NET-8-Projekte:

| Projekt | Rolle | Direkte Projektabhängigkeiten |
| --- | --- | --- |
| `Kompass.Domain` | Domänenmodell und Basistypen | keine |
| `Kompass.Application` | Anwendungsabstraktionen, DTOs und B56-Anwendungslogik | `Kompass.Domain` |
| `Kompass.Persistence` | EF Core, SQLite, Migrationen, OpenXML und Infrastrukturservices | `Kompass.Application`, `Kompass.Domain` |
| `Kompass.Api` | ASP.NET-Core-Host und HTTP-Endpunkte | `Kompass.Application`, `Kompass.Persistence` |
| `Kompass.Desktop` | WPF-Client | `Kompass.Application` |
| `Kompass.Tests` | xUnit-Tests | `Kompass.Application`, `Kompass.Domain`, `Kompass.Persistence` |

Der grundsätzliche Abhängigkeitsgraph entspricht einer geschichteten Architektur: Domain ist unabhängig, Application hängt von Domain ab, und Infrastruktur beziehungsweise Hosts hängen nach innen. Die tatsächliche Ablage und die Namespaces mehrerer B56-Typen verletzen diesen Graphen jedoch.

Die Solution umfasst 126 C#-Quelldateien. Es existiert nur eine mit `[Fact]` markierte Testmethode. Ein `global.json` sowie zentrale Build- oder Paketdefinitionen (`Directory.Build.props`, `Directory.Packages.props`) fehlen; die tatsächlich verwendete SDK-Version hängt damit von der lokalen Umgebung ab.

## Compilerfehler

Alle neun Buildfehler entstehen im B56-Bereich von `Kompass.Application`:

- Achtmal `CS0246`: `B56ImportErgebnis` oder `IB56DateiPruefer` kann nicht aufgelöst werden.
- Einmal `CS0535`: `B56ImportService` implementiert die von `IB56ImportService` geforderte Signatur nicht.

Die Fehler sind Symptome vertauschter oder inhaltlich falsch zugeordneter Dateien:

- `B56ImportErgebnis.cs` deklariert keinen Ergebnistyp, sondern einen `B56ImportService` im Namespace `Kompass.Persistence.Services`.
- `IB56DateiPruefer.cs` deklariert kein Interface, sondern eine konkrete Klasse `B56DateiPruefer`.
- `IB56ImportService.cs` deklariert kein Interface, sondern eine zweite konkrete Klasse `B56ImportService`.
- `B56ImportService.cs` deklariert dagegen das Interface `IB56ImportService`.
- Die beiden vorhandenen `B56ImportService`-Implementierungen verwenden unterschiedliche Eingaben und unterschiedliche Importabläufe.

Damit fehlen die erwarteten öffentlichen Verträge nicht nur dem Compiler; die fachliche Zuständigkeit des B56-Imports ist ebenfalls uneindeutig.

## NuGet-Pakete

Direkte Paketgruppen:

- Application: `Microsoft.Extensions.Options 10.0.10`
- Persistence: `DocumentFormat.OpenXml 3.5.1`, EF Core Design/SQLite/Tools `8.0.29`, Options Configuration Extensions `10.0.10`
- API: EF Core Design `8.0.29`, Configuration Binder und Options Configuration Extensions `10.0.10`, Swashbuckle `6.6.2`
- Desktop: `Microsoft.Extensions.DependencyInjection 10.0.10`
- Tests: coverlet `6.0.0`, Microsoft.NET.Test.Sdk `17.8.0`, xUnit und Runner `2.5.3`

Die Paketgenerationen sind uneinheitlich: Zielplattform und EF Core liegen auf Version 8, während mehrere `Microsoft.Extensions.*`-Pakete aus Version 10 eingebunden sind. Das erzeugt im transitiven Graphen gleichzeitig 8.x- und 10.x-Versionen zentraler Extensions-Bibliotheken und erhöht das Risiko von Laufzeit- und Wartungsproblemen.

`dotnet list Kompass.sln package --vulnerable --include-transitive` meldet folgende High-Severity-Funde:

- `SQLitePCLRaw.lib.e_sqlite3 2.1.6` in Persistence, API und Tests (`GHSA-2m69-gcr7-jv3q`)
- `System.Net.Http 4.3.0` transitiv in Tests (`GHSA-7jgj-8wvc-jh57`)
- `System.Text.RegularExpressions 4.3.0` transitiv in Tests (`GHSA-cmhx-cq75-c4mj`)

Die alten System-Pakete im Testgraphen stammen aus der betagten Test-Toolchain. Paketupdates sind erst nach Wiederherstellung eines grünen Builds sinnvoll verifizierbar.

## Entity Framework Core

EF Core `8.0.29` nutzt SQLite. `KompassDbContext` stellt `Projekte` und `B56ImportEintraege` bereit und lädt Konfigurationen per `ApplyConfigurationsFromAssembly`. Acht `IEntityTypeConfiguration<T>`-Klassen und drei Migrationen einschließlich Model Snapshot sind vorhanden. Eine Design-Time-Factory existiert.

Positive Punkte:

- Persistenzkonfigurationen sind aus dem DbContext ausgelagert.
- Migrationen und Design-Time-Erzeugung sind vorhanden.
- Der DbContext wird scoped über `AddDbContext` registriert.

Risiken:

- Die Solution ist nicht buildfähig; Modell, Migrationen und Design-Time-Factory können daher nicht gegen den aktuellen Stand validiert werden.
- EF-Design/Tools sind sowohl in Persistence als auch teilweise im API-Host eingebunden. Die Ownership der Migrationen sollte eindeutig bei Persistence liegen.
- B56-Domänentypen existieren parallel in `Kompass.Domain.B56.Import`, `Kompass.Application.B56Import` und `Kompass.Application.B56Import.Domain`. Dadurch ist unklar, welche Typen persistiert und welche nur Transportmodelle sind.
- Der Ordner `Kompass.Persistence/Enities` ist falsch geschrieben, obwohl sein Namespace `Kompass.Persistence.Data.Entities` lautet.

## Dependency Injection

API:

- `AddPersistence` registriert `KompassDbContext` und `IProjektService`.
- `AddB56Import` registriert einen Teil der B56-Pipeline.

Desktop:

- Verwendet einen eigenen ServiceProvider mit fest codierter API-Adresse.
- Registriert HTTP-Client, Dialog-/Navigationsdienste, ViewModels und Windows.

Festgestellte DI-Probleme:

- `AddB56Import` registriert kein `IB56DateiPruefer`, obwohl `B56ImportService` davon abhängt.
- `IB56BauteilzuordnungsRepository` besitzt eine konkrete JSON-Implementierung, wird aber nicht registriert.
- Mehrere B56-Implementierungen liegen in abweichenden Namespaces (`Kompass.Persistence`, `Kompass.Persistence.Services`, `Kompass.Persistence.B56Import`), wodurch Registrierung und Auflösung unnötig fragil sind.
- `B56ImportOptionen` werden als manuell erzeugter Singleton mit fest codierten Werten registriert, statt konsistent über das Options-Pattern und Konfiguration gebunden zu werden.
- Das Archivverzeichnis `D:\KOMPASS\B56-Archiv` und die Desktop-API-Adresse `https://localhost:7275/` sind fest codiert und umgebungsabhängig.
- Für `HttpClient` wird im Desktop ein manuell erzeugter Singleton statt `IHttpClientFactory` verwendet.
- Die Service-Lifetimes wurden nicht durch Integrationstests validiert.

## Namespaces und Schichtengrenzen

Offensichtliche Abweichungen zwischen Pfad, Assembly und Namespace:

- Dateien unter `Kompass.Application/B56Import` deklarieren teilweise `Kompass.Persistence.Services` oder `Kompass.Persistence.B56Import`.
- Konkrete dateisystem-, JSON- und Importimplementierungen befinden sich im Application-Projekt, obwohl ihre Namespaces und Aufgaben Persistence zugeordnet sind.
- `Kompass.Api` enthält zwei `ProjekteController` mit identischer Route. Einer wird über `<Compile Remove="Controllers\ProjekteController.cs" />` ausgeblendet, statt bereinigt zu sein.
- `Kompass.Api` enthält weiterhin das WeatherForecast-Beispiel.
- `Kompass.Desktop.csproj` enthält Ausschlüsse und einen leeren Ordner `NewFolder`, die wie Entwicklungsartefakte wirken.

Diese Ausnahmen machen die Architektur vom Verhalten einzelner Projektdateien abhängig und erschweren Navigation, Refactoring und statische Analyse.

## Tote und doppelte Typen

Eine rein statische Referenzsuche kann Reflection-, XAML- und EF-Konventionen nicht vollständig bewerten. Folgende Befunde sind dennoch belastbar oder klar prüfbedürftig.

Klare Duplikate beziehungsweise konkurrierende Implementierungen:

- Zwei `B56ImportService`-Klassen mit unterschiedlichen Abläufen und Signaturen
- Zwei `B56DateiPruefer`-Klassen
- Zwei `B56BauteilcodeParser`-Klassen
- Zwei `ProjekteController` mit derselben Route; einer ist vom Build ausgeschlossen
- Zwei B56-Bauteilmodelle und zwei B56-Modernisierungsalternativen in parallelen Application-Namespaces
- `JsonB56ImportRegister` und `EfB56ImportRegister` als alternative Registerimplementierungen ohne dokumentierte Auswahlstrategie

Wahrscheinlich tot oder derzeit nicht in den produktiven Pfad eingebunden:

- der vom API-Projekt explizit ausgeschlossene `Controllers/ProjekteController.cs`
- WeatherForecast-Modell und -Controller
- `JsonB56ImportRegister`
- `JsonB56BauteilzuordnungsRepository`
- Teile des parallelen B56-Modells unter `Kompass.Application.B56Import.Domain`
- `B56ImportStatus`, sofern keine geplante externe Serialisierung oder spätere Pipeline-Nutzung besteht

EF-Konfigurationen, Migrationen, WPF-`App` und Design-Time-Factory dürfen trotz geringer direkter Referenzzahl nicht als tot eingestuft werden, da sie durch Framework-Konventionen, Reflection oder XAML verwendet werden.

## Wichtigste technische Schulden

1. **Buildblocker im B56-Modul:** Vertauschte Dateiinhalte, fehlende Verträge und konkurrierende Service-Signaturen verhindern Build und Tests.
2. **Unklare B56-Architektur:** Domain-, Application- und Persistence-Verantwortung sind vermischt; Modelle und Implementierungen existieren mehrfach.
3. **Unvollständige DI-Komposition:** Mindestens eine zwingende Abhängigkeit fehlt, weitere Implementierungen sind nicht eindeutig registriert.
4. **Bekannte Paketrisiken:** Drei High-Severity-Advisories im transitiven Paketgraphen.
5. **Extrem geringe Testabdeckung:** Eine einzige Testmethode für 126 C#-Dateien; zentrale Domain-, Service-, EF- und API-Pfade sind ungetestet.
6. **Ausgeschlossener und generierter Beispielcode:** Doppelte Controller, WeatherForecast und `NewFolder` verschleiern den tatsächlich unterstützten Produktumfang.
7. **Umgebungswerte im Code:** Archivpfad und API-Basisadresse sind fest codiert.
8. **Nicht reproduzierbare Toolchain:** Kein `global.json` und keine zentrale Paketverwaltung.

## Priorisierte Maßnahmen

### P0 – Build und Testausführung wiederherstellen

1. Gewünschten B56-Vertrag festlegen: eine `IB56ImportService`-Signatur, ein `B56ImportErgebnis` und eine konkrete Implementierung.
2. Vertauschte Dateiinhalte korrigieren und Typnamen, Dateinamen, Assembly und Namespace konsistent ausrichten.
3. Doppelte `B56ImportService`- und `B56DateiPruefer`-Implementierungen entfernen oder bewusst als getrennte Verantwortlichkeiten benennen.
4. `dotnet restore`, `dotnet build` und `dotnet test` erneut ausführen; erst dann die vollständige Warnungsbaseline erheben.

### P1 – Laufzeit- und Sicherheitsrisiken reduzieren

1. DI-Registrierungen mit einem Composition-Root-Test validieren und fehlende B56-Abhängigkeiten ergänzen.
2. High-Severity-Paketfunde durch kontrollierte Updates der SQLite- und Testabhängigkeiten beheben.
3. `Microsoft.Extensions.*`-Versionen an die Zielplattformstrategie angleichen und den gemischten 8.x/10.x-Graphen auflösen.
4. Archivpfad, API-Adresse und B56-Optionen aus Konfiguration binden und beim Start validieren.

### P2 – Architektur konsolidieren

1. Kanonisches B56-Domänenmodell bestimmen und parallele Modelle entfernen oder explizit als DTOs mappen.
2. Interfaces in Application, fachliche Entitäten in Domain und technische Implementierungen in Persistence halten.
3. API-Controller konsolidieren, WeatherForecast entfernen und Compile-Ausschlüsse bereinigen.
4. EF-Migrationsverantwortung ausschließlich in Persistence verankern.

### P3 – Qualität und Wartbarkeit

1. Tests für Domainregeln, B56-Import, DI-Auflösung, EF-Konfiguration, API-Endpunkte und Fehlerfälle ergänzen.
2. `global.json`, zentrale Buildregeln und optional zentrale Paketverwaltung einführen.
3. Compilerwarnungen als Fehler oder über eine schrittweise No-Warnings-Policy etablieren.
4. Automatisierte Architekturtests für zulässige Projekt- und Namespace-Abhängigkeiten ergänzen.

## Baseline-Fazit

Der aktuelle Stand ist wiederherstellbar, aber nicht releasefähig: Restore funktioniert, Build und Tests sind blockiert, die B56-Komponente besitzt widersprüchliche Verträge und Schichtenzuordnungen, und der Paketgraph enthält bekannte High-Severity-Risiken. Die erste Maßnahme muss die eindeutige Konsolidierung des B56-Vertrags sein; danach können Warnungen, Tests, DI und Paketupdates verlässlich bewertet werden.
