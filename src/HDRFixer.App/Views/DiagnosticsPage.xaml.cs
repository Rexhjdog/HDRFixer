using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using HDRFixer.Core.Display;
using HDRFixer.Core.Registry;
using HDRFixer.Core.Diagnostics;
using System.Text;

namespace HDRFixer.App.Views;

public sealed partial class DiagnosticsPage : Page
{
    public DiagnosticsPage()
    {
        this.InitializeComponent();
    }

    private void RunDiagnostic_Click(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        try
        {
            sb.AppendLine($"OS: {Environment.OSVersion}");
            sb.AppendLine($"Date: {DateTime.Now}");
            sb.AppendLine();

            var detector = new DxgiDisplayDetector();
            var displays = detector.DetectDisplays();
            sb.AppendLine($"Displays found: {displays.Count}");
            foreach (var d in displays)
            {
                sb.AppendLine($"  Name: {d.DeviceName}");
                sb.AppendLine($"  GPU: {d.GpuName} ({d.GpuVendor})");
                sb.AppendLine($"  HDR: {(d.IsHdrEnabled ? "Active" : "Inactive")}");
                sb.AppendLine($"  Bits per color: {d.BitsPerColor}");
                sb.AppendLine($"  Peak luminance: {d.MaxLuminance:F0} nits");
                sb.AppendLine($"  Min luminance: {d.MinLuminance:F4} nits");
                sb.AppendLine($"  Max full-frame: {d.MaxFullFrameLuminance:F0} nits");
                sb.AppendLine();
            }

            var registry = new HdrRegistryManager();
            sb.AppendLine($"Auto HDR: {(registry.IsAutoHdrEnabled() ? "Enabled" : "Disabled")}");
            sb.AppendLine($"Screen Split Debug: {(registry.IsAutoHdrScreenSplitEnabled() ? "Enabled" : "Disabled")}");
            sb.AppendLine();

            var report = new DiagnosticReport { Displays = displays };
            int score = new HealthScoreCalculator().Calculate(report);
            sb.AppendLine($"Health Score: {score}/100");
        }
        catch (Exception ex)
        {
            sb.AppendLine($"Error: {ex.Message}");
        }

        DiagnosticOutput.Text = sb.ToString();
    }

    private async void ExportReport_Click(object sender, RoutedEventArgs e)
    {
        string report = DiagnosticOutput.Text;
        if (string.IsNullOrEmpty(report) || report.StartsWith("Click")) return;

        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"HDRFixer_Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        await File.WriteAllTextAsync(path, report);
        DiagnosticOutput.Text += $"\n\nReport exported to: {path}";
    }
}
