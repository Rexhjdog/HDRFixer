#include "core/registry/registry_backup.h"
#include <algorithm>
#include <fstream>
#include <sstream>
#include <format>
#include <shlobj.h>

namespace hdrfixer::registry {

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

// Convert wstring to UTF-8 string for file I/O
static std::string to_utf8(const std::wstring& wstr) {
    if (wstr.empty()) return {};
    int size = WideCharToMultiByte(CP_UTF8, 0, wstr.data(),
                                   static_cast<int>(wstr.size()),
                                   nullptr, 0, nullptr, nullptr);
    std::string result(size, '\0');
    WideCharToMultiByte(CP_UTF8, 0, wstr.data(),
                        static_cast<int>(wstr.size()),
                        result.data(), size, nullptr, nullptr);
    return result;
}

// Convert UTF-8 string to wstring
static std::wstring from_utf8(const std::string& str) {
    if (str.empty()) return {};
    int size = MultiByteToWideChar(CP_UTF8, 0, str.data(),
                                    static_cast<int>(str.size()),
                                    nullptr, 0);
    std::wstring result(size, L'\0');
    MultiByteToWideChar(CP_UTF8, 0, str.data(),
                         static_cast<int>(str.size()),
                         result.data(), size);
    return result;
}

// Escape pipe characters in a wstring for safe serialization
static std::wstring escape_pipes(const std::wstring& s) {
    std::wstring result;
    result.reserve(s.size());
    for (wchar_t c : s) {
        if (c == L'|') {
            result += L"\\|";
        } else if (c == L'\\') {
            result += L"\\\\";
        } else {
            result += c;
        }
    }
    return result;
}

// Unescape pipe characters
static std::wstring unescape_pipes(const std::wstring& s) {
    std::wstring result;
    result.reserve(s.size());
    for (size_t i = 0; i < s.size(); ++i) {
        if (s[i] == L'\\' && i + 1 < s.size()) {
            if (s[i + 1] == L'|') {
                result += L'|';
                ++i;
            } else if (s[i + 1] == L'\\') {
                result += L'\\';
                ++i;
            } else {
                result += s[i];     // keep the backslash
                result += s[i + 1]; // keep the following character
                ++i;
            }
        } else {
            result += s[i];
        }
    }
    return result;
}

// Split a line on unescaped pipe characters
static std::vector<std::wstring> split_on_pipe(const std::wstring& line) {
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
    if (!current.empty()) {
        parts.push_back(current);
    }
    return parts;
}

// Get the backup directory path (%APPDATA%\HDRFixer\backups)
static std::filesystem::path get_backup_directory() {
    wchar_t* appdata_path = nullptr;
    if (SUCCEEDED(SHGetKnownFolderPath(FOLDERID_RoamingAppData, 0, nullptr, &appdata_path))) {
        std::filesystem::path dir = std::filesystem::path(appdata_path) / L"HDRFixer" / L"backups";
        CoTaskMemFree(appdata_path);
        return dir;
    }

    // Fallback: use APPDATA environment variable
    const wchar_t* env = _wgetenv(L"APPDATA");
    if (env) {
        return std::filesystem::path(env) / L"HDRFixer" / L"backups";
    }

    // Last resort fallback
    return std::filesystem::path(L"C:\\HDRFixer\\backups");
}

// ---------------------------------------------------------------------------
// RegistryBackupManager implementation
// ---------------------------------------------------------------------------

RegistryBackupManager::RegistryBackupManager()
    : backup_dir_(get_backup_directory()) {
    // Ensure the backup directory exists
    std::error_code ec;
    std::filesystem::create_directories(backup_dir_, ec);
}

std::expected<void, std::string> RegistryBackupManager::save_backup(const BackupSet& backup) {
    // Ensure directory exists
    std::error_code ec;
    std::filesystem::create_directories(backup_dir_, ec);
    if (ec) {
        return std::unexpected(
            std::format("Failed to create backup directory: {}", ec.message()));
    }

    // Build the file path
    std::filesystem::path file_path = backup_dir_ / (backup.name + ".bak");

    // Open file for writing (UTF-8)
    std::ofstream file(file_path, std::ios::binary | std::ios::trunc);
    if (!file.is_open()) {
        return std::unexpected(
            std::format("Failed to open backup file for writing: {}",
                        file_path.string()));
    }

    // Write header: timestamp as epoch seconds
    auto epoch = std::chrono::duration_cast<std::chrono::seconds>(
                     backup.created_at.time_since_epoch())
                     .count();
    file << "# HDRFixer Registry Backup\n";
    file << "# Name: " << backup.name << "\n";
    file << "# Timestamp: " << epoch << "\n";

    // Write each entry: key_path|value_name|value_kind|original_value
    for (const auto& entry : backup.entries) {
        std::string line;
        line += to_utf8(escape_pipes(entry.key_path));
        line += '|';
        line += to_utf8(escape_pipes(entry.value_name));
        line += '|';
        line += std::to_string(entry.value_kind);
        line += '|';
        line += to_utf8(escape_pipes(entry.original_value));
        line += '\n';
        file << line;
    }

    if (!file.good()) {
        return std::unexpected("Failed to write backup data");
    }

    return {};
}

std::expected<BackupSet, std::string> RegistryBackupManager::load_backup(const std::string& name) {
    std::filesystem::path file_path = backup_dir_ / (name + ".bak");

    if (!std::filesystem::exists(file_path)) {
        return std::unexpected(
            std::format("Backup file not found: {}", file_path.string()));
    }

    std::ifstream file(file_path, std::ios::binary);
    if (!file.is_open()) {
        return std::unexpected(
            std::format("Failed to open backup file: {}", file_path.string()));
    }

    BackupSet backup;
    backup.name = name;
    backup.created_at = std::chrono::system_clock::now(); // Default

    std::string line;
    while (std::getline(file, line)) {
        // Remove trailing \r if present (Windows line endings)
        if (!line.empty() && line.back() == '\r') {
            line.pop_back();
        }

        // Skip empty lines
        if (line.empty()) continue;

        // Parse header comments
        if (line[0] == '#') {
            // Check for timestamp
            const std::string ts_prefix = "# Timestamp: ";
            if (line.substr(0, ts_prefix.size()) == ts_prefix) {
                try {
                    auto epoch = std::stoll(line.substr(ts_prefix.size()));
                    backup.created_at = std::chrono::system_clock::time_point(
                        std::chrono::seconds(epoch));
                } catch (...) {
                    // Ignore parse errors for timestamp
                }
            }
            continue;
        }

        // Parse entry line: key_path|value_name|value_kind|original_value
        std::wstring wline = from_utf8(line);
        auto parts = split_on_pipe(wline);
        if (parts.size() < 4) continue; // Skip malformed lines

        BackupEntry entry;
        entry.key_path = unescape_pipes(parts[0]);
        entry.value_name = unescape_pipes(parts[1]);

        try {
            entry.value_kind = static_cast<DWORD>(std::stoul(to_utf8(parts[2])));
        } catch (...) {
            entry.value_kind = 0;
        }

        entry.original_value = unescape_pipes(parts[3]);
        backup.entries.push_back(std::move(entry));
    }

    return backup;
}

std::vector<std::string> RegistryBackupManager::list_backups() const {
    std::vector<std::string> names;

    std::error_code ec;
    if (!std::filesystem::exists(backup_dir_, ec)) {
        return names;
    }

    for (const auto& entry : std::filesystem::directory_iterator(backup_dir_, ec)) {
        if (entry.is_regular_file() && entry.path().extension() == ".bak") {
            names.push_back(entry.path().stem().string());
        }
    }

    std::sort(names.begin(), names.end());
    return names;
}

} // namespace hdrfixer::registry
