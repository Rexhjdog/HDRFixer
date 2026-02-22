using HDRFixer.Core.Display;
using HDRFixer.Core.Fixes;
using HDRFixer.Core.ColorProfile;
using Xunit;

namespace HDRFixer.Core.Tests.Fixes;

public class IccConflictFixTests
{
    [Fact]
    public void Diagnose_ReturnsNotNeeded_WhenHdrDisabled()
    {
        var display = new DisplayInfo { IsHdrEnabled = false };
        var fix = new IccConflictFix(display, new ColorProfileInstaller());
        var status = fix.Diagnose();
        Assert.Equal(FixState.NotNeeded, status.State);
    }

    [Fact]
    public void Diagnose_ReturnsNotApplied_WhenHdrEnabledButProfileNotInstalled()
    {
        var display = new DisplayInfo { IsHdrEnabled = true };
        // We can't easily mock the installer's File.Exists check without more refactoring
        // but we can verify the logic branch.
        var fix = new IccConflictFix(display, new ColorProfileInstaller());
        var status = fix.Diagnose();
        // Since the profile HDRFixer_Gamma22.icm won't exist in the sandbox:
        Assert.Equal(FixState.NotApplied, status.State);
    }
}
