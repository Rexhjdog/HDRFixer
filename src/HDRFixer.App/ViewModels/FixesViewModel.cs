using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HDRFixer.Core.Fixes;

namespace HDRFixer.App.ViewModels;

public partial class FixViewModel : ObservableObject
{
    private readonly IFix _fix;

    public string Name => _fix.Name;
    public string Description => _fix.Description;
    public string Category => _fix.Category.ToString();

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private FixState _state;

    public FixViewModel(IFix fix)
    {
        _fix = fix;
        _ = UpdateStatusAsync();
    }

    [RelayCommand]
    public async Task Apply()
    {
        StatusMessage = "Applying...";
        await _fix.ApplyAsync();
        await UpdateStatusAsync();
    }

    [RelayCommand]
    public async Task Revert()
    {
        StatusMessage = "Reverting...";
        await _fix.RevertAsync();
        await UpdateStatusAsync();
    }

    public async Task UpdateStatusAsync()
    {
        var status = await _fix.DiagnoseAsync();
        StatusMessage = status.Message;
        State = status.State;
    }
}

public partial class FixesViewModel : BaseViewModel
{
    private readonly FixEngine _fixEngine;

    public ObservableCollection<FixViewModel> Fixes { get; } = new();

    public FixesViewModel()
    {
        Title = "Fixes";
        _fixEngine = FixEngineFactory.Create();
        RefreshCommand = new RelayCommand(Refresh);
    }

    public IRelayCommand RefreshCommand { get; }

    public void Refresh()
    {
        IsBusy = true;
        Fixes.Clear();
        foreach (var fix in _fixEngine.GetAllFixes())
        {
            Fixes.Add(new FixViewModel(fix));
        }
        IsBusy = false;
    }
}
