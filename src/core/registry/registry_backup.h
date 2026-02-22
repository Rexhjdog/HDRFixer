#pragma once
#include <string>
#include <vector>
#include <filesystem>
#include <chrono>
#include <expected>

#ifdef _WIN32
#include <windows.h>
#else
// Mock for Linux testing
using DWORD = unsigned long;
#endif

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
    explicit RegistryBackupManager(std::filesystem::path backup_dir);
    std::expected<void, std::string> save_backup(const BackupSet& backup);
    std::expected<BackupSet, std::string> load_backup(const std::string& name);
    std::vector<std::string> list_backups() const;
private:
    std::filesystem::path backup_dir_;
};

} // namespace hdrfixer::registry
