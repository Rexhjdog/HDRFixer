using HDRFixer.Core.ColorMath;
using HDRFixer.Core.ColorProfile;
using HDRFixer.Core.Display;

namespace HDRFixer.Core.Fixes;

public class GammaCorrectionFix : IFix
{
    private readonly ColorProfileInstaller _installer;
    private readonly DisplayInfo _display;
    private const string ProfileName = "HDRFixer_Gamma22.icm";

    public string Name => "SDR Tone Curve Correction";
    public string Description => "Replaces piecewise sRGB with gamma 2.2 for correct SDR rendering in HDR mode";
    public FixCategory Category => FixCategory.ToneCurve;
    public FixStatus Status { get; private set; } = new();

    public GammaCorrectionFix(ColorProfileInstaller installer, DisplayInfo display)
    { _installer = installer; _display = display; }

    public FixResult Apply()
    {
        try
        {
            double whiteLevelNits = _display.SdrWhiteLevelNits > 0 ? _display.SdrWhiteLevelNits : 200.0;
            var lut1d = GammaCorrectionLut.GenerateHdrSrgbToGamma22Lut(4096, whiteLevelNits);
            var regammaLut = new double[3, 4096];
            for (int ch = 0; ch < 3; ch++)
                for (int i = 0; i < 4096; i++)
                    regammaLut[ch, i] = lut1d[i];
            var identityMatrix = new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 } };
            byte[] profileData = Mhc2ProfileWriter.CreateProfile(
                "HDRFixer Gamma 2.2 Correction", _display.MaxLuminance, _display.MinLuminance, identityMatrix, regammaLut);
            string tempPath = Path.Combine(Path.GetTempPath(), ProfileName);
            File.WriteAllBytes(tempPath, profileData);
            _installer.InstallAndAssociate(tempPath, _display);
            Status = new FixStatus { State = FixState.Applied, Message = "Gamma 2.2 profile installed" };
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
        try
        {
            _installer.Uninstall(ProfileName, _display);
            Status = new FixStatus { State = FixState.NotApplied, Message = "Profile removed" };
            return new FixResult { Success = true, Message = "Gamma correction reverted" };
        }
        catch (Exception ex) { return new FixResult { Success = false, Message = ex.Message }; }
    }

    public FixStatus Diagnose()
    {
        Status = _installer.IsProfileInstalled(ProfileName)
            ? new FixStatus { State = FixState.Applied }
            : new FixStatus { State = FixState.NotApplied };
        return Status;
    }
}
