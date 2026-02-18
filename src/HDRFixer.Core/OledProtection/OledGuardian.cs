using System.Diagnostics;

namespace HDRFixer.Core.OledProtection;

public class OledUsageTracker
{
    private readonly Stopwatch _sessionTimer = new();
    private double _totalHours;
    public double CurrentSessionMinutes => _sessionTimer.Elapsed.TotalMinutes;
    public double TotalHours => _totalHours + _sessionTimer.Elapsed.TotalHours;
    public void Start() => _sessionTimer.Start();
    public void Stop() => _sessionTimer.Stop();
    public void SetTotalHours(double hours) => _totalHours = hours;
    public bool ShouldRemindPixelRefresh() => TotalHours >= 4.0;
}

public class OledProtectionSettings
{
    public bool PixelShiftEnabled { get; set; }
    public bool AutoHideTaskbar { get; set; }
    public bool DarkModeEnforced { get; set; }
    public int StaticContentTimeoutMinutes { get; set; } = 5;
    public double PixelRefreshReminderHours { get; set; } = 4.0;
}
