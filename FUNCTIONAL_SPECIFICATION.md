# KOMPASS – Fachliche Gesamtspezifikation

**Version:** 1.0  
**Stand:** 25. Juli 2026  
**Status:** Konsolidierter Arbeitsstand aus den bisherigen Projektentscheidungen  
**Verbindlichkeit:** Fachliche Referenz für Architektur, Codex-Aufträge, Implementierung und Review

## 1. Zweck

Dieses Dokument konsolidiert die bisher im Projekt getroffenen fachlichen und technischen Entscheidungen für KOMPASS. Nicht beschriebene oder ausdrücklich offene Punkte dürfen nicht durch Codex eigenmächtig fachlich entschieden werden.

## 2. Produktziel

KOMPASS ist ein Beratungs- und Prozesswerkzeug für Energieberatungsprojekte. Es ergänzt Fachsoftware, ersetzt sie jedoch nicht.

Schwerpunkte:

- Übernahme und Archivierung von B56-Ergebnissen,
- Verwaltung von Projekten und Berechnungsständen,
- strukturierte Bearbeitung von Modernisierungsalternativen,
- Wirtschaftlichkeits- und Fördermittelbewertung,
- Abgleich bilanzierter und realer Verbräuche,
- Berichtserstellung und Entscheidungsunterstützung,
- Wissens- und Referenzdatenmanagement,
- Prozesssteuerung und Nachvollziehbarkeit.

## 3. Verbindliche Systemgrenzen

### 3.1 B56

B56 bleibt der einzige energetische Rechenkern. KOMPASS importiert, archiviert, strukturiert und bewertet Ergebnisse, führt aber keine eigene DIN-V-18599-Bilanzierung durch.

### 3.2 ThermCAD

ThermCAD bleibt das Fachprogramm für Wärmebrückenberechnung und Wärmebrückennachweis. KOMPASS verwaltet, dokumentiert und ordnet Ergebnisse zu, berechnet jedoch keine Psi-Werte.

### 3.3 IFC und gbXML

KOMPASS führt keine IFC- oder gbXML-Auswertung durch. Externe Modellreferenzen können später gespeichert werden; Geometrieauswertung gehört nicht zu Version 1.0.

## 4. Fachliche Begriffe

### Variante

Eine Variante ist in B56 ein eigener Plan- oder Berechnungsstand.

### Modernisierungsalternative

Eine Modernisierungsalternative ist eine in B56 innerhalb einer Variante definierte Maßnahme oder Alternative. B56 unterstützt bis zu neun Modernisierungsalternativen je Variante. Der Begriff ist verbindlich.

### Maßnahmenpaket

Ein Maßnahmenpaket kann aus mehreren Einzelmaßnahmen bestehen und in B56 als Modernisierungsalternative innerhalb einer gesonderten Variante abgebildet werden.

### Nutzungsprofil

Der Begriff „Nutzungsprofil“ ist zu verwenden. „Zone“ wird nur für einen ausdrücklich technischen B56-Datenbegriff verwendet.

### Bauteilcode

B56-Bauteilcodes bleiben als fachliche Referenzen erhalten und müssen in Kosten-, Maßnahmen- und Berichtsdaten referenziert werden können.

## 5. Fachliche Prozessreihenfolge

1. Bauteilberechnung,
2. Bilanzierung,
3. Wärmeschutznachweis,
4. sommerlicher Wärmeschutznachweis,
5. Wärmebrückennachweis,
6. Wirtschaftlichkeit und Förderung,
7. Berichte und Entscheidungsunterlagen.

## 6. Erster produktiver End-to-End-Anwendungsfall

