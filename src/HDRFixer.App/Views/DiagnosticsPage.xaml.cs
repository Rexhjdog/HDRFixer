using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using HDRFixer.Core.Display;
using HDRFixer.Core.Registry;
using HDRFixer.Core.Diagnostics;
using HDRFixer.Core.Fixes;
using System.Text;

namespace HDRFixer.App.Views;

public sealed partial class DiagnosticsPage : Page
{
    private readonly FixEngine _fixEngine;

    public DiagnosticsPage()
    {
        this.InitializeComponent();
        _fixEngine = FixEngineFactory.Create();
    }

    private void RunDiagnostic_Click(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        try
        {
            var builder = new DiagnosticReportBuilder();
            var report = builder.Build(_fixEngine);

            sb.AppendLine($"OS: {report.OsVersion}");
            sb.AppendLine($"Build: {report.OsBuild}");
            sb.AppendLine($"Date: {report.Timestamp:yyyy-MM-dd HH:mm:ss UTC}");
            sb.AppendLine();

            sb.AppendLine($"Displays found: {report.Displays.Count}");
            foreach (var d in report.Displays)
            {
                sb.AppendLine($"  Name: {d.DeviceName}");
                sb.AppendLine($"  GPU: {d.GpuName} ({d.GpuVendor})");
                sb.AppendLine($"  HDR: {(d.IsHdrEnabled ? "Active" : "Inactive")}");
                sb.AppendLine($"  Bits per color: {d.BitsPerColor}");
                sb.AppendLine($"  Peak luminance: {d.MaxLuminance:F0} nits");
                sb.AppendLine($"  Min luminance: {d.MinLuminance:F4} nits");
                sb.AppendLine($"  Max full-frame: {d.MaxFullFrameLuminance:F0} nits");
                sb.AppendLine($"  SDR white level: {d.SdrWhiteLevelNits:F0} nits");
                sb.AppendLine();
            }

            sb.AppendLine($"Auto HDR: {(report.AutoHdrEnabled ? "Enabled" : "Disabled")}");
            sb.AppendLine($"Screen Split Debug: {(report.AutoHdrScreenSplit ? "Enabled" : "Disabled")}");
            sb.AppendLine();

            sb.AppendLine("--- Fix Status ---");
            var diagnostics = _fixEngine.DiagnoseAll();
            foreach (var (name, status) in diagnostics)
            {
                sb.AppendLine($"  {name}: {status.State} - {status.Message}");
            }
            sb.AppendLine();

            int score = new HealthScoreCalculator().Calculate(report);
            sb.AppendLine($"Health Score: {score}/100");
            sb.AppendLine($"  Gamma correction: {(report.GammaCorrectionApplied ? "Yes" : "No")}");
            sb.AppendLine($"  SDR brightness optimal: {(report.SdrBrightnessOptimal ? "Yes" : "No")}");
            sb.AppendLine($"  Pixel format optimal: {(report.PixelFormatOptimal ? "Yes" : "No")}");
            sb.AppendLine($"  No ICC conflicts: {(report.NoIccConflicts ? "Yes" : "No")}");
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

        string path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            $"HDRFixer_Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        await File.WriteAllTextAsync(path, report);
        DiagnosticOutput.Text += $"\n\nReport exported to: {path}";
    }
}