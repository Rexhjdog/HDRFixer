#include "app/ui/settings_wnd.h"

namespace hdrfixer::ui {

void show_settings_window(HWND parent) {
    MessageBoxW(
        parent,
        L"Settings configuration will be available in a future update.",
        L"HDRFixer - Settings - Coming Soon",
        MB_OK | MB_ICONINFORMATION
    );
}

} // namespace hdrfixer::ui
