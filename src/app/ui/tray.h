#pragma once
#include <windows.h>
#include <functional>

namespace hdrfixer::ui {

// Menu item IDs
constexpr UINT IDM_APPLY_ALL = 1001;
constexpr UINT IDM_REVERT_ALL = 1002;
constexpr UINT IDM_SHARE_MODE = 1003;
constexpr UINT IDM_SETTINGS = 1004;
constexpr UINT IDM_EXIT = 1005;

// Hotkey IDs
constexpr int HOTKEY_TOGGLE = 1;   // Ctrl+Shift+H
constexpr int HOTKEY_SHARE = 2;    // Ctrl+Shift+S

// Custom window messages
constexpr UINT WM_WATCHDOG_TRIGGER = WM_APP + 2;
constexpr UINT WM_DISPLAY_CHANGE = WM_APP + 3;

struct TrayCallbacks {
    std::function<void()> on_apply_all;
    std::function<void()> on_revert_all;
    std::function<void()> on_share_mode;
    std::function<void()> on_settings;
    std::function<void()> on_exit;
    std::function<void()> on_watchdog_trigger;
    std::function<void()> on_display_change;
};

class TrayIcon {
public:
    TrayIcon(HINSTANCE hInstance, const TrayCallbacks& callbacks);
    ~TrayIcon();

    bool create();
    void destroy();
    void set_tooltip(const wchar_t* text);
    void show_balloon(const wchar_t* title, const wchar_t* text, DWORD flags = 0x00000001 /*NIIF_INFO*/);
    void set_share_mode(bool active);

    HWND hwnd() const { return hwnd_; }

private:
    static LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);
    void show_context_menu();

    HINSTANCE hinstance_;
    HWND hwnd_ = nullptr;
    TrayCallbacks callbacks_;
    bool share_mode_ = false;
};

} // namespace hdrfixer::ui
