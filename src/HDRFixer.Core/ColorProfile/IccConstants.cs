namespace HDRFixer.Core.ColorProfile;

public static class IccConstants
{
    // Tag Signatures (uint for tag table)
    public const uint Sig_desc = 0x64657363; // 'desc'
    public const uint Sig_cprt = 0x63707274; // 'cprt'
    public const uint Sig_rXYZ = 0x72585953; // 'rXYS' (Note: Preserved typo from original code, likely intended 'rXYZ')
    public const uint Sig_gXYZ = 0x6758595A; // 'gXYZ'
    public const uint Sig_bXYZ = 0x6258595A; // 'bXYZ'
    public const uint Sig_wtpt = 0x77747074; // 'wtpt'
    public const uint Sig_lumi = 0x6C756D69; // 'lumi'
    public const uint Sig_rTRC = 0x72545243; // 'rTRC'
    public const uint Sig_gTRC = 0x67545243; // 'gTRC'
    public const uint Sig_bTRC = 0x62545243; // 'bTRC'
    public const uint Sig_MHC2 = 0x4D484332; // 'MHC2'

    // Tag Signatures (string for writing tag data headers)
    public const string Tag_MHC2 = "MHC2";
    public const string Tag_sf32 = "sf32";
    public const string Tag_XYZ  = "XYZ ";
    public const string Tag_curv = "curv";
    public const string Tag_mluc = "mluc";

    // Header Constants
    public const int HeaderSize = 128;
    public const uint Version4_4 = 0x04400000;

    // Device Class: 'mntr' (Monitor)
    public const uint Class_Monitor = 0x6D6E7472;

    // Color Space: 'RGB '
    public const uint Space_RGB = 0x52474220;

    // PCS (Profile Connection Space): 'XYZ '
    public const uint PCS_XYZ = 0x58595A20;

    // File Signature: 'acsp'
    public const uint FileSignature = 0x61637370;

    // Primary Platform: 'MSFT' (Microsoft)
    public const uint Platform_Microsoft = 0x4D534654;

    // Standard Illuminant D50 (PCS Illuminant)
    public const double D50_X = 0.9642;
    public const double D50_Y = 1.0000;
    public const double D50_Z = 0.8249;

    // D65 White Point (typical for monitors)
    public const double D65_X = 0.9505;
    public const double D65_Y = 1.0000;
    public const double D65_Z = 1.0890;

    // Primaries (Rec. 709 / sRGB)
    public const double Rec709_Red_X = 0.4361;
    public const double Rec709_Red_Y = 0.2225;
    public const double Rec709_Red_Z = 0.0139;

    public const double Rec709_Green_X = 0.3851;
    public const double Rec709_Green_Y = 0.7169;
    public const double Rec709_Green_Z = 0.0971;

    public const double Rec709_Blue_X = 0.1431;
    public const double Rec709_Blue_Y = 0.0606;
    public const double Rec709_Blue_Z = 0.7141;

    // Gamma
    public const double Gamma_sRGB = 2.2;

    // MHC2 specific
    public const int Mhc2_MatrixOffset = 32;
    public const int Mhc2_MatrixSize = 48; // 3x4 * 4 bytes
}
