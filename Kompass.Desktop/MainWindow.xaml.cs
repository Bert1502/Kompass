using Kompass.Desktop.ViewModels;
using System.Windows;

namespace Kompass.Desktop;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(
        MainWindowViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;

        DataContext = _viewModel;

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        Loaded -= MainWindow_Loaded;

        await _viewModel.InitialisierenAsync();
    }
}