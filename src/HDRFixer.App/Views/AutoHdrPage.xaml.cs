using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using HDRFixer.Core.Registry;
using HDRFixer.Core.AutoHdr;

namespace HDRFixer.App.Views;

public sealed partial class AutoHdrPage : Page
{
    private readonly HdrRegistryManager _registry = new();
    private readonly AutoHdrGameManager _gameManager = new();

    public AutoHdrPage()
    {
        this.InitializeComponent();
        Loaded += (_, _) => LoadState();
    }

    private void LoadState()
    {
        AutoHdrToggle.IsOn = _registry.IsAutoHdrEnabled();
        ScreenSplitToggle.IsOn = _registry.IsAutoHdrScreenSplitEnabled();
        RefreshGameList();
    }

    private void RefreshGameList()
    {
        var games = _gameManager.GetForcedAutoHdrGames();
        GameList.ItemsSource = games.Select(g => g.ExecutablePath).ToList();
    }

    private void AutoHdrToggle_Toggled(object sender, RoutedEventArgs e)
    {
        try { _registry.SetAutoHdrEnabled(AutoHdrToggle.IsOn); } catch { }
    }

    private void ScreenSplitToggle_Toggled(object sender, RoutedEventArgs e)
    {
        try { _registry.SetAutoHdrScreenSplit(ScreenSplitToggle.IsOn); } catch { }
    }

    private void AddGame_Click(object sender, RoutedEventArgs e)
    {
        string path = GamePathBox.Text.Trim();
        if (!string.IsNullOrEmpty(path))
        {
            _gameManager.SetForceAutoHdr(path, true);
            GamePathBox.Text = "";
            RefreshGameList();
        }
    }
}
