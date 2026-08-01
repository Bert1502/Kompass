# B56-Gap-Analyse

**Stand:** 1. August 2026 (aktualisiert nach Paket-17-Implementierung: Verbrauchsvergleichsbericht – VerbrauchsvergleichBericht, VerbrauchsvergleichZeile, VerbrauchsvergleichErzeugenAsync, API GET .../berichte/verbrauchsvergleich; Verbrauchsdaten-Zusammenfassung – VerbrauchsZusammenfassungJeEnergietraeger, ZusammenfassenAsync, API GET .../verbrauchsdaten/zusammenfassung; 304 Tests)

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

Seit der letzten Gap-Analyse wurden die Pakete 6 bis 15 erfolgreich
implementiert:

- vollständiger erster Anwenderprozess: ergänzbare Projektdaten
  (`InterneBezeichnung`, `Bearbeitungsstatus`), PATCH-Endpunkt
  `api/projekte/{id}/projektdaten`, Migration
  `AddErgaenzbareProjektdaten`;
- durchgängiger HTTP-End-to-End-Test, der alle 18 Abnahmekriterien
  aus `FUNCTIONAL_SPECIFICATION.md` Abschnitt 21 prüft;
- Wirtschaftlichkeit (bilanziert und praktisch): Domain-Aggregat
  `Wirtschaftlichkeitsannahmen` mit `Berechnen`-Methode,
  `Kostenposition`-Domain, `IWirtschaftlichkeitsService`,
  `IKostenpositionService`, `EfWirtschaftlichkeitsService`,
  `EfKostenpositionService`, API-Endpunkte für Annahmen und
  Kostenpositionen, Migration `AddWirtschaftlichkeitsannahmen`;
- Förderprogramm-Katalog (erste Stufe): `Foerderprogramm`-Aggregat
  mit zeitabhängigen Regeltypen (`FoerderquoteRegel`,
  `HoechstbetragRegel`, `Kumulierbarkeitsregel`,
  `PflichtnachweisRegel`, `Gueltigkeitsregel`),
  `IFoerderprogrammService`, `EfFoerderprogrammService`, API-Endpunkte
  GET/POST/Regelergänzung, Migrationen `AddFoerderprogramme` und
  `RefineFoerderprogrammRegeln`;
- Förderprogramm-Zuordnung zu Alternativen (Paket 9):
  `FoerderungZuordnung`-Entity, `Foerderberechnungsergebnis`-Ergebnis,
  `IAlternativeFoerderungService`, `EfAlternativeFoerderungService`,
  API-Endpunkte GET (Liste), PUT (Zuordnen), DELETE (Entfernen) und
  POST/berechnen (fachliche Vorprüfung mit Stichtag),
  Migration `AddAlternativeFoerderungZuordnung`;
- Wärmebrückenmanagement (erste Stufe, Paket 10):
  `Waermebruecke`-Aggregat mit allen Pflichtfeldern aus Abschnitt 16
  (`InterneNummer`, `Bezeichnung`, `Lage`, `Planreferenz`,
  `Detailreferenz`, `Fremdnummer`, `Laenge`, `Typ`, `Status`,
  `GleichwertigkeitStatus`, `Beiblatt2Referenz`, `ThermCadReferenz`,
  `PsiWert`, `FRsi`, `Pruefanmerkung`, `Berichtsdarstellung`),
  `IWaermebrueckeService`, `EfWaermebrueckeService`, API-Endpunkte
  GET (Liste), GET (Einzelabruf), POST (Anlegen), PATCH (Aktualisieren),
  DELETE (Löschen), Migration `AddWaermebruecken`;
- Berichtswesen (erste Stufe, Paket 11):
  `Berichtstyp`-Enum, `Berichtskopf`-Record,
  `AlternativenvergleichBericht`, `WaermebrueckenuebersichtBericht`
  im Domain; `IBerichtsService` in Application; `BerichtsService`
  in Persistence; API-Endpunkte GET `alternativenvergleich` und
  GET `waermebrueckenuebersicht` unter
  `api/projekte/{id}/berichte/...`; keine eigene Datenbankmigration
  erforderlich (ausschließlich Aggregation vorhandener Domänendaten
  gemäß ADR-0007);
- Persistierte Snapshot-Vergleiche (Paket 12):
  `B56SnapshotVergleichEntity` mit `VergleichId`, `ProjektId`,
  `VorgaengerSnapshotId`, `NachfolgerSnapshotId`, `HatAenderungen`,
  `VergleichJson` und `ErstelltAm`; eindeutiger Index über Projekt +
  Vorgänger + Nachfolger; `IB56ImportRegister` um
  `VergleichAbrufenAsync` und `VergleichSpeichernAsync` erweitert;
  `EfB56ImportRegister` implementiert beide Methoden; der
  `B56SnapshotVergleichService` liest beim zweiten Aufruf den
  gespeicherten Vergleich aus der Datenbank; Migration
  `AddPersistedB56SnapshotVergleiche` abgeschlossen; abgeleitete
  Konflikte werden zusammen mit dem Vergleich persistiert.
- Berichtswesen zweite Stufe (Paket 13):
  `WirtschaftlichkeitsberichtZeile`- und
  `WirtschaftlichkeitsberichtBericht`-Domain-Records für den
  Wirtschaftlichkeitsbericht je Berechnungsbasis; `FoerderuebersichtAlternative`-
  und `FoerderuebersichtBericht`-Domain-Records für die konsolidierte
  Förderübersicht; `IBerichtsService` um
  `WirtschaftlichkeitsberichtErzeugenAsync(projektId, basis)` und
  `FoerderuebersichtErzeugenAsync(projektId)` erweitert; `BerichtsService`
  implementiert beide Methoden durch Aggregation vorhandener Domänendaten
  (ADR-0007); API-Endpunkte GET
  `api/projekte/{id}/berichte/wirtschaftlichkeit/{basis}` und GET
  `api/projekte/{id}/berichte/foerderuebersicht` hinzugefügt.
