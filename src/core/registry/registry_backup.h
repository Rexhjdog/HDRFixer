#pragma once
#include <string>
#include <vector>

namespace hdrfixer {
namespace registry {

// Helper to parse CSV/pipe-separated values, handling escapes
std::vector<std::wstring> parse_csv_line(const std::wstring& line);

class RegistryBackup {
public:
    void backup_key(const std::wstring& key_path);
    void restore_backup(const std::wstring& backup_name);
    std::vector<std::wstring> list_backups();
};

} // namespace registry
} // namespace hdrfixer
