using HDRFixer.Core.ColorMath;
using Xunit;

namespace HDRFixer.Core.Tests.ColorMath;

public class GammaCorrectionLutTests
{
    [Fact]
    public void SdrLut_HasCorrectSize()
    {
        Assert.Equal(1024, GammaCorrectionLut.GenerateSrgbToGamma22Lut(1024).Length);
    }

    [Fact]
    public void SdrLut_StartsAtZero()
    {
        Assert.Equal(0.0, GammaCorrectionLut.GenerateSrgbToGamma22Lut(1024)[0], precision: 6);
    }

    [Fact]
    public void SdrLut_EndsAtOne()
    {
        var lut = GammaCorrectionLut.GenerateSrgbToGamma22Lut(1024);
        Assert.Equal(1.0, lut[^1], precision: 6);
    }

    [Fact]
    public void SdrLut_MidpointDarkerThanLinear()
    {
        var lut = GammaCorrectionLut.GenerateSrgbToGamma22Lut(1024);
        double midInput = 512.0 / 1023;
        Assert.True(lut[512] <= midInput);
    }

    [Fact]
    public void HdrLut_HasCorrectSize()
    {
        Assert.Equal(4096, GammaCorrectionLut.GenerateHdrSrgbToGamma22Lut(4096, 200.0).Length);
    }

    [Fact]
    public void HdrLut_PassthroughAboveWhiteLevel()
    {
        var lut = GammaCorrectionLut.GenerateHdrSrgbToGamma22Lut(4096, 200.0);
        int highIndex = 2457;
        double input = (double)highIndex / 4095;
        Assert.Equal(input, lut[highIndex], precision: 4);
    }

    [Fact]
    public void HdrLut_IsMonotonicallyIncreasing()
    {
        var lut = GammaCorrectionLut.GenerateHdrSrgbToGamma22Lut(4096, 200.0);
        for (int i = 1; i < lut.Length; i++)
            Assert.True(lut[i] >= lut[i - 1], $"Not monotonic at {i}: {lut[i]} < {lut[i - 1]}");
    }
}
