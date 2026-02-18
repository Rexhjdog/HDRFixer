using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using HDRFixer.Core.Settings;

namespace HDRFixer.App.Views;

public sealed partial class SettingsPage : Page
{
    private readonly SettingsManager _settingsManager = new();

    public SettingsPage()
    {
        this.InitializeComponent();
        Loaded += (_, _) => LoadSettings();
    }

    private void LoadSettings()
    {
        var s = _settingsManager.Load();
        StartupToggle.IsOn = s.RunAtStartup;
        TrayToggle.IsOn = s.MinimizeToTray;
        ServiceToggle.IsOn = s.EnableBackgroundService;
        WatchdogToggle.IsOn = s.EnableFixWatchdog;
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        var s = new AppSettings
        {
            RunAtStartup = StartupToggle.IsOn,
            MinimizeToTray = TrayToggle.IsOn,
            EnableBackgroundService = ServiceToggle.IsOn,
            EnableFixWatchdog = WatchdogToggle.IsOn,
        };
        _settingsManager.Save(s);
        SettingsStatus.Text = "Settings saved.";
    }

    private void ResetSettings_Click(object sender, RoutedEventArgs e)
    {
        _settingsManager.Save(new AppSettings());
        LoadSettings();
        SettingsStatus.Text = "Settings reset to defaults.";
    }
}