1. Anwender legt ein KOMPASS-Projekt an.
2. Anwender importiert eine B56-Excel-Datei.
3. KOMPASS prüft Datei, Struktur und Mindestinhalte.
4. KOMPASS bildet einen unveränderlichen Import-Snapshot.
5. KOMPASS archiviert die Originaldatei mit Prüfsumme.
6. KOMPASS zeigt Projektdaten, Berechnungsstände und Modernisierungsalternativen.
7. Anwender prüft Warnungen und bestätigt den Import.
8. KOMPASS erzeugt oder aktualisiert daraus ein bearbeitbares Projektmodell.
9. Anwender ergänzt Projektdaten, Kosten, Förderinformationen und Annahmen.
10. Anwender speichert das Projekt.
11. Projekt kann geschlossen und vollständig wieder geöffnet werden.
12. Bei erneutem B56-Import wird ein neuer Snapshot angelegt und ein Vergleich angeboten.
13. Änderungen werden nicht ungeprüft in das Projektmodell übernommen.
14. Aus bestätigten Projektdaten entstehen Wirtschaftlichkeitsauswertungen und Berichte.

## 7. Datenhoheit

### Import-Snapshot

Der Import-Snapshot ist die unveränderliche Abbildung eines konkreten B56-Exports. Er ist versioniert, projektbezogen und enthält Importzeitpunkt, Dateihash, archivierte Quelldatei, Parser-/Schema-Version sowie Warnungen und Validierungsergebnisse.

### Projektmodell

Das Projektmodell ist die bearbeitbare Arbeitsgrundlage. Es enthält bestätigte Importdaten sowie manuell ergänzte Projektdaten, Kosten, Förderparameter, reale Verbrauchsdaten, Annahmen, Berichtseinstellungen und Bearbeitungsstatus.

### Verhältnis

Der Snapshot dokumentiert die Quelle. Das Projektmodell dokumentiert den bearbeiteten Arbeitsstand. Ein erneuter Import überschreibt das Projektmodell nicht automatisch.

## 8. B56-Import

### Unterstützte Dateien

- `.xlsx`
- `.xlsm`

Die Quelldatei wird bei jedem Import archiviert.

### Prüfungen

- Dateityp und Lesbarkeit,
- maximale Dateigröße,
- bekannte Arbeitsblätter,
- Pflichtfelder und Datentypen,
- Plausibilität,
- Projektzuordnung,
- Duplikat anhand Dateihash,
- Parser- und Snapshot-Version.

### Nach dem Import sichtbare Daten

- Projektname und Dateiname,
- Importzeitpunkt,
- B56-Variante beziehungsweise Berechnungsstand,
- erkannte und nicht erkannte Tabellen,
- Importstatus, Warnungen und Fehler,
- wesentliche Gebäudegrunddaten,
- Flächen- und Bezugsgrößen,
- Nutzungsprofile beziehungsweise Zonenübersicht, soweit freigegeben,
- Endenergie nach Energieträgern,
- Primärenergie und CO₂, soweit vorhanden,
- B56-Bauteilcodes, Bezeichnungen, Flächen und U-Werte,
- bis zu neun Modernisierungsalternativen mit Nummer, Bezeichnung, Kurztext, Bauteilen und energetischen Ergebnissen.

### Nicht direkt änderbar

- energetische Bilanzwerte,
- U-Werte aus B56,
- importierte Flächen,
- B56-Bauteilcodes,
- B56-interne Zuordnungen,
- importierte Ergebnisse der Modernisierungsalternativen,
- Quelldatei und Prüfsumme.

### Im Projektmodell bearbeitbar oder ergänzbar

- interne Projektbezeichnung,
- Auftraggeber und Ansprechpartner,
- Notizen und Bearbeitungsstatus,
- Kostendaten, Umfeldkosten und Fachplanerkosten,
- Förderparameter,
- Energiepreise und Preissteigerungsraten,
- CO₂-Preisannahmen,
- Wartungs- und Instandhaltungskosten,
- Nutzungsdauer, Diskontsatz und Restwert,
- reale Verbrauchsdaten,
- Berichtsauswahl, Empfehlung und Kommentierung.

Abweichende Annahmen werden separat gespeichert und ersetzen Originalwerte nicht unsichtbar.