- Projektstammdaten (Paket 14):
  Projekt-Aggregat um Auftraggeber, Ansprechpartner, Strasse, Ort, Postleitzahl,
  Gebäudeart erweitert; `StammdatenAktualisieren`-Methode mit Validierung;
  Migration `AddProjektStammdaten`; API-Endpunkt
  `PATCH api/projekte/{id}/stammdaten` implementiert.
- Reale Verbrauchsdaten (Paket 15):
  `VerbrauchsDaten`-Aggregat mit Abrechnungsperiode, Energieträger, Menge (kWh),
  Kosten (EUR), optionalem Witterungsbereinigungsfaktor, Flächenbezug,
  B56-Vergleichswert, Anpassungsfaktor, Anpassungsbegründung und
  Abweichungsursache; berechnete Eigenschaften `WitterungsbereinigteMenge`
  und `MengeJeFlaeche`; `IVerbrauchsDatenService` in Application;
  `EfVerbrauchsDatenService` in Persistence; API-Endpunkte GET (Liste),
  GET (Einzelabruf), POST, PATCH, DELETE unter
  `api/projekte/{id}/verbrauchsdaten`; Migration `AddVerbrauchsDaten`;
  Domain, Service und Controller durch je eigene Tests abgesichert.
- Feldweise Konfliktlösung (Paket 16):
  `B56KonfliktEntscheidungsTyp`-Enum (`Offen`, `Uebernehmen`, `Behalten`);
  `B56KonfliktEintrag`-Klasse mit Bereich, Schlüssel, Feld, Aenderung,
  `AlterWert`, `NeuerWert`, Entscheidung und Auditfeldern; `IB56KonfliktService`
  in Application; `EfB56KonfliktService` in Persistence (lazy Erstellung aus
  persistiertem Vergleich, idempotent beim zweiten Aufruf, `AlterWert`/`NeuerWert`
  für Bestandskennwerte, Bauteile und Modernisierungsalternativen aufbereitet);
  API-Endpunkte GET (Liste mit lazy Erstellung), PATCH (Entscheidung setzen) und
  POST `alle-uebernehmen` unter
  `api/projekte/{id}/b56-importe/{importId}/konflikte`; Migration
  `AddB56KonfliktEintraege`; Domain, Service und Controller durch je eigene Tests
  abgesichert.

`dotnet test` bestätigt 291/291 Tests bestanden.

Offene Schwerpunkte für die nächste Ausbaustufe:

- Freigabestatus und Änderungshistorie für das Projektmodell;
- Förderprogramm-Verknüpfung mit Alternativenberechnung;
- weitere Berichtstypen (Energieberatungsbericht, Executive Summary, Prüferunterlagen).

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

### 3.12 Ergänzbare Projektdaten und vollständiger erster Anwenderprozess

- `InterneBezeichnung` (nullable, max. 200 Zeichen) und
  `Bearbeitungsstatus` (Enum) wurden in `Projekte` ergänzt.
- Migration `AddErgaenzbareProjektdaten` fügt beide Felder
  rückwärtskompatibel mit Standardwert hinzu.
- PATCH-Endpunkt `api/projekte/{id}/projektdaten` speichert
  Änderungen und gibt die aktualisierte `ProjektUebersicht` zurück.
- `VollstaendigerAnwenderprozessHttpEndToEndTests` prüft alle 18
  Abnahmekriterien aus `FUNCTIONAL_SPECIFICATION.md` Abschnitt 21 in
  einem einzigen durchgängigen HTTP-Test, inklusive:
  - ergänzbare Projektdaten ändern und speichern (Kriterium 11),
  - Projekt schließen und wieder öffnen mit persistierter
    Ergänzung (Kriterium 12),
  - manuelle Ergänzung nach zweitem Import unverändert (Kriterium 15).

Nachweise:

- `Kompass.Persistence/Migrations/20260727160844_AddErgaenzbareProjektdaten.cs`
- `Kompass.Api/Projects/ProjektdatenAktualisierenRequest.cs`
- `Kompass.Api/Projects/ProjekteController.cs`
- `Kompass.Tests/VollstaendigerAnwenderprozessHttpEndToEndTests.cs`

### 3.13 Wirtschaftlichkeitsannahmen und Kostenpositionen

- `Wirtschaftlichkeitsannahmen`-Aggregat enthält
  Betrachtungszeitraum, Diskontsatz, Inflationsrate,
  Wartungsmehrkosten, Nutzungsdauer und Förderbetrag.
- Energieträgerspezifische Annahmen (`EnergietraegerAnnahme`) sind
  dem Aggregat untergeordnet: Preis, Preissteigerungsrate, Einsparung,
  CO₂-Faktor und CO₂-Preispfad.
- `WirtschaftlichkeitsBasis`-Enum unterscheidet bilanzierte und
  praktische Wirtschaftlichkeit.
- `Berechnen`-Methode berechnet Amortisationsdauer (statisch und
  dynamisch), kumulierte Energiekosteneinsparung, Kapitalwert und
  Kosten-Nutzen-Verhältnis.
- `Kostenposition`-Domain modelliert Einzelkosten mit Kostenart,
  Bauteilbezug, Menge, Einheitspreis und förderfähigem Anteil.
- `IWirtschaftlichkeitsService` und `EfWirtschaftlichkeitsService`
  persistieren Annahmen und liefern berechnete Ergebnisse.
- `IKostenpositionService` und `EfKostenpositionService` verwalten
  Listen von Kostenpositionen je Alternative.
- API-Endpunkte: `api/projekte/{id}/alternativen/{id}/wirtschaftlichkeit/annahmen/{basis}`
  (GET, PUT) und `.../berechnen/{basis}` (GET) sowie
  `api/projekte/{id}/alternativen/{id}/kostenpositionen` (GET, POST,
  DELETE).
