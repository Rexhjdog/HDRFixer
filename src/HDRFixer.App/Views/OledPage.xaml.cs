using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using HDRFixer.Core.Settings;

namespace HDRFixer.App.Views;

public sealed partial class OledPage : Page
{
    private readonly SettingsManager _settingsManager = new();
    private bool _isLoading;

    public OledPage()
    {
        this.InitializeComponent();
        TimeoutSlider.ValueChanged += (s, e) => TimeoutLabel.Text = $"{(int)e.NewValue} minutes";
        PixelShiftToggle.Toggled += OnToggleChanged;
        AutoHideTaskbarToggle.Toggled += OnToggleChanged;
        DarkModeToggle.Toggled += OnToggleChanged;
        Loaded += (_, _) => LoadSettings();
    }

    private void LoadSettings()
    {
        _isLoading = true;
        var s = _settingsManager.Load();
        PixelShiftToggle.IsOn = s.OledPixelShiftEnabled;
        AutoHideTaskbarToggle.IsOn = s.OledAutoHideTaskbar;
        DarkModeToggle.IsOn = s.OledDarkModeEnforced;
        TimeoutSlider.Value = s.OledStaticContentTimeoutMinutes;
        _isLoading = false;
    }

    private void OnToggleChanged(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        var s = _settingsManager.Load();
        s.OledPixelShiftEnabled = PixelShiftToggle.IsOn;
        s.OledAutoHideTaskbar = AutoHideTaskbarToggle.IsOn;
        s.OledDarkModeEnforced = DarkModeToggle.IsOn;
        s.OledStaticContentTimeoutMinutes = (int)TimeoutSlider.Value;
        _settingsManager.Save(s);
    }
}
