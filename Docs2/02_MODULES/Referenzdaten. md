# REFERENZDATENMODUL

## 1. Zweck

Das Referenzdatenmodul stellt zentrale, versionierte Fach- und Stammdaten für andere KOMPASS-Module bereit.

Es versorgt insbesondere:

- Wirtschaftlichkeitsberechnung
- Fördermittelbewertung
- Berichtswesen
- Plausibilitätsprüfungen
- projektspezifische Annahmen

Das Modul führt keine energetischen Berechnungen durch.

---

## 2. Grundprinzipien

### 2.1 Zentrale Datenhaltung

Referenzdaten dürfen nicht mehrfach in unterschiedlichen Modulen gepflegt werden.

Förderung, Wirtschaftlichkeit und Berichte greifen auf dieselben freigegebenen Referenzdatensätze zu.

### 2.2 Versionierung

Jeder Referenzdatensatz besitzt:

- eine stabile Identität,
- eine fachliche Version,
- einen Gültigkeitszeitraum,
- eine Quelle,
- einen Bearbeitungsstatus,
- einen Freigabestatus.

Bestehende Versionen dürfen nicht überschrieben werden.

### 2.3 Reproduzierbarkeit

Jede Berechnung und jeder Bericht muss den verwendeten Referenzdatenstand dauerhaft referenzieren.

Spätere Änderungen an Referenzdaten dürfen bestehende Berechnungsergebnisse nicht rückwirkend verändern.

### 2.4 Keine stillschweigenden Aktualisierungen

Neue Referenzdaten dürfen nicht automatisch auf bestehende Projekte angewendet werden.

Der Anwender entscheidet, ob ein Projekt auf einen neuen Referenzdatenstand umgestellt wird.

### 2.5 Quellenpflicht

Jeder freigegebene Referenzdatensatz benötigt eine nachvollziehbare Quelle.

Eine Quelle kann sein:

- Gesetz
- Verordnung
- Richtlinie
- Norm
- offizielles Merkblatt
- offizielle Förderrichtlinie
- statistische Veröffentlichung
- Energieabrechnung
- Herstellerangabe
- interne, fachlich freigegebene Vorgabe

---

# 3. Abgrenzung

## 3.1 Bestandteil des Moduls

Das Referenzdatenmodul verwaltet:

- Förderprogramme
- Förderregeln
- technische Förderanforderungen
- Energiepreise
- Energiepreissteigerungen
- CO₂-Emissionsfaktoren
- CO₂-Preispfade
- Nutzungsdauern
- Wartungssätze
- Instandhaltungssätze
- Betriebskostensätze
- Preisindizes
- Kapital- und Diskontsätze
- Inflationsannahmen
- normative oder interne Grenzwerte
- Gültigkeits- und Versionsinformationen
- Quellen und Fundstellen

## 3.2 Nicht Bestandteil des Moduls

Das Referenzdatenmodul:

- berechnet keine Energiebilanzen,
- berechnet keine Wärmebrücken,
- berechnet keine Wirtschaftlichkeit,
- entscheidet nicht abschließend über Förderfähigkeit,
- verändert keine B56-Ergebnisse,
- ersetzt keine fachliche Prüfung,
- lädt keine Daten ohne Freigabe direkt in bestehende Projekte.

---

# 4. Referenzdatenarten

## 4.1 Förderprogramme

Ein Förderprogramm muss mindestens enthalten:

- Förderprogramm-ID
- Programmbezeichnung
- Fördergeber
- Programmtyp
- Gültig-ab-Datum
- Gültig-bis-Datum
- Versionsbezeichnung
- Status
- Quelle
- Fundstelle
- Veröffentlichungsdatum
- interne fachliche Hinweise

Beispiele für Programmtypen:

- Zuschuss
- Kredit
- Tilgungszuschuss
- Steuerförderung
- regionale Förderung
- kommunale Förderung
- EU-Förderung

---

## 4.2 Förderregeln

