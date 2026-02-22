using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HDRFixer.Core.AutoHdr;
using HDRFixer.Core.Registry;

namespace HDRFixer.App.ViewModels;

public partial class GameProfileViewModel : ObservableObject
{
    private readonly GameHdrProfile _profile;
    private readonly AutoHdrGameManager _manager;

    public string Name => _profile.DisplayName;
    public string Path => _profile.ExecutablePath;

    [ObservableProperty]
    private bool _forceAutoHdr;

    public GameProfileViewModel(GameHdrProfile profile, AutoHdrGameManager manager)
    {
        _profile = profile;
        _manager = manager;
        _forceAutoHdr = profile.ForceAutoHdr;
    }

    partial void OnForceAutoHdrChanged(bool value)
    {
        _manager.SetForceAutoHdr(_profile.ExecutablePath, value);
    }
}

public partial class AutoHdrViewModel : BaseViewModel
{
    private readonly HdrRegistryManager _registry;
    private readonly AutoHdrGameManager _gameManager;

    [ObservableProperty]
    private bool _isGlobalEnabled;

    [ObservableProperty]
    private bool _isScreenSplitEnabled;

    public ObservableCollection<GameProfileViewModel> Games { get; } = new();

    public AutoHdrViewModel()
    {
        Title = "Auto HDR";
        _registry = new HdrRegistryManager();
        _gameManager = new AutoHdrGameManager();
        RefreshCommand = new RelayCommand(Refresh);
    }

    public IRelayCommand RefreshCommand { get; }

    public void Refresh()
    {
        IsBusy = true;
        IsGlobalEnabled = _registry.IsAutoHdrEnabled();
        IsScreenSplitEnabled = _registry.IsAutoHdrScreenSplitEnabled();

        Games.Clear();
        foreach (var game in _gameManager.GetForcedAutoHdrGames())
        {
            Games.Add(new GameProfileViewModel(game, _gameManager));
        }
        IsBusy = false;
    }

    partial void OnIsGlobalEnabledChanged(bool value) => _registry.SetAutoHdrEnabled(value);
    partial void OnIsScreenSplitEnabledChanged(bool value) => _registry.SetAutoHdrScreenSplit(value);
}
