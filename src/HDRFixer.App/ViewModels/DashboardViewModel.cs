using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HDRFixer.Core.Diagnostics;
using HDRFixer.Core.Display;
using HDRFixer.Core.Fixes;

namespace HDRFixer.App.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly FixEngine _fixEngine;
    private readonly IDisplayDetector _detector;
    private readonly HealthScoreCalculator _healthCalculator;

    [ObservableProperty]
    private string _displayName = "Detecting...";

    [ObservableProperty]
    private string _hdrStatus = string.Empty;

    [ObservableProperty]
    private string _peakLuminanceText = string.Empty;

    [ObservableProperty]
    private string _gpuInfo = string.Empty;

    [ObservableProperty]
    private int _healthScore;

    [ObservableProperty]
    private string _healthLabel = "Checking...";

    public DashboardViewModel()
    {
        Title = "Dashboard";
        _fixEngine = FixEngineFactory.Create();
        _detector = new DxgiDisplayDetector();
        _healthCalculator = new HealthScoreCalculator();
        RefreshCommand = new RelayCommand(Refresh);
        FixAllCommand = new RelayCommand(FixAll);
    }

    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand FixAllCommand { get; }

    public void Refresh()
    {
        IsBusy = true;
        try
        {
            var displays = _detector.DetectDisplays();
            if (displays.Count > 0)
            {
                var p = displays[0];
                DisplayName = !string.IsNullOrEmpty(p.MonitorName) ? p.MonitorName : p.DeviceName;
                HdrStatus = p.IsHdrEnabled ? "HDR Active" : "SDR Only";
                PeakLuminanceText = $"{p.MaxLuminance:F0} nits peak";
                GpuInfo = $"{p.GpuName} ({p.GpuVendor})";
            }
            else
            {
                DisplayName = "No displays detected";
            }

            var builder = new DiagnosticReportBuilder();
            var report = builder.Build(_fixEngine, _detector, displays);
            HealthScore = _healthCalculator.Calculate(report);
            HealthLabel = $"{HealthScore}/100 - {(HealthScore >= 80 ? "Excellent" : HealthScore >= 50 ? "Needs Work" : "Poor")}";
        }
        catch (Exception ex)
        {
            DisplayName = "Error: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void FixAll()
    {
        _fixEngine.ApplyAll();
        Refresh();
    }
}
