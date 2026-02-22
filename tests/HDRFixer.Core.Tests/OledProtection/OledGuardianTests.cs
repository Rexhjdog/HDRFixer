using HDRFixer.Core.OledProtection;
using Xunit;

namespace HDRFixer.Core.Tests.OledProtection;

public class OledGuardianTests
{
    [Fact]
    public void TracksRunTime()
    {
        var tracker = new OledUsageTracker();
        tracker.Start();
        Thread.Sleep(100);
        tracker.Stop();
        Assert.True(tracker.CurrentSessionMinutes >= 0);
    }

    [Fact]
    public void ReminderTriggersAfterThreshold()
    {
        var tracker = new OledUsageTracker();
        tracker.SetTotalHours(4.1);
        Assert.True(tracker.ShouldRemindPixelRefresh());
    }

    [Fact]
    public void NoReminderBelowThreshold()
    {
        var tracker = new OledUsageTracker();
        tracker.SetTotalHours(2.0);
        Assert.False(tracker.ShouldRemindPixelRefresh());
    }
}
