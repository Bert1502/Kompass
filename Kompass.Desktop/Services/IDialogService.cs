namespace Kompass.Desktop.Services;

public interface IDialogService
{
    bool LoeschenBestaetigen(
        string projektname);

    void FehlerAnzeigen(
        string nachricht);
}
