namespace HDRFixer.Core.Fixes;

public class FixEngine
{
    private readonly List<IFix> _fixes = new();
    public void Register(IFix fix) => _fixes.Add(fix);
    public IReadOnlyList<IFix> GetAllFixes() => _fixes.AsReadOnly();
    public IReadOnlyList<IFix> GetFixesByCategory(FixCategory category)
        => _fixes.Where(f => f.Category == category).ToList().AsReadOnly();

    public async Task<List<FixResult>> ApplyAllAsync()
    {
        var tasks = _fixes.Select(f => f.ApplyAsync());
        return (await Task.WhenAll(tasks)).ToList();
    }

    public async Task<List<FixResult>> RevertAllAsync()
    {
        var tasks = _fixes.Select(f => f.RevertAsync());
        return (await Task.WhenAll(tasks)).ToList();
    }

    public async Task<Dictionary<string, FixStatus>> DiagnoseAllAsync()
    {
        var tasks = _fixes.Select(async f => new { Name = f.Name, Status = await f.DiagnoseAsync() });
        var results = await Task.WhenAll(tasks);
        return results.ToDictionary(x => x.Name, x => x.Status);
    }
}
