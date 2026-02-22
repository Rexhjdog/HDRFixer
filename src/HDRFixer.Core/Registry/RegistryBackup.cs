using System.Text.Json;
using Microsoft.Win32;

namespace HDRFixer.Core.Registry;

public class RegistryBackupEntry
{
    public string KeyPath { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public RegistryValueKind ValueKind { get; set; }
    public string? OriginalValue { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class RegistryBackupSet
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<RegistryBackupEntry> Entries { get; set; } = new();
}

public class RegistryBackupManager
{
    private readonly string _backupDir;

    public RegistryBackupManager(string? backupDir = null)
    {
        _backupDir = backupDir ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HDRFixer", "backups");
        Directory.CreateDirectory(_backupDir);
    }

    public string SaveBackup(RegistryBackupSet backupSet)
    {
        // Sanitize the name to prevent path traversal
        string sanitizedName = backupSet.Name;
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            sanitizedName = sanitizedName.Replace(c, '_');
        }
        sanitizedName = sanitizedName.Replace('/', '_').Replace('\\', '_');

        string filename = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}_{sanitizedName}.json";
        string path = Path.GetFullPath(Path.Combine(_backupDir, filename));

        // Ensure the path is still within the backup directory
        string fullBackupDir = Path.GetFullPath(_backupDir);
        if (!fullBackupDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            fullBackupDir += Path.DirectorySeparatorChar;
        }

        if (!path.StartsWith(fullBackupDir, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Attempted to save backup outside of the designated backup directory.");
        }

        string json = JsonSerializer.Serialize(backupSet, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
        return path;
    }

    public RegistryBackupSet? LoadBackup(string path)
    {
        if (!File.Exists(path)) return null;

        // Ensure we are only loading from the backup directory
        string fullPath = Path.GetFullPath(path);
        string fullBackupDir = Path.GetFullPath(_backupDir);
        if (!fullBackupDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            fullBackupDir += Path.DirectorySeparatorChar;
        }

        if (!fullPath.StartsWith(fullBackupDir, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Attempted to load backup from outside the designated backup directory.");
        }

        return JsonSerializer.Deserialize<RegistryBackupSet>(File.ReadAllText(path));
    }

    public List<string> ListBackups()
    {
        return Directory.GetFiles(_backupDir, "*.json").OrderByDescending(f => f).ToList();
    }
}
