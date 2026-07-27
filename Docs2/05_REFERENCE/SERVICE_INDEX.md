# SERVICE_INDEX

| Typ | Verantwortung |
|---|---|
| `IProjektService` | Projektanwendungsfälle |
| `ProjektService` | persistente Implementierung |
| `IB56DateiPruefer` | Dateiprüfung |
| `IB56HashService` | Hash-Abstraktion |
| `IB56ArchivService` | Archiv-Abstraktion |
| `B56TabellenImportService` | bestehender B56-Tabellenparser |

Keine gleichnamige konkrete Application- und Persistence-Klasse `ProjektService`, wenn dadurch mehrdeutige Verweise entstehen.
