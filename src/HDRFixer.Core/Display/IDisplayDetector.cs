namespace HDRFixer.Core.Display;

public interface IDisplayDetector : IDisposable
{
    List<DisplayInfo> DetectDisplays();
}
