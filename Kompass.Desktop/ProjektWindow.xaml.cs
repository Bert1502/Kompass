using Kompass.Desktop.ViewModels;
using System.Windows;

namespace Kompass.Desktop;

public partial class ProjektWindow : Window
{
    public ProjektWindow(
        ProjektWorkspaceViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}