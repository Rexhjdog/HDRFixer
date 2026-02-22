using HDRFixer.Core.ColorProfile;
using System.Text;
using Xunit;

namespace HDRFixer.Core.Tests.ColorProfile;

public class IccBinaryHelpersTests
{
    [Fact]
    public void ToS15F16_ConvertsCorrectly()
    {
        Assert.Equal(65536, IccBinaryHelpers.ToS15F16(1.0));
        Assert.Equal(0, IccBinaryHelpers.ToS15F16(0.0));
        Assert.Equal(32768, IccBinaryHelpers.ToS15F16(0.5));
        Assert.Equal(-65536, IccBinaryHelpers.ToS15F16(-1.0));
        Assert.Equal(131072, IccBinaryHelpers.ToS15F16(2.0));
    }

    [Fact]
    public void WriteBE32_Int_WritesBigEndian()
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        IccBinaryHelpers.WriteBE32(w, 0x12345678);
        var bytes = ms.ToArray();
        Assert.Equal(new byte[] { 0x12, 0x34, 0x56, 0x78 }, bytes);
    }

    [Fact]
    public void WriteBE32_UInt_WritesBigEndian()
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        IccBinaryHelpers.WriteBE32(w, 0x12345678U);
        var bytes = ms.ToArray();
        Assert.Equal(new byte[] { 0x12, 0x34, 0x56, 0x78 }, bytes);
    }

    [Fact]
    public void WriteBE16_UShort_WritesBigEndian()
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        IccBinaryHelpers.WriteBE16(w, 0x1234);
        var bytes = ms.ToArray();
        Assert.Equal(new byte[] { 0x12, 0x34 }, bytes);
    }

    [Fact]
    public void WriteTag_WritesFourCC()
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        IccBinaryHelpers.WriteTag(w, "TEST");
        var bytes = ms.ToArray();
        Assert.Equal(Encoding.ASCII.GetBytes("TEST"), bytes);
    }

    [Fact]
    public void BuildXyzTag_CreatesCorrectStructure()
    {
        var bytes = IccBinaryHelpers.BuildXyzTag(1.0, 0.5, 0.0);

        // Expected structure:
        // "XYZ " (4 bytes)
        // 0 (4 bytes)
        // X (4 bytes, S15.16)
        // Y (4 bytes, S15.16)
        // Z (4 bytes, S15.16)
        // Total 20 bytes

        Assert.Equal(20, bytes.Length);
        Assert.Equal("XYZ ", Encoding.ASCII.GetString(bytes, 0, 4));
        Assert.Equal(0, BitConverter.ToInt32(bytes, 4));

        int x = (bytes[8] << 24) | (bytes[9] << 16) | (bytes[10] << 8) | bytes[11];
        int y = (bytes[12] << 24) | (bytes[13] << 16) | (bytes[14] << 8) | bytes[15];
        int z = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];

        Assert.Equal(65536, x);
        Assert.Equal(32768, y);
        Assert.Equal(0, z);
    }

    [Fact]
    public void BuildCurvTag_CreatesCorrectStructure()
    {
        var bytes = IccBinaryHelpers.BuildCurvTag(2.2);

        // Expected structure:
        // "curv" (4 bytes)
        // 0 (4 bytes)
        // count = 1 (4 bytes)
        // gamma * 256 (2 bytes)
        // padding (2 bytes)
        // Total 16 bytes

        Assert.Equal(16, bytes.Length);
        Assert.Equal("curv", Encoding.ASCII.GetString(bytes, 0, 4));

        // Count
        int count = (bytes[8] << 24) | (bytes[9] << 16) | (bytes[10] << 8) | bytes[11];
        Assert.Equal(1, count);

        // Gamma
        ushort gamma = (ushort)((bytes[12] << 8) | bytes[13]);
        Assert.Equal((ushort)Math.Round(2.2 * 256.0), gamma);
    }

    [Fact]
    public void BuildMlucTag_CreatesCorrectStructure()
    {
        string text = "Test";
        var bytes = IccBinaryHelpers.BuildMlucTag(text);

        // Header: "mluc" (4), 0 (4), count=1 (4), recordSize=12 (4)
        Assert.Equal("mluc", Encoding.ASCII.GetString(bytes, 0, 4));

        int count = (bytes[8] << 24) | (bytes[9] << 16) | (bytes[10] << 8) | bytes[11];
        Assert.Equal(1, count);

        int recordSize = (bytes[12] << 24) | (bytes[13] << 16) | (bytes[14] << 8) | bytes[15];
        Assert.Equal(12, recordSize);

        // Record: 'en' (2), 'US' (2), len (4), offset (4)
        // "en" = 0x656E
        Assert.Equal(0x65, bytes[16]);
        Assert.Equal(0x6E, bytes[17]);

        // "US" = 0x5553
        Assert.Equal(0x55, bytes[18]);
        Assert.Equal(0x53, bytes[19]);

        // String length in bytes (UTF-16BE "Test" = 8 bytes)
        int strLen = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
        Assert.Equal(8, strLen);

        // Offset (28)
        int offset = (bytes[24] << 24) | (bytes[25] << 16) | (bytes[26] << 8) | bytes[27];
        Assert.Equal(28, offset);

        // Data Check
        byte[] expectedData = Encoding.BigEndianUnicode.GetBytes(text);
        byte[] actualData = new byte[expectedData.Length];
        Array.Copy(bytes, 28, actualData, 0, expectedData.Length);

        Assert.Equal(expectedData, actualData);
    }
}
