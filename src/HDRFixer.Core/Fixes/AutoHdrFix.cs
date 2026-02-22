using HDRFixer.Core.Registry;

namespace HDRFixer.Core.Fixes;

public class AutoHdrFix : IFix
{
    private readonly HdrRegistryManager _registry;

    public string Name => "Auto HDR Configuration";
    public string Description => "Enables and configures Auto HDR for optimal HDR rendering of SDR games";
    public FixCategory Category => FixCategory.AutoHdr;
    public FixStatus Status { get; private set; } = new();

    public AutoHdrFix(HdrRegistryManager? registry = null)
    {
        _registry = registry ?? new HdrRegistryManager();
    }

    public Task<FixResult> ApplyAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                _registry.SetAutoHdrEnabled(true);
                Status = new FixStatus { State = FixState.Applied, Message = "Auto HDR enabled" };
                return new FixResult { Success = true, Message = Status.Message };
            }
            catch (Exception ex)
            {
                Status = new FixStatus { State = FixState.Error, Message = ex.Message };
                return new FixResult { Success = false, Message = ex.Message };
            }
        });
    }

    public Task<FixResult> RevertAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                _registry.SetAutoHdrEnabled(false);
                Status = new FixStatus { State = FixState.NotApplied, Message = "Auto HDR disabled" };
                return new FixResult { Success = true, Message = Status.Message };
            }
            catch (Exception ex)
            {
                return new FixResult { Success = false, Message = ex.Message };
            }
        });
    }

    public Task<FixStatus> DiagnoseAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                bool enabled = _registry.IsAutoHdrEnabled();
                Status = enabled
                    ? new FixStatus { State = FixState.Applied, Message = "Auto HDR is enabled" }
                    : new FixStatus { State = FixState.NotApplied, Message = "Auto HDR is disabled" };
            }
            catch (Exception ex)
            {
                Status = new FixStatus { State = FixState.Error, Message = ex.Message };
            }
            return Status;
        });
    }
}
