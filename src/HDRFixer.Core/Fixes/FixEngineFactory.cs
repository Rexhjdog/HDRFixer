using HDRFixer.Core.ColorProfile;
using HDRFixer.Core.Display;

namespace HDRFixer.Core.Fixes;

public static class FixEngineFactory
{
    public static FixEngine Create(IDisplayDetector? detector = null)
    {
        var engine = new FixEngine();
        detector ??= new DxgiDisplayDetector();
        
        List<DisplayInfo> displays;
        try { displays = detector.DetectDisplays(); }
        catch { displays = new List<DisplayInfo>(); }
        
        var primaryDisplay = displays.FirstOrDefault() ?? new DisplayInfo();
        var installer = new ColorProfileInstaller();
        
        engine.Register(new GammaCorrectionFix(installer, primaryDisplay));
        engine.Register(new SdrBrightnessFix(primaryDisplay));
        engine.Register(new AutoHdrFix());
        engine.Register(new PixelFormatFix(primaryDisplay));
        
        return engine;
    }
}
