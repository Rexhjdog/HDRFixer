using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HDRFixer.Core.Diagnostics;
using HDRFixer.Core.Fixes;
using HDRFixer.Core.Display;

namespace HDRFixer.App.ViewModels;

public partial class DiagnosticsViewModel : BaseViewModel
{
    private readonly FixEngine _fixEngine;
    private readonly DiagnosticReportBuilder _builder;

    [ObservableProperty]
    private string _fullReportText = string.Empty;

    public DiagnosticsViewModel()
    {
        Title = "Diagnostics";
        _fixEngine = FixEngineFactory.Create();
        _builder = new DiagnosticReportBuilder();
        RunDiagnosticsCommand = new RelayCommand(RunDiagnostics);
    }

    public IRelayCommand RunDiagnosticsCommand { get; }

    public void RunDiagnostics()
    {
        IsBusy = true;
        var report = _builder.Build(_fixEngine);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"HDRFixer Diagnostic Report - {report.Timestamp}");
        sb.AppendLine("-------------------------------------------");
        sb.AppendLine($"OS: {report.OsVersion} (Build {report.OsBuild})");
        sb.AppendLine($"Auto HDR Enabled: {report.AutoHdrEnabled}");
        sb.AppendLine($"Gamma Correction Applied: {report.GammaCorrectionApplied}");
        sb.AppendLine($"SDR Brightness Optimal: {report.SdrBrightnessOptimal}");
        sb.AppendLine($"Pixel Format Optimal: {report.PixelFormatOptimal}");
        sb.AppendLine("");
        sb.AppendLine("Displays:");
        foreach (var d in report.Displays)
        {
            sb.AppendLine($"- {d.MonitorName} ({d.DeviceName})");
            sb.AppendLine($"  HDR Enabled: {d.IsHdrEnabled}");
            sb.AppendLine($"  Peak Brightness: {d.MaxLuminance} nits");
            sb.AppendLine($"  Bit Depth: {d.BitsPerColor}-bit");
            sb.AppendLine($"  GPU: {d.GpuName}");
        }

        FullReportText = sb.ToString();
        IsBusy = false;
    }
}
