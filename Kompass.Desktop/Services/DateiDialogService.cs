using Microsoft.Win32;

namespace Kompass.Desktop.Services;

public sealed class DateiDialogService : IDateiDialogService
{
    public string? B56DateiAuswaehlen()
    {
        var dialog = new OpenFileDialog
        {
            Title = "B56-Excel-Datei auswählen",

            Filter =
                "B56-Excel-Dateien (*.xlsx;*.xlsm)|*.xlsx;*.xlsm|" +
                "Excel-Arbeitsmappen (*.xlsx)|*.xlsx|" +
                "Excel-Arbeitsmappen mit Makros (*.xlsm)|*.xlsm|" +
                "Alle Dateien (*.*)|*.*",

            FilterIndex = 1,

            CheckFileExists = true,

            CheckPathExists = true,

            Multiselect = false,

            RestoreDirectory = true
        };

        var ergebnis = dialog.ShowDialog();

        return ergebnis == true
            ? dialog.FileName
            : null;
    }
}
