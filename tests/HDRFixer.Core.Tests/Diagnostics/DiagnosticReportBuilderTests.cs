using HDRFixer.Core.Diagnostics;
using HDRFixer.Core.Display;
using HDRFixer.Core.Fixes;
using Xunit;

namespace HDRFixer.Core.Tests.Diagnostics;

public class DiagnosticReportBuilderTests
{
    [Fact]
    public void Build_PopulatesOsVersion()
    {
        var builder = new DiagnosticReportBuilder();
        var report = builder.Build(detector: new FakeDetector());
        Assert.NotEmpty(report.OsVersion);
    }

    [Fact]
    public void Build_PopulatesOsBuild()
    {
        var builder = new DiagnosticReportBuilder();
        var report = builder.Build(detector: new FakeDetector());
        Assert.True(report.OsBuild > 0);
    }

    [Fact]
    public void Build_DetectsDisplays()
    {
        var builder = new DiagnosticReportBuilder();
        var report = builder.Build(detector: new FakeDetector());
        Assert.Single(report.Displays);
    }

    [Fact]
    public void Build_ChecksPixelFormat()
    {
        var builder = new DiagnosticReportBuilder();
        var report = builder.Build(detector: new FakeDetector());
        Assert.True(report.PixelFormatOptimal);
    }

    [Fact]
    public void Build_SuboptimalPixelFormat()
    {
        var builder = new DiagnosticReportBuilder();
        var report = builder.Build(detector: new LowBitDetector());
        Assert.False(report.PixelFormatOptimal);
    }

    [Fact]
    public void Build_WithFixEngine_ChecksFixStates()
    {
        var engine = new FixEngine();
        engine.Register(new AppliedTestFix("SDR Tone Curve Correction", FixCategory.ToneCurve));
        engine.Register(new AppliedTestFix("SDR Brightness Optimization", FixCategory.SdrBrightness));

        var builder = new DiagnosticReportBuilder();
        var report = builder.Build(engine, new FakeDetector());
        Assert.True(report.GammaCorrectionApplied);
        Assert.True(report.SdrBrightnessOptimal);
    }

    [Fact]
    public void Build_NoDisplays_HandlesGracefully()
    {
        var builder = new DiagnosticReportBuilder();
        var report = builder.Build(detector: new EmptyDetector());
        Assert.Empty(report.Displays);
        Assert.False(report.PixelFormatOptimal);
    }

    [Fact]
    public void Build_SetsTimestamp()
    {
        var before = DateTime.UtcNow;
        var builder = new DiagnosticReportBuilder();
        var report = builder.Build(detector: new FakeDetector());
        Assert.True(report.Timestamp >= before);
    }

    private class FakeDetector : IDisplayDetector
    {
        public List<DisplayInfo> DetectDisplays() => new()
        {
            new DisplayInfo { DeviceName = "Test", IsHdrEnabled = true, BitsPerColor = 10, MaxLuminance = 800f }
        };
    }

    private class LowBitDetector : IDisplayDetector
    {
        public List<DisplayInfo> DetectDisplays() => new()
        {
            new DisplayInfo { DeviceName = "Test", BitsPerColor = 8 }
        };
    }

    private class EmptyDetector : IDisplayDetector
    {
        public List<DisplayInfo> DetectDisplays() => new();
    }

    private class AppliedTestFix : IFix
    {
        public string Name { get; }
        public string Description => "test";
        public FixCategory Category { get; }
        public FixStatus Status { get; } = new() { State = FixState.Applied };
        public AppliedTestFix(string name, FixCategory category) { Name = name; Category = category; }
        public FixResult Apply() => new() { Success = true };
        public FixResult Revert() => new() { Success = true };
        public FixStatus Diagnose() => Status;
    }
}
