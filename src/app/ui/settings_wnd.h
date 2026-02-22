#pragma once
#include <windows.h>
#include "core/config/settings.h"

namespace hdrfixer::ui {

// Displays the settings dialog
void show_settings_window(HWND parent, hdrfixer::config::SettingsManager& settings);

class SettingsWindow {
public:
    static void show(HWND parent, hdrfixer::config::SettingsManager& settings);

private:
    static INT_PTR CALLBACK DlgProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);
    static void init_controls(HWND hwnd);
    static void save_settings(HWND hwnd);
    static void update_labels(HWND hwnd);

    // Pointer to the settings manager for the active dialog
    static hdrfixer::config::SettingsManager* s_settings_mgr;
};

} // namespace hdrfixer::ui