- Migration `AddWirtschaftlichkeitsannahmen` ergänzt die Tabellen.
- Domain, Service und Controller sind durch je eigene Tests abgesichert.

Nachweise:

- `Kompass.Domain/Economics/Wirtschaftlichkeitsannahmen.cs`
- `Kompass.Domain/Economics/Kostenposition.cs`
- `Kompass.Application/Economics/IWirtschaftlichkeitsService.cs`
- `Kompass.Application/Economics/IKostenpositionService.cs`
- `Kompass.Persistence/Services/EfWirtschaftlichkeitsService.cs`
- `Kompass.Persistence/Services/EfKostenpositionService.cs`
- `Kompass.Api/Economics/WirtschaftlichkeitsannahmenController.cs`
- `Kompass.Api/Economics/KostenpositionenController.cs`
- `Kompass.Persistence/Migrations/20260728033325_AddWirtschaftlichkeitsannahmen.cs`
- `Kompass.Tests/WirtschaftlichkeitsannahmenDomainTests.cs` (12 Tests)
- `Kompass.Tests/WirtschaftlichkeitsannahmenControllerTests.cs` (6 Tests)
- `Kompass.Tests/EfWirtschaftlichkeitsServiceTests.cs` (9 Tests)

### 3.14 Förderprogramm-Katalog (erste Stufe)

- `Foerderprogramm`-Aggregat verwaltet Programmkennung, Version,
  Gültigkeitszeitraum, Zielgruppe, Fördergegenstand, technische
  Mindestanforderungen, Fördersatz, Höchstbetrag, Kumulierbarkeit,
  Pflichtnachweise und Quellenstand.
- Zeitabhängige Detailregeln sind als separate Werteobjekte modelliert:
  `FoerderquoteRegel`, `HoechstbetragRegel`, `Kumulierbarkeitsregel`,
  `PflichtnachweisRegel`, `Gueltigkeitsregel`.
- Pauschalwerte werden automatisch als Standardregel angelegt, wenn
  keine Detailregeln übergeben werden.
- Domain-Invarianten werden vollständig durch den Konstruktor erzwungen
  (Pflichtfelder, Zeitraum, Fördersatz ≥ 0).
- `IFoerderprogrammService` und `EfFoerderprogrammService` erlauben
  Anlegen und Auflisten von Förderprogrammen.
- API-Endpunkte: `api/foerderprogramme` (GET, POST) sowie
  regelspezifische POST-Endpunkte.
- Migrationen `AddFoerderprogramme` und `RefineFoerderprogrammRegeln`
  schaffen das relationale Datenmodell.
- Domain, Service und Controller sind durch je eigene Tests abgesichert.

Nachweise:

- `Kompass.Domain/Funding/Foerderprogramm.cs`
- `Kompass.Application/Funding/IFoerderprogrammService.cs`
- `Kompass.Persistence/Services/EfFoerderprogrammService.cs`
- `Kompass.Api/Funding/FoerderprogrammeController.cs`
- `Kompass.Persistence/Migrations/20260728064802_AddFoerderprogramme.cs`
- `Kompass.Persistence/Migrations/20260728070720_RefineFoerderprogrammRegeln.cs`
- `Kompass.Tests/FoerderprogrammDomainTests.cs` (8 Tests)
- `Kompass.Tests/FoerderprogrammeControllerTests.cs` (6 Tests)
- `Kompass.Tests/EfFoerderprogrammServiceTests.cs` (4 Tests)

### 3.15 Berichtswesen zweite Stufe (Wirtschaftlichkeitsbericht und Förderübersicht)

- `WirtschaftlichkeitsberichtZeile`-Record enthält AlternativeId, B56-Position,
  Bezeichnung, Basis, Investitionskosten, Förderbetrag, Betrachtungszeitraum,
  Diskontsatz, Inflationsrate und das vollständige `Wirtschaftlichkeitsergebnis`.
- `WirtschaftlichkeitsberichtBericht`-Record fasst Kopf und Liste aller
  Alternativen mit Annahmen zusammen; Alternativen ohne hinterlegte Annahmen
  zur gewählten Basis werden ausgelassen.
- `FoerderuebersichtAlternative`-Record enthält AlternativeId, B56-Position,
  Bezeichnung, Gesamtkosten und die vollständige Liste zugeordneter
  `Foerderprogramm`-Aggregate.
- `FoerderuebersichtBericht`-Record fasst Kopf und alle Alternativen mit
  ihren Förderprogrammen zusammen.
- `IBerichtsService` um `WirtschaftlichkeitsberichtErzeugenAsync(projektId, basis)`
  und `FoerderuebersichtErzeugenAsync(projektId)` erweitert.
- `BerichtsService` implementiert beide Methoden durch Aggregation
  vorhandener Domänendaten ohne eigene Datenbanktabelle (ADR-0007).
- API-Endpunkte GET `api/projekte/{id}/berichte/wirtschaftlichkeit/{basis}`
  und GET `api/projekte/{id}/berichte/foerderuebersicht` hinzugefügt.
- Domain, Service und Controller sind durch je eigene Tests abgesichert.

Nachweise:

- `Kompass.Domain/Reports/WirtschaftlichkeitsberichtZeile.cs`
- `Kompass.Domain/Reports/WirtschaftlichkeitsberichtBericht.cs`
- `Kompass.Domain/Reports/FoerderuebersichtAlternative.cs`
- `Kompass.Domain/Reports/FoerderuebersichtBericht.cs`
- `Kompass.Application/Reports/IBerichtsService.cs`
- `Kompass.Persistence/Services/BerichtsService.cs`
- `Kompass.Api/Reports/BerichteController.cs`
- `Kompass.Tests/BerichtsDomainTests.cs` (11 Tests)
- `Kompass.Tests/BerichteControllerTests.cs` (8 Tests)
- `Kompass.Tests/BerichtsServiceTests.cs` (16 Tests)

