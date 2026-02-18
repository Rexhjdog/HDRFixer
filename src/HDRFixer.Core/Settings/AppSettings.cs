using System.Text.Json;

namespace HDRFixer.Core.Settings;

public class AppSettings
{
    public bool RunAtStartup { get; set; }
    public bool MinimizeToTray { get; set; } = true;
    public bool EnableFixWatchdog { get; set; } = true;
    public bool EnableBackgroundService { get; set; } = true;
    public float PreferredSdrBrightnessNits { get; set; } = 200f;
    public bool OledPixelShiftEnabled { get; set; }
    public int OledStaticContentTimeoutMinutes { get; set; } = 5;
    public bool OledDarkModeEnforced { get; set; }
    public bool OledAutoHideTaskbar { get; set; }
    public Dictionary<string, bool> EnabledFixes { get; set; } = new();
}

public class SettingsManager
{
    private readonly string _settingsPath;

    public SettingsManager(string? configDir = null)
    {
        configDir ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HDRFixer");
        Directory.CreateDirectory(configDir);
        _settingsPath = Path.Combine(configDir, "settings.json");
    }

    public void Save(AppSettings settings)
    {
        File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
    }

    public AppSettings Load()
    {
        if (!File.Exists(_settingsPath)) return new AppSettings();
        return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath)) ?? new AppSettings();
    }
}
