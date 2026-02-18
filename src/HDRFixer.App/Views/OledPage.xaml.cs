using Microsoft.UI.Xaml.Controls;

namespace HDRFixer.App.Views;

public sealed partial class OledPage : Page
{
    public OledPage()
    {
        this.InitializeComponent();
        TimeoutSlider.ValueChanged += (s, e) => TimeoutLabel.Text = $"{(int)e.NewValue} minutes";
    }
}
