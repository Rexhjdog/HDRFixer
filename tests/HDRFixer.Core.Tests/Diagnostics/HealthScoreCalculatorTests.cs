using HDRFixer.Core.Diagnostics;
using HDRFixer.Core.Display;
using Xunit;

namespace HDRFixer.Core.Tests.Diagnostics;

public class HealthScoreCalculatorTests
{
    private readonly HealthScoreCalculator _calculator = new();

    [Fact]
    public void Calculate_WhenNoDisplays_ReturnsZero()
    {
        var report = new DiagnosticReport { Displays = new List<DisplayInfo>() };
        var score = _calculator.Calculate(report);
        Assert.Equal(0, score);
    }

    [Theory]
    [InlineData(true, 10, true, true, true, true, 100)] // All optimal
    [InlineData(false, 8, false, false, false, false, 0)] // All suboptimal
    [InlineData(true, 8, false, false, false, false, 20)] // Just HDR
    [InlineData(false, 10, false, false, false, false, 15)] // Just BitsPerColor
    [InlineData(false, 8, true, false, false, false, 20)] // Just Gamma
    [InlineData(false, 8, false, true, false, false, 15)] // Just SDR Brightness
    [InlineData(false, 8, false, false, true, false, 15)] // Just Pixel Format
    [InlineData(false, 8, false, false, false, true, 15)] // Just No ICC Conflicts
    public void Calculate_WhenSingleDisplay_ReturnsExpectedScore(
        bool isHdr, uint bitsPerColor, bool gamma, bool sdrOptimal, bool pixelOptimal, bool noConflicts, int expectedScore)
    {
        var report = new DiagnosticReport
        {
            Displays = new List<DisplayInfo>
            {
                new DisplayInfo { IsHdrEnabled = isHdr, BitsPerColor = bitsPerColor }
            },
            GammaCorrectionApplied = gamma,
            SdrBrightnessOptimal = sdrOptimal,
            PixelFormatOptimal = pixelOptimal,
            NoIccConflicts = noConflicts
        };

        var score = _calculator.Calculate(report);
        Assert.Equal(expectedScore, score);
    }

    [Fact]
    public void Calculate_WhenMultipleDisplays_ApplyAnyAndAllLogic()
    {
        // Scenario 1: One display has HDR, one doesn't. Score should increase (+20) because Any() is used.
        var reportAnyHdr = new DiagnosticReport
        {
            Displays = new List<DisplayInfo>
            {
                new DisplayInfo { IsHdrEnabled = true, BitsPerColor = 8 },
                new DisplayInfo { IsHdrEnabled = false, BitsPerColor = 8 }
            }
        };
        // Expected: 20 (HDR) + 0 (Bits) + 0 (Gamma) + 0 (SDR) + 0 (Pixel) + 0 (ICC) = 20
        Assert.Equal(20, _calculator.Calculate(reportAnyHdr));

        // Scenario 2: One display has 10 bits, one has 8 bits. Score should NOT increase (+0) because All() is used for BitsPerColor.
        var reportNotAllBits = new DiagnosticReport
        {
            Displays = new List<DisplayInfo>
            {
                new DisplayInfo { IsHdrEnabled = false, BitsPerColor = 10 },
                new DisplayInfo { IsHdrEnabled = false, BitsPerColor = 8 }
            }
        };
        // Expected: 0 (HDR) + 0 (Bits) + 0 (Gamma) + 0 (SDR) + 0 (Pixel) + 0 (ICC) = 0
        Assert.Equal(0, _calculator.Calculate(reportNotAllBits));

        // Scenario 3: All displays have 10 bits. Score should increase (+15).
        var reportAllBits = new DiagnosticReport
        {
            Displays = new List<DisplayInfo>
            {
                new DisplayInfo { IsHdrEnabled = false, BitsPerColor = 10 },
                new DisplayInfo { IsHdrEnabled = false, BitsPerColor = 10 }
            }
        };
        // Expected: 0 (HDR) + 15 (Bits) + 0 (Gamma) + 0 (SDR) + 0 (Pixel) + 0 (ICC) = 15
        Assert.Equal(15, _calculator.Calculate(reportAllBits));
    }
}