## 9. Erneuter Import und Versionierung

Jeder fachlich neue B56-Import erzeugt einen neuen Snapshot. Ein neuer Snapshot ersetzt weder ältere Snapshots noch das Projektmodell oder manuelle Ergänzungen.

Der Vergleich umfasst mindestens:

- Projekt- und Gebäudedaten,
- Varianten,
- Modernisierungsalternativen,
- Bauteile, Flächen und U-Werte,
- energetische Kennwerte und Energieträger,
- hinzugefügte und entfernte Datensätze.

Ein identischer Dateihash innerhalb desselben Projekts wird als Duplikat erkannt.

## 10. Modernisierungsalternativen

Bis zu neun Modernisierungsalternativen je B56-Variante.

Anzeige:

- B56-Nummer beziehungsweise Position,
- B56-Bezeichnung,
- B56-Kurztext,
- optionale interne ergänzende Bezeichnung.

Vergleich mindestens nach:

- Investitionskosten,
- förderfähigen Kosten und Förderung,
- Eigenanteil,
- Endenergie je Energieträger,
- Primärenergie und CO₂,
- Energie- und Energiekosteneinsparung,
- Amortisationsdauer,
- Kapitalwert, sofern aktiviert,
- Kosten-Nutzen-Verhältnis,
- praktischer Wirtschaftlichkeit,
- qualitativen Kriterien und Umsetzbarkeit.

Für mehr als neun Maßnahmen können mehrere B56-Varianten verwendet werden, insbesondere „Einzelmaßnahmen“ und „Pakete“.

## 11. Validierung

### Blockierende Fehler

- unlesbare oder beschädigte Datei,
- nicht unterstützter Dateityp,
- fehlende eindeutige Projektzuordnung ohne manuelle Zuordnung,
- fehlende zwingende Tabellen,
- strukturell nicht interpretierbare Pflichtbereiche,
- ungültige numerische Pflichtwerte,
- widersprüchliche Snapshot- oder Schema-Version,
- schwerwiegende interne Inkonsistenz,
- fehlgeschlagene Archivierung oder Hashbildung.

### Warnungen

- optionale Tabelle fehlt,
- optionale Kennwerte fehlen,
- unbekannte Zusatzspalten,
- leere Kurztexte,
- nicht zugeordnete optionale Bauteile,
- ungewöhnliche, aber mögliche Werte.

Mögliche Importzustände:

- hochgeladen,
- technisch geprüft,
- mit Warnungen,
- blockiert,
- fachlich bestätigt,
- in Projektmodell übernommen,
- verworfen.

## 12. Projektverwaltung

Ein Projekt enthält mindestens:

- eindeutige ID,
- Projektname,
- Auftraggeber und Ansprechpartner,
- Standortdaten und Gebäudetyp,
- Bearbeitungsstatus,
- B56-Snapshots,
- Projektvarianten und Modernisierungsalternativen,
- Kosten, Förderungen und Verbrauchsdaten,
- Wirtschaftlichkeitsannahmen,
- Berichte,
- Änderungs- und Freigabeinformationen.

## 13. Kostenmodell

Mindestens:

- Kostengruppe,
- Einzelkosten,
- Bauteilbezug und B56-Bauteilcode,
- Menge, Einheit, Einheitspreis und Gesamtpreis,
- Umfeldkosten,
- Fachplanerkosten,
- Wartung und Instandhaltung,
- förderfähiger Anteil,
- Preisstand, Quelle und Kommentar.

Kosten können aus einer separaten Eingabetabelle von Architektur oder TGA übernommen werden.

## 14. Wirtschaftlichkeit

KOMPASS unterscheidet:

1. bilanzierte Wirtschaftlichkeit,
2. praktische Wirtschaftlichkeit.

Die bilanzierte Wirtschaftlichkeit basiert auf B56-Ergebnissen unter Normklima und normativen Nutzungsprofilen.

