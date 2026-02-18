using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace HDRFixer.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        Title = "HDRFixer - Windows HDR Toolkit";
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item)
        {
            string tag = item.Tag?.ToString() ?? "Dashboard";
            NavigateToPage(tag);
        }
    }

    private void NavigateToPage(string tag)
    {
        var pageType = tag switch
        {
            "Dashboard" => typeof(Views.DashboardPage),
            "Fixes" => typeof(Views.FixesPage),
            "AutoHdr" => typeof(Views.AutoHdrPage),
            "Oled" => typeof(Views.OledPage),
            "Diagnostics" => typeof(Views.DiagnosticsPage),
            "Settings" => typeof(Views.SettingsPage),
            _ => typeof(Views.DashboardPage)
        };
        ContentFrame.Navigate(pageType);
    }
}
