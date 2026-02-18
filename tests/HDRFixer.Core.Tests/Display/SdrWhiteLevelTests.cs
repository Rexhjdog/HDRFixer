using HDRFixer.Core.Display;
using Xunit;

namespace HDRFixer.Core.Tests.Display;

public class SdrWhiteLevelTests
{
    [Theory]
    [InlineData(1000u, 80f)]
    [InlineData(2000u, 160f)]
    [InlineData(2500u, 200f)]
    [InlineData(5000u, 400f)]
    public void ConvertRawToNits(uint raw, float expectedNits)
    {
        Assert.Equal(expectedNits, SdrWhiteLevelHelper.RawToNits(raw), precision: 1);
    }

    [Theory]
    [InlineData(80f, 1000u)]
    [InlineData(200f, 2500u)]
    [InlineData(400f, 5000u)]
    public void ConvertNitsToRaw(float nits, uint expectedRaw)
    {
        Assert.Equal(expectedRaw, SdrWhiteLevelHelper.NitsToRaw(nits));
    }
}
