#pragma once
#include <string>
#include <vector>
#include <filesystem>
#include <chrono>
#include <expected>
#include <windows.h>

namespace hdrfixer::registry {

struct BackupEntry {
    std::wstring key_path;
    std::wstring value_name;
    DWORD value_kind = 0;
    std::wstring original_value;
};

struct BackupSet {
    std::string name;
    std::chrono::system_clock::time_point created_at;
    std::vector<BackupEntry> entries;
};

class RegistryBackupManager {
public:
    RegistryBackupManager();
    std::expected<void, std::string> save_backup(const BackupSet& backup);
    std::expected<BackupSet, std::string> load_backup(const std::string& name);
    std::vector<std::string> list_backups() const;
private:
    std::filesystem::path backup_dir_;
};

} // namespace hdrfixer::registry
