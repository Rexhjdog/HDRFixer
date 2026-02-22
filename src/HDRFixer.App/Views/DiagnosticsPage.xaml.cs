using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using HDRFixer.App.ViewModels;

namespace HDRFixer.App.Views;

public sealed partial class DiagnosticsPage : Page
{
    public DiagnosticsViewModel ViewModel { get; } = new();

    public DiagnosticsPage()
    {
        this.InitializeComponent();
        Loaded += (_, _) => ViewModel.RunDiagnosticsCommand.Execute(null);
    }

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        var savePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "HDRFixer_Report.txt");
        ViewModel.ExportReportCommand.Execute(savePath);
    }
}
