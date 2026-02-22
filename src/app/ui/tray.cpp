#include "app/ui/tray.h"
#include <shellapi.h>
#include <dbt.h>

namespace hdrfixer::ui {

namespace {
constexpr UINT WM_TRAYICON = WM_APP + 1;
constexpr wchar_t WNDCLASS_NAME[] = L"HDRFixerTrayClass";
constexpr UINT TRAY_ICON_ID = 1;
static UINT WM_TASKBAR_CREATED = 0;
} // namespace

// ---------------------------------------------------------------------------
// Construction / Destruction
// ---------------------------------------------------------------------------

TrayIcon::TrayIcon(HINSTANCE hInstance, const TrayCallbacks& callbacks)
    : hinstance_(hInstance), callbacks_(callbacks) {}

TrayIcon::~TrayIcon() {
    destroy();
}

// ---------------------------------------------------------------------------
// create() -- register class, create hidden window, add tray icon, hotkeys
// ---------------------------------------------------------------------------

bool TrayIcon::create() {
    // Register the window class (ignore ERROR_CLASS_ALREADY_EXISTS)
    WNDCLASSEXW wc{};
    wc.cbSize = sizeof(wc);
    wc.lpfnWndProc = WndProc;
    wc.hInstance = hinstance_;
    wc.lpszClassName = WNDCLASS_NAME;
    RegisterClassExW(&wc);

    // Create an invisible message-only window
    hwnd_ = CreateWindowExW(
        0,
        WNDCLASS_NAME,
        L"HDRFixer",
        WS_POPUP,          // invisible popup
        0, 0, 0, 0,
        HWND_MESSAGE,       // message-only window
        nullptr,
        hinstance_,
        nullptr
    );

    if (!hwnd_) {
        return false;
    }

    // Store 'this' so the static WndProc can reach instance members
    SetWindowLongPtrW(hwnd_, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(this));

    // Add the icon to the system tray
    NOTIFYICONDATAW nid{};
    nid.cbSize = sizeof(nid);
    nid.hWnd = hwnd_;
    nid.uID = TRAY_ICON_ID;
    nid.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
    nid.uCallbackMessage = WM_TRAYICON;
    nid.hIcon = LoadIconW(nullptr, IDI_APPLICATION);
    wcscpy_s(nid.szTip, L"HDRFixer");

    if (!Shell_NotifyIconW(NIM_ADD, &nid)) {
        DestroyWindow(hwnd_);
        hwnd_ = nullptr;
        return false;
    }

    // Register global hotkeys
    // Ctrl+Shift+H -- toggle apply / revert
    RegisterHotKey(hwnd_, HOTKEY_TOGGLE, MOD_CONTROL | MOD_SHIFT, 'H');
    // Ctrl+Shift+S -- share mode
    RegisterHotKey(hwnd_, HOTKEY_SHARE, MOD_CONTROL | MOD_SHIFT, 'S');

    // Register for TaskbarCreated so we can re-add the icon after explorer restarts
    WM_TASKBAR_CREATED = RegisterWindowMessageW(L"TaskbarCreated");

    return true;
}

// ---------------------------------------------------------------------------
// destroy() -- remove tray icon, unregister hotkeys, destroy window
// ---------------------------------------------------------------------------

void TrayIcon::destroy() {
    if (!hwnd_) return;

    UnregisterHotKey(hwnd_, HOTKEY_TOGGLE);
    UnregisterHotKey(hwnd_, HOTKEY_SHARE);

    NOTIFYICONDATAW nid{};
    nid.cbSize = sizeof(nid);
    nid.hWnd = hwnd_;
    nid.uID = TRAY_ICON_ID;
    Shell_NotifyIconW(NIM_DELETE, &nid);

    DestroyWindow(hwnd_);
    hwnd_ = nullptr;
}

// ---------------------------------------------------------------------------
// set_tooltip()
// ---------------------------------------------------------------------------

void TrayIcon::set_tooltip(const wchar_t* text) {
    if (!hwnd_) return;

    NOTIFYICONDATAW nid{};
    nid.cbSize = sizeof(nid);
    nid.hWnd = hwnd_;
    nid.uID = TRAY_ICON_ID;
    nid.uFlags = NIF_TIP;
    wcsncpy_s(nid.szTip, text, _TRUNCATE);
    Shell_NotifyIconW(NIM_MODIFY, &nid);
}

// ---------------------------------------------------------------------------
// show_balloon()
// ---------------------------------------------------------------------------

void TrayIcon::show_balloon(const wchar_t* title, const wchar_t* text, DWORD flags) {
    if (!hwnd_) return;

    NOTIFYICONDATAW nid{};
    nid.cbSize = sizeof(nid);
    nid.hWnd = hwnd_;
    nid.uID = TRAY_ICON_ID;
    nid.uFlags = NIF_INFO;
    nid.dwInfoFlags = flags;
    wcsncpy_s(nid.szInfoTitle, title, _TRUNCATE);
    wcsncpy_s(nid.szInfo, text, _TRUNCATE);
    Shell_NotifyIconW(NIM_MODIFY, &nid);
}

// ---------------------------------------------------------------------------
// set_share_mode()
// ---------------------------------------------------------------------------

void TrayIcon::set_share_mode(bool active) {
    share_mode_ = active;

    if (active) {
        set_tooltip(L"HDRFixer - Share Mode Active");
    } else {
        set_tooltip(L"HDRFixer");
    }
}

// ---------------------------------------------------------------------------
// WndProc -- static, dispatches to instance via GWLP_USERDATA
// ---------------------------------------------------------------------------

LRESULT CALLBACK TrayIcon::WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam) {
    auto* self = reinterpret_cast<TrayIcon*>(GetWindowLongPtrW(hwnd, GWLP_USERDATA));

    // Handle TaskbarCreated (registered dynamically, so can't be in the switch)
    if (WM_TASKBAR_CREATED && msg == WM_TASKBAR_CREATED && self) {
        // Explorer restarted — re-add the tray icon
        NOTIFYICONDATAW nid{};
        nid.cbSize = sizeof(nid);
        nid.hWnd = hwnd;
        nid.uID = TRAY_ICON_ID;
        nid.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
        nid.uCallbackMessage = WM_TRAYICON;
        nid.hIcon = LoadIconW(nullptr, IDI_APPLICATION);
        wcscpy_s(nid.szTip, L"HDRFixer");
        Shell_NotifyIconW(NIM_ADD, &nid);
        return 0;
    }

    switch (msg) {

    case WM_TRAYICON:
        if (LOWORD(lParam) == WM_RBUTTONUP || LOWORD(lParam) == WM_CONTEXTMENU) {
            if (self) self->show_context_menu();
        }
        return 0;

    case WM_COMMAND:
        if (self) {
            switch (LOWORD(wParam)) {
            case IDM_APPLY_ALL:
                if (self->callbacks_.on_apply_all) self->callbacks_.on_apply_all();
                break;
            case IDM_REVERT_ALL:
                if (self->callbacks_.on_revert_all) self->callbacks_.on_revert_all();
                break;
            case IDM_SHARE_MODE:
                if (self->callbacks_.on_share_mode) self->callbacks_.on_share_mode();
                break;
            case IDM_SETTINGS:
                if (self->callbacks_.on_settings) self->callbacks_.on_settings();
                break;
            case IDM_EXIT:
                if (self->callbacks_.on_exit) self->callbacks_.on_exit();
                break;
            }
        }
        return 0;

    case WM_HOTKEY:
        if (self) {
            if (wParam == HOTKEY_TOGGLE) {
                // Toggle: apply if not applied, revert if applied
                if (self->share_mode_) {
                    if (self->callbacks_.on_revert_all) self->callbacks_.on_revert_all();
                } else {
                    if (self->callbacks_.on_apply_all) self->callbacks_.on_apply_all();
                }
            } else if (wParam == HOTKEY_SHARE) {
                if (self->callbacks_.on_share_mode) self->callbacks_.on_share_mode();
            }
        }
        return 0;

    // C2 fix: watchdog posts to main thread instead of calling directly
    case WM_WATCHDOG_TRIGGER:
        if (self && self->callbacks_.on_watchdog_trigger) {
            self->callbacks_.on_watchdog_trigger();
        }
        return 0;

    // C3 fix: handle display hotplug notifications
    case WM_DEVICECHANGE:
        if (self && self->callbacks_.on_display_change) {
            if (wParam == DBT_DEVICEARRIVAL || wParam == DBT_DEVICEREMOVECOMPLETE) {
                self->callbacks_.on_display_change();
            }
        }
        return 0;

    case WM_DESTROY:
        // Do NOT PostQuitMessage here — on_exit callback handles it
        return 0;

    default:
        break;
    }

    return DefWindowProcW(hwnd, msg, wParam, lParam);
}

