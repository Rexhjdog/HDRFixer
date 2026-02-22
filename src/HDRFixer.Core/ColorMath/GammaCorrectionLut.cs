namespace HDRFixer.Core.ColorMath;

public static class GammaCorrectionLut
{
    public static double[] GenerateSrgbToGamma22Lut(int lutSize = 1024)
    {
        var lut = new double[lutSize];
        for (int i = 0; i < lutSize; i++)
        {
            double input = (double)i / (lutSize - 1);
            double linear = TransferFunctions.SrgbEotf(input);
            lut[i] = TransferFunctions.GammaInvEotf(linear, 2.2);
        }
        return lut;
    }

    public static double[] GenerateHdrSrgbToGamma22Lut(int lutSize = 4096, double whiteLevelNits = 200.0, double blackLevelNits = 0.0)
    {
        var lut = new double[lutSize];
        for (int i = 0; i < lutSize; i++)
        {
            double pqInput = (double)i / (lutSize - 1);
            double nits = TransferFunctions.PqEotf(pqInput);
            if (nits > whiteLevelNits) { lut[i] = pqInput; continue; }
            double normalizedL = nits / whiteLevelNits;
            double srgbSignal = TransferFunctions.SrgbInvEotf(normalizedL);
            double gamma22Nits = (whiteLevelNits - blackLevelNits) * Math.Pow(srgbSignal, 2.2) + blackLevelNits;
            lut[i] = TransferFunctions.PqInvEotf(gamma22Nits);
        }
        return lut;
    }
}