Die praktische Wirtschaftlichkeit basiert auf realen Energieabrechnungen und einem nachvollziehbaren Abgleich. Starke Abweichungen, beispielsweise reale Energiekosten von nur 19 % der bilanzierten Kosten, werden transparent dargestellt.

Kennwerte:

- Amortisationsdauer,
- Kosten-Nutzen-Verhältnis,
- kumulierte Energiekosteneinsparung,
- optional Kapitalwert und Restwert,
- Eigenanteil nach Förderung,
- Betrachtungszeitraum,
- Nutzungsdauer,
- Wartungs- und Instandhaltungskosten.

Energieträger werden getrennt behandelt. Je Energieträger können Preis, Preissteigerung, CO₂-Faktor und CO₂-Kostenpfad hinterlegt werden.

Alle Annahmen müssen im Bericht offengelegt werden.

## 15. Förderung

Vorgesehene Bereiche:

- BEG EM,
- KfW,
- EFRE,
- KNN,
- weitere Programme nach Freigabe.

Je Programm werden Gültigkeitszeitraum, Zielgruppe, Fördergegenstand, technische Mindestanforderungen, Fördersatz, Höchstbetrag, förderfähige Kosten, Kumulierbarkeit, Nachweise, Fristen und Quellenstand verwaltet.

Förderregeln sind zeitabhängig und zu versionieren. KOMPASS gibt keine Förderzusage, sondern eine fachliche Vorprüfung.

## 16. Wärmebrückenmanagement

### Fall A – Markierung im Plan

- potenzielle Wärmebrücke in Grundriss oder Schnitt markieren,
- interne Nummer vergeben, beispielsweise WB01,
- Detailanfrage an Architektur erzeugen,
- Status verfolgen,
- ThermCAD-Berechnung zuordnen.

### Fall B – vorhandene Architekturdetails

- fremdes Benennungs- und Nummerierungsschema erfassen,
- Details in Grundrissen oder Schnitten lokalisieren,
- Relevanz bestimmen,
- Länge ermitteln,
- Gleichwertigkeit nach DIN 4108 Beiblatt 2 dokumentieren,
- ThermCAD-Wärmebrücke zuordnen,
- Prüferübersicht erzeugen.

### Fachobjekt Wärmebrücke

- interne Nummer und Bezeichnung,
- Lage und Planreferenz,
- Detailreferenz und Fremdnummer,
- Länge und Typ,
- Status,
- Gleichwertigkeitsstatus,
- Beiblatt-2-Referenz,
- ThermCAD-Projekt oder Berechnung,
- Psi-Wert als externes Ergebnis,
- fRsi, soweit relevant,
- Prüfanmerkung und Berichtsdarstellung.

## 17. Berichte

Berichte greifen auf das Projektmodell zu und referenzieren zugrunde liegende Snapshots. Es gibt keine separate fachliche Wahrheit im Berichtssystem.

Vorgesehene Ausgaben:

- Energieberatungsbericht,
- Wirtschaftlichkeitsbericht,
- Förderübersicht,
- Executive Summary,
- Vergleich von Modernisierungsalternativen,
- Wärmebrückenübersicht,
- Prüferunterlagen,
- Präsentationen,
- Kommunikationsunterlagen.

Jeder Bericht enthält Projektstand, Datenquellen, Importversion, Annahmen, Berechnungsdatum, Verantwortlichkeit und Hinweise zu Unsicherheiten.

## 18. Reale Verbrauchsdaten

KOMPASS unterstützt Abrechnungsperioden, Mengen, Kosten, Energieträger, Witterungsbereinigung, Flächenbezug, Vergleich mit B56, nachvollziehbare Anpassungsfaktoren und dokumentierte Abweichungsursachen.

## 19. Wissensdatenbank

Referenzdaten:

