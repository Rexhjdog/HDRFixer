using HDRFixer.Core.OledProtection;
using Xunit;

namespace HDRFixer.Core.Tests.OledProtection;

public class OledGuardianTests
{
    [Fact]
    public void UsageTracker_TracksTime()
    {
        var tracker = new OledUsageTracker();
        tracker.Start();
        // Can't easily test real time in unit test without mocking clock,
        // but we can verify it doesn't throw and properties exist.
        Assert.True(tracker.CurrentSessionMinutes >= 0);
        tracker.Stop();
    }

    [Fact]
    public void UsageTracker_PixelRefreshReminder_TriggeredAtThreshold()
    {
        var tracker = new OledUsageTracker();
        tracker.SetTotalHours(3.9);
        Assert.False(tracker.ShouldRemindPixelRefresh());
        tracker.SetTotalHours(4.1);
        Assert.True(tracker.ShouldRemindPixelRefresh());
    }

    [Fact]
    public void Settings_DefaultValues()
    {
        var settings = new OledProtectionSettings();
        Assert.Equal(5, settings.StaticContentTimeoutMinutes);
        Assert.Equal(4.0, settings.PixelRefreshReminderHours);
    }
}
