using HDRFixer.Core.Display;
using Xunit;

namespace HDRFixer.Core.Tests.Display;

public class EdidReaderTests
{
    [Fact]
    public void ParseManufacturer_DecodesCompressedAscii()
    {
        byte[] edid = new byte[128];
        ushort encoded = (ushort)(((('D' - 'A' + 1) & 0x1F) << 10) |
                                  ((('E' - 'A' + 1) & 0x1F) << 5) |
                                  (('L' - 'A' + 1) & 0x1F));
        edid[8] = (byte)(encoded >> 8);
        edid[9] = (byte)(encoded & 0xFF);
        Assert.Equal("DEL", EdidParser.ParseManufacturerId(edid));
    }

    [Fact]
    public void ParseMonitorName_ExtractsFromDescriptor()
    {
        byte[] edid = new byte[128];
        edid[54] = 0; edid[55] = 0; edid[56] = 0; edid[57] = 0xFC; edid[58] = 0;
        byte[] name = System.Text.Encoding.ASCII.GetBytes("LG OLED48C2\n ");
        Array.Copy(name, 0, edid, 59, name.Length);
        Assert.Equal("LG OLED48C2", EdidParser.ParseMonitorName(edid));
    }

    [Fact]
    public void ParseMonitorName_ReturnsNull_WhenNoDescriptor()
    {
        Assert.Null(EdidParser.ParseMonitorName(new byte[128]));
    }

    [Fact]
    public void ParseEdid_RejectsShortData()
    {
        Assert.Throws<ArgumentException>(() => EdidParser.ParseManufacturerId(new byte[64]));
    }
}
