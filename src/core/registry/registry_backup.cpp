#include "registry_backup.h"
#include <iostream>

namespace hdrfixer {
namespace registry {

std::vector<std::wstring> parse_csv_line(const std::wstring& line) {
    std::vector<std::wstring> parts;
    std::wstring current;
    for (size_t i = 0; i < line.size(); ++i) {
        if (line[i] == L'\\' && i + 1 < line.size()) {
            current += line[i];
            current += line[i + 1];
            ++i;
        } else if (line[i] == L'|') {
            parts.push_back(current);
            current.clear();
        } else {
            current += line[i];
        }
    }
    // Fix: Always push the last field, even if empty (unless we want to skip trailing empty fields, but standard CSV/split logic usually keeps them)
    // The issue description says "Trailing empty fields dropped", so we fix it by pushing.
    parts.push_back(current);

    return parts;
}

void RegistryBackup::backup_key(const std::wstring& key_path) {
    // Implementation stub
}

void RegistryBackup::restore_backup(const std::wstring& backup_name) {
    // Implementation stub
}

std::vector<std::wstring> RegistryBackup::list_backups() {
    // Implementation stub
    return {};
}

} // namespace registry
} // namespace hdrfixer
