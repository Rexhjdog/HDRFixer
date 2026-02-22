#pragma once
#include <windows.h>
#include <dbt.h>

namespace hdrfixer::fixes {

/// Manages registration for display (monitor) hotplug notifications
/// via RegisterDeviceNotification / WM_DEVICECHANGE.
class Hotplug {
public:
    Hotplug() = default;
    ~Hotplug();

    Hotplug(const Hotplug&) = delete;
    Hotplug& operator=(const Hotplug&) = delete;

    /// Register the given window to receive WM_DEVICECHANGE for monitors.
    /// Returns true on success.
    bool register_hotplug(HWND hwnd);

    /// Unregister previously registered device notification.
    void unregister_hotplug();

    /// Check whether a WM_DEVICECHANGE message represents a monitor
    /// arrival or removal.  Call this from your window procedure when
    /// you receive WM_DEVICECHANGE.
    static bool is_display_change_event(WPARAM wParam, LPARAM lParam);

private:
    HDEVNOTIFY dev_notify_{nullptr};
};

} // namespace hdrfixer::fixes
