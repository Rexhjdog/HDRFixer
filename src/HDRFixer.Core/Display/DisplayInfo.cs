namespace HDRFixer.Core.Display;

public enum GpuVendor
{
    Unknown = 0,
    Nvidia = 0x10DE,
    Amd = 0x1002,
    Intel = 0x8086
}

public class DisplayInfo
{
    public string DeviceName { get; set; } = string.Empty;
    public string MonitorName { get; set; } = string.Empty;
    public string GpuName { get; set; } = string.Empty;
    public GpuVendor GpuVendor { get; set; } = GpuVendor.Unknown;
    public bool IsHdrEnabled { get; set; }
    public uint BitsPerColor { get; set; }
    public float MinLuminance { get; set; }
    public float MaxLuminance { get; set; }
    public float MaxFullFrameLuminance { get; set; }
    public float SdrWhiteLevelNits { get; set; }
    public (float X, float Y) RedPrimary { get; set; }
    public (float X, float Y) GreenPrimary { get; set; }
    public (float X, float Y) BluePrimary { get; set; }
    public (float X, float Y) WhitePoint { get; set; }
    public bool IsHdrCapable => MaxLuminance > 250f;
    public uint AdapterLuidLow { get; set; }
    public int AdapterLuidHigh { get; set; }
    public uint SourceId { get; set; }
    public uint TargetId { get; set; }
}