### 3.16 Reale Verbrauchsdaten (Paket 15)

- `VerbrauchsDaten`-Aggregat modelliert eine Abrechnungsperiode je Energieträger
  mit Pflichtfeldern `ProjektId`, `PeriodeVon`, `PeriodeBis`, `Energietraeger`,
  `Menge` (kWh) und `Kosten` (EUR) sowie optionalen Feldern
  `WitterungsbereinigungsFaktor`, `Flaeche`, `B56VergleichsWert`,
  `AnpassungsFaktor`, `AnpassungsBegruendung` und `Abweichungsursache`.
- Berechnete Eigenschaften `WitterungsbereinigteMenge` (Menge × Faktor, falls
  gesetzt) und `MengeJeFlaeche` (kWh/m², falls Fläche > 0 gesetzt).
- `Aktualisieren`-Methode setzt alle Felder inkl. Validierung.
- `IVerbrauchsDatenService` und `EfVerbrauchsDatenService` implementieren
  vollständiges CRUD (Listen, Abrufen, Anlegen, Aktualisieren, Löschen).
- API-Endpunkte: GET, GET/{id}, POST, PATCH/{id}, DELETE/{id} unter
  `api/projekte/{id}/verbrauchsdaten`.
- Migration `AddVerbrauchsDaten` erzeugt Tabelle `VerbrauchsDaten`.
- Domain, Service und Controller sind durch je eigene Tests abgesichert.

Nachweise:

- `Kompass.Domain/Verbrauch/VerbrauchsDaten.cs`
- `Kompass.Application/Verbrauch/IVerbrauchsDatenService.cs`
- `Kompass.Persistence/Services/EfVerbrauchsDatenService.cs`
- `Kompass.Api/Verbrauch/VerbrauchsDatenController.cs`
- `Kompass.Persistence/Migrations/20260729152143_AddVerbrauchsDaten.cs`
- `Kompass.Tests/VerbrauchsDatenDomainTests.cs` (9 Tests)
- `Kompass.Tests/EfVerbrauchsDatenServiceTests.cs` (7 Tests)
- `Kompass.Tests/VerbrauchsDatenControllerTests.cs` (8 Tests)

### 3.17 Feldweise Konfliktlösung (Paket 16)

- `B56KonfliktEntscheidungsTyp`-Enum mit Werten `Offen`, `Uebernehmen` und
  `Behalten` modelliert den Entscheidungsstatus eines Konflikts.
- `B56KonfliktEintrag`-Modell hält Bereich, Schluessel, Feld, Aenderung,
  Entscheidung und Auditfelder (`EntschiedenAm`, `ErstelltAm`).
- `IB56KonfliktService` mit `ListenOderErzeugenAsync` (lazy Erstellung
  aus persistiertem Vergleich, idempotent) und `EntscheidungSetzenAsync`.
- `EfB56KonfliktService` liest beim ersten Aufruf den gespeicherten
  `B56SnapshotVergleich`-JSON und leitet daraus `B56KonfliktEintrag`-Zeilen
  ab; beim zweiten Aufruf werden die bereits gespeicherten Einträge
  zurückgegeben ohne Duplikate.
- Migration `AddB56KonfliktEintraege` erzeugt Tabelle `B56KonfliktEintraege`
  mit Indizes auf `(ProjektId, NachfolgerImportId)` und
  `(ProjektId, VorgaengerImportId, NachfolgerImportId)`.
- API-Endpunkte: GET `konflikte?vorgaenger={id}` (Auflisten mit lazy
  Erstellung) und PATCH `konflikte/{id}` (Entscheidung setzen) unter
  `api/projekte/{id}/b56-importe/{importId}/konflikte`.
- Service und Controller sind durch je eigene Tests abgesichert.

Nachweise:

- `Kompass.Application/B56Import/B56KonfliktEntscheidungsTyp.cs`
- `Kompass.Application/B56Import/B56KonfliktEintrag.cs`
- `Kompass.Application/B56Import/IB56KonfliktService.cs`
- `Kompass.Persistence/Data/Entities/B56KonfliktEintragEntity.cs`
- `Kompass.Persistence/Services/EfB56KonfliktService.cs`
- `Kompass.Api/B56Import/B56KonfliktController.cs`
- `Kompass.Persistence/Migrations/20260730070445_AddB56KonfliktEintraege.cs`
- `Kompass.Tests/EfB56KonfliktServiceTests.cs` (9 Tests)
- `Kompass.Tests/B56KonfliktControllerTests.cs` (6 Tests)


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

### 4.7 Persistierte Snapshot-Vergleichsergebnisse

Snapshot-Vergleiche werden seit Paket 12 dauerhaft in der Tabelle
`B56SnapshotVergleiche` gespeichert. Ein eindeutiger Index über
`ProjektId + VorgaengerSnapshotId + NachfolgerSnapshotId` verhindert
Duplikate. Beim zweiten Aufruf desselben Vergleichs gibt der Service
das persistierte Ergebnis zurück, ohne neu zu berechnen. Abgeleitete
Konflikte sind im JSON-Payload enthalten.

Nachweise:

- `Kompass.Persistence/Data/Entities/B56SnapshotVergleichEntity.cs`
- `Kompass.Application/B56Import/IB56ImportRegister.cs` (`VergleichAbrufenAsync`, `VergleichSpeichernAsync`)
- `Kompass.Persistence/Services/EfB56ImportRegister.cs`
- `Kompass.Application/B56Import/B56SnapshotVergleichService.cs`
- `Kompass.Persistence/Migrations/20260729043015_AddPersistedB56SnapshotVergleiche.cs`
- `Kompass.Tests/B56SnapshotVergleichServiceTests.cs` – Test `Vergleich_wird_persistiert_und_enthaelt_Konflikte`

Noch offen:

- expliziter Synchronisations-Use-Case (nach fachlicher Spezifikation);
- Schutzregel, die verhindert, dass manuelle Ergänzungen automatisch
  durch Snapshot-Werte überschrieben werden, auch nach Synchronisation.

