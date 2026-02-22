using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HDRFixer.Core.OledProtection;

namespace HDRFixer.App.ViewModels;

public partial class OledViewModel : BaseViewModel
{
    private readonly OledGuardian _guardian;
    private readonly OledProtectionSettings _settings;

    [ObservableProperty]
    private bool _pixelShiftEnabled;

    [ObservableProperty]
    private bool _autoHideTaskbar;

    [ObservableProperty]
    private bool _darkModeEnforced;

    [ObservableProperty]
    private string _usageTimeText = "0.0 hours";

    public OledViewModel()
    {
        Title = "OLED Protection";
        _settings = new OledProtectionSettings();
        _guardian = new OledGuardian(_settings);
        RefreshCommand = new RelayCommand(Refresh);
    }

    public IRelayCommand RefreshCommand { get; }

    public void Refresh()
    {
        // Load from settings would happen here
        PixelShiftEnabled = _settings.PixelShiftEnabled;
        AutoHideTaskbar = _settings.AutoHideTaskbar;
        DarkModeEnforced = _settings.DarkModeEnforced;
    }

    partial void OnPixelShiftEnabledChanged(bool value) { _settings.PixelShiftEnabled = value; UpdateGuardian(); }
    partial void OnAutoHideTaskbarChanged(bool value) { _settings.AutoHideTaskbar = value; UpdateGuardian(); }
    partial void OnDarkModeEnforcedChanged(bool value) { _settings.DarkModeEnforced = value; UpdateGuardian(); }

    private void UpdateGuardian()
    {
        _guardian.ApplyAll(); // In a real app, this would be more granular
    }
}
