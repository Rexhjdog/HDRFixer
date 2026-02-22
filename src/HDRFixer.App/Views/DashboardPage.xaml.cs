using Microsoft.UI.Xaml.Controls;
using HDRFixer.App.ViewModels;

namespace HDRFixer.App.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; } = new();

    public DashboardPage()
    {
        this.InitializeComponent();
        Loaded += (_, _) => ViewModel.RefreshCommand.Execute(null);
    }
}
