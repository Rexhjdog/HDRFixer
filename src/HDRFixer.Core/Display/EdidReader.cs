using System.Management;
using System.Text;

namespace HDRFixer.Core.Display;

public class HdrMetadata
{
    public bool Supported { get; set; }
    public float MaxLuminance { get; set; }
    public float MinLuminance { get; set; }
    public float MaxCll { get; set; }
    public float MaxFall { get; set; }
}

public static class EdidParser
{
    public static string ParseManufacturerId(byte[] edid)
    {
        if (edid.Length < 128)
            throw new ArgumentException("EDID data must be at least 128 bytes", nameof(edid));
        ushort mfg = (ushort)((edid[8] << 8) | edid[9]);
        char c1 = (char)(((mfg >> 10) & 0x1F) + 'A' - 1);
        char c2 = (char)(((mfg >> 5) & 0x1F) + 'A' - 1);
        char c3 = (char)((mfg & 0x1F) + 'A' - 1);
        return $"{c1}{c2}{c3}";
    }

    public static string? ParseMonitorName(byte[] edid)
    {
        if (edid.Length < 128) return null;
        for (int offset = 54; offset <= 108; offset += 18)
        {
            if (edid[offset] == 0 && edid[offset + 1] == 0 && edid[offset + 3] == 0xFC)
            {
                string name = Encoding.ASCII.GetString(edid, offset + 5, 13).Trim('\n', '\r', ' ', '\0');
                return string.IsNullOrEmpty(name) ? null : name;
            }
        }
        return null;
    }

    public static HdrMetadata ParseHdrMetadata(byte[] edid)
    {
        var metadata = new HdrMetadata();
        if (edid.Length < 256) return metadata; // Need at least one extension block

        int extBlocks = edid[126];
        for (int b = 1; b <= extBlocks; b++)
        {
            int blockOffset = b * 128;
            if (edid[blockOffset] != 0x02) continue; // Not a CEA block

            int dOffset = blockOffset + 4;
            int dEnd = blockOffset + edid[blockOffset + 2];

            while (dOffset < dEnd)
            {
                int tag = (edid[dOffset] & 0xE0) >> 5;
                int len = edid[dOffset] & 0x1F;

                if (tag == 0x07) // Extended Tag
                {
                    int extTag = edid[dOffset + 1];
                    if (extTag == 0x06) // HDR Static Metadata Data Block
                    {
                        metadata.Supported = true;
                        if (len >= 3)
                        {
                            // Simplified parsing of luminance data if present
                            // This varies by EDID version and spec
                        }
                    }
                }
                dOffset += 1 + len;
            }
        }
        return metadata;
    }

    public static ushort ParseProductCode(byte[] edid)
    {
        if (edid.Length < 128)
            throw new ArgumentException("EDID data must be at least 128 bytes", nameof(edid));
        return (ushort)(edid[10] | (edid[11] << 8));
    }
}

public class EdidWmiReader
{
    public List<(string InstanceName, byte[] EdidData)> ReadAllEdids()
    {
        var results = new List<(string, byte[])>();
        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT * FROM WmiMonitorID");
            foreach (ManagementObject mo in searcher.Get())
            {
                string instanceName = mo["InstanceName"]?.ToString() ?? "Unknown";
                if (mo["BaseEdidCode"] is byte[] edid)
                {
                    results.Add((instanceName, edid));
                }
            }
        }
        catch { /* WMI not available */ }
        return results;
    }
}
