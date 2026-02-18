namespace HDRFixer.Core.Fixes;

public enum FixCategory { ToneCurve, SdrBrightness, PixelFormat, AutoHdr, IccConflict, EdidValidation, OledProtection }
public enum FixState { NotApplied, Applied, Error, NotNeeded }

public class FixStatus
{
    public FixState State { get; set; } = FixState.NotApplied;
    public string Message { get; set; } = string.Empty;
}

public class FixResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public interface IFix
{
    string Name { get; }
    string Description { get; }
    FixCategory Category { get; }
    FixStatus Status { get; }
    FixResult Apply();
    FixResult Revert();
    FixStatus Diagnose();
}
