using Microsoft.Win32;

namespace HDRFixer.Core.AutoHdr;

public class GameHdrProfile
{
    public string ExecutablePath { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool ForceAutoHdr { get; set; }
    public string ExecutableName => ExecutablePath.Contains('\\')
        ? ExecutablePath.Substring(ExecutablePath.LastIndexOf('\\') + 1)
        : Path.GetFileName(ExecutablePath);
}

public class AutoHdrGameManager
{
    private const string Direct3DKey = @"Software\Microsoft\Direct3D";

    public List<GameHdrProfile> GetForcedAutoHdrGames()
    {
        var games = new List<GameHdrProfile>();
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(Direct3DKey);
        if (key == null) return games;
        foreach (string valueName in key.GetValueNames())
        {
            if (valueName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                games.Add(new GameHdrProfile
                {
                    ExecutablePath = valueName,
                    ForceAutoHdr = true,
                    DisplayName = Path.GetFileNameWithoutExtension(valueName)
                });
            }
        }
        return games;
    }

    public void SetForceAutoHdr(string executablePath, bool force)
    {
        using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(Direct3DKey);
        if (force)
            key.SetValue(executablePath, 1, RegistryValueKind.DWord);
        else
            key.DeleteValue(executablePath, throwOnMissingValue: false);
    }
}
