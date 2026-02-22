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

    public static double[] GenerateHdrSrgbToGamma22Lut(int lutSize = 4096, double whiteLevelNits = 200.0)
    {
        var lut = new double[lutSize];
        for (int i = 0; i < lutSize; i++)
        {
            double pqInput = (double)i / (lutSize - 1);
            double nits = TransferFunctions.PqEotf(pqInput);

            if (nits <= 0.0001) { lut[i] = 0; continue; }
            if (nits > whiteLevelNits) { lut[i] = pqInput; continue; }

            // Map piecewise sRGB back to linear, then apply Gamma 2.2
            // Windows HDR maps SDR content such that 'srgb 1.0' = 'whiteLevelNits'
            double relativeL = nits / whiteLevelNits;
            double srgbSignal = TransferFunctions.SrgbInvEotf(relativeL);
            double gamma22L = Math.Pow(srgbSignal, 2.2);
            double correctedNits = gamma22L * whiteLevelNits;

            lut[i] = TransferFunctions.PqInvEotf(correctedNits);
        }
        return lut;
    }
}
