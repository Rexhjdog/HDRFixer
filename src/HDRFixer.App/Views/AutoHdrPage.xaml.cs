using Microsoft.UI.Xaml.Controls;
using HDRFixer.App.ViewModels;

namespace HDRFixer.App.Views;

public sealed partial class AutoHdrPage : Page
{
    public AutoHdrViewModel ViewModel { get; } = new();

    public AutoHdrPage()
    {
        this.InitializeComponent();
        Loaded += (_, _) => ViewModel.RefreshCommand.Execute(null);
    }
}
