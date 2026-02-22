using HDRFixer.Core.Registry;
using Xunit;

namespace HDRFixer.Core.Tests.Registry;

public class HdrRegistryManagerTests
{
    [Fact]
    public void RegistryPaths_AreCorrect()
    {
        Assert.Equal(@"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", HdrRegistryPaths.GraphicsDrivers);
        Assert.Equal(@"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\MonitorDataStore", HdrRegistryPaths.MonitorDataStore);
        Assert.Equal(@"Software\Microsoft\Direct3D", HdrRegistryPaths.Direct3D);
    }

    [Fact]
    public void ParseDirectXSettings_ExtractsValues()
    {
        string settings = "SwapEffectUpgradeEnable=1;AutoHDREnable=1;VRROptimizeEnable=0;";
        var parsed = DirectXSettingsParser.Parse(settings);
        Assert.Equal("1", parsed["AutoHDREnable"]);
        Assert.Equal("1", parsed["SwapEffectUpgradeEnable"]);
        Assert.Equal("0", parsed["VRROptimizeEnable"]);
    }

    [Fact]
    public void SerializeDirectXSettings_ProducesCorrectString()
    {
        var settings = new Dictionary<string, string> { ["AutoHDREnable"] = "1", ["SwapEffectUpgradeEnable"] = "1" };
        string result = DirectXSettingsParser.Serialize(settings);
        Assert.Contains("AutoHDREnable=1", result);
        Assert.Contains("SwapEffectUpgradeEnable=1", result);
    }

    [Fact]
    public void ParseDirectXSettings_HandlesEmptyString()
    {
        Assert.Empty(DirectXSettingsParser.Parse(""));
    }
}
