using HDRFixer.Core.ColorMath;
using Xunit;

namespace HDRFixer.Core.Tests.ColorMath;

public class TransferFunctionTests
{
    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(1.0, 1.0)]
    [InlineData(0.5, 0.214041)]
    public void SrgbEotf_ConvertsCorrectly(double input, double expected)
    {
        Assert.Equal(expected, TransferFunctions.SrgbEotf(input), precision: 4);
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(1.0, 1.0)]
    public void SrgbInvEotf_RoundTrips(double value, double expected)
    {
        double linear = TransferFunctions.SrgbEotf(value);
        double roundTripped = TransferFunctions.SrgbInvEotf(linear);
        Assert.Equal(expected, roundTripped, precision: 6);
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(1.0, 1.0)]
    [InlineData(0.5, 0.217638)]
    public void Gamma22Eotf_ConvertsCorrectly(double input, double expected)
    {
        Assert.Equal(expected, TransferFunctions.GammaEotf(input, 2.2), precision: 4);
    }

    [Fact]
    public void PqEotf_At1_Returns10000Nits()
    {
        Assert.Equal(10000.0, TransferFunctions.PqEotf(1.0), precision: 0);
    }

    [Fact]
    public void PqEotf_At0_Returns0Nits()
    {
        Assert.Equal(0.0, TransferFunctions.PqEotf(0.0), precision: 4);
    }

    [Theory]
    [InlineData(-0.1, 0.0)]
    [InlineData(-1.0, 0.0)]
    [InlineData(double.MinValue, 0.0)]
    public void PqEotf_ClampsNegativeValuesToZero(double input, double expected)
    {
        Assert.Equal(expected, TransferFunctions.PqEotf(input), precision: 4);
    }

    [Theory]
    [InlineData(1.1, 10000.0)]
    [InlineData(2.0, 10000.0)]
    [InlineData(double.MaxValue, 10000.0)]
    public void PqEotf_ClampsLargeValuesToMaxNits(double input, double expected)
    {
        Assert.Equal(expected, TransferFunctions.PqEotf(input), precision: 4);
    }

    [Fact]
    public void PqInvEotf_RoundTrips()
    {
        double nits = 500.0;
        double pq = TransferFunctions.PqInvEotf(nits);
        Assert.Equal(nits, TransferFunctions.PqEotf(pq), precision: 2);
    }

    [Fact]
    public void SrgbBrighterThanGamma22InShadows()
    {
        Assert.True(TransferFunctions.SrgbEotf(0.1) > TransferFunctions.GammaEotf(0.1, 2.2));
    }
}
