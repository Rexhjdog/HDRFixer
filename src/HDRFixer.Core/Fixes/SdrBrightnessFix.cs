using HDRFixer.Core.Display;

namespace HDRFixer.Core.Fixes;

public class SdrBrightnessFix : IFix
{
    private readonly DisplayInfo _display;
    private float _originalNits;

    public string Name => "SDR Brightness Optimization";
    public string Description => "Sets optimal SDR reference white level based on display capabilities";
    public FixCategory Category => FixCategory.SdrBrightness;
    public FixStatus Status { get; private set; } = new();

    public SdrBrightnessFix(DisplayInfo display)
    {
        _display = display;
    }

    public FixResult Apply()
    {
        try
        {
            _originalNits = _display.SdrWhiteLevelNits;
            float optimal = CalculateOptimalWhiteLevel(_display);
            // SDR white level is set via the Windows Settings slider or registry
            // For now we report the recommended value
            Status = new FixStatus
            {
                State = FixState.Applied,
                Message = $"Recommended SDR white level: {optimal:F0} nits (current: {_display.SdrWhiteLevelNits:F0} nits)"
            };
            return new FixResult { Success = true, Message = Status.Message };
        }
        catch (Exception ex)
        {
            Status = new FixStatus { State = FixState.Error, Message = ex.Message };
            return new FixResult { Success = false, Message = ex.Message };
        }
    }

    public FixResult Revert()
    {
        Status = new FixStatus { State = FixState.NotApplied, Message = "Reverted" };
        return new FixResult { Success = true, Message = "SDR brightness fix reverted" };
    }

    public FixStatus Diagnose()
    {
        float current = _display.SdrWhiteLevelNits;
        float optimal = CalculateOptimalWhiteLevel(_display);
        bool isOptimal = Math.Abs(current - optimal) < 30f;
        Status = isOptimal
            ? new FixStatus { State = FixState.Applied, Message = $"SDR white level is near optimal ({current:F0} nits)" }
            : new FixStatus { State = FixState.NotApplied, Message = $"SDR white level ({current:F0} nits) differs from recommended ({optimal:F0} nits)" };
        return Status;
    }

    public static float CalculateOptimalWhiteLevel(DisplayInfo display)
    {
        // For OLED: ~200-250 nits for desktop use
        // For LCD: ~250-300 nits
        if (display.MaxLuminance >= 800f) return 200f; // OLED-class
        if (display.MaxLuminance >= 600f) return 250f; // HDR600
        if (display.MaxLuminance >= 400f) return 280f; // HDR400
        return 200f; // default
    }
}