- Nutzungsdauern,
- Wartungs- und Instandhaltungsansätze,
- Energiepreise und Preissteigerungen,
- CO₂-Faktoren und CO₂-Preisannahmen,
- U-Wert-Anforderungen,
- Förderkriterien,
- technische Pflichtnachweise,
- Norm- und Quellenstände.

Referenzdaten werden versioniert, datiert, mit Quellen versehen, projektbezogen überschreibbar und im Bericht nachvollziehbar gehalten.

## 20. Nichtfunktionale Anforderungen

- .NET 8,
- WPF-Desktop,
- API-basierte Trennung,
- SQLite für lokalen Betrieb,
- klare Schichten,
- Domain ohne Infrastrukturabhängigkeiten,
- Warnungen als Fehler,
- reproduzierbarer Build,
- automatische Tests,
- nachvollziehbare Migrationen,
- archivierte Originalimporte,
- keine stillen Datenüberschreibungen,
- deutsche fachliche Begriffe im Domänenmodell.

## 21. Abnahmekriterien erster End-to-End-Prozess

1. Projekt kann angelegt werden.
2. Typische B56-Datei kann importiert werden.
3. Originaldatei wird archiviert.
4. Hash und Importzeitpunkt werden gespeichert.
5. Snapshot wird unveränderlich gespeichert.
6. Bis zu neun Modernisierungsalternativen werden vollständig angezeigt.
7. Blockierende Fehler verhindern eine Bestätigung.
8. Warnungen werden sichtbar angezeigt.
9. Import kann bestätigt werden.
10. Projektmodell wird nachvollziehbar aus dem Snapshot erzeugt.
11. Ergänzbare Projektdaten können geändert und gespeichert werden.
12. Projekt kann geschlossen und wieder geöffnet werden.
13. Ein zweiter Import erzeugt einen neuen Snapshot.
14. Unterschiede werden angezeigt.
15. Projektänderungen werden nicht automatisch überschrieben.
16. Debug- und Release-Build sind erfolgreich.
17. Tests sind erfolgreich.
18. Domain-, API- und Persistenztests decken den Prozess ab.

## 22. Verbindliche Codex-Regeln

Codex darf technische Lösungsvorschläge machen, im vereinbarten Umfang implementieren, Tests ergänzen und Dokumentationsabweichungen melden.

Codex darf nicht ohne Entscheidung:

- B56 durch eigene Berechnung ersetzen,
- IFC-Auswertung einführen,
- importierte Quellwerte editierbar machen,
- Snapshots überschreiben,
- Varianten und Modernisierungsalternativen vermischen,
- Förder- oder Normregeln erfinden,
- große neue Module außerhalb des Auftrags implementieren.

Bei fachlicher Unklarheit muss Codex stoppen und eine konkrete Frage stellen.

## 23. Offene Punkte

1. Exakte Feldliste je unterstütztem B56-Exportblatt.
2. Regeln für Projektidentifikation aus B56.
3. Mapping aller B56-Bauteilcodes.
4. Vergleichslogik bei umbenannten Modernisierungsalternativen.
5. Konfliktauflösung je Feld beim Re-Import.
6. Archivaufbewahrung und Löschregeln.
7. Exakte Förderprogramm-Datenmodelle.
8. Berichtsvorlagen und Corporate Design.
9. Rollenmodell für Mehrbenutzerbetrieb.
10. Import weiterer B56-Exportbereiche nach fachlicher Freigabe.

Offene Punkte sind keine Freigabe für freie Implementierungsentscheidungen.

## 24. Nächste freigegebene Arbeitspakete

1. ADR zu Snapshot und Projektmodell.
2. Domain- und Projekt-CRUD-Tests.
3. Snapshot-Schema-Versionierung.
4. Echter HTTP-End-to-End-Test.
5. Re-Import und Versionsvergleich.
6. Vollständiger erster Anwenderprozess.
7. Danach Wirtschaftlichkeit.
8. Danach Förderung.
9. Danach Berichtswesen.
10. Wärmebrückenmanagement gemäß Gesamtprozess.
