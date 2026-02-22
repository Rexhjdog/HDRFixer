using HDRFixer.Core.Display;
using HDRFixer.Core.Fixes;
using HDRFixer.Core.Registry;
using Xunit;

namespace HDRFixer.Core.Tests.Fixes;

public class SdrBrightnessFixTests
{
    [Fact]
    public void Category_IsSdrBrightness()
    {
        var fix = new SdrBrightnessFix(new DisplayInfo());
        Assert.Equal(FixCategory.SdrBrightness, fix.Category);
    }

    [Fact]
    public void Name_IsCorrect()
    {
        var fix = new SdrBrightnessFix(new DisplayInfo());
        Assert.Equal("SDR Brightness Optimization", fix.Name);
    }

    [Fact]
    public async Task Apply_ReturnsSuccess()
    {
        var fix = new SdrBrightnessFix(
            new DisplayInfo { MaxLuminance = 800f },
            new FakeRegistryManager());
        var result = await fix.ApplyAsync();
        Assert.True(result.Success);
        Assert.Equal(FixState.Applied, fix.Status.State);
    }

    [Fact]
    public async Task Revert_ReturnsSuccess()
    {
        var fix = new SdrBrightnessFix(new DisplayInfo());
        await fix.ApplyAsync();
        var result = await fix.RevertAsync();
        Assert.True(result.Success);
        Assert.Equal(FixState.NotApplied, fix.Status.State);
    }

    [Theory]
    [InlineData(1000f, 200f)]  // OLED-class
    [InlineData(800f, 200f)]   // OLED-class
    [InlineData(600f, 250f)]   // HDR600
    [InlineData(400f, 280f)]   // HDR400
    [InlineData(100f, 200f)]   // default
    public void CalculateOptimalWhiteLevel_ReturnsExpected(float maxLuminance, float expected)
    {
        var display = new DisplayInfo { MaxLuminance = maxLuminance };
        float result = SdrBrightnessFix.CalculateOptimalWhiteLevel(display);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Diagnose_NearOptimal_ReportsApplied()
    {
        var display = new DisplayInfo { MaxLuminance = 800f, SdrWhiteLevelNits = 210f };
        var fix = new SdrBrightnessFix(display);
        var status = await fix.DiagnoseAsync();
        Assert.Equal(FixState.Applied, status.State);
    }

    [Fact]
    public async Task Diagnose_FarFromOptimal_ReportsNotApplied()
    {
        var display = new DisplayInfo { MaxLuminance = 800f, SdrWhiteLevelNits = 80f };
        var fix = new SdrBrightnessFix(display);
        var status = await fix.DiagnoseAsync();
        Assert.Equal(FixState.NotApplied, status.State);
    }

    private class FakeRegistryManager : IHdrRegistryManager
    {
        public List<string> GetMonitorIds() => new() { "MONITOR123" };
        public bool IsAdvancedColorEnabled(string monitorId) => true;
        public bool IsAutoHdrEnabled() => true;
        public bool IsAutoHdrScreenSplitEnabled() => false;
        public void SetAdvancedColorEnabled(string monitorId, bool enabled) { }
        public void SetAutoHdrEnabled(bool enabled) { }
        public void SetAutoHdrScreenSplit(bool enabled) { }
        public void SetSdrWhiteLevel(string monitorId, float nits) { }
        public void SetAutoHdrPerGame(string exePath, bool enabled) { }
    }
}
