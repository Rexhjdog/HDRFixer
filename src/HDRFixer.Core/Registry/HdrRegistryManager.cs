using Microsoft.Win32;

namespace HDRFixer.Core.Registry;

public static class HdrRegistryPaths
{
    public const string GraphicsDrivers = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers";
    public const string MonitorDataStore = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\MonitorDataStore";
    public const string Direct3D = @"Software\Microsoft\Direct3D";
}

public static class DirectXSettingsParser
{
    public static Dictionary<string, string> Parse(string settingsString)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(settingsString)) return result;
        foreach (var pair in settingsString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2) result[parts[0].Trim()] = parts[1].Trim();
        }
        return result;
    }

    public static string Serialize(Dictionary<string, string> settings)
    {
        return string.Join(";", settings.Select(kv => $"{kv.Key}={kv.Value}")) + ";";
    }
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
    void SetSdrWhiteLevel(string monitorId, float nits);
    void SetAutoHdrPerGame(string exePath, bool enabled);
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

    public void SetSdrWhiteLevel(string monitorId, float nits)
    {
        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey($@"{HdrRegistryPaths.MonitorDataStore}\{monitorId}", writable: true);
        if (key != null)
        {
            // SDRWhiteLevel is stored as (nits / 80) * 1000
            int value = (int)((nits / 80f) * 1000f);
            key.SetValue("SDRWhiteLevel", value, RegistryValueKind.DWord);
        }
    }

    public void SetAutoHdrPerGame(string exePath, bool enabled)
    {
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(HdrRegistryPaths.Direct3D, writable: true);
        if (key == null) return;

        using var appKey = key.CreateSubKey(Path.GetFileName(exePath), writable: true);
        appKey.SetValue("AutoHDR", enabled ? 1 : 0, RegistryValueKind.DWord);
    }
}
