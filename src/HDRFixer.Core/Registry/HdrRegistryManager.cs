using Microsoft.Win32;

namespace HDRFixer.Core.Registry;

public static class HdrRegistryPaths
{
    public const string GraphicsDrivers = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers";
    public const string MonitorDataStore = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\MonitorDataStore";
    public const string Direct3D = @"Software\Microsoft\Direct3D";
}

public interface IHdrRegistryManager
{
    bool IsAutoHdrEnabled();
    void SetAutoHdrEnabled(bool enabled);
    bool IsAutoHdrScreenSplitEnabled();
    void SetAutoHdrScreenSplit(bool enabled);
    List<string> GetMonitorIds();
    bool IsAdvancedColorEnabled(string monitorId);
    void SetAdvancedColorEnabled(string monitorId, bool enabled);
}

public class HdrRegistryManager : IHdrRegistryManager
{
    public bool IsAutoHdrEnabled()
    {
        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(HdrRegistryPaths.GraphicsDrivers);
        return key?.GetValue("AutoHDREnabled") is int val && val == 1;
    }

    public void SetAutoHdrEnabled(bool enabled)
    {
        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(HdrRegistryPaths.GraphicsDrivers, writable: true);
        key?.SetValue("AutoHDREnabled", enabled ? 1 : 0, RegistryValueKind.DWord);
    }

    public bool IsAutoHdrScreenSplitEnabled()
    {
        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(HdrRegistryPaths.GraphicsDrivers);
        return key?.GetValue("AutoHDR.ScreenSplit") is int val && val == 1;
    }

    public void SetAutoHdrScreenSplit(bool enabled)
    {
        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(HdrRegistryPaths.GraphicsDrivers, writable: true);
        key?.SetValue("AutoHDR.ScreenSplit", enabled ? 1 : 0, RegistryValueKind.DWord);
    }

    public List<string> GetMonitorIds()
    {
        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(HdrRegistryPaths.MonitorDataStore);
        return key?.GetSubKeyNames().ToList() ?? new List<string>();
    }

    public bool IsAdvancedColorEnabled(string monitorId)
    {
        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey($@"{HdrRegistryPaths.MonitorDataStore}\{monitorId}");
        return key?.GetValue("AdvancedColorEnabled") is int val && val == 1;
    }

    public void SetAdvancedColorEnabled(string monitorId, bool enabled)
    {
        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey($@"{HdrRegistryPaths.MonitorDataStore}\{monitorId}", writable: true);
        key?.SetValue("AdvancedColorEnabled", enabled ? 1 : 0, RegistryValueKind.DWord);
    }
}
