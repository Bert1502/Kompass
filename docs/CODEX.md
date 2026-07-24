# CODEX.md

## Mission

KOMPASS ist eine Beratungssoftware für energetische Sanierungen. B56 ist
der einzige energetische Rechenkern.

## Nicht ändern

-   Architektur nur auf ausdrücklichen Auftrag ändern.
-   Keine eigenen DIN V 18599 Berechnungen implementieren.
-   Keine IFC-Auswertung implementieren.

## Vor jeder Änderung

1.  dotnet restore
2.  dotnet build
3.  dotnet test

## Nach jeder Änderung

-   Build erfolgreich.
-   Änderungen dokumentieren.
-   Pull Request erstellen.

## Fachliche Regeln

-   Modernisierungsalternativen stammen aus B56.
-   Varianten sind Planstände.
-   Wärmebrückenprozess einhalten.