Eine Förderregel muss mindestens enthalten:

- Förderregel-ID
- Förderprogramm-ID
- Regeltyp
- Bezeichnung
- Beschreibung
- Gültigkeitszeitraum
- Priorität
- Prüfstatus
- Quelle
- Versionsbezug

Mögliche Regeltypen:

- Förderquote
- Mindestinvestition
- Höchstbetrag
- förderfähige Kosten
- technische Mindestanforderung
- Kumulierbarkeit
- Ausschlussregel
- Pflichtnachweis
- Antragsvoraussetzung
- Frist
- Gebäudetyp
- Antragstellergruppe
- Maßnahmenart

---

## 4.3 Energiepreise

Ein Energiepreisdatensatz muss mindestens enthalten:

- Energiepreis-ID
- Energieträger
- Preiswert
- Einheit
- Preisbestandteil
- Bezugszeitraum
- Gültig-ab-Datum
- Gültig-bis-Datum
- Region
- Quelle
- Preisstand
- Status

Mögliche Energieträger:

- Strom
- Erdgas
- Heizöl
- Fernwärme
- Biomasse
- Flüssiggas
- Umweltwärme
- sonstige Energieträger

Mögliche Einheiten:

- EUR/kWh
- EUR/MWh
- EUR/l
- EUR/kg
- EUR/a

Preisbestandteile müssen unterscheidbar sein:

- Arbeitspreis
- Grundpreis
- Leistungspreis
- Messpreis
- sonstiger Preisbestandteil

---

## 4.4 Energiepreissteigerungen

Ein Datensatz muss mindestens enthalten:

- Steigerungs-ID
- Energieträger
- jährliche Steigerungsrate
- Gültigkeitszeitraum
- Szenario
- Quelle
- Status

Szenarien können sein:

- konservativ
- Standard
- hoch
- projektspezifisch

---

## 4.5 CO₂-Emissionsfaktoren

Ein Datensatz muss mindestens enthalten:

- Emissionsfaktor-ID
- Energieträger
- Emissionsfaktor
- Einheit
- Bilanzierungsart
- Gültigkeitszeitraum
- Quelle
- Status

Mögliche Einheiten:

- kg CO₂e/kWh
- t CO₂e/MWh

Bilanzierungsarten müssen unterscheidbar sein:

- direkte Emissionen
- vorgelagerte Emissionen
- Gesamtfaktor
- Strommix
- vertraglicher Faktor

---

## 4.6 CO₂-Preispfade

Ein CO₂-Preispfad muss mindestens enthalten:

- CO₂-Preispfad-ID
- Bezeichnung
- Jahr
- Preis
- Einheit
- Gültigkeitsstand
- Quelle
- Status

Mögliche Einheit:

- EUR/t CO₂

Ein Preispfad besteht aus mehreren jahresbezogenen Datensätzen.

---

## 4.7 Nutzungsdauern

Ein Nutzungsdauerdatensatz muss mindestens enthalten:

- Nutzungsdauer-ID
- Bauteil- oder Anlagenkategorie
- Beschreibung
- Mindestwert
- Standardwert
- Höchstwert
- Einheit
- Gültigkeitszeitraum
- Quelle
- Status

Mögliche Kategorien:

- Fenster
- Außentür
- Wärmedämmverbundsystem
- Dachabdichtung
- Dämmstoff
- Heizkessel
- Wärmepumpe
- Lüftungsanlage
- Photovoltaikanlage
- Beleuchtung
- Gebäudeautomation

---

## 4.8 Wartungs- und Instandhaltungssätze

Ein Datensatz muss mindestens enthalten:

- Satz-ID
- Bauteil- oder Anlagenkategorie
- Satzart
- Prozentsatz
- Bezugsgröße
- Intervall
- Gültigkeitszeitraum
- Quelle
- Status

Satzarten:

- Wartung
- Instandhaltung
- Inspektion
- Betrieb
- Ersatzinvestition

Bezugsgrößen:

