using Microsoft.UI.Xaml.Controls;
using HDRFixer.App.ViewModels;

namespace HDRFixer.App.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; } = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }
}
