using HDRFixer.Core.AutoHdr;
using Xunit;

namespace HDRFixer.Core.Tests.AutoHdr;

public class AutoHdrGameManagerTests
{
    [Fact]
    public void GameHdrProfile_StoresProperties()
    {
        var profile = new GameHdrProfile { ExecutablePath = @"C:\Games\game.exe", ForceAutoHdr = true, DisplayName = "My Game" };
        Assert.Equal(@"C:\Games\game.exe", profile.ExecutablePath);
        Assert.True(profile.ForceAutoHdr);
    }

    [Fact]
    public void GameHdrProfile_ExecutableName_ExtractsFilename()
    {
        var profile = new GameHdrProfile { ExecutablePath = @"C:\Program Files\Steam\steamapps\common\Game\game.exe" };
        Assert.Equal("game.exe", profile.ExecutableName);
    }
}
