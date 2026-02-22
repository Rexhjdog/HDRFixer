using HDRFixer.Core.Display;
using HDRFixer.Core.Registry;

namespace HDRFixer.Core.Fixes;

public class SdrBrightnessFix : IFix
{
    private readonly DisplayInfo _display;
    private readonly IHdrRegistryManager _registry;
    private float _originalNits;

    public string Name => "SDR Brightness Optimization";
    public string Description => "Sets optimal SDR reference white level based on display capabilities";
    public FixCategory Category => FixCategory.SdrBrightness;
    public FixStatus Status { get; private set; } = new();

    public SdrBrightnessFix(DisplayInfo display, IHdrRegistryManager? registry = null)
    {
        _display = display;
        _registry = registry ?? new HdrRegistryManager();
    }

    public FixResult Apply()
    {
        try
        {
            _originalNits = _display.SdrWhiteLevelNits;
            float optimal = CalculateOptimalWhiteLevel(_display);

            var monitorIds = _registry.GetMonitorIds();
            // Match the display using MonitorDevicePath
            string? monitorId = monitorIds.FirstOrDefault(id =>
                !string.IsNullOrEmpty(_display.MonitorDevicePath) &&
                _display.MonitorDevicePath.Contains(id, StringComparison.OrdinalIgnoreCase));

            // Fallback to first if only one display detected overall
            if (monitorId == null && monitorIds.Count == 1) monitorId = monitorIds[0];

            if (monitorId != null)
            {
                _registry.SetSdrWhiteLevel(monitorId, optimal);
                Status = new FixStatus
                {
                    State = FixState.Applied,
                    Message = $"SDR white level set to {optimal:F0} nits for {monitorId} (was: {_display.SdrWhiteLevelNits:F0} nits)"
                };
            }
            else
            {
                Status = new FixStatus { State = FixState.Error, Message = "Could not find monitor in registry" };
            }

            return new FixResult { Success = Status.State == FixState.Applied, Message = Status.Message };
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
