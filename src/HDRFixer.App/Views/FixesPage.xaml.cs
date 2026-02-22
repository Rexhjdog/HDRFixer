using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace HDRFixer.App.Views;

public sealed partial class FixesPage : Page
{
    public FixesPage()
    {
        this.InitializeComponent();
        BrightnessSlider.ValueChanged += (s, e) => BrightnessLabel.Text = $"{(int)e.NewValue} nits";
    }

    private void ApplyGammaFix_Click(object sender, RoutedEventArgs e)
    {
        GammaFixStatus.Text = "Applying...";
        try
        {
            var detector = new HDRFixer.Core.Display.DxgiDisplayDetector();
            var displays = detector.DetectDisplays();
            if (displays.Count > 0)
            {
                var installer = new HDRFixer.Core.ColorProfile.ColorProfileInstaller();
                var fix = new HDRFixer.Core.Fixes.GammaCorrectionFix(installer, displays[0]);
                var result = fix.Apply();
                GammaFixStatus.Text = result.Success ? "Applied" : $"Error: {result.Message}";
            }
        }
        catch (Exception ex) { GammaFixStatus.Text = $"Error: {ex.Message}"; }
    }

    private void RevertGammaFix_Click(object sender, RoutedEventArgs e)
    {
        GammaFixStatus.Text = "Reverting...";
        try
        {
            var detector = new HDRFixer.Core.Display.DxgiDisplayDetector();
            var displays = detector.DetectDisplays();
            if (displays.Count > 0)
            {
                var installer = new HDRFixer.Core.ColorProfile.ColorProfileInstaller();
                var fix = new HDRFixer.Core.Fixes.GammaCorrectionFix(installer, displays[0]);
                var result = fix.Revert();
                GammaFixStatus.Text = result.Success ? "Reverted" : $"Error: {result.Message}";
            }
        }
        catch (Exception ex) { GammaFixStatus.Text = $"Error: {ex.Message}"; }
    }
}
