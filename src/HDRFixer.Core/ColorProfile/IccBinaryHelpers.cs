using System.Buffers.Binary;
using System.Text;

namespace HDRFixer.Core.ColorProfile;

public static class IccBinaryHelpers
{
    public static int ToS15F16(double v) => (int)Math.Round(v * 65536.0);

    public static void WriteBE32(BinaryWriter w, int v) => w.Write(BinaryPrimitives.ReverseEndianness(v));
    public static void WriteBE32(BinaryWriter w, uint v) => w.Write(BinaryPrimitives.ReverseEndianness(v));
    public static void WriteBE16(BinaryWriter w, ushort v) => w.Write(BinaryPrimitives.ReverseEndianness(v));
    public static void WriteTag(BinaryWriter w, string fourCC) => w.Write(Encoding.ASCII.GetBytes(fourCC));

    public static byte[] BuildTag(string signature, Action<BinaryWriter> writeBody)
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        WriteTag(w, signature);
        WriteBE32(w, 0);
        writeBody(w);
        return ms.ToArray();
    }

    public static byte[] BuildXyzTag(double x, double y, double z)
    {
        return BuildTag("XYZ ", w => {
            WriteBE32(w, ToS15F16(x)); WriteBE32(w, ToS15F16(y)); WriteBE32(w, ToS15F16(z));
        });
    }

    public static byte[] BuildCurvTag(double gamma)
    {
        return BuildTag("curv", w => {
            WriteBE32(w, 1);
            WriteBE16(w, (ushort)Math.Round(gamma * 256.0));
            WriteBE16(w, 0);
        });
    }

    public static byte[] BuildMlucTag(string text)
    {
        return BuildTag("mluc", w => {
            WriteBE32(w, 1); WriteBE32(w, 12);
            WriteBE16(w, (ushort)'e'); WriteBE16(w, (ushort)'n');
            WriteBE16(w, (ushort)'U'); WriteBE16(w, (ushort)'S');
            var encoded = Encoding.BigEndianUnicode.GetBytes(text);
            WriteBE32(w, encoded.Length); WriteBE32(w, 28);
            w.Write(encoded);
            while (w.BaseStream.Length % 4 != 0) w.Write((byte)0);
        });
    }
}
