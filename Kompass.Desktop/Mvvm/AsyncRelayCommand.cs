using System.Windows.Input;

namespace Kompass.Desktop.Mvvm;

public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;

    private bool _wirdAusgefuehrt;

    public AsyncRelayCommand(
        Func<Task> execute,
        Func<bool>? canExecute = null)
    {
        _execute =
            execute
            ?? throw new ArgumentNullException(
                nameof(execute));

        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(
        object? parameter)
    {
        return !_wirdAusgefuehrt
            && (_canExecute?.Invoke() ?? true);
    }

    public async void Execute(
        object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        try
        {
            _wirdAusgefuehrt = true;

            Aktualisieren();

            await _execute();
        }
        finally
        {
            _wirdAusgefuehrt = false;

            Aktualisieren();
        }
    }

    public void Aktualisieren()
    {
        CanExecuteChanged?.Invoke(
            this,
            EventArgs.Empty);
    }
}
