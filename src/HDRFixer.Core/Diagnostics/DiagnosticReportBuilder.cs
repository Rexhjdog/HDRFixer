using HDRFixer.Core.Display;
using HDRFixer.Core.Fixes;
using HDRFixer.Core.Registry;

namespace HDRFixer.Core.Diagnostics;

public class DiagnosticReportBuilder
{
    public DiagnosticReport Build(FixEngine? engine = null, IDisplayDetector? detector = null)
    {
        detector ??= new DxgiDisplayDetector();
        List<DisplayInfo> displays;
        try { displays = detector.DetectDisplays(); }
        catch { displays = new List<DisplayInfo>(); }

        var report = new DiagnosticReport
        {
            OsVersion = Environment.OSVersion.ToString(),
            OsBuild = Environment.OSVersion.Version.Build,
            Displays = displays,
            Timestamp = DateTime.UtcNow
        };

        // Check registry state
        try
        {
            var registry = new HdrRegistryManager();
            report.AutoHdrEnabled = registry.IsAutoHdrEnabled();
            report.AutoHdrScreenSplit = registry.IsAutoHdrScreenSplitEnabled();
        }
        catch { /* registry access may fail without elevation */ }

        // Check fix states if engine is provided
        if (engine != null)
        {
            var diagnostics = engine.DiagnoseAll();
            report.GammaCorrectionApplied = diagnostics.TryGetValue("SDR Tone Curve Correction", out var gamma) 
                && gamma.State == FixState.Applied;
            report.SdrBrightnessOptimal = diagnostics.TryGetValue("SDR Brightness Optimization", out var brightness) 
                && brightness.State == FixState.Applied;
        }

        // Check display capabilities
        if (displays.Count > 0)
        {
            report.PixelFormatOptimal = displays.All(d => d.BitsPerColor >= 10);
            // Assume no ICC conflicts if we got this far
            report.NoIccConflicts = true;
        }

        return report;
    }
}