Seit Paket 16 implementiert:

- `B56KonfliktEntscheidungsTyp`-Enum und `B56KonfliktEintrag`-Record bilden
  das feldweise Konfliktmodell ab.
- `EfB56KonfliktService` initialisiert Einträge automatisch aus dem
  gespeicherten `VergleichJson` (Bereiche: Bestandskennwert, Bauteil,
  Modernisierungsalternative); `AlterWert` und `NeuerWert` werden
  menschenlesbar aufbereitet.
- API-Endpunkte erlauben feldgenaue Bestätigung (`POST …/entscheiden`)
  und Massenbestätigung (`POST …/alle-akzeptieren`).
- Eindeutiger Index verhindert Doppeleinträge.
- Tabelle `B56KonfliktEintraege` mit Migration `AddB56KonfliktEintraege`.

Noch offen (nach fachlicher Klärung):

- tatsächliche Anwendung der Entscheidung auf das Projektmodell
  (Synchronisations-Use-Case);
- Schutzregel gegen unbeabsichtigtes Überschreiben manueller Ergänzungen.

## 5. Nicht erfüllt

### 5.1 Bearbeitbarer vollständiger Projektstand

Das Projektmodell enthält derzeit Name, interne Bezeichnung,
Bearbeitungsstatus, Modernisierungsalternativen, alternative Bauteile,
Kostenpositionen, Wirtschaftlichkeitsannahmen, Herkunftsreferenz, seit
Paket 14 Auftraggeber, Ansprechpartner, Strasse, Ort, Postleitzahl und
Gebäudeart, seit Paket 15 reale Verbrauchsdaten je Abrechnungsperiode
und Energieträger sowie seit Paket 16 feldweise Konfliktentscheidungen
für Re-Import-Konflikte.

Noch nicht modelliert sind unter anderem:

- Freigabestatus;
- projektbezogene Förderparameter (Verknüpfung mit
  Förderprogramm-Katalog);
- Energiepreise und Preissteigerungen auf Projektebene;
- CO₂-Preisannahmen auf Projektebene;
- Berichtseinstellungen;
- nachvollziehbare abweichende Annahmen gegenüber Normwerten.

### 5.2 Förderprogramm-Verknüpfung mit Alternativenberechnung

Der Förderprogramm-Katalog ist implementiert. Die projektbezogene
Programmzuordnung je Alternative sowie die fachliche Förder-Vorprüfung
(`FoerderungBerechnen`) sind in Paket 9 umgesetzt. Noch nicht umgesetzt ist:

- Kumulierbarkeitsregel zwischen mehreren zugeordneten Programmen wird
  als Metadaten-Status ausgewiesen, aber rechnerisch nicht erzwungen
  (fachlich noch nicht freigegeben);
- Prüfung technischer Mindestanforderungen je Programm.

### 5.3 Berichtswesen

In Paket 11 (erste Stufe) und Paket 13 (zweite Stufe) implementiert:

- `Berichtstyp`-Enum mit allen Typen aus Abschnitt 17 der Fachspezifikation;
- `Berichtskopf`-Record (Projektstand, Datenquelle, Berichtstyp,
  Erstellungszeitpunkt);
- `AlternativenvergleichBericht`: fasst alle Modernisierungsalternativen
  mit Gesamtkosten, B56-Position und Snapshot-Präsenzstatus zusammen;
- `WaermebrueckenuebersichtBericht`: listet alle Wärmebrücken eines
  Projekts;
- `WirtschaftlichkeitsberichtBericht`: fasst je Basis alle Alternativen
  mit Annahmen und berechneten Ergebnissen (Amortisation, Kapitalwert,
  Kosten-Nutzen-Verhältnis) zusammen; Alternativen ohne Annahmen werden
  ausgelassen;
- `FoerderuebersichtBericht`: listet alle Alternativen mit ihren
  zugeordneten Förderprogrammen;
- `IBerichtsService` und `BerichtsService` ohne eigene Datenbanktabelle
  (Aggregation vorhandener Domänendaten, ADR-0007);
- API-Endpunkte GET `api/projekte/{id}/berichte/alternativenvergleich`,
  GET `api/projekte/{id}/berichte/waermebrueckenuebersicht`,
  GET `api/projekte/{id}/berichte/wirtschaftlichkeit/{basis}` und
  GET `api/projekte/{id}/berichte/foerderuebersicht`.

Noch nicht umgesetzt:

- Energieberatungsbericht, Executive Summary, Prüferunterlagen,
  Präsentationen, Kommunikationsunterlagen;
- persistiertes Berichtsarchiv (nach fachlicher Klärung ob notwendig).

### 5.4 Wärmebrückenmanagement

Das Fachobjekt `Waermebruecke` mit allen Pflichtfeldern aus
`FUNCTIONAL_SPECIFICATION.md` Abschnitt 16 ist in Paket 10
implementiert. API-Endpunkte für Anlegen, Abrufen, Aktualisieren und
Löschen sind vorhanden; Migration `AddWaermebruecken` ist abgeschlossen.

Noch nicht umgesetzt:
- Verknüpfung mit ThermCAD-Datenobjekten (externes System, fachliche
  Spezifikation offen);
- Prüferübersicht als aggregierte Ausgabe;
- Architekturdetail-Anfrageworkflow (Fall A);
- Gleichwertigkeitsnachweis-Workflow mit DIN 4108 Beiblatt 2 (Fall B).

## 6. Datenbankmigrationen – Überblick

### 6.1 Abgeschlossene Migrationen

