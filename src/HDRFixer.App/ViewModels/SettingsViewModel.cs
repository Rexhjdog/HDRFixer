using CommunityToolkit.Mvvm.ComponentModel;
using HDRFixer.Core.Settings;
using System.Threading.Tasks;
using System;

namespace HDRFixer.App.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly SettingsManager _settingsManager;
    private AppSettings _settings;
    private bool _isLoading;

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
        _settings = new AppSettings();

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        _isLoading = true;
        try
        {
            _settings = await _settingsManager.LoadAsync();

            RunAtStartup = _settings.RunAtStartup;
            MinimizeToTray = _settings.MinimizeToTray;
            EnableFixWatchdog = _settings.EnableFixWatchdog;
            EnableBackgroundService = _settings.EnableBackgroundService;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    public async void Save()
    {
        if (_isLoading) return;

        _settings.RunAtStartup = RunAtStartup;
        _settings.MinimizeToTray = MinimizeToTray;
        _settings.EnableFixWatchdog = EnableFixWatchdog;
        _settings.EnableBackgroundService = EnableBackgroundService;

        try
        {
            await _settingsManager.SaveAsync(_settings);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex}");
        }
    }

    partial void OnRunAtStartupChanged(bool value) => Save();
    partial void OnMinimizeToTrayChanged(bool value) => Save();
    partial void OnEnableFixWatchdogChanged(bool value) => Save();
    partial void OnEnableBackgroundServiceChanged(bool value) => Save();
}
