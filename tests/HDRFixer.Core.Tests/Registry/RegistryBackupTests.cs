using System.Text.Json;
using HDRFixer.Core.Registry;
using Xunit;

namespace HDRFixer.Core.Tests.Registry;

public class RegistryBackupTests
{
    [Fact]
    public void BackupEntry_SerializesToJson()
    {
        var entry = new RegistryBackupEntry
        {
            KeyPath = @"HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
            ValueName = "AutoHDREnabled",
            ValueKind = Microsoft.Win32.RegistryValueKind.DWord,
            OriginalValue = "1",
            Timestamp = new DateTime(2026, 2, 18, 12, 0, 0, DateTimeKind.Utc)
        };
        string json = JsonSerializer.Serialize(entry);
        var deserialized = JsonSerializer.Deserialize<RegistryBackupEntry>(json);
        Assert.NotNull(deserialized);
        Assert.Equal(entry.KeyPath, deserialized!.KeyPath);
        Assert.Equal(entry.OriginalValue, deserialized.OriginalValue);
    }

    [Fact]
    public void BackupSet_TracksMultipleEntries()
    {
        var backupSet = new RegistryBackupSet { Name = "AutoHDR Fix" };
        backupSet.Entries.Add(new RegistryBackupEntry
        {
            KeyPath = "test\\path", ValueName = "TestValue",
            ValueKind = Microsoft.Win32.RegistryValueKind.DWord, OriginalValue = "0"
        });
        Assert.Single(backupSet.Entries);
        Assert.Equal("AutoHDR Fix", backupSet.Name);
    }

    [Fact]
    public void SettingsManager_SavesAndLoads()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "hdrfixer_backup_test_" + Guid.NewGuid());
        try
        {
            var manager = new RegistryBackupManager(tempDir);
            var backupSet = new RegistryBackupSet { Name = "TestBackup" };
            backupSet.Entries.Add(new RegistryBackupEntry { KeyPath = "test", ValueName = "val" });
            string path = manager.SaveBackup(backupSet);
            var loaded = manager.LoadBackup(path);
            Assert.NotNull(loaded);
            Assert.Single(loaded!.Entries);
        }
        finally { Directory.Delete(tempDir, recursive: true); }
    }
}
