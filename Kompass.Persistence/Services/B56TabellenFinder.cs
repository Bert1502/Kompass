using Kompass.Application.B56Import;

namespace Kompass.Persistence.Services;

public sealed class B56TabellenFinder : IB56TabellenFinder
{
    public IReadOnlyList<B56Tabelle> Analysieren(
        B56Arbeitsmappe arbeitsmappe)
    {
        var tabellen =
            new List<B56Tabelle>();

        foreach (var arbeitsblatt in arbeitsmappe.Arbeitsblaetter)
        {
            for (int i = 0; i < arbeitsblatt.Zeilen.Count; i++)
            {
                var zeile =
                    arbeitsblatt.Zeilen[i];

                var ueberschriften =
                    zeile.Zellen
                        .Select(z => z.Wert.Trim())
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToList();

                if (ueberschriften.Count < 3)
                    continue;

                if (IstTabellenkopf(ueberschriften))
                {
                    tabellen.Add(
                        new B56Tabelle
                        {
                            Arbeitsblatt = arbeitsblatt.Name,
                            Titel = string.Join(" | ", ueberschriften),
                            Kopfzeile = i,
                            ErsteDatenzeile = i + 1,
                            LetzteDatenzeile =
                                arbeitsblatt.Zeilen.Count - 1,
                            Spalten =
                                ueberschriften
                        });
                }
            }
        }

        return tabellen;
    }

    private static bool IstTabellenkopf(
        IReadOnlyCollection<string> texte)
    {
        return texte.Any(t =>
                    t.Contains("Bauteil",
                        StringComparison.OrdinalIgnoreCase))
            || texte.Any(t =>
                    t.Contains("U-Wert",
                        StringComparison.OrdinalIgnoreCase))
            || texte.Any(t =>
                    t.Contains("Fläche",
                        StringComparison.OrdinalIgnoreCase))
            || texte.Any(t =>
                    t.Contains("Anlage",
                        StringComparison.OrdinalIgnoreCase))
            || texte.Any(t =>
                    t.Contains("Endenergie",
                        StringComparison.OrdinalIgnoreCase))
            || texte.Any(t =>
                    t.Contains("Primärenergie",
                        StringComparison.OrdinalIgnoreCase));
    }
}
