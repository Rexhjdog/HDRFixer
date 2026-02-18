using System.Runtime.InteropServices;
using HDRFixer.Core.Display;

namespace HDRFixer.Core.ColorProfile;

public class ColorProfileInstaller
{
    private static readonly string ColorDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.System), "spool", "drivers", "color");

    public void InstallAndAssociate(string profilePath, DisplayInfo display)
    {
        if (!File.Exists(profilePath)) throw new FileNotFoundException("Profile not found", profilePath);
        if (!WcsNativeMethods.InstallColorProfileW(null, profilePath))
            throw new InvalidOperationException($"Failed to install color profile: {Marshal.GetLastWin32Error()}");

        string profileFileName = Path.GetFileName(profilePath);
        var luid = new WcsNativeMethods.LUID { LowPart = display.AdapterLuidLow, HighPart = display.AdapterLuidHigh };
        int hr = WcsNativeMethods.ColorProfileAddDisplayAssociation(
            WcsNativeMethods.WCS_PROFILE_MANAGEMENT_SCOPE.CURRENT_USER,
            profileFileName, luid, display.SourceId, setAsDefault: true, associateAsAdvancedColor: true);
        Marshal.ThrowExceptionForHR(hr);
    }

    public void Uninstall(string profileFileName, DisplayInfo display)
    {
        var luid = new WcsNativeMethods.LUID { LowPart = display.AdapterLuidLow, HighPart = display.AdapterLuidHigh };
        WcsNativeMethods.ColorProfileRemoveDisplayAssociation(
            WcsNativeMethods.WCS_PROFILE_MANAGEMENT_SCOPE.CURRENT_USER,
            profileFileName, luid, display.SourceId, dissociateAdvancedColor: true);
        WcsNativeMethods.UninstallColorProfileW(null, profileFileName, bDelete: true);
    }

    public bool IsProfileInstalled(string profileFileName) => File.Exists(Path.Combine(ColorDir, profileFileName));
}
