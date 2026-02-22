using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

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

public class OledGuardian
{
    private readonly OledProtectionSettings _settings;
    private readonly OledUsageTracker _tracker;

    public OledGuardian(OledProtectionSettings settings)
    {
        _settings = settings;
        _tracker = new OledUsageTracker();
    }

    public void ApplyAll()
    {
        if (_settings.AutoHideTaskbar) SetAutoHideTaskbar(true);
        if (_settings.DarkModeEnforced) SetDarkMode(true);
        _tracker.Start();
    }

    public void RevertAll()
    {
        if (_settings.AutoHideTaskbar) SetAutoHideTaskbar(false);
        if (_settings.DarkModeEnforced) SetDarkMode(false);
        _tracker.Stop();
    }

    private void SetAutoHideTaskbar(bool enable)
    {
        var data = new APPBARDATA();
        data.cbSize = Marshal.SizeOf(data);
        data.hWnd = FindWindow("Shell_TrayWnd", null);
        if (data.hWnd == IntPtr.Zero) return;

        data.lParam = enable ? (IntPtr)0x01 : (IntPtr)0x02; // 0x01 = ABS_AUTOHIDE, 0x02 = ABS_ALWAYSONTOP (standard)
        SHAppBarMessage(ABM_SETSTATE, ref data);
    }

    private void SetDarkMode(bool enable)
    {
        const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(PersonalizeKey, writable: true);
        if (key != null)
        {
            int val = enable ? 0 : 1; // 0 is Dark, 1 is Light for Apps/SystemUseLightTheme
            key.SetValue("AppsUseLightTheme", val, RegistryValueKind.DWord);
            key.SetValue("SystemUsesLightTheme", val, RegistryValueKind.DWord);
        }
    }

    private const int ABM_SETSTATE = 0x0000000a;

    [StructLayout(LayoutKind.Sequential)]
    private struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public RECT rc;
        public IntPtr lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [DllImport("shell32.dll")]
    private static extern IntPtr SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);
}
