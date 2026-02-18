using HDRFixer.Core.Display;

namespace HDRFixer.Core.Diagnostics;

public class DiagnosticReport
{
    public string OsVersion { get; set; } = string.Empty;
    public int OsBuild { get; set; }
    public List<DisplayInfo> Displays { get; set; } = new();
    public bool AutoHdrEnabled { get; set; }
    public bool AutoHdrScreenSplit { get; set; }
    public List<string> InstalledColorProfiles { get; set; } = new();
    public bool GammaCorrectionApplied { get; set; }
    public bool SdrBrightnessOptimal { get; set; }
    public bool PixelFormatOptimal { get; set; }
    public bool NoIccConflicts { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class HealthScoreCalculator
{
    public int Calculate(DiagnosticReport report)
    {
        if (report.Displays.Count == 0) return 0;
        int score = 0, maxScore = 0;
        maxScore += 20; if (report.Displays.Any(d => d.IsHdrEnabled)) score += 20;
        maxScore += 15; if (report.Displays.All(d => d.BitsPerColor >= 10)) score += 15;
        maxScore += 20; if (report.GammaCorrectionApplied) score += 20;
        maxScore += 15; if (report.SdrBrightnessOptimal) score += 15;
        maxScore += 15; if (report.PixelFormatOptimal) score += 15;
        maxScore += 15; if (report.NoIccConflicts) score += 15;
        return maxScore > 0 ? (int)Math.Round((double)score / maxScore * 100) : 0;
    }
}
