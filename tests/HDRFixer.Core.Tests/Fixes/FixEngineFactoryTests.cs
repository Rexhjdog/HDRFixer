using HDRFixer.Core.Display;
using HDRFixer.Core.Fixes;
using Xunit;

namespace HDRFixer.Core.Tests.Fixes;

public class FixEngineFactoryTests
{
    [Fact]
    public void Create_RegistersFourFixes()
    {
        var engine = FixEngineFactory.Create(new FakeDetector());
        Assert.Equal(4, engine.GetAllFixes().Count);
    }

    [Fact]
    public void Create_RegistersGammaCorrectionFix()
    {
        var engine = FixEngineFactory.Create(new FakeDetector());
        Assert.Contains(engine.GetAllFixes(), f => f.Category == FixCategory.ToneCurve);
    }

    [Fact]
    public void Create_RegistersSdrBrightnessFix()
    {
        var engine = FixEngineFactory.Create(new FakeDetector());
        Assert.Contains(engine.GetAllFixes(), f => f.Category == FixCategory.SdrBrightness);
    }

    [Fact]
    public void Create_RegistersAutoHdrFix()
    {
        var engine = FixEngineFactory.Create(new FakeDetector());
        Assert.Contains(engine.GetAllFixes(), f => f.Category == FixCategory.AutoHdr);
    }

    [Fact]
    public void Create_HandlesNoDisplaysGracefully()
    {
        var engine = FixEngineFactory.Create(new EmptyDetector());
        Assert.Equal(4, engine.GetAllFixes().Count);
    }

    [Fact]
    public void Create_HandlesDetectorException()
    {
        var engine = FixEngineFactory.Create(new ThrowingDetector());
        Assert.Equal(4, engine.GetAllFixes().Count);
    }

    private class FakeDetector : IDisplayDetector
    {
        public List<DisplayInfo> DetectDisplays() => new()
        {
            new DisplayInfo
            {
                DeviceName = "Test Display",
                IsHdrEnabled = true,
                MaxLuminance = 800f,
                MinLuminance = 0.001f,
                BitsPerColor = 10
            }
        };
        public void Dispose() {}
    }

    private class EmptyDetector : IDisplayDetector
    {
        public List<DisplayInfo> DetectDisplays() => new();
        public void Dispose() {}
    }

    private class ThrowingDetector : IDisplayDetector
    {
        public List<DisplayInfo> DetectDisplays() => throw new Exception("No GPU");
        public void Dispose() {}
    }
}
