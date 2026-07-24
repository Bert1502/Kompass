using System.Windows;

namespace Kompass.Desktop.Services;

public sealed class DialogService : IDialogService
{
    public bool LoeschenBestaetigen(
        string projektname)
    {
        var ergebnis =
            MessageBox.Show(
                $"Soll das Projekt '{projektname}' wirklich gelöscht werden?",
                "Projekt löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

        return ergebnis == MessageBoxResult.Yes;
    }

    public void FehlerAnzeigen(
        string nachricht)
    {
        MessageBox.Show(
            nachricht,
            "KOMPASS",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
