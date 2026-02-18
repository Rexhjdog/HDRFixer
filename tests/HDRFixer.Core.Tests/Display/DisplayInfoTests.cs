using HDRFixer.Core.Display;
using Xunit;

namespace HDRFixer.Core.Tests.Display;

public class DisplayInfoTests
{
    [Fact]
    public void DisplayInfo_ShouldStoreProperties()
    {
        var info = new DisplayInfo
        {
            DeviceName = @"\\.\DISPLAY1",
            IsHdrEnabled = true,
            BitsPerColor = 10,
            MaxLuminance = 800f,
        };
        Assert.True(info.IsHdrEnabled);
        Assert.Equal(10u, info.BitsPerColor);
        Assert.Equal(800f, info.MaxLuminance);
    }

    [Fact]
    public void IsHdrCapable_WhenAbove250() => Assert.True(new DisplayInfo { MaxLuminance = 400f }.IsHdrCapable);

    [Fact]
    public void IsNotHdrCapable_WhenBelow250() => Assert.False(new DisplayInfo { MaxLuminance = 200f }.IsHdrCapable);

    [Theory]
    [InlineData(GpuVendor.Nvidia)]
    [InlineData(GpuVendor.Amd)]
    [InlineData(GpuVendor.Intel)]
    [InlineData(GpuVendor.Unknown)]
    public void GpuVendor_AllValuesExist(GpuVendor vendor) => Assert.True(Enum.IsDefined(vendor));
}
