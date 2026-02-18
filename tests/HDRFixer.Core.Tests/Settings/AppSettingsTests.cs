using System.Text.Json;
using HDRFixer.Core.Settings;
using Xunit;

namespace HDRFixer.Core.Tests.Settings;

public class AppSettingsTests
{
    [Fact]
    public void Defaults()
    {
        var s = new AppSettings();
        Assert.False(s.RunAtStartup);
        Assert.True(s.MinimizeToTray);
        Assert.True(s.EnableFixWatchdog);
        Assert.Equal(200f, s.PreferredSdrBrightnessNits);
    }

    [Fact]
    public void SerializesAndDeserializes()
    {
        var s = new AppSettings { RunAtStartup = true, PreferredSdrBrightnessNits = 250f, OledPixelShiftEnabled = true };
        string json = JsonSerializer.Serialize(s);
        var d = JsonSerializer.Deserialize<AppSettings>(json)!;
        Assert.True(d.RunAtStartup);
        Assert.Equal(250f, d.PreferredSdrBrightnessNits);
        Assert.True(d.OledPixelShiftEnabled);
    }

    [Fact]
    public void SavesAndLoads()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "hdrfixer_settings_test_" + Guid.NewGuid());
        try
        {
            var mgr = new SettingsManager(tempDir);
            mgr.Save(new AppSettings { PreferredSdrBrightnessNits = 300f });
            Assert.Equal(300f, mgr.Load().PreferredSdrBrightnessNits);
        }
        finally { Directory.Delete(tempDir, recursive: true); }
    }
}
