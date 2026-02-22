#include "doctest.h"
#include "core/config/settings.h"
#include <filesystem>
#include <cstdlib>
#include <string>

using namespace hdrfixer::config;

// Helper to set environment variable for testing
void set_test_env(const char* name, const char* value) {
#ifdef _WIN32
    _putenv_s(name, value);
#else
    setenv(name, value, 1);
#endif
}

void unset_test_env(const char* name) {
#ifdef _WIN32
    _putenv_s(name, "");
#else
    unsetenv(name);
#endif
}

struct TestEnv {
    std::filesystem::path test_dir;

    TestEnv() {
        test_dir = std::filesystem::temp_directory_path() / "hdrfixer_test_env";
        std::filesystem::create_directories(test_dir);
        set_test_env("LOCALAPPDATA", test_dir.string().c_str());
    }

    ~TestEnv() {
        std::filesystem::remove_all(test_dir);
        unset_test_env("LOCALAPPDATA");
    }
};

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
    TestEnv env;
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
    TestEnv env;
    SettingsManager mgr;
    (void)mgr.save();
    CHECK(std::filesystem::exists(SettingsManager::settings_path()));
}

TEST_CASE("SettingsManager load with missing file uses defaults") {
    TestEnv env;
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
    TestEnv env;
    auto path = SettingsManager::settings_path();
    CHECK(path.filename() == "settings.ini");
    CHECK(path.parent_path().filename() == "HDRFixer");
    // Verify it is inside our test dir
    auto test_dir = std::filesystem::path(std::getenv("LOCALAPPDATA"));
    // On Linux/mock, the path is constructed as LOCALAPPDATA/HDRFixer/settings.ini
    // But SettingsManager::settings_path implementation for env var is:
    // path(env) / "HDRFixer" / "settings.ini"
    CHECK(path == test_dir / "HDRFixer" / "settings.ini");
}

TEST_CASE("SettingsManager deserialization edge cases") {
    // deserialize doesn't depend on filesystem, but nice to have consistent env
    SettingsManager mgr;

    SUBCASE("Comments and Empty Lines") {
        std::string input = R"(
            # This is a comment
            ; This is also a comment

            run_at_startup=true
        )";
        mgr.deserialize(input);
        CHECK(mgr.get().run_at_startup == true);
        // Defaults should remain
        CHECK(mgr.get().preferred_sdr_brightness_nits == doctest::Approx(200.0f));
    }

    SUBCASE("Whitespace Handling") {
        std::string input = "  minimize_to_tray  =  false  \n";
        mgr.deserialize(input);
        CHECK(mgr.get().minimize_to_tray == false);
    }

    SUBCASE("Boolean Parsing") {
        CHECK(mgr.get().enable_fix_watchdog == true); // Default

        mgr.deserialize("enable_fix_watchdog=false");
        CHECK(mgr.get().enable_fix_watchdog == false);

        mgr.deserialize("enable_fix_watchdog=1");
        CHECK(mgr.get().enable_fix_watchdog == true);

        mgr.deserialize("enable_fix_watchdog=0");
        CHECK(mgr.get().enable_fix_watchdog == false);

        mgr.deserialize("enable_fix_watchdog=yes");
        CHECK(mgr.get().enable_fix_watchdog == true);

        mgr.deserialize("enable_fix_watchdog=no");
        CHECK(mgr.get().enable_fix_watchdog == false);

        mgr.deserialize("enable_fix_watchdog=True"); // Case sensitive check
        CHECK(mgr.get().enable_fix_watchdog == false);
    }

    SUBCASE("Numeric Parsing") {
        mgr.deserialize("preferred_sdr_brightness_nits=abc");
        CHECK(mgr.get().preferred_sdr_brightness_nits == doctest::Approx(200.0f)); // Should keep default

        mgr.deserialize("preferred_sdr_brightness_nits=300.5");
        CHECK(mgr.get().preferred_sdr_brightness_nits == doctest::Approx(300.5f));

        mgr.deserialize("oled_static_content_timeout_minutes=invalid");
        CHECK(mgr.get().oled_static_content_timeout_minutes == 5); // Default
    }

    SUBCASE("Fix Configuration") {
        std::string input = "fix.my_cool_fix=true\nfix.another_fix=false";
        mgr.deserialize(input);
        CHECK(mgr.get().enabled_fixes.at("my_cool_fix") == true);
        CHECK(mgr.get().enabled_fixes.at("another_fix") == false);
    }

    SUBCASE("Malformed Lines") {
        std::string input = "run_at_startup\n=true\nrun_at_startup=false";
        mgr.deserialize(input);
        // The malformed lines should be ignored
        CHECK(mgr.get().run_at_startup == false);
    }
}
