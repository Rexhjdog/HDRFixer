# HDRFixer

A comprehensive Windows 11 HDR fix and enhancement toolkit for OLED displays. Addresses well-documented flaws in Windows HDR implementation through automated fixes, a background monitoring service, and a modern WinUI 3 GUI.

## The Problem

Windows 11's HDR implementation has several issues that degrade image quality:

- **Washed-out SDR content** — Windows uses a piecewise sRGB transfer function instead of gamma 2.2, causing milky blacks and reduced contrast
- **Incorrect SDR brightness** — Default SDR white level is often wrong for your display
- **Suboptimal pixel format** — GPU may output YCbCr 4:2:2 or limited range RGB instead of full RGB 10-bit
- **Scattered Auto HDR controls** — Settings spread across multiple registry keys with no unified UI
- **ICC profile conflicts** — SDR color profiles interfere with HDR mode
- **No OLED burn-in protection** — Windows lacks built-in features for OLED panel care

## Features

### HDR Fixes
| Fix | Description |
|-----|-------------|
| **SDR Tone Curve Correction** | Generates an MHC2 ICC profile with a gamma 2.2 LUT to replace Windows' broken piecewise sRGB transfer function |
| **SDR Brightness Optimization** | Auto-calculates optimal SDR white level based on display class (OLED/HDR600/HDR400) |
| **Auto HDR Configuration** | Unified control for global toggle, per-game overrides, and debug split-screen |
| **Pixel Format Verification** | Checks for 10-bit RGB full range output via DXGI |
| **ICC Profile Conflict Resolution** | Detects and manages conflicting color profiles |
| **EDID Validation** | Display capability detection and HDR metadata verification |

### OLED Protection
- Pixel shift (subtle viewport shifting)
- Auto-hide taskbar when idle
- Dark mode enforcement
- Static content detection with configurable timeout
- Usage tracking with pixel refresh reminders

### GUI
Six-page WinUI 3 dashboard:
- **Dashboard** — Display status, health score (0-100), quick Fix All / Diagnose buttons
- **Fixes** — Apply/revert individual fixes with status indicators
- **Auto HDR** — Global toggle, per-game overrides, debug split-screen
- **OLED Protection** — Burn-in prevention toggles and usage tracking
- **Diagnostics** — Full system report with export to file
- **Settings** — Startup, tray, service, and watchdog configuration

### Background Service
- **Display Monitor** — Watches for display changes and re-applies fixes
- **Fix Watchdog** — Periodically verifies fixes haven't been reverted by Windows Updates
- **IPC** — Named pipes communication between GUI and service

## Architecture

```
HDRFixer.Core (class library)      ← shared logic
    │
    ├── HDRFixer.App (WinUI 3 GUI)
    │       │
    │       └── Named Pipes IPC
    │               │
    └── HDRFixer.Service (Windows Service)
```

| Component | Technology | Purpose |
|-----------|-----------|---------|
| HDRFixer.Core | .NET 8 class library | Display detection, color math, MHC2 profiles, registry management |
| HDRFixer.App | WinUI 3 + CommunityToolkit.Mvvm | Dashboard GUI with 6 pages |
| HDRFixer.Service | .NET 8 Worker Service | Background monitoring and fix watchdog |

## Requirements

- Windows 11 (23H2 or later)
- .NET 8 Runtime
- Windows App SDK 1.6 Runtime
- HDR-capable display

## Building

```bash
# Clone
git clone https://github.com/Rexhjdog/HDRFixer.git
cd HDRFixer

# Build
dotnet build

# Run tests
dotnet test

# Publish
dotnet publish src/HDRFixer.App -c Release -r win-x64 --self-contained false -p:Platform=x64
dotnet publish src/HDRFixer.Service -c Release -r win-x64 --self-contained false
```

## Running

```bash
# Run the GUI app
dotnet run --project src/HDRFixer.App -p:Platform=x64

# Install and start the background service
sc create HDRFixerService binPath="<path>\HDRFixer.Service.exe"
sc start HDRFixerService
```

## Technical References

- [dylanraga/win11hdr-srgb-to-gamma2.2-icm](https://github.com/dylanraga/win11hdr-srgb-to-gamma2.2-icm) — Gamma 2.2 MHC2 ICC profile technique
- [dantmnf/MHC2](https://github.com/dantmnf/MHC2Gen) — MHC2 ICC profile generation documentation
- [Microsoft: Display Calibration MHC Pipeline](https://learn.microsoft.com/en-us/windows-hardware/drivers/display/) — MHC2 profile specification
- [Microsoft: Use DirectX with Advanced Color](https://learn.microsoft.com/en-us/windows/win32/direct3darticles/high-dynamic-range) — HDR programming guide

## License

MIT
