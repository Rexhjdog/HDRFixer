#include "core/config/settings.h"
#include <fstream>
#include <sstream>
#include <filesystem>
#include <windows.h>
#include <shlobj.h>

namespace hdrfixer::config {

SettingsManager::SettingsManager() = default;

std::filesystem::path SettingsManager::settings_path() {
    wchar_t* appdata = nullptr;
    if (SUCCEEDED(SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, nullptr, &appdata))) {
        std::filesystem::path dir = std::filesystem::path(appdata) / L"HDRFixer";
        CoTaskMemFree(appdata);
        return dir / "settings.ini";
    }
    // Fallback: use %LOCALAPPDATA% environment variable
    const char* env = std::getenv("LOCALAPPDATA");
    if (env) {
        std::filesystem::path dir = std::filesystem::path(env) / "HDRFixer";
        return dir / "settings.ini";
    }
    // Last resort fallback
    return std::filesystem::path("HDRFixer") / "settings.ini";
}

std::expected<void, std::string> SettingsManager::load() {
    auto path = settings_path();

    if (!std::filesystem::exists(path)) {
        // No settings file yet; use defaults
        settings_ = AppSettings{};
        return {};
    }

    std::ifstream file(path);
    if (!file.is_open()) {
        return std::unexpected("Failed to open settings file: " + path.string());
    }

    std::ostringstream ss;
    ss << file.rdbuf();
    file.close();

    deserialize(ss.str());
    return {};
}

std::expected<void, std::string> SettingsManager::save() const {
    auto path = settings_path();
    auto dir = path.parent_path();

    // Create directory if it doesn't exist
    std::error_code ec;
    if (!std::filesystem::exists(dir)) {
        std::filesystem::create_directories(dir, ec);
        if (ec) {
            return std::unexpected("Failed to create settings directory: " + ec.message());
        }
    }

    std::ofstream file(path, std::ios::trunc);
    if (!file.is_open()) {
        return std::unexpected("Failed to open settings file for writing: " + path.string());
    }

    file << serialize();
    file.close();

    if (file.fail()) {
        return std::unexpected("Failed to write settings file");
    }

    return {};
}

std::string SettingsManager::serialize() const {
    std::ostringstream ss;
    ss << "run_at_startup=" << (settings_.run_at_startup ? "true" : "false") << "\n";
    ss << "minimize_to_tray=" << (settings_.minimize_to_tray ? "true" : "false") << "\n";
    ss << "enable_fix_watchdog=" << (settings_.enable_fix_watchdog ? "true" : "false") << "\n";
    ss << "preferred_sdr_brightness_nits=" << settings_.preferred_sdr_brightness_nits << "\n";
    ss << "oled_pixel_shift_enabled=" << (settings_.oled_pixel_shift_enabled ? "true" : "false") << "\n";
    ss << "oled_static_content_timeout_minutes=" << settings_.oled_static_content_timeout_minutes << "\n";

    // Serialize enabled_fixes as "fix.<name>=true/false"
    for (const auto& [name, enabled] : settings_.enabled_fixes) {
        ss << "fix." << name << "=" << (enabled ? "true" : "false") << "\n";
    }

    return ss.str();
}

void SettingsManager::deserialize(const std::string& data) {
    settings_ = AppSettings{};  // Start from defaults

    std::istringstream stream(data);
    std::string line;

    while (std::getline(stream, line)) {
        // Skip empty lines and comments
        if (line.empty() || line[0] == '#' || line[0] == ';') {
            continue;
        }

        // Trim trailing \r if present (Windows line endings)
        if (!line.empty() && line.back() == '\r') {
            line.pop_back();
        }

        auto eq_pos = line.find('=');
        if (eq_pos == std::string::npos) {
            continue;
        }

        std::string key = line.substr(0, eq_pos);
        std::string value = line.substr(eq_pos + 1);

        // Trim whitespace from key and value
        auto trim = [](std::string& s) {
            size_t start = s.find_first_not_of(" \t");
            size_t end = s.find_last_not_of(" \t");
            if (start == std::string::npos) {
                s.clear();
            } else {
                s = s.substr(start, end - start + 1);
            }
        };
        trim(key);
        trim(value);

        auto parse_bool = [](const std::string& v) -> bool {
            return v == "true" || v == "1" || v == "yes";
        };

        if (key == "run_at_startup") {
            settings_.run_at_startup = parse_bool(value);
        } else if (key == "minimize_to_tray") {
            settings_.minimize_to_tray = parse_bool(value);
        } else if (key == "enable_fix_watchdog") {
            settings_.enable_fix_watchdog = parse_bool(value);
        } else if (key == "preferred_sdr_brightness_nits") {
            try {
                settings_.preferred_sdr_brightness_nits = std::stof(value);
            } catch (...) {
                // Keep default on parse failure
            }
        } else if (key == "oled_pixel_shift_enabled") {
            settings_.oled_pixel_shift_enabled = parse_bool(value);
        } else if (key == "oled_static_content_timeout_minutes") {
            try {
                settings_.oled_static_content_timeout_minutes = std::stoi(value);
            } catch (...) {
                // Keep default on parse failure
            }
        } else if (key.starts_with("fix.")) {
            std::string fix_name = key.substr(4);
            if (!fix_name.empty()) {
                settings_.enabled_fixes[fix_name] = parse_bool(value);
            }
        }
    }
}

} // namespace hdrfixer::config
