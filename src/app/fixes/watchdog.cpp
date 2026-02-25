#include "watchdog.h"
#include "core/log/logger.h"
#include "core/registry/hdr_registry.h"
#include <format>

namespace hdrfixer::fixes {

// Timeout between forced re-checks even when no notification fires (ms).
static constexpr DWORD kFallbackTimeoutMs = 60'000;

Watchdog::Watchdog(std::function<void()> on_change)
    : on_change_(std::move(on_change))
{
    // Manual-reset event used to signal the thread to stop.
    stop_event_ = ::CreateEventW(nullptr, TRUE, FALSE, nullptr);
    if (!stop_event_) {
        LOG_ERROR("Watchdog: failed to create stop event");
    }
}

Watchdog::~Watchdog()
{
    stop();
    if (stop_event_) {
        ::CloseHandle(stop_event_);
    }
}

void Watchdog::start()
{
    // Use exchange to atomically check-and-set, preventing concurrent start() calls
    bool was_running = running_.exchange(true, std::memory_order_acq_rel);
    if (was_running) {
        return; // already running
    }

    if (!stop_event_) {
        LOG_ERROR("Watchdog: cannot start -- stop event is invalid");
        running_.store(false, std::memory_order_release);
        return;
    }

    // Ensure the stop event is non-signaled before launching the thread.
    ::ResetEvent(stop_event_);
    thread_ = std::thread(&Watchdog::thread_func, this);
    LOG_INFO("Watchdog: started");
}

void Watchdog::stop()
{
    bool was_running = running_.exchange(false, std::memory_order_acq_rel);

    // Wake the thread out of WaitForMultipleObjects.
    if (stop_event_) {
        ::SetEvent(stop_event_);
    }

    // Always join if joinable — thread_func may have set running_ to false
    // before we got here, but the thread still needs to be joined.
    if (thread_.joinable()) {
        thread_.join();
    }

    if (was_running) {
        LOG_INFO("Watchdog: stopped");
    }
}

void Watchdog::thread_func()
{
    // Open the registry key for notifications.
    HKEY hkey = nullptr;
    LONG rc = ::RegOpenKeyExW(
        HKEY_LOCAL_MACHINE,
        registry::kGraphicsDrivers,
        0,
        KEY_NOTIFY,
        &hkey);

    if (rc != ERROR_SUCCESS) {
        LOG_ERROR(std::format("Watchdog: RegOpenKeyExW failed ({})", rc));
        running_.store(false, std::memory_order_release);
        return;
    }

    // Auto-reset event that RegNotifyChangeKeyValue will signal.
    HANDLE reg_event = ::CreateEventW(nullptr, FALSE, FALSE, nullptr);
    if (!reg_event) {
        LOG_ERROR("Watchdog: failed to create registry event");
        ::RegCloseKey(hkey);
        running_.store(false, std::memory_order_release);
        return;
    }

    HANDLE wait_handles[2] = { stop_event_, reg_event };

    while (running_.load(std::memory_order_acquire)) {
        // Register for asynchronous notification of any change under the key.
        rc = ::RegNotifyChangeKeyValue(
            hkey,
            TRUE,                                 // watch subtree
            REG_NOTIFY_CHANGE_LAST_SET |          // value changes
                REG_NOTIFY_CHANGE_NAME,           // subkey add/delete
            reg_event,
            TRUE);                                // async

        if (rc != ERROR_SUCCESS) {
            LOG_ERROR(std::format("Watchdog: RegNotifyChangeKeyValue failed ({})", rc));
            break;
        }

        DWORD result = ::WaitForMultipleObjects(
            2, wait_handles,
            FALSE,                    // wait for ANY handle
            kFallbackTimeoutMs);      // 60-second fallback

        if (!running_.load(std::memory_order_acquire)) {
            break; // stop() was called
        }

        switch (result) {
        case WAIT_OBJECT_0:
            // stop_event_ signaled -- exit loop.
            break;

        case WAIT_OBJECT_0 + 1:
            // Registry change detected.
            LOG_INFO("Watchdog: registry change detected");
            if (on_change_) {
                on_change_();
            }
            break;

        case WAIT_TIMEOUT:
            // Periodic fallback fire.
            LOG_DEBUG("Watchdog: fallback timeout -- re-checking");
            if (on_change_) {
                on_change_();
            }
            break;

        default:
            LOG_ERROR(std::format("Watchdog: WaitForMultipleObjects unexpected result ({})", result));
            break;
        }
    }

    ::CloseHandle(reg_event);
    ::RegCloseKey(hkey);
    running_.store(false, std::memory_order_release);
}

} // namespace hdrfixer::fixes
