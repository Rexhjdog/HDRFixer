using Microsoft.UI.Xaml.Controls;
using HDRFixer.App.ViewModels;

namespace HDRFixer.App.Views;

public sealed partial class FixesPage : Page
{
    public FixesViewModel ViewModel { get; } = new();

    public FixesPage()
    {
        this.InitializeComponent();
        Loaded += (_, _) => ViewModel.RefreshCommand.Execute(null);
    }
}
