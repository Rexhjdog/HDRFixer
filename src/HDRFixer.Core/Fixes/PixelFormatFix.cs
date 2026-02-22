using HDRFixer.Core.Display;

namespace HDRFixer.Core.Fixes;

public class PixelFormatFix : IFix
{
    private readonly DisplayInfo _display;

    public string Name => "Pixel Format Verification";
    public string Description => "Ensures the GPU is outputting 10-bit RGB full range for maximum HDR quality";
    public FixCategory Category => FixCategory.PixelFormat;
    public FixStatus Status { get; private set; } = new();

    public PixelFormatFix(DisplayInfo display)
    {
        _display = display;
    }

    public async Task<FixResult> ApplyAsync()
    {
        // This fix is mostly informational/diagnostic as changing pixel format
        // usually requires GPU-specific APIs (NVAPI/ADL) or user intervention in GPU control panel.
        var status = await DiagnoseAsync();
        return new FixResult
        {
            Success = status.State == FixState.Applied || status.State == FixState.NotNeeded,
            Message = status.Message
        };
    }

    public Task<FixResult> RevertAsync()
    {
        return Task.FromResult(new FixResult { Success = true, Message = "Nothing to revert" });
    }

    public Task<FixStatus> DiagnoseAsync()
    {
        return Task.Run(() =>
        {
            if (!_display.IsHdrEnabled)
            {
                Status = new FixStatus { State = FixState.NotNeeded, Message = "HDR is not enabled" };
                return Status;
            }

            if (_display.BitsPerColor >= 10)
            {
                Status = new FixStatus { State = FixState.Applied, Message = $"Outputting {_display.BitsPerColor}-bit color (Optimal)" };
            }
            else
            {
                Status = new FixStatus { State = FixState.Error, Message = $"Outputting only {_display.BitsPerColor}-bit color. 10-bit or higher is recommended for HDR." };
            }
            return Status;
        });
    }
}
