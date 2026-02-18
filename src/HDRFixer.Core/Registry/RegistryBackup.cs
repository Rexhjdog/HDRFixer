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
        string filename = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}_{backupSet.Name.Replace(' ', '_')}.json";
        string path = Path.Combine(_backupDir, filename);
        string json = JsonSerializer.Serialize(backupSet, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
        return path;
    }

    public RegistryBackupSet? LoadBackup(string path)
    {
        if (!File.Exists(path)) return null;
        return JsonSerializer.Deserialize<RegistryBackupSet>(File.ReadAllText(path));
    }

    public List<string> ListBackups()
    {
        return Directory.GetFiles(_backupDir, "*.json").OrderByDescending(f => f).ToList();
    }
}
