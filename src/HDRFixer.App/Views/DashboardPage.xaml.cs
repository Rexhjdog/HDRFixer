using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using HDRFixer.Core.Display;
using HDRFixer.Core.Diagnostics;
using HDRFixer.Core.Fixes;

namespace HDRFixer.App.Views;

public sealed partial class DashboardPage : Page
{
    private readonly FixEngine _fixEngine;

    public DashboardPage()
    {
        this.InitializeComponent();
        _fixEngine = FixEngineFactory.Create();
        Loaded += (_, _) => RefreshUI();
    }

    private void RefreshUI()
    {
        try
        {
            var detector = new DxgiDisplayDetector();
            var displays = detector.DetectDisplays();
            if (displays.Count > 0)
            {
                var p = displays[0];
                DisplayNameText.Text = p.MonitorName.Length > 0 ? p.MonitorName : p.DeviceName;
                HdrStatusText.Text = p.IsHdrEnabled ? "HDR Active" : "SDR Only";
                PeakLuminanceText.Text = $"{p.MaxLuminance:F0} nits peak";
                GpuInfoText.Text = $"{p.GpuName} ({p.GpuVendor})";
            }
            else { DisplayNameText.Text = "No displays detected"; }

            var builder = new DiagnosticReportBuilder();
            var report = builder.Build(_fixEngine);
            int score = new HealthScoreCalculator().Calculate(report);
            HealthRing.Value = score;
            HealthLabelText.Text = $"{score}/100 - {(score >= 80 ? "Excellent" : score >= 50 ? "Needs Work" : "Poor")}";
        }
        catch (Exception ex) { DisplayNameText.Text = "Error: " + ex.Message; }
    }

    private void FixAll_Click(object sender, RoutedEventArgs e) { _fixEngine.ApplyAll(); RefreshUI(); }
    private void Diagnose_Click(object sender, RoutedEventArgs e) => RefreshUI();
}