- Investitionskosten
- Wiederbeschaffungswert
- jährliche Kosten
- absolute Kosten

---

## 4.9 Kapital- und Diskontsätze

Ein Datensatz muss mindestens enthalten:

- Zinssatz-ID
- Bezeichnung
- Zinssatz
- Satzart
- Gültigkeitszeitraum
- Szenario
- Quelle
- Status

Satzarten:

- Kapitalzins
- Diskontsatz
- Kalkulationszins
- Realzins
- Nominalzins

---

## 4.10 Preisindizes

Ein Preisindexdatensatz muss mindestens enthalten:

- Preisindex-ID
- Indexart
- Basisjahr
- Bezugsjahr
- Indexwert
- Region
- Quelle
- Status

Indexarten:

- Baupreisindex
- Verbraucherpreisindex
- Energiepreisindex
- anlagenspezifischer Index

---

# 5. Domänenmodell

## 5.1 Aggregate

Das Modul soll mindestens folgende Aggregate oder fachliche Einheiten enthalten:

```text
ReferenzdatenKatalog
├── ReferenzdatenVersion
├── ReferenzdatenQuelle
├── Förderprogramm
│   └── Förderregel
├── Energiepreis
├── Energiepreissteigerung
├── Emissionsfaktor
├── CO2Preispfad
├── Nutzungsdauer
├── WartungsUndInstandhaltungssatz
├── Zinssatz
└── Preisindex

5.2 Gemeinsame Eigenschaften
Alle versionierten Referenzdatensätze benötigen mindestens:

Id
Version
GueltigAb
GueltigBis
Status
QuelleId
ErstelltAm
ErstelltVon
GeaendertAm
GeaendertVon
FreigegebenAm
FreigegebenVon

5.3 Statusmodell
Mindestens folgende Statuswerte sind vorzusehen:
Entwurf
InPruefung
Freigegeben
Gesperrt
Abgelaufen
Archiviert
5.4 Statusregeln
Nur freigegebene Datensätze dürfen regulär für Berechnungen verwendet werden.
Gesperrte Datensätze dürfen nicht neu ausgewählt werden.
Abgelaufene Datensätze bleiben für historische Berechnungen lesbar.
Archivierte Datensätze dürfen nicht gelöscht werden, wenn sie referenziert sind.
Ein freigegebener Datensatz darf nicht direkt bearbeitet werden.
Änderungen an freigegebenen Datensätzen erzeugen eine neue Version.
6. Versionierungsregeln
6.1 Neue Version
Eine neue Version ist anzulegen, wenn sich mindestens eines der folgenden Merkmale ändert:
Wert
Einheit
Gültigkeitszeitraum
Quelle
Förderquote
Höchstbetrag
technische Mindestanforderung
Kumulierbarkeitsregel
Berechnungsvoraussetzung
Anwendungsbereich
6.2 Historische Referenzen
Bestehende Projekte müssen weiterhin auf den ursprünglich verwendeten Datensatz verweisen.
Eine Referenz darf nicht nachträglich auf eine neuere Version umgebogen werden.
6.3 Gültigkeitsprüfung
Bei Auswahl eines Referenzdatensatzes ist mindestens zu prüfen:
Projektdatum
Berechnungsdatum
Antragsdatum
Gültig-ab-Datum
Gültig-bis-Datum
Status
Region
Gebäudetyp
Anwendungsbereich
7. Quellenverwaltung
7.1 Referenzdatenquelle
Eine Quelle muss mindestens enthalten:
Quellen-ID
Titel
Herausgeber
Dokumenttyp
Veröffentlichungsdatum
Versionsstand
URL oder Dateireferenz
Abrufdatum
lokaler Archivpfad
Dateihash
Bemerkung
7.2 Archivierung
Offizielle Quelldokumente sollen lokal archiviert werden.
Zu speichern sind:
Originaldatei
Dateihash
Abrufdatum
Veröffentlichungsdatum
Quelle
Versionsstand
7.3 Nachvollziehbarkeit
Von jedem Referenzwert muss zur zugrunde liegenden Quelle navigiert werden können.
8. Freigabeprozess
8.1 Erfassung
Referenzdaten können erfasst werden durch:
manuelle Eingabe,
strukturierte Importdatei,
Datenbankimport,
KI-gestützten Änderungsvorschlag.
8.2 KI-gestützte Erfassung
KI darf:
Quellen analysieren,
Änderungen erkennen,
strukturierte Vorschläge erzeugen,
mögliche Regeländerungen markieren.
KI darf nicht:
Datensätze selbst freigeben,
bestehende freigegebene Daten überschreiben,
Quellen ohne Kennzeichnung ergänzen,
Förderfähigkeit verbindlich bestätigen.
8.3 Prüfung
Vor Freigabe sind mindestens zu prüfen:
fachlicher Inhalt
Zahlenwert
Einheit
Gültigkeitszeitraum
Quelle
Anwendungsbereich
Überschneidung mit bestehenden Versionen
Auswirkungen auf andere Regeln
8.4 Freigabe
Die Freigabe erfolgt durch einen fachlich verantwortlichen Benutzer.
Die Freigabe muss protokolliert werden.
9. Projektspezifische Werte
9.1 Referenzwert und Projektwert
Ein Projekt kann einen Referenzwert übernehmen oder durch einen projektspezifischen Wert ersetzen.
Beispiel:
Referenzwert Energiepreis Strom:
0,35 EUR/kWh

Projektwert:
0,29 EUR/kWh

Quelle Projektwert:
Energieabrechnung 2025
9.2 Überschreibungsregeln
Eine projektspezifische Überschreibung muss enthalten:
Referenzdatensatz-ID
abweichender Projektwert
Begründung
Quelle
Benutzer
Zeitpunkt
Der ursprüngliche Referenzwert bleibt sichtbar.
9.3 Keine globale Rückwirkung
Ein projektspezifischer Wert darf den zentralen Referenzdatensatz nicht verändern.
10. Schnittstellen zu anderen Modulen
10.1 Wirtschaftlichkeit
Das Wirtschaftlichkeitsmodul benötigt:
Energiepreise
Energiepreissteigerungen
CO₂-Preispfade
Nutzungsdauern
Wartungssätze
Instandhaltungssätze
Kapital- und Diskontsätze
Preisindizes
Das Wirtschaftlichkeitsmodul speichert bei jeder Berechnung:
verwendete Referenzdaten-IDs
verwendete Versionen
projektspezifische Überschreibungen
Berechnungsdatum
10.2 Förderung
Das Fördermodul benötigt:
Förderprogramme
Förderregeln
technische Mindestanforderungen
Höchstbeträge
Förderquoten
Kumulierbarkeitsregeln
Pflichtnachweise
Gültigkeitszeiträume
10.3 Berichtswesen
Berichte müssen ausweisen können:
verwendeter Referenzdatenstand
Quellen
Gültigkeitszeiträume
projektspezifische Abweichungen
Freigabestatus
10.4 Projektverwaltung
Ein Projekt kann einen bevorzugten Referenzdatenstand speichern.
Der bevorzugte Stand ist keine automatische Aktualisierungsfreigabe.

11. Anwendungsfälle
11.1 Referenzdatensatz anlegen
Benutzer öffnet Referenzdatenverwaltung
→ wählt Referenzdatenart
→ erfasst Wert und Metadaten
→ ordnet Quelle zu
→ speichert als Entwurf
11.2 Referenzdatensatz freigeben
Entwurf auswählen
→ fachlich prüfen
→ Validierung ausführen
→ Freigabe erteilen
→ Freigabedaten protokollieren
11.3 Neue Version erstellen
freigegebenen Datensatz auswählen
→ neue Version erzeugen
→ Änderungen erfassen
→ neuen Gültigkeitszeitraum festlegen
→ prüfen
→ freigeben
11.4 Projektwert überschreiben
Referenzwert auswählen
→ projektspezifischen Wert eingeben
→ Begründung und Quelle erfassen
→ für Projektberechnung speichern
11.5 Referenzdatenstand aktualisieren
neuen Referenzdatenstand erkennen
→ Änderungen anzeigen
→ betroffene Projekte anzeigen
→ keine automatische Übernahme
→ Anwender entscheidet je Projekt
11.6 Quelle archivieren
Quelldokument auswählen
→ Hash berechnen
→ Datei archivieren
→ Quelle erfassen
→ Referenzdatensätze mit Quelle verknüpfen
12. Validierungsregeln
12.1 Allgemein
GueltigAb ist erforderlich.
GueltigBis darf nicht vor GueltigAb liegen.
Einheit ist bei numerischen Werten erforderlich.
Quelle ist vor Freigabe erforderlich.
Freigegebene Datensätze sind unveränderlich.
Versionen derselben fachlichen Regel dürfen sich nicht unkontrolliert überschneiden.
Numerische Werte müssen in einem fachlich zulässigen Wertebereich liegen.
12.2 Energiepreise
Preis darf nicht negativ sein.
Energieträger ist erforderlich.
Einheit ist erforderlich.
Bezugszeitraum ist erforderlich.
12.3 Prozentsätze
Prozentsätze werden intern eindeutig gespeichert.
Speicherung als Dezimalwert oder Prozentwert muss systemweit einheitlich sein.
Benutzeroberfläche muss die Einheit eindeutig anzeigen.
12.4 Förderregeln
Förderquote darf nicht negativ sein.
Höchstbeträge benötigen eine Währung und Bezugsgröße.
Kumulierbarkeitsregeln dürfen nicht ohne Programmbezug gespeichert werden.
technische Anforderungen benötigen eine Quelle.
12.5 CO₂-Faktoren
Emissionsfaktor darf nicht negativ sein.
Bilanzierungsart und Einheit sind erforderlich.
Energieträger ist erforderlich.
13. Persistenzanforderungen
13.1 Tabellen
Mindestens folgende Tabellen oder äquivalente Entity-Strukturen sind vorzusehen:
reference_data_sources
reference_data_versions
funding_programs
funding_rules
energy_prices
energy_price_escalations
emission_factors
co2_price_paths
service_lives
maintenance_rates
interest_rates
price_indices
project_reference_overrides
Die tatsächliche Benennung muss den bestehenden KOMPASS-Namensregeln folgen.
13.2 Löschregeln
Referenzierte Datensätze dürfen nicht physisch gelöscht werden.
Löschen erfolgt fachlich als Sperren oder Archivieren.
Quellen dürfen nicht gelöscht werden, wenn Referenzdaten darauf verweisen.
Historische Berechnungsreferenzen müssen erhalten bleiben.
13.3 Indizes
Mindestens zu indizieren:
Referenzdatenart
fachliche Identität
Versionsnummer
Gültig-ab-Datum
Gültig-bis-Datum
Status
Quelle
Energieträger
Förderprogramm

14. Application Layer
14.1 Erforderliche Anwendungsfälle
Mindestens vorzusehen:
Referenzdatensatz anlegen
Referenzdatensatz ändern
Neue Version erzeugen
Referenzdatensatz prüfen
Referenzdatensatz freigeben
Referenzdatensatz sperren
Referenzdatensatz archivieren
Referenzdaten suchen
Gültigen Referenzwert ermitteln
Referenzdatenstand vergleichen
Quelle archivieren
Projektwert überschreiben
Projektüberschreibung entfernen
14.2 Services
Mögliche Application-Interfaces:
IReferenzdatenService
IReferenzdatenVersionsService
IReferenzdatenFreigabeService
IReferenzdatenQuelleService
IReferenzdatenAuswahlService
IProjektReferenzdatenService
Die konkrete Benennung ist an den bestehenden Code anzupassen.
Es dürfen keine parallelen, gleichnamigen konkreten Services in mehreren Schichten erzeugt werden.
15. API-Anforderungen
Mindestens benötigte Endpunkte:
GET    /api/referenzdaten
GET    /api/referenzdaten/{id}
POST   /api/referenzdaten
PUT    /api/referenzdaten/{id}
POST   /api/referenzdaten/{id}/versionen
POST   /api/referenzdaten/{id}/freigeben
POST   /api/referenzdaten/{id}/sperren
POST   /api/referenzdaten/{id}/archivieren

GET    /api/referenzdaten/gueltig
GET    /api/referenzdaten/versionen
GET    /api/referenzdaten/quellen

POST   /api/referenzdaten/quellen
POST   /api/projekte/{projektId}/referenzdaten-ueberschreibungen
DELETE /api/projekte/{projektId}/referenzdaten-ueberschreibungen/{id}
Die Endpunktstruktur ist an die vorhandene API-Konvention anzupassen.
16. Desktop-Anforderungen
Die WPF-Oberfläche benötigt mindestens:
16.1 Referenzdatenübersicht
Filter:
Referenzdatenart
Status
Gültigkeitszeitraum
Quelle
Version
Energieträger
Förderprogramm
16.2 Detailansicht
Anzeige:
Wert
Einheit
Version
Status
Gültigkeit
Quelle
Freigabe
Historie
verwendende Projekte
16.3 Bearbeitung
Funktionen:
Entwurf anlegen
Entwurf bearbeiten
neue Version erzeugen
Quelle zuordnen
Validierung anzeigen
Freigabe auslösen
sperren
archivieren
16.4 Projektansicht
Anzeige:
verwendeter Referenzdatenstand
projektspezifische Überschreibungen
neuere verfügbare Versionen
Auswirkungen einer möglichen Aktualisierung
Keine automatische Aktualisierung.
17. Audit und Historie
Folgende Aktionen müssen protokolliert werden:
Anlage
Änderung
neue Version
Prüfung
Freigabe
Sperrung
Archivierung
Quellenänderung
Projektüberschreibung
Wechsel des Referenzdatenstands
Auditdaten:
Aktion
Objekt-ID
Objekttyp
Vorheriger Wert
Neuer Wert
Benutzer
Zeitpunkt
Begründung
18. Import und Export
18.1 Strukturierter Import
Das Modul soll später strukturierte Referenzdatenimporte unterstützen.
Geeignete Formate:
JSON
CSV
XLSX
Ein Import darf nur Entwürfe erzeugen.
Importierte Datensätze müssen vor Freigabe geprüft werden.
18.2 Export
Referenzdaten sollen exportierbar sein für:
Sicherung
Prüfung
Verteilung
Übernahme in andere KOMPASS-Installationen
Export muss enthalten:
Datensatz
Version
Quelle
Gültigkeit
Freigabestatus
Prüfsumme

19. Sicherheit und Berechtigungen
Mindestens folgende Rollen sind fachlich vorzusehen:
Leser
Bearbeiter
Pruefer
Freigeber
Administrator
Berechtigungen
Leser
freigegebene Referenzdaten anzeigen
Bearbeiter
Entwürfe anlegen und bearbeiten
Prüfer
Datensätze prüfen
Prüfbefunde erfassen
Freigeber
Datensätze freigeben
Datensätze sperren
Administrator
technische Verwaltung
Import/Export
Rollenverwaltung
Ein Bearbeiter soll seinen eigenen Datensatz nicht ohne gesonderte Berechtigung freigeben.
20. Tests
20.1 Domain-Tests
Mindestens:
Statusübergänge
Versionserzeugung
Unveränderlichkeit freigegebener Datensätze
Gültigkeitszeiträume
Quellenpflicht
Sperrung
Archivierung
Überschneidungsprüfung
20.2 Application-Tests
Mindestens:
gültigen Referenzwert ermitteln
keine automatische Projektaktualisierung
neue Version erzeugen
Freigabeprozess
Projektüberschreibung
Quellenzuordnung
historische Version lesen
20.3 Persistence-Tests
Mindestens:
vollständiger Roundtrip
Versionen bleiben erhalten
referenzierte Datensätze können nicht gelöscht werden
Gültigkeitsabfragen
Indizes
Migrationen
Auditprotokoll
20.4 API-Tests
Mindestens:
Anlegen
Bearbeiten
Versionieren
Freigeben
Sperren
Archivieren
gültigen Wert abfragen
ungültige Eingaben
fehlende Quelle
Berechtigungsfehler
Konflikte

20.5 Desktop-Tests

Mindestens:
Filter
Detailanzeige
Statusdarstellung
Validierungsfehler
Projektüberschreibung
Anzeige neuerer Versionen
21. Akzeptanzkriterien
Das Modul gilt als fachlich umgesetzt, wenn:
Referenzdaten zentral gespeichert werden können.
Jeder Datensatz versioniert ist.
jeder freigegebene Datensatz eine Quelle besitzt.
freigegebene Datensätze nicht mehr bearbeitet werden können.
Änderungen neue Versionen erzeugen.
historische Versionen lesbar bleiben.
Projekte dauerhaft auf den verwendeten Stand verweisen.
Wirtschaftlichkeitsberechnungen ihre Referenzdaten vollständig dokumentieren.
Förderbewertungen den verwendeten Regelwerksstand ausweisen.
projektspezifische Werte den zentralen Datenbestand nicht verändern.
keine automatische Aktualisierung bestehender Projekte erfolgt.
Auditinformationen vollständig gespeichert werden.
physisches Löschen referenzierter Datensätze verhindert wird.
Quellen bis zum Originaldokument nachvollziehbar sind.
Build und automatisierte Tests erfolgreich sind.
22. Implementierungsreihenfolge
Copilot soll das Modul in folgender Reihenfolge umsetzen:
1. Bestehenden Repository-Code und Namenskonventionen analysieren
2. Gemeinsames Referenzdaten-Basismodell definieren
3. Quellenmodell implementieren
4. Versionierung und Statusmodell implementieren
5. Persistenz und Migrationen erstellen
6. generische Abfragen und Gültigkeitslogik implementieren
7. Energiepreise und Wirtschaftlichkeitsreferenzdaten umsetzen
8. Förderprogramme und Förderregeln umsetzen
9. Projektüberschreibungen umsetzen
10. Application-Services ergänzen
11. API-Endpunkte ergänzen
12. WPF-Ansichten und ViewModels ergänzen
13. Audit und Historie ergänzen
14. Import und Export erst danach ergänzen
15. vollständige Tests ergänzen
23. Verbindliche Anweisung an Copilot
Copilot darf:
bestehende Architektur und Namenskonventionen verwenden,
fehlende technische Adapter ergänzen,
Migrationen und Tests erstellen,
gemeinsame Basistypen verwenden, sofern sie bereits im Repository etabliert sind.
Copilot darf nicht:
B56-Berechnungen implementieren,
Wirtschaftlichkeitsformeln in das Referenzdatenmodul verschieben,
Förderfähigkeit ohne Fördermodul bewerten,
freigegebene Datensätze überschreiben,
automatische Internetaktualisierungen ohne fachliche Freigabe einführen,
nicht belegte Standardwerte in die Datenbank schreiben,
bestehende Projekte automatisch auf neue Referenzdatenstände umstellen,
neue externe Abhängigkeiten ohne technische Notwendigkeit einführen,
bestehende Klassen großflächig umbenennen.
Vor der Implementierung muss Copilot:
den aktuellen Solution-Aufbau analysieren;
bestehende Referenzdaten-, Förder- und Wirtschaftlichkeitsklassen suchen;
vorhandene Interfaces und Namenskonventionen wiederverwenden;
einen kurzen Umsetzungsplan erstellen;
danach schrittweise implementieren;
nach jedem Schritt Build und Tests ausführen.
