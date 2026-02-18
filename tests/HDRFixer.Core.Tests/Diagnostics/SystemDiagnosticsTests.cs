using HDRFixer.Core.Diagnostics;
using HDRFixer.Core.Display;
using Xunit;

namespace HDRFixer.Core.Tests.Diagnostics;

public class SystemDiagnosticsTests
{
    [Fact]
    public void DiagnosticReport_StoresFields()
    {
        var report = new DiagnosticReport { OsBuild = 26100, AutoHdrEnabled = true,
            InstalledColorProfiles = new() { "p.icm" } };
        Assert.Equal(26100, report.OsBuild);
        Assert.True(report.AutoHdrEnabled);
        Assert.Single(report.InstalledColorProfiles);
    }

    [Fact]
    public void HealthScore_100_WhenAllOptimal()
    {
        var report = new DiagnosticReport
        {
            Displays = new() { new DisplayInfo { IsHdrEnabled = true, BitsPerColor = 10, MaxLuminance = 800 } },
            GammaCorrectionApplied = true, SdrBrightnessOptimal = true,
            PixelFormatOptimal = true, NoIccConflicts = true
        };
        Assert.Equal(100, new HealthScoreCalculator().Calculate(report));
    }

    [Fact]
    public void HealthScore_Low_WhenNoHdr()
    {
        var report = new DiagnosticReport
        { Displays = new() { new DisplayInfo { IsHdrEnabled = false, BitsPerColor = 8, MaxLuminance = 250 } } };
        Assert.True(new HealthScoreCalculator().Calculate(report) < 30);
    }
}
