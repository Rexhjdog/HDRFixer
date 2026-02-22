using HDRFixer.Core.Diagnostics;
using HDRFixer.Core.Display;
using HDRFixer.Core.Fixes;
using Xunit;

namespace HDRFixer.Core.Tests.Diagnostics;

public class DiagnosticReportBuilderTests_Optimization
{
    private class CountingDetector : IDisplayDetector
    {
        public int CallCount { get; private set; }

        public List<DisplayInfo> DetectDisplays()
        {
            CallCount++;
            return new List<DisplayInfo>();
        }
    }

    [Fact]
    public void Build_WithDisplays_SkipsDetection()
    {
        var builder = new DiagnosticReportBuilder();
        var detector = new CountingDetector();
        var displays = new List<DisplayInfo>();

        builder.Build(detector: detector, preDetectedDisplays: displays);

        Assert.Equal(0, detector.CallCount);
    }

    [Fact]
    public void Build_WithoutDisplays_PerformsDetection()
    {
        var builder = new DiagnosticReportBuilder();
        var detector = new CountingDetector();

        builder.Build(detector: detector, preDetectedDisplays: null);

        Assert.Equal(1, detector.CallCount);
    }
}
