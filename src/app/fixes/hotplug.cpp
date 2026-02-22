#include "hotplug.h"
#include "../../core/log/logger.h"

#include <initguid.h>

namespace hdrfixer::fixes {

// {E6F07B5F-EE97-4a90-B076-33F57BF4EAA7}
static const GUID GUID_DEVINTERFACE_MONITOR_LOCAL =
    {0xe6f07b5f, 0xee97, 0x4a90, {0xb0, 0x76, 0x33, 0xf5, 0x7b, 0xf4, 0xea, 0xa7}};

Hotplug::~Hotplug()
{
    unregister_hotplug();
}

bool Hotplug::register_hotplug(HWND hwnd)
{
    if (dev_notify_) {
        // Already registered -- unregister first to allow re-registration.
        unregister_hotplug();
    }

    DEV_BROADCAST_DEVICEINTERFACE filter{};
    filter.dbcc_size       = sizeof(filter);
    filter.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
    filter.dbcc_classguid  = GUID_DEVINTERFACE_MONITOR_LOCAL;

    dev_notify_ = ::RegisterDeviceNotificationW(
        hwnd,
        &filter,
        DEVICE_NOTIFY_WINDOW_HANDLE);

    if (!dev_notify_) {
        LOG_ERROR("Hotplug: RegisterDeviceNotification failed");
        return false;
    }

    LOG_INFO("Hotplug: registered for monitor device notifications");
    return true;
}

void Hotplug::unregister_hotplug()
{
    if (dev_notify_) {
        ::UnregisterDeviceNotification(dev_notify_);
        dev_notify_ = nullptr;
        LOG_INFO("Hotplug: unregistered device notifications");
    }
}

bool Hotplug::is_display_change_event(WPARAM wParam, LPARAM lParam)
{
    // We only care about device arrival and removal.
    if (wParam != DBT_DEVICEARRIVAL && wParam != DBT_DEVICEREMOVECOMPLETE) {
        return false;
    }

    if (!lParam) {
        return false;
    }

    auto* hdr = reinterpret_cast<const DEV_BROADCAST_HDR*>(lParam);
    if (hdr->dbch_devicetype != DBT_DEVTYP_DEVICEINTERFACE) {
        return false;
    }

    auto* dev = reinterpret_cast<const DEV_BROADCAST_DEVICEINTERFACE*>(lParam);
    return ::IsEqualGUID(dev->dbcc_classguid, GUID_DEVINTERFACE_MONITOR_LOCAL);
}

} // namespace hdrfixer::fixes
