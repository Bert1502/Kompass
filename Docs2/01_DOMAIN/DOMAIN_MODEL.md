# DOMAIN_MODEL

```text
Projekt
├── Gebäude
├── B56-Importe
│   ├── Originaldatei
│   ├── Snapshot
│   └── Validierungsbefunde
├── Projektmodell
│   ├── Varianten
│   │   └── Modernisierungsalternativen
│   │       ├── Bauteilreferenzen
│   │       ├── Kosten
│   │       ├── Förderung
│   │       └── Wirtschaftlichkeit
│   ├── reale Verbrauchsdaten
│   └── Annahmen
├── Wärmebrücken
├── Dokumente
└── Berichte
```

## Kerngrenzen
- Snapshot dokumentiert Quelle.
- Projektmodell dokumentiert Bearbeitungsstand.
- Reimport erzeugt neuen Snapshot.
- Übernahme in das Projektmodell erfolgt explizit.
