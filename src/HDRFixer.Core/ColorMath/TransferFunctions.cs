namespace HDRFixer.Core.ColorMath;

public static class TransferFunctions
{
    private const double SrgbLinearThreshold = 0.04045;
    private const double SrgbLinearScale = 12.92;
    private const double SrgbGammaOffset = 0.055;
    private const double SrgbGammaBase = 1.055;
    private const double SrgbGammaExponent = 2.4;
    private const double PqM1 = 2610.0 / 16384.0;
    private const double PqM2 = 128.0 * 2523.0 / 4096.0;
    private const double PqC1 = 3424.0 / 4096.0;
    private const double PqC2 = 32.0 * 2413.0 / 4096.0;
    private const double PqC3 = 32.0 * 2392.0 / 4096.0;

    public static double SrgbEotf(double v)
    {
        if (v <= SrgbLinearThreshold) return v / SrgbLinearScale;
        return Math.Pow((v + SrgbGammaOffset) / SrgbGammaBase, SrgbGammaExponent);
    }

    public static double SrgbInvEotf(double l)
    {
        if (l <= 0.0031308) return l * SrgbLinearScale;
        return SrgbGammaBase * Math.Pow(l, 1.0 / SrgbGammaExponent) - SrgbGammaOffset;
    }

    public static double GammaEotf(double v, double gamma) => Math.Pow(v, gamma);
    public static double GammaInvEotf(double l, double gamma) => Math.Pow(l, 1.0 / gamma);

    public static double PqEotf(double v)
    {
        double vp = Math.Pow(v, 1.0 / PqM2);
        double num = Math.Max(vp - PqC1, 0.0);
        double den = PqC2 - PqC3 * vp;
        return 10000.0 * Math.Pow(num / den, 1.0 / PqM1);
    }

    public static double PqInvEotf(double nits)
    {
        double y = Math.Pow(nits / 10000.0, PqM1);
        return Math.Pow((PqC1 + PqC2 * y) / (1.0 + PqC3 * y), PqM2);
    }
}