| Migration | Inhalt | Status |
|-----------|--------|--------|
| `20260725075146_VersionB56Snapshots` | `SnapshotSchemaVersion`, `ParserVersion` | ✅ umgesetzt |
| `20260725084054_AddB56SnapshotLifecycle` | `SnapshotStatus`, `BestaetigtAm`, `VerworfenAm` | ✅ umgesetzt |
| `20260725085558_AddB56ProjectModelOrigin` | `QuellSnapshotId`, `ProjektmodellVersion` | ✅ umgesetzt |
| `20260725101500_TrackB56AlternativePresence` | `B56Position`, `IstImAktuellenB56SnapshotVorhanden` | ✅ umgesetzt |
| `20260727160844_AddErgaenzbareProjektdaten` | `InterneBezeichnung`, `Bearbeitungsstatus` | ✅ umgesetzt |
| `20260728033325_AddWirtschaftlichkeitsannahmen` | `Wirtschaftlichkeitsannahmen`, `EnergietraegerAnnahmen`, `Kostenpositionen` | ✅ umgesetzt |
| `20260728064802_AddFoerderprogramme` | `Foerderprogramme` und Regeltypen initial | ✅ umgesetzt |
| `20260728070720_RefineFoerderprogrammRegeln` | Spaltenverfeinerungen Förderregeln | ✅ umgesetzt |
| `20260728075125_AddAlternativeFoerderungZuordnung` | `FoerderungZuordnungen` | ✅ umgesetzt |
| `20260728103810_AddWaermebruecken` | `Waermebruecken` mit allen Fachfeldern | ✅ umgesetzt |
| `20260729043015_AddPersistedB56SnapshotVergleiche` | `B56SnapshotVergleiche` mit eindeutigem Index | ✅ umgesetzt |
| `20260729140029_AddProjektStammdaten` | `Auftraggeber`, `Ansprechpartner`, `Strasse`, `Ort`, `Postleitzahl`, `Gebaeudeart` in `Projekte` | ✅ umgesetzt |
| `20260729152143_AddVerbrauchsDaten` | `VerbrauchsDaten` mit allen Fachfeldern und Indizes | ✅ umgesetzt |
| `20260730070445_AddB56KonfliktEintraege` | `B56KonfliktEintraege` mit Entscheidungsstatus und Indizes | ✅ umgesetzt |
| `20260730092438_AddB56KonfliktAlterNeuerWert` | `AlterWert`, `NeuerWert` Spalten und eindeutiger Index auf `B56KonfliktEintraege` | ✅ umgesetzt |

### 6.2 Ausstehende Migrationen

**Migration: Vollständiger Projektstand**

Für Freigabestatus und Berichtseinstellungen werden weitere
Tabellen oder JSON-Spalten benötigt. Umfang und Struktur sind nach
fachlicher Freigabe zu definieren.
**Migration: Vollständiger Projektstand**

Für Freigabestatus und Berichtseinstellungen werden weitere
Tabellen oder JSON-Spalten benötigt. Umfang und Struktur sind nach
fachlicher Freigabe zu definieren.

**Hinweis:** Eine kaskadierende Löschung zwischen Projekt und Snapshot
darf nicht eingeführt werden. Snapshots müssen nach Projektlöschung
für die Nachweisbarkeit erhalten bleiben.

## 7. Notwendige Tests

### 7.1 Bereits implementierte Tests (Übersicht)

| Testdatei | Inhalt | Anzahl |
|-----------|--------|--------|
| `B56ArchivServiceTests.cs` | Archivservice | 2 |
| `B56DateiPrueferTests.cs` | Dateiprüfung | 6 |
| `B56ImportControllerTests.cs` | API-Controller | 8 |
| `B56ImportDependencyInjectionTests.cs` | DI-Komposition | 1 |
| `B56ImportEndToEndSmokeTests.cs` | Smoke-Test | 1 |
| `B56ImportHttpEndToEndTests.cs` | HTTP E2E (Import + Vergleich) | 3 |
| `B56ImportServiceIntegrationTests.cs` | Import-Pipeline | 3 |
| `B56ProjektmodellControllerTests.cs` | Übernahme-Controller | 1 |
| `B56ProjektmodellUebernahmeServiceTests.cs` | Übernahme-Service | 3 |
| `B56SnapshotLebenszyklusControllerTests.cs` | Lebenszyklus-Controller | 3 (Theory) |
| `B56SnapshotLebenszyklusServiceTests.cs` | Lebenszyklus-Service | 3 |
| `B56SnapshotVergleichServiceTests.cs` | Vergleich (alle Fälle + Persistierung) | 13 |
| `B56TabellenImportServiceTests.cs` | Tabellenimport | 2 |
| `EfB56ImportRegisterTests.cs` | EF-Register inkl. Versionen | 3 |
| `EfFoerderprogrammServiceTests.cs` | Förderprogramm-Persistence | 4 |
| `EfWirtschaftlichkeitsServiceTests.cs` | Wirtschaftlichkeit-Persistence | 9 |
| `FoerderprogrammDomainTests.cs` | Förderprogramm-Domain | 8 |
| `FoerderprogrammeControllerTests.cs` | Förderprogramme-API | 6 |
| `KompassDbContextMigrationTests.cs` | Migrationen | 2 |
| `OpenXmlB56ArbeitsmappenLeserTests.cs` | OpenXML-Leser | 2 |
| `ProjektB56ImportBeziehungTests.cs` | Projekt-Import-Beziehung | 2 |
| `ProjektDomainTests.cs` | Domain-Invarianten | 13 |
| `ProjektServiceTests.cs` | Projektservice | 8 |
| `ProjekteControllerTests.cs` | Projekte-API | 15 |
| `Sha256HashServiceTests.cs` | Hash-Service | 2 |
| `VollstaendigerAnwenderprozessHttpEndToEndTests.cs` | Alle 18 Abnahmekriterien E2E | 1 |
| `WirtschaftlichkeitsannahmenControllerTests.cs` | Wirtschaftlichkeit-API | 6 |
| `AlternativeFoerderprogrammeControllerTests.cs` | Förderprogramm-Zuordnung API | 7 |
| `EfAlternativeFoerderungServiceTests.cs` | Förderprogramm-Zuordnung Persistence | 9 |
| `WaermebrueckeDomainTests.cs` | Wärmebrücke-Domain-Invarianten | 7 |
| `EfWaermebrueckeServiceTests.cs` | Wärmebrücke-Persistence | 10 |
| `WaermebrueckenControllerTests.cs` | Wärmebrücken-API | 9 |
| `BerichtsDomainTests.cs` | Berichte-Domain-Modelle | 11 |
| `BerichteControllerTests.cs` | Berichte-API | 8 |
| `BerichtsServiceTests.cs` | Berichte-Persistence | 16 |
| `VerbrauchsDatenDomainTests.cs` | Verbrauchsdaten-Domain-Invarianten | 9 |
| `EfVerbrauchsDatenServiceTests.cs` | Verbrauchsdaten-Persistence | 7 |
| `VerbrauchsDatenControllerTests.cs` | Verbrauchsdaten-API | 8 |
| `B56KonfliktControllerTests.cs` | Konflikte-API (inkl. AlleUebernehmen) | 8 |
| `EfB56KonfliktServiceTests.cs` | Konflikte-Persistence (inkl. Auto-Init, AlterWert/NeuerWert) | 9 |
| **Gesamt** | | **291** |

