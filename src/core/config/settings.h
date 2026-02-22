#pragma once
#include <string>
#include <map>
#include <filesystem>
#include <expected>

namespace hdrfixer::config {

struct AppSettings {
    bool run_at_startup = false;
    bool minimize_to_tray = true;
    bool enable_fix_watchdog = true;
    float preferred_sdr_brightness_nits = 200.0f;
    bool oled_pixel_shift_enabled = false;
    int oled_static_content_timeout_minutes = 5;
    std::map<std::string, bool> enabled_fixes;
};

class SettingsManager {
public:
    SettingsManager();

    const AppSettings& get() const { return settings_; }
    AppSettings& get_mut() { return settings_; }

    std::expected<void, std::string> load();
    std::expected<void, std::string> save() const;

    static std::filesystem::path settings_path();

    // Simple key=value serialization (no JSON dependency)
    std::string serialize() const;
    void deserialize(const std::string& data);

private:
    AppSettings settings_;
};

} // namespace hdrfixer::config
