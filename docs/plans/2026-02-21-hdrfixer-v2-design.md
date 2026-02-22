# HDRFixer v2 — Full C++ Native Rebuild Design

## Overview
Complete rewrite of HDRFixer as a single native C++20 Win32 executable (~500KB-1MB) with zero runtime dependencies. Focuses on getting core HDR fixes working perfectly with proper error handling, per-display targeting, and auto-recovery.

## Architecture

```
HDRFixer.exe (static CRT, no dependencies)
├── Core Layer
│   ├── display/       — DXGI 1.6 enumeration, EDID parsing, HDR state, DisplayConfig
│   ├── color/         — Transfer functions (sRGB, PQ, gamma 2.2), LUT generation
│   ├── profile/       — MHC2 ICC profile generation, WCS install/uninstall
│   ├── registry/      — HDR registry read/write, backup/restore, change notification
│   └── config/        — JSON settings persistence
│
├── Fix Engine
│   ├── fix_engine     — Fix registry, apply/revert/diagnose lifecycle
│   ├── watchdog       — Re-apply after Windows Update reverts, timer-based polling
│   ├── hotplug        — WM_DISPLAYCHANGE + RegisterDeviceNotification
│   └── share_helper   — Hotkey-triggered SDR white level reduction for screen sharing
│
├── UI
│   ├── tray           — Shell_NotifyIcon, context menu, global hotkeys
│   ├── settings_wnd   — Win32 dialog for configuration
│   └── notify         — Toast/balloon notifications for state changes
│
└── Service (optional)
    ├── service_main   — SCM registration for boot-time operation
    └── ipc            — Named pipe for GUI↔Service communication
```

## Tech Stack
- **Language:** C++20 (MSVC)
- **Build:** CMake 3.20+
- **UI:** Win32 API (Shell_NotifyIcon, DialogBox)
- **Graphics:** DXGI 1.6 (display detection only, no rendering)
- **Color:** Windows Color System (Mscms.dll) for ICC profile management
- **Linking:** Static CRT (/MT), no external dependencies

## Core Fixes

### Fix 1: Gamma Correction (HIGH confidence)
- Generate MHC2 ICC profile: sRGB EOTF → linear → gamma 2.2 inverse EOTF
- LUT: 4096 entries for HDR, 1024 for SDR
- Install via WcsAssociateColorProfileWithDevice
- Target correct display via adapter LUID + source/target ID from QueryDisplayConfig
- Revert: uninstall profile, restore previous association

### Fix 2: SDR White Level (MEDIUM confidence — needs spike)
- Read via DISPLAYCONFIG_SDR_WHITE_LEVEL + DisplayConfigGetDeviceInfo
- Calculate optimal based on EDID max luminance
- Write via DisplayConfigSetDeviceInfo or registry fallback
- Manual override via UI slider
- Screen-share mode: hotkey sets to 80 nits, restores on second press

### Fix 3: HDR Registry Watchdog (HIGH confidence)
- Monitor HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers
- RegNotifyChangeKeyValue for real-time change detection
- Backup registry state before any modification
- Auto-restore if external process (Windows Update) reverts settings

### Fix 4: Pixel Format Detection (HIGH detect / warn-only)
- Query IDXGIOutput6::GetDesc1() for ColorSpace and bit depth
- Detect 8-bit YCbCr vs 10-bit RGB full range
- Warn user with driver-specific instructions (not auto-fix)

### Fix 5: Screen-Share Helper (HIGH confidence with hotkey approach)
- Global hotkey (Ctrl+Shift+S) toggles "share mode"
- Share mode: temporarily sets SDR white level to 80 nits on all displays
- Second press: restores original SDR white levels
- Tray icon changes to indicate share mode active

### Fix 6: Display Hotplug (HIGH confidence)
- WM_DISPLAYCHANGE message handler
- RegisterDeviceNotification with GUID_DEVINTERFACE_MONITOR
- Re-enumerate displays on change
- Re-apply active fixes to new/changed displays

## Color Math

### Transfer Functions
```
sRGB EOTF:     L = (V <= 0.04045) ? V/12.92 : ((V+0.055)/1.055)^2.4
Gamma 2.2:     L = V^2.2
PQ ST.2084:    L = ((max(V^(1/m2) - c1, 0)) / (c2 - c3*V^(1/m2)))^(1/m1) * 10000
```

### MHC2 ICC Profile Structure
- ICC v4 header (128 bytes)
- Tag table: desc, cprt, rXYZ, gXYZ, bXYZ, wtpt, lumi, rTRC, gTRC, bTRC, MHC2
- MHC2 tag: 3x4 identity matrix + RGB LUT tables in S15.16 fixed-point
- D65 white point, BT.709 primaries

## Error Handling
- All Windows API calls check HRESULT/Win32 error codes
- Structured logging to %LOCALAPPDATA%\HDRFixer\hdrfixer.log
- User-visible errors via tray balloon notifications
- Admin elevation detection: if not admin, show UAC prompt or warn about limited functionality

## Settings
- JSON file at %LOCALAPPDATA%\HDRFixer\settings.json
- Per-display fix configuration (keyed by EDID serial or LUID)
- Global: auto-start, hotkeys, log level, watchdog interval

## Build
- CMake 3.20+ with MSVC toolchain
- Static CRT linking (/MT)
- Single output: HDRFixer.exe
- No installer needed (xcopy deployment), optional NSIS installer later