### 7.2 Noch fehlende Tests

- beschädigter Payload bei gültiger Schema-Version erzeugt definierten
  Fehler;
- Snapshot nach Projektlöschung erreichbar halten (sobald Use-Case
  entschieden);
- Anwendung der Konfliktentscheidungen auf das Projektmodell
  (Synchronisations-Use-Case, sobald fachlich spezifiziert);
- Förderprogramm-Verknüpfung mit Alternativenberechnung;
- Kumulierbarkeit mehrerer Förderprogramme.

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

**Restrisiko:** Auftraggeber, Standort, reale Verbrauchsdaten und
Berichtseinstellungen sind noch nicht im Projektmodell modelliert. Die
Konflikterkennung für diese Felder bei Re-Import ist noch offen.

### R3 – Benutzeränderungen werden bei Re-Import überschrieben

**Priorität: mitigiert.**
Re-Import erzeugt nur einen neuen Snapshot. Die Synchronisation in das
Projektmodell bleibt einem separaten, fachlich noch zu spezifizierenden
Use-Case vorbehalten. `IstImAktuellenB56SnapshotVorhanden` schützt
vorhandene Alternativen. Der vollständige E2E-Test belegt, dass manuelle
Ergänzungen nach dem zweiten Import erhalten bleiben.

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
Technisches Aufrufergebnis (`B56ImportErgebnis`) und persistierter
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

### R10 – Wirtschaftlichkeit noch nicht mit realen Kostendaten verknüpft

**Priorität: mitigiert.**
`Wirtschaftlichkeitsannahmen` und `Kostenpositionen` sind getrennt
modelliert. Die Berechnung verwendet `alternative.Gesamtkosten`, das
automatisch aus den Kostenpositionen der Modernisierungsalternative
aggregiert wird. Die Verknüpfung realer Verbrauchsdaten mit der
Wirtschaftlichkeitsberechnung (praktische Basis) ist durch den
`ZusammenfassenAsync`-Endpunkt in Paket 17 unterstützt: Anwender
erhalten je Energieträger annualisierte Jahresmengen als Eingabehilfe
für `EndenergieIstZustand` in `EnergietraegerAnnahme`.

**Restrisiko:** Die automatische Übernahme von Jahresmengen in die
Annahmen ist noch nicht implementiert (fachliche Spezifikation offen).

### R11 – Förderprogramm-Katalog noch nicht mit Alternativen verknüpft

**Priorität: mitigiert.**
`FoerderungZuordnung` verbindet Förderprogramme mit Alternativen.
`EfAlternativeFoerderungService.FoerderungBerechnenAsync` berechnet die
Förderhöhe je Programm als fachliche Vorprüfung.

**Restrisiko:** Die Kumulierbarkeit mehrerer Programme wird als Statusinformation
ausgewiesen, aber rechnerisch nicht durchgesetzt (fachliche Entscheidung ausstehend).
Technische Mindestanforderungen je Programm sind noch nicht prüfbar.

## 9. Priorisierte nächste Arbeitspakete

### P1 – Vollständiger erster Anwenderprozess (Paket 6) ✅ abgeschlossen

Alle 18 Abnahmekriterien aus `FUNCTIONAL_SPECIFICATION.md` Abschnitt
21 sind durch `VollstaendigerAnwenderprozessHttpEndToEndTests` abgedeckt.

### P2 – Wirtschaftlichkeit (Paket 7) ✅ erste Stufe abgeschlossen

Domain, Persistence und API für `Wirtschaftlichkeitsannahmen` und
`Kostenpositionen` sind implementiert. Offene Anschlussaufgaben:

- Aggregation der Kostenpositionen als Investitionskosteneingabe;
- Verknüpfung mit Förderprogramm-Katalog für Förderbetragsübergabe.

### P3 – Förderung (Paket 8 + 9) ✅ erste und zweite Stufe abgeschlossen

Förderprogramm-Katalog mit zeitabhängigen Regeltypen ist implementiert.
Förderprogramm-Zuordnung zu Alternativen und fachliche Förder-Vorprüfung
sind in Paket 9 umgesetzt. Offene Anschlussaufgaben:

- rechnerische Kumulierbarkeitsregel zwischen mehreren Programmen
  (nach fachlicher Freigabe);
- Prüfung technischer Mindestanforderungen je Programm.

### P4 – Berichtswesen (Paket 11 + 13) ✅ zweite Stufe abgeschlossen

