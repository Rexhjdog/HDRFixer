#pragma once
#include <windows.h>
#include <winreg.h>
#include <thread>
#include <atomic>
#include <functional>

namespace hdrfixer::fixes {

/// Background thread that monitors the GraphicsDrivers registry key for changes.
/// When a change is detected (or a 60-second timeout elapses), calls the
/// user-supplied callback.  Thread-safe start/stop via atomic flag.
class Watchdog {
public:
    /// Construct with a callback that will be invoked on every detected change.
    explicit Watchdog(std::function<void()> on_change);
    ~Watchdog();

    Watchdog(const Watchdog&) = delete;
    Watchdog& operator=(const Watchdog&) = delete;

    /// Start the background monitoring thread.  No-op if already running.
    void start();

    /// Signal the background thread to stop and join it.  No-op if not running.
    void stop();

    /// Returns true if the watchdog thread is currently active.
    bool running() const noexcept { return running_.load(std::memory_order_relaxed); }

private:
    void thread_func();

    std::function<void()> on_change_;
    std::atomic<bool>     running_{false};
    HANDLE                stop_event_{nullptr};
    std::thread           thread_;
};

} // namespace hdrfixer::fixes
