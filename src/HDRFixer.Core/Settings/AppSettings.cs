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
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public SettingsManager(string? configDir = null)
    {
        configDir ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HDRFixer");
        Directory.CreateDirectory(configDir);
        _settingsPath = Path.Combine(configDir, "settings.json");
    }

    public void Save(AppSettings settings)
    {
        _semaphore.Wait();
        try
        {
            File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SaveAsync(AppSettings settings)
    {
        await _semaphore.WaitAsync();
        try
        {
            await File.WriteAllTextAsync(_settingsPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public AppSettings Load()
    {
        _semaphore.Wait();
        try
        {
            if (!File.Exists(_settingsPath)) return new AppSettings();
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath)) ?? new AppSettings();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<AppSettings> LoadAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!File.Exists(_settingsPath)) return new AppSettings();
            string json = await File.ReadAllTextAsync(_settingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
