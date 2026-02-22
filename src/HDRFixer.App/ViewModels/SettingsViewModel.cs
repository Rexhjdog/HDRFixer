using CommunityToolkit.Mvvm.ComponentModel;
using HDRFixer.Core.Settings;

namespace HDRFixer.App.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly SettingsManager _settingsManager;
    private AppSettings _settings;

    [ObservableProperty]
    private bool _runAtStartup;

    [ObservableProperty]
    private bool _minimizeToTray;

    [ObservableProperty]
    private bool _enableFixWatchdog;

    [ObservableProperty]
    private bool _enableBackgroundService;

    public SettingsViewModel()
    {
        Title = "Settings";
        _settingsManager = new SettingsManager();
        _settings = _settingsManager.Load();

        _runAtStartup = _settings.RunAtStartup;
        _minimizeToTray = _settings.MinimizeToTray;
        _enableFixWatchdog = _settings.EnableFixWatchdog;
        _enableBackgroundService = _settings.EnableBackgroundService;
    }

    public void Save()
    {
        _settings.RunAtStartup = RunAtStartup;
        _settings.MinimizeToTray = MinimizeToTray;
        _settings.EnableFixWatchdog = EnableFixWatchdog;
        _settings.EnableBackgroundService = EnableBackgroundService;
        _settingsManager.Save(_settings);
    }

    partial void OnRunAtStartupChanged(bool value) => Save();
    partial void OnMinimizeToTrayChanged(bool value) => Save();
    partial void OnEnableFixWatchdogChanged(bool value) => Save();
    partial void OnEnableBackgroundServiceChanged(bool value) => Save();
}
