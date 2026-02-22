#include "app/ui/settings_wnd.h"
#include "app/resource.h"
#include "core/config/settings.h"
#include "core/log/logger.h"
#include <commctrl.h>
#include <string>

namespace hdrfixer::ui {

hdrfixer::config::SettingsManager* SettingsWindow::s_settings_mgr = nullptr;

void show_settings_window(HWND parent, hdrfixer::config::SettingsManager& settings) {
    SettingsWindow::show(parent, settings);
}

void SettingsWindow::show(HWND parent, hdrfixer::config::SettingsManager& settings) {
    s_settings_mgr = &settings;
    DialogBoxW(
        GetModuleHandleW(nullptr),
        MAKEINTRESOURCEW(IDD_SETTINGS),
        parent,
        SettingsWindow::DlgProc
    );
    s_settings_mgr = nullptr;
}

INT_PTR CALLBACK SettingsWindow::DlgProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam) {
    switch (msg) {
    case WM_INITDIALOG:
        init_controls(hwnd);
        return TRUE;

    case WM_COMMAND:
        switch (LOWORD(wParam)) {
        case IDC_BTN_OK:
            save_settings(hwnd);
            EndDialog(hwnd, IDOK);
            return TRUE;

        case IDC_BTN_CANCEL:
            EndDialog(hwnd, IDCANCEL);
            return TRUE;

        case IDC_CHECK_AUTOSTART:
        case IDC_CHECK_WATCHDOG:
            break;

        case IDC_COMBO_LOGLEVEL:
            break;
        }
        break;

    case WM_HSCROLL:
        if ((HWND)lParam == GetDlgItem(hwnd, IDC_SLIDER_BRIGHTNESS)) {
            update_labels(hwnd);
        }
        break;

    case WM_CLOSE:
        EndDialog(hwnd, IDCANCEL);
        return TRUE;
    }

    return FALSE;
}

void SettingsWindow::init_controls(HWND hwnd) {
    if (!s_settings_mgr) return;
    auto& settings = s_settings_mgr->get();

    CheckDlgButton(hwnd, IDC_CHECK_AUTOSTART, settings.run_at_startup ? BST_CHECKED : BST_UNCHECKED);
    CheckDlgButton(hwnd, IDC_CHECK_WATCHDOG, settings.enable_fix_watchdog ? BST_CHECKED : BST_UNCHECKED);

    HWND hSlider = GetDlgItem(hwnd, IDC_SLIDER_BRIGHTNESS);
    if (hSlider) {
        SendMessageW(hSlider, TBM_SETRANGE, TRUE, MAKELPARAM(0, 500));
        SendMessageW(hSlider, TBM_SETPOS, TRUE, static_cast<LPARAM>(settings.preferred_sdr_brightness_nits));
    }

    HWND hCombo = GetDlgItem(hwnd, IDC_COMBO_LOGLEVEL);
    if (hCombo) {
        SendMessageW(hCombo, CB_ADDSTRING, 0, (LPARAM)L"Error");
        SendMessageW(hCombo, CB_ADDSTRING, 0, (LPARAM)L"Warning");
        SendMessageW(hCombo, CB_ADDSTRING, 0, (LPARAM)L"Info");
        SendMessageW(hCombo, CB_ADDSTRING, 0, (LPARAM)L"Debug");
        SendMessageW(hCombo, CB_SETCURSEL, 2, 0); // Info
    }

    update_labels(hwnd);
}

void SettingsWindow::save_settings(HWND hwnd) {
    if (!s_settings_mgr) return;
    auto settings = s_settings_mgr->get();

    settings.run_at_startup = IsDlgButtonChecked(hwnd, IDC_CHECK_AUTOSTART) == BST_CHECKED;
    settings.enable_fix_watchdog = IsDlgButtonChecked(hwnd, IDC_CHECK_WATCHDOG) == BST_CHECKED;

    HWND hSlider = GetDlgItem(hwnd, IDC_SLIDER_BRIGHTNESS);
    if (hSlider) {
        LRESULT pos = SendMessageW(hSlider, TBM_GETPOS, 0, 0);
        settings.preferred_sdr_brightness_nits = static_cast<float>(pos);
    }

    s_settings_mgr->get_mut() = settings;
    s_settings_mgr->save();
}

void SettingsWindow::update_labels(HWND hwnd) {
    HWND hSlider = GetDlgItem(hwnd, IDC_SLIDER_BRIGHTNESS);
    if (hSlider) {
        LRESULT pos = SendMessageW(hSlider, TBM_GETPOS, 0, 0);
        std::wstring text = std::to_wstring(pos) + L" nits";
        SetDlgItemTextW(hwnd, IDC_LBL_BRIGHTNESS, text.c_str());
    }
}

} // namespace hdrfixer::ui
