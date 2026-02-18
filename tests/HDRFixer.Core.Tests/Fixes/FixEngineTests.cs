using HDRFixer.Core.Fixes;
using Xunit;

namespace HDRFixer.Core.Tests.Fixes;

public class FixEngineTests
{
    [Fact]
    public void FixStatus_DefaultIsPending() => Assert.Equal(FixState.NotApplied, new FixStatus().State);

    [Fact]
    public void FixCategory_AllCategoriesExist() => Assert.Equal(7, Enum.GetValues<FixCategory>().Length);

    [Fact]
    public void FixEngine_RegistersFixes()
    {
        var engine = new FixEngine();
        engine.Register(new TestFix());
        Assert.Single(engine.GetAllFixes());
        Assert.Equal("Test Fix", engine.GetAllFixes()[0].Name);
    }

    [Fact]
    public void FixEngine_ApplyAll_AppliesAllRegistered()
    {
        var engine = new FixEngine();
        engine.Register(new TestFix("Fix1"));
        engine.Register(new TestFix("Fix2"));
        var results = engine.ApplyAll();
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.Success));
    }

    private class TestFix : IFix
    {
        public string Name { get; }
        public string Description => "A test fix";
        public FixCategory Category => FixCategory.ToneCurve;
        public FixStatus Status { get; private set; } = new();
        public TestFix(string name = "Test Fix") => Name = name;
        public FixResult Apply() { Status = new FixStatus { State = FixState.Applied }; return new FixResult { Success = true }; }
        public FixResult Revert() { Status = new FixStatus { State = FixState.NotApplied }; return new FixResult { Success = true }; }
        public FixStatus Diagnose() => Status;
    }
}
