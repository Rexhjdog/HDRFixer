using System;
using System.IO;
using System.Text.Json;
using HDRFixer.Core.Registry;
using Xunit;

namespace HDRFixer.Core.Tests.Registry;

public class SecurityTests
{
    [Fact]
    public void SaveBackup_PathTraversal_IsMitigated()
    {
        string tempBase = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "HDRFixerTests_" + Guid.NewGuid()));
        string backupDir = Path.Combine(tempBase, "backups");
        Directory.CreateDirectory(backupDir);

        var manager = new RegistryBackupManager(backupDir);

        var backupSet = new RegistryBackupSet
        {
            Name = "../traversed"
        };

        string savedPath = manager.SaveBackup(backupSet);
        string fullSavedPath = Path.GetFullPath(savedPath);
        string fullBackupDir = Path.GetFullPath(backupDir);
        if (!fullBackupDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
            fullBackupDir += Path.DirectorySeparatorChar;

        try
        {
            // The fix should have kept it inside backupDir
            Assert.StartsWith(fullBackupDir, fullSavedPath, StringComparison.OrdinalIgnoreCase);
            // '../' should have become '.._'
            Assert.Contains(".._traversed", Path.GetFileName(fullSavedPath));
        }
        finally
        {
            if (Directory.Exists(tempBase))
            {
                Directory.Delete(tempBase, true);
            }
        }
    }

    [Fact]
    public void LoadBackup_OutsidePath_ThrowsException()
    {
        string tempBase = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "HDRFixerTests_" + Guid.NewGuid()));
        string backupDir = Path.Combine(tempBase, "backups");
        Directory.CreateDirectory(backupDir);

        string outsideFile = Path.Combine(tempBase, "outside.json");
        File.WriteAllText(outsideFile, "{}");

        var manager = new RegistryBackupManager(backupDir);

        try
        {
            Assert.Throws<UnauthorizedAccessException>(() => manager.LoadBackup(outsideFile));
        }
        finally
        {
            if (Directory.Exists(tempBase))
            {
                Directory.Delete(tempBase, true);
            }
        }
    }
}
