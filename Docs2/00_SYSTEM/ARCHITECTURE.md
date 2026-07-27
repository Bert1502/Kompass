# ARCHITECTURE

## Bekannte Solution
```text
Kompass.Domain
Kompass.Application
Kompass.Persistence
Kompass.Api
Kompass.Desktop
```
Historische Planungen nannten zusätzlich `Kompass.Infrastructure`, `Kompass.Import.B56`, `Kompass.Export` und `Kompass.Reporting`. Der aktuelle Code entscheidet.

## Abhängigkeiten
```text
Desktop / API -> Application -> Domain
Persistence implementiert Application-Abstraktionen
```

## Regeln
- Domain kennt weder EF Core noch HTTP noch WPF.
- Fachlogik gehört nicht in Controller oder ViewModels.
- Externe Systeme schreiben nicht direkt in das Domänenmodell.
- Import wird vor Commit validiert.
