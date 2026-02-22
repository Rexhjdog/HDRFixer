#include "doctest.h"
#include "core/config/settings.h"
#include <filesystem>

using namespace hdrfixer::config;

TEST_CASE("AppSettings defaults") {
    AppSettings s{};
    CHECK(s.run_at_startup == false);
    CHECK(s.minimize_to_tray == true);
    CHECK(s.enable_fix_watchdog == true);
    CHECK(s.preferred_sdr_brightness_nits == doctest::Approx(200.0f));
    CHECK(s.oled_pixel_shift_enabled == false);
    CHECK(s.oled_static_content_timeout_minutes == 5);
    CHECK(s.enabled_fixes.empty());
}

TEST_CASE("SettingsManager serialize round-trip") {
    SettingsManager mgr;
    mgr.get_mut().run_at_startup = true;
    mgr.get_mut().preferred_sdr_brightness_nits = 250.0f;
    mgr.get_mut().oled_pixel_shift_enabled = true;
    mgr.get_mut().oled_static_content_timeout_minutes = 10;
    mgr.get_mut().enabled_fixes["gamma_correction"] = true;
    mgr.get_mut().enabled_fixes["sdr_brightness"] = false;

    auto result = mgr.save();
    CHECK(result.has_value());

    SettingsManager mgr2;
    auto result2 = mgr2.load();
    CHECK(result2.has_value());
    CHECK(mgr2.get().run_at_startup == true);
    CHECK(mgr2.get().preferred_sdr_brightness_nits == doctest::Approx(250.0f));
    CHECK(mgr2.get().oled_pixel_shift_enabled == true);
    CHECK(mgr2.get().oled_static_content_timeout_minutes == 10);
    CHECK(mgr2.get().enabled_fixes.at("gamma_correction") == true);
    CHECK(mgr2.get().enabled_fixes.at("sdr_brightness") == false);
}

TEST_CASE("Settings path exists after save") {
    SettingsManager mgr;
    mgr.save();
    CHECK(std::filesystem::exists(SettingsManager::settings_path()));
}

TEST_CASE("SettingsManager load with missing file uses defaults") {
    // Remove the settings file if it exists
    auto path = SettingsManager::settings_path();
    if (std::filesystem::exists(path)) {
        std::filesystem::remove(path);
    }

    SettingsManager mgr;
    auto result = mgr.load();
    CHECK(result.has_value());
    CHECK(mgr.get().run_at_startup == false);
    CHECK(mgr.get().minimize_to_tray == true);
    CHECK(mgr.get().enable_fix_watchdog == true);
    CHECK(mgr.get().preferred_sdr_brightness_nits == doctest::Approx(200.0f));
}

TEST_CASE("Settings path is under LOCALAPPDATA") {
    auto path = SettingsManager::settings_path();
    CHECK(path.filename() == "settings.ini");
    CHECK(path.parent_path().filename() == "HDRFixer");
}
