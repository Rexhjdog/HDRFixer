using System.Runtime.InteropServices;

namespace HDRFixer.Core.ColorProfile;

internal static class WcsNativeMethods
{
    public enum WCS_PROFILE_MANAGEMENT_SCOPE { SYSTEM_WIDE = 0, CURRENT_USER = 1 }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID { public uint LowPart; public int HighPart; }

    [DllImport("Mscms.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool InstallColorProfileW(string? pMachineName, string pProfileName);

    [DllImport("Mscms.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UninstallColorProfileW(string? pMachineName, string pProfileName, [MarshalAs(UnmanagedType.Bool)] bool bDelete);

    [DllImport("Mscms.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WcsAssociateColorProfileWithDevice(WCS_PROFILE_MANAGEMENT_SCOPE scope, string pProfileName, string pDeviceName);

    [DllImport("Mscms.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WcsDisassociateColorProfileFromDevice(WCS_PROFILE_MANAGEMENT_SCOPE scope, string pProfileName, string pDeviceName);

    [DllImport("Mscms.dll", CharSet = CharSet.Unicode)]
    public static extern int ColorProfileAddDisplayAssociation(WCS_PROFILE_MANAGEMENT_SCOPE scope, string profileName, LUID targetAdapterID, uint sourceID, [MarshalAs(UnmanagedType.Bool)] bool setAsDefault, [MarshalAs(UnmanagedType.Bool)] bool associateAsAdvancedColor);

    [DllImport("Mscms.dll", CharSet = CharSet.Unicode)]
    public static extern int ColorProfileRemoveDisplayAssociation(WCS_PROFILE_MANAGEMENT_SCOPE scope, string profileName, LUID targetAdapterID, uint sourceID, [MarshalAs(UnmanagedType.Bool)] bool dissociateAdvancedColor);
}
