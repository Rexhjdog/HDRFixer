#include "core/core.h"
#include "core/display/dxgi_detector.h"
#include "core/display/display_info.h"
#include "core/config/settings.h"
#include "core/log/logger.h"

#include "fixes/fix_engine.h"
#include "fixes/gamma_fix.h"
#include "fixes/sdr_brightness_fix.h"
#include "fixes/pixel_format_fix.h"
#include "fixes/share_helper.h"
#include "fixes/watchdog.h"
#include "fixes/hotplug.h"

#include "ui/tray.h"
#include "ui/settings_wnd.h"

#include <memory>

using namespace hdrfixer;

// Global state
static std::unique_ptr<fixes::FixEngine> g_engine;
static std::unique_ptr<fixes::Watchdog> g_watchdog;
static std::unique_ptr<fixes::Hotplug> g_hotplug;
static std::unique_ptr<ui::TrayIcon> g_tray;
static config::SettingsManager g_settings;
static std::vector<display::DisplayInfo> g_displays;

static void refresh_displays() {
    auto result = display::detect_displays();
    if (result.has_value()) {
        g_displays = std::move(result.value());
        LOG_INFO(std::format("Detected {} display(s)", g_displays.size()));
        for (const auto& d : g_displays) {
            LOG_INFO(std::format("  {} - HDR:{} {}bpc MaxLum:{:.0f}nits SDRWhite:{:.0f}nits",
                std::string(d.device_name.begin(), d.device_name.end()),
                d.is_hdr_enabled ? "ON" : "OFF",
                d.bits_per_color,
                d.max_luminance,
                d.sdr_white_level_nits));
        }
    } else {
        LOG_ERROR(std::format("Display detection failed: {}", result.error()));
    }
}

static void build_fix_engine() {
    g_engine = std::make_unique<fixes::FixEngine>();

    if (g_displays.empty()) {
        LOG_WARN("No displays detected, fix engine will be empty");
        return;
    }

    // Register fixes for the primary display
    auto& primary = g_displays[0];
    g_engine->register_fix(std::make_unique<fixes::GammaFix>(primary));
    g_engine->register_fix(std::make_unique<fixes::SdrBrightnessFix>(primary));
    g_engine->register_fix(std::make_unique<fixes::PixelFormatFix>(primary));
    g_engine->register_fix(std::make_unique<fixes::ShareHelper>());

    LOG_INFO(std::format("Fix engine initialized with {} fixes", g_engine->fix_count()));
}

static void on_watchdog_trigger() {
    if (!g_engine) return;
    LOG_INFO("Watchdog triggered, diagnosing fixes...");
    auto statuses = g_engine->diagnose_all();
    for (const auto& s : statuses) {
        if (s.state == fixes::FixState::Error || s.state == fixes::FixState::NotApplied) {
            LOG_INFO("Re-applying fixes due to detected changes");
            g_engine->apply_all();
            if (g_tray) {
                g_tray->show_balloon(L"HDRFixer", L"Fixes re-applied after system change");
            }
            break;
        }
    }
}

static void on_display_change() {
    LOG_INFO("Display change detected, refreshing...");
    refresh_displays();
    build_fix_engine();
    if (g_settings.get().enable_fix_watchdog) {
        g_engine->apply_all();
    }
    if (g_tray) {
        g_tray->show_balloon(L"HDRFixer", L"Display configuration changed, fixes updated");
    }
}

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE, LPWSTR, int) {
    // Prevent multiple instances
    HANDLE hMutex = CreateMutexW(nullptr, TRUE, L"HDRFixerSingletonV2");
    if (GetLastError() == ERROR_ALREADY_EXISTS) {
        CloseHandle(hMutex);
        return 0;
    }

    // Initialize COM (needed for DXGI)
    CoInitializeEx(nullptr, COINIT_MULTITHREADED);

    // Load settings
    g_settings.load();

    // Initialize logger
    LOG_INFO("HDRFixer v2.0.0 starting");

    // Detect displays
    refresh_displays();

    // Build fix engine
    build_fix_engine();

    // Create tray icon
    ui::TrayCallbacks callbacks{};
    callbacks.on_apply_all = [] {
        if (g_engine) {
            g_engine->apply_all();
            if (g_tray) g_tray->show_balloon(L"HDRFixer", L"All fixes applied");
        }
    };
    callbacks.on_revert_all = [] {
        if (g_engine) {
            g_engine->revert_all();
            if (g_tray) g_tray->show_balloon(L"HDRFixer", L"All fixes reverted");
        }
    };
    callbacks.on_share_mode = [] {
        if (!g_engine) return;
        auto* share = dynamic_cast<fixes::ShareHelper*>(g_engine->get_fix("Screen-Share Helper"));
        if (share) {
            auto status = share->diagnose();
            if (status.state == fixes::FixState::Applied) {
                share->revert();
                if (g_tray) {
                    g_tray->set_share_mode(false);
                    g_tray->show_balloon(L"HDRFixer", L"Share mode OFF - SDR brightness restored");
                }
            } else {
                share->apply();
                if (g_tray) {
                    g_tray->set_share_mode(true);
                    g_tray->show_balloon(L"HDRFixer", L"Share mode ON - SDR brightness set to 80 nits");
                }
            }
        }
    };
    callbacks.on_settings = [&hInstance] {
        ui::show_settings_window(nullptr);
    };
    callbacks.on_exit = [] {
        if (g_engine) g_engine->revert_all();
        PostQuitMessage(0);
    };

    g_tray = std::make_unique<ui::TrayIcon>(hInstance, callbacks);
    if (!g_tray->create()) {
        LOG_ERROR("Failed to create tray icon");
        MessageBoxW(nullptr, L"Failed to create system tray icon", L"HDRFixer Error", MB_ICONERROR);
        CoUninitialize();
        CloseHandle(hMutex);
        return 1;
    }

    // Register for display hotplug
    g_hotplug = std::make_unique<fixes::Hotplug>();
    g_hotplug->register_hotplug(g_tray->hwnd());

    // Start watchdog
    if (g_settings.get().enable_fix_watchdog) {
        g_watchdog = std::make_unique<fixes::Watchdog>(on_watchdog_trigger);
        g_watchdog->start();
        LOG_INFO("Registry watchdog started");
    }

    // Auto-apply fixes on startup
    if (g_engine && !g_displays.empty()) {
        g_engine->apply_all();
        LOG_INFO("Startup fixes applied");
    }

    g_tray->set_tooltip(L"HDRFixer v2.0 - Active");
    LOG_INFO("HDRFixer ready, entering message loop");

    // Message loop
    MSG msg;
    while (GetMessage(&msg, nullptr, 0, 0)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    // Cleanup
    LOG_INFO("HDRFixer shutting down");
    if (g_watchdog) g_watchdog->stop();
    g_hotplug.reset();
    g_tray.reset();
    g_engine.reset();

    g_settings.save();
    CoUninitialize();
    CloseHandle(hMutex);

    return static_cast<int>(msg.wParam);
}