// ---------------------------------------------------------------------------
// show_context_menu()
// ---------------------------------------------------------------------------

void TrayIcon::show_context_menu() {
    HMENU hMenu = CreatePopupMenu();
    if (!hMenu) return;

    AppendMenuW(hMenu, MF_STRING, IDM_APPLY_ALL, L"Apply All Fixes");
    AppendMenuW(hMenu, MF_STRING, IDM_REVERT_ALL, L"Revert All Fixes");
    AppendMenuW(hMenu, MF_SEPARATOR, 0, nullptr);
    AppendMenuW(hMenu, share_mode_ ? (MF_STRING | MF_CHECKED) : MF_STRING,
                IDM_SHARE_MODE, L"Share Mode");
    AppendMenuW(hMenu, MF_SEPARATOR, 0, nullptr);
    AppendMenuW(hMenu, MF_STRING, IDM_SETTINGS, L"Settings...");
    AppendMenuW(hMenu, MF_SEPARATOR, 0, nullptr);
    AppendMenuW(hMenu, MF_STRING, IDM_EXIT, L"Exit");

    // Required so the menu dismisses when the user clicks elsewhere
    SetForegroundWindow(hwnd_);

    POINT pt;
    GetCursorPos(&pt);
    TrackPopupMenu(hMenu, TPM_RIGHTBUTTON, pt.x, pt.y, 0, hwnd_, nullptr);

    // Win32 workaround: post a benign message so TrackPopupMenu works correctly
    PostMessage(hwnd_, WM_NULL, 0, 0);

    DestroyMenu(hMenu);
}

} // namespace hdrfixer::ui
