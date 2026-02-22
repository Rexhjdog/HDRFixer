using Microsoft.UI.Xaml.Controls;
using HDRFixer.App.ViewModels;

namespace HDRFixer.App.Views;

public sealed partial class OledPage : Page
{
    public OledViewModel ViewModel { get; } = new();

    public OledPage()
    {
        this.InitializeComponent();
    }
}