`Berichtstyp`, `Berichtskopf`, `AlternativenvergleichBericht`,
`WaermebrueckenuebersichtBericht`, `WirtschaftlichkeitsberichtBericht`,
`FoerderuebersichtBericht`, `IBerichtsService`, `BerichtsService`
und die API-Endpunkte `alternativenvergleich`,
`waermebrueckenuebersicht`, `wirtschaftlichkeit/{basis}` und
`foerderuebersicht` sind implementiert. Keine Datenbankmigration
erforderlich (ADR-0007). Offene Anschlussaufgaben:

- Energieberatungsbericht, Executive Summary, Prüferunterlagen.

### P5 – Wärmebrückenmanagement (Paket 10) ✅ erste Stufe abgeschlossen

Das `Waermebruecke`-Aggregat mit allen Fachfeldern aus Abschnitt 16,
`IWaermebrueckeService`, `EfWaermebrueckeService`, vollständige CRUD-API
und Migration `AddWaermebruecken` sind implementiert. Offene Anschlussaufgaben:

- ThermCAD-Objektverknüpfung (externes System, fachliche Spezifikation offen);
- Prüferübersicht als aggregierte Ausgabe;
- Architekturdetail-Anfrageworkflow (Fall A);
- Gleichwertigkeitsnachweis-Workflow mit DIN 4108 Beiblatt 2 (Fall B).

### P6 – Persistierte Vergleichs- und Konfliktergebnisse (Paket 12) ✅ abgeschlossen

Die Tabelle `B56SnapshotVergleiche` speichert berechnete Vergleiche
dauerhaft. Der Service reusiert persistierte Ergebnisse beim zweiten
Aufruf. Abgeleitete Konflikte sind im JSON-Payload enthalten.

Feldweise Benutzerbestätigung ist in Paket 16 umgesetzt (siehe P9).

### P7 – Projektstammdaten (Paket 14) ✅ abgeschlossen

Das `Projekt`-Aggregat wurde um Auftraggeber, Ansprechpartner, Strasse,
Ort, Postleitzahl und Gebäudeart erweitert. Die Felder werden durch
`StammdatenAktualisieren` validiert und bereinigt. Migration
`AddProjektStammdaten` und API-Endpunkt `PATCH api/projekte/{id}/stammdaten`
sind implementiert. Offene Anschlussaufgaben:

- Freigabestatus und Änderungshistorie;
- Berichtseinstellungen.

### P8 – Reale Verbrauchsdaten (Paket 15) ✅ abgeschlossen

`VerbrauchsDaten`-Aggregat mit vollständigem CRUD (Anlegen, Abrufen,
Auflisten, Aktualisieren, Löschen), allen Fachfeldern aus Abschnitt 18
(Abrechnungsperiode, Energieträger, Menge, Kosten, Witterungsbereinigung,
Flächenbezug, B56-Vergleich, Anpassungsfaktor, Abweichungsursache),
`IVerbrauchsDatenService`, `EfVerbrauchsDatenService`, Migration
`AddVerbrauchsDaten` und API-Endpunkte
`api/projekte/{id}/verbrauchsdaten` sind implementiert. Offene
Anschlussaufgaben:

- aggregierter Vergleich realer Verbrauchsdaten gegenüber B56-Bilanz
  im Berichtswesen – in Paket 17 umgesetzt (siehe P10).
- Verknüpfung realer Verbrauchsdaten mit Wirtschaftlichkeitsberechnung
  (praktische Basis) – Zusammenfassung je Energieträger in Paket 17
  umgesetzt (siehe P10).

### P9 – Feldweise Konfliktlösung (Paket 16) ✅ abgeschlossen

`B56KonfliktEntscheidungsTyp`-Enum (`Offen`, `Uebernehmen`, `Behalten`),
`B56KonfliktEintrag`-Modell, `IB56KonfliktService` in Application,
`EfB56KonfliktService` in Persistence (lazy Erstellung aus persistiertem
Vergleich, idempotent beim zweiten Aufruf), Migration
`AddB56KonfliktEintraege`, API-Endpunkte GET `konflikte?vorgaenger={id}`
(Auflisten mit lazy Erstellung) und PATCH `konflikte/{id}` (Entscheidung
setzen) unter `api/projekte/{id}/b56-importe/{importId}/konflikte`
sind implementiert. Offene Anschlussaufgaben:

- expliziter Synchronisations-Use-Case (nach fachlicher Spezifikation).

### P10 – Verbrauchsvergleichsbericht und Verbrauchsdaten-Zusammenfassung (Paket 17) ✅ abgeschlossen

`VerbrauchsvergleichZeile`-Record (Periode, Energieträger, Menge,
witterungsbereinigte Menge, B56-Vergleichswert, Abweichung, Abweichungs-
prozent) und `VerbrauchsvergleichBericht`-Record im Domain;
`Berichtstyp.Verbrauchsvergleich` hinzugefügt;
`IBerichtsService.VerbrauchsvergleichErzeugenAsync` und
`BerichtsService`-Implementierung (Aggregation realer Verbräuche mit
B56-Gegenüberstellung, ADR-0007); API-Endpunkt
`GET api/projekte/{id}/berichte/verbrauchsvergleich` hinzugefügt.

`VerbrauchsZusammenfassungJeEnergietraeger`-Record (Energieträger,
Anzahl Perioden, Gesamtmenge, witterungsbereinigte Gesamtmenge,
hochgerechnete Jahresmenge, Gesamtkosten) im Domain;
`IVerbrauchsDatenService.ZusammenfassenAsync` und
`EfVerbrauchsDatenService`-Implementierung (Gruppierung nach
Energieträger, annualisierte Jahresmenge als Eingabehilfe für die
Wirtschaftlichkeitsberechnung auf praktischer Basis);
API-Endpunkt `GET api/projekte/{id}/verbrauchsdaten/zusammenfassung`
hinzugefügt. Offene Anschlussaufgaben:

- automatische Übernahme von Jahresmengenwerten in
  `EnergietraegerAnnahme.EndenergieIstZustand` (nach fachlicher
  Spezifikation des Übernahme-Workflows).

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
