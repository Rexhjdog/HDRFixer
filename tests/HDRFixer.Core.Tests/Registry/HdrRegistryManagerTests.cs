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
        Assert.Equal(@"Software\Microsoft\DirectX\UserGpuPreferences", HdrRegistryPaths.DirectXUserPrefs);
    }

}
