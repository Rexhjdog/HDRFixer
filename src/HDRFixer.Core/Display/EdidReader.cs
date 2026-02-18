using System.Management;
using System.Text;

namespace HDRFixer.Core.Display;

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
            using var mc = new ManagementClass(@"\\.\root\wmi:WmiMonitorDescriptorMethods");
            foreach (ManagementObject mo in mc.GetInstances())
            {
                string instanceName = mo["InstanceName"]?.ToString() ?? "Unknown";
                var edidBytes = new List<byte>();
                for (int blockId = 0; blockId < 256; blockId++)
                {
                    try
                    {
                        var inParams = mo.GetMethodParameters("WmiGetMonitorRawEEdidV1Block");
                        inParams["BlockId"] = blockId;
                        var outParams = mo.InvokeMethod("WmiGetMonitorRawEEdidV1Block", inParams, null);
                        byte[] block = (byte[])outParams["BlockContent"];
                        edidBytes.AddRange(block);
                    }
                    catch { break; }
                }
                if (edidBytes.Count >= 128)
                    results.Add((instanceName, edidBytes.ToArray()));
            }
        }
        catch { /* WMI not available */ }
        return results;
    }
}
