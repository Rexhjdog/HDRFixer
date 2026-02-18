using HDRFixer.Core.ColorProfile;
using Xunit;

namespace HDRFixer.Core.Tests.ColorProfile;

public class Mhc2ProfileWriterTests
{
    [Fact]
    public void S15Fixed16_EncodesCorrectly()
    {
        Assert.Equal(65536, IccBinaryHelpers.ToS15F16(1.0));
        Assert.Equal(0, IccBinaryHelpers.ToS15F16(0.0));
        Assert.Equal(32768, IccBinaryHelpers.ToS15F16(0.5));
        Assert.Equal(-65536, IccBinaryHelpers.ToS15F16(-1.0));
    }

    private (double[,] matrix, double[,] lut) CreateTestData()
    {
        var lut = new double[3, 256];
        for (int ch = 0; ch < 3; ch++)
            for (int i = 0; i < 256; i++)
                lut[ch, i] = (double)i / 255;
        var matrix = new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 } };
        return (matrix, lut);
    }

    [Fact]
    public void CreateProfile_GeneratesValidIccHeader()
    {
        var (matrix, lut) = CreateTestData();
        byte[] profile = Mhc2ProfileWriter.CreateProfile("Test", 1000, 0.001, matrix, lut);
        Assert.True(profile.Length > 128);
        int size = (profile[0] << 24) | (profile[1] << 16) | (profile[2] << 8) | profile[3];
        Assert.Equal(profile.Length, size);
        Assert.Equal("acsp", System.Text.Encoding.ASCII.GetString(profile, 36, 4));
        Assert.Equal("mntr", System.Text.Encoding.ASCII.GetString(profile, 12, 4));
        Assert.Equal("RGB ", System.Text.Encoding.ASCII.GetString(profile, 16, 4));
    }

    [Fact]
    public void CreateProfile_ContainsMhc2Tag()
    {
        var (matrix, lut) = CreateTestData();
        byte[] profile = Mhc2ProfileWriter.CreateProfile("Test MHC2", 800, 0.001, matrix, lut);
        int tagCount = (profile[128] << 24) | (profile[129] << 16) | (profile[130] << 8) | profile[131];
        bool found = false;
        for (int i = 0; i < tagCount; i++)
        {
            string sig = System.Text.Encoding.ASCII.GetString(profile, 132 + i * 12, 4);
            if (sig == "MHC2") { found = true; break; }
        }
        Assert.True(found, "Profile must contain MHC2 tag");
    }

    [Fact]
    public void CreateProfile_CanBeSavedToFile()
    {
        var (matrix, lut) = CreateTestData();
        byte[] profile = Mhc2ProfileWriter.CreateProfile("File Test", 1000, 0, matrix, lut);
        string tempPath = Path.Combine(Path.GetTempPath(), "test_mhc2_" + Guid.NewGuid() + ".icm");
        try
        {
            File.WriteAllBytes(tempPath, profile);
            Assert.True(File.Exists(tempPath));
            Assert.Equal(profile.Length, new FileInfo(tempPath).Length);
        }
        finally { File.Delete(tempPath); }
    }
}
