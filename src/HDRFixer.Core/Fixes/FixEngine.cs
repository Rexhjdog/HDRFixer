namespace HDRFixer.Core.Fixes;

public class FixEngine
{
    private readonly List<IFix> _fixes = new();
    public void Register(IFix fix) => _fixes.Add(fix);
    public IReadOnlyList<IFix> GetAllFixes() => _fixes.AsReadOnly();
    public IReadOnlyList<IFix> GetFixesByCategory(FixCategory category)
        => _fixes.Where(f => f.Category == category).ToList().AsReadOnly();
    public List<FixResult> ApplyAll() => _fixes.Select(f => f.Apply()).ToList();
    public List<FixResult> RevertAll() => _fixes.Select(f => f.Revert()).ToList();
    public Dictionary<string, FixStatus> DiagnoseAll() => _fixes.ToDictionary(f => f.Name, f => f.Diagnose());
}
