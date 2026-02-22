# HDRFixer v2 — C++ Native Rebuild Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Rewrite HDRFixer as a single native C++20 Win32 executable with zero runtime dependencies, fixing all bugs from the C# version and adding proper error handling, per-display targeting, and auto-recovery.

**Architecture:** Layered C++20 modules — core (display detection, color math, ICC profiles, registry), fix engine (apply/revert/diagnose lifecycle with watchdog), and UI (system tray with hotkeys). Build with CMake + MSVC, static CRT, single .exe output.

**Tech Stack:** C++20, CMake 3.20+, MSVC, Win32 API, DXGI 1.6, WCS/Mscms, doctest (testing)

---

## Task 1: Project Scaffolding & Build System

**Files:**
- Create: `src/CMakeLists.txt` (root)
- Create: `src/core/CMakeLists.txt`
- Create: `src/app/CMakeLists.txt`
- Create: `tests/CMakeLists.txt`
- Create: `src/app/main.cpp`
- Create: `src/core/core.h` (precompiled header)
- Create: `tests/doctest.h` (single-header test framework)
- Create: `tests/test_main.cpp`

**Step 1: Create root CMakeLists.txt**

```cmake
cmake_minimum_required(VERSION 3.20)
project(HDRFixer VERSION 2.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

# Windows 11 SDK target
add_compile_definitions(
    WINVER=0x0A00
    _WIN32_WINNT=0x0A00
    UNICODE
    _UNICODE
    NOMINMAX
    WIN32_LEAN_AND_MEAN
)

add_subdirectory(src/core)
add_subdirectory(src/app)

option(BUILD_TESTS "Build tests" ON)
if(BUILD_TESTS)
    enable_testing()
    add_subdirectory(tests)
endif()
```

**Step 2: Create core library CMakeLists.txt**

```cmake
add_library(hdrfixer_core STATIC
    color/transfer_functions.cpp
    color/gamma_lut.cpp
    display/dxgi_detector.cpp
    display/display_config.cpp
    display/edid_reader.cpp
    profile/mhc2_writer.cpp
    profile/wcs_installer.cpp
    registry/hdr_registry.cpp
    registry/registry_backup.cpp
    config/settings.cpp
    log/logger.cpp
)

target_include_directories(hdrfixer_core PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/..)
target_link_libraries(hdrfixer_core PUBLIC
    dxgi.lib
    user32.lib
    shell32.lib
    advapi32.lib
    mscms.lib
    ole32.lib
)
target_precompile_headers(hdrfixer_core PUBLIC core.h)
```

**Step 3: Create app CMakeLists.txt**

```cmake
add_executable(HDRFixer WIN32
    main.cpp
    ui/tray.cpp
    ui/settings_wnd.cpp
    fixes/fix_engine.cpp
    fixes/gamma_fix.cpp
    fixes/sdr_brightness_fix.cpp
    fixes/pixel_format_fix.cpp
    fixes/share_helper.cpp
    fixes/watchdog.cpp
    fixes/hotplug.cpp
    resource.rc
)

target_link_libraries(HDRFixer PRIVATE hdrfixer_core comctl32.lib)
```

**Step 4: Create test CMakeLists.txt**

```cmake
# Download doctest header
file(DOWNLOAD
    "https://raw.githubusercontent.com/doctest/doctest/v2.4.11/doctest/doctest.h"
    "${CMAKE_CURRENT_SOURCE_DIR}/doctest.h"
    EXPECTED_HASH SHA256=3a5846ab7be60e40e30b7011e19299a920abd52ec1b5b1a67b4fbb7c3b3f4910
    STATUS DOWNLOAD_STATUS
)

add_executable(hdrfixer_tests
    test_main.cpp
    test_transfer_functions.cpp
    test_gamma_lut.cpp
    test_edid_reader.cpp
    test_mhc2_writer.cpp
    test_sdr_white_level.cpp
    test_display_info.cpp
    test_fix_engine.cpp
    test_registry.cpp
    test_settings.cpp
)

target_link_libraries(hdrfixer_tests PRIVATE hdrfixer_core)
target_include_directories(hdrfixer_tests PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})

add_test(NAME unit_tests COMMAND hdrfixer_tests)
```

**Step 5: Create core.h precompiled header**

```cpp
#pragma once
#define WINVER 0x0A00
#define _WIN32_WINNT 0x0A00
#define NOMINMAX
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <dxgi1_6.h>
#include <wrl/client.h>
#include <icm.h>
#include <shellapi.h>

#include <cstdint>
#include <cmath>
#include <string>
#include <vector>
#include <optional>
#include <expected>
#include <format>
#include <fstream>
#include <filesystem>
#include <functional>
#include <memory>
#include <mutex>
#include <thread>
#include <chrono>
#include <algorithm>
#include <array>
#include <map>
#include <span>
```

**Step 6: Create minimal main.cpp**

```cpp
#include "core/core.h"

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE, LPWSTR, int) {
    // Prevent multiple instances
    HANDLE hMutex = CreateMutexW(nullptr, TRUE, L"HDRFixerSingletonV2");
    if (GetLastError() == ERROR_ALREADY_EXISTS) {
        CloseHandle(hMutex);
        return 0;
    }

    MSG msg;
    while (GetMessage(&msg, nullptr, 0, 0)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    CloseHandle(hMutex);
    return static_cast<int>(msg.wParam);
}
```

**Step 7: Create test_main.cpp**

```cpp
#define DOCTEST_CONFIG_IMPLEMENT_WITH_MAIN
#include "doctest.h"
```

**Step 8: Verify build compiles**

Run: `cmake -B build -G "Visual Studio 17 2022" -A x64 src && cmake --build build --config Debug`
Expected: Compiles with 0 errors (some files will be stubs)

**Step 9: Commit**

```bash
git add -A
git commit -m "feat: scaffold C++20 project with CMake, core lib, app, and test targets"
```

---

## Task 2: Core — Transfer Functions

**Files:**
- Create: `src/core/color/transfer_functions.h`
- Create: `src/core/color/transfer_functions.cpp`
- Create: `tests/test_transfer_functions.cpp`

**Step 1: Write the failing tests**

```cpp
#include "doctest.h"
#include "core/color/transfer_functions.h"

using namespace hdrfixer::color;

TEST_CASE("sRGB EOTF") {
    CHECK(srgb_eotf(0.0) == doctest::Approx(0.0));
    CHECK(srgb_eotf(1.0) == doctest::Approx(1.0));
    CHECK(srgb_eotf(0.5) == doctest::Approx(0.214041).epsilon(0.001));
    // Below linear threshold
    CHECK(srgb_eotf(0.04045) == doctest::Approx(0.04045 / 12.92).epsilon(0.0001));
}

TEST_CASE("sRGB round-trip") {
    for (double v = 0.0; v <= 1.0; v += 0.1) {
        CHECK(srgb_inv_eotf(srgb_eotf(v)) == doctest::Approx(v).epsilon(0.0001));
    }
}

TEST_CASE("PQ EOTF") {
    CHECK(pq_eotf(0.0) == doctest::Approx(0.0).epsilon(0.01));
    CHECK(pq_eotf(1.0) == doctest::Approx(10000.0).epsilon(1.0));
    // 500 nits round-trip
    double pq500 = pq_inv_eotf(500.0);
    CHECK(pq_eotf(pq500) == doctest::Approx(500.0).epsilon(0.1));
}

TEST_CASE("Gamma power law") {
    CHECK(gamma_eotf(0.5, 2.2) == doctest::Approx(std::pow(0.5, 2.2)).epsilon(0.0001));
    CHECK(gamma_inv_eotf(0.5, 2.2) == doctest::Approx(std::pow(0.5, 1.0/2.2)).epsilon(0.0001));
}

TEST_CASE("sRGB brighter than gamma 2.2 in shadows") {
    double srgb_mid = srgb_eotf(0.5);
    double g22_mid = gamma_eotf(0.5, 2.2);
    CHECK(srgb_mid > g22_mid);
}
```

**Step 2: Run test to verify it fails**

Run: `cmake --build build --config Debug --target hdrfixer_tests && build\tests\Debug\hdrfixer_tests.exe`
Expected: FAIL — header not found

**Step 3: Write the implementation**

```cpp
// transfer_functions.h
#pragma once
#include <cmath>
#include <algorithm>

namespace hdrfixer::color {

// sRGB IEC 61966-2-1
inline constexpr double kSrgbLinearThreshold = 0.04045;
inline constexpr double kSrgbLinearScale = 12.92;
inline constexpr double kSrgbGammaOffset = 0.055;
inline constexpr double kSrgbGammaBase = 1.055;
inline constexpr double kSrgbGammaExponent = 2.4;
inline constexpr double kSrgbInvLinearThreshold = 0.0031308;

// PQ ST 2084
inline constexpr double kPqM1 = 2610.0 / 16384.0;
inline constexpr double kPqM2 = 128.0 * 2523.0 / 4096.0;
inline constexpr double kPqC1 = 3424.0 / 4096.0;
inline constexpr double kPqC2 = 32.0 * 2413.0 / 4096.0;
inline constexpr double kPqC3 = 32.0 * 2392.0 / 4096.0;
inline constexpr double kPqMaxNits = 10000.0;

double srgb_eotf(double v);
double srgb_inv_eotf(double l);
double pq_eotf(double v);       // returns nits
double pq_inv_eotf(double nits); // returns PQ signal
double gamma_eotf(double v, double gamma);
double gamma_inv_eotf(double l, double gamma);

} // namespace hdrfixer::color
```

```cpp
// transfer_functions.cpp
#include "transfer_functions.h"

namespace hdrfixer::color {

double srgb_eotf(double v) {
    if (v <= kSrgbLinearThreshold)
        return v / kSrgbLinearScale;
    return std::pow((v + kSrgbGammaOffset) / kSrgbGammaBase, kSrgbGammaExponent);
}

double srgb_inv_eotf(double l) {
    if (l <= kSrgbInvLinearThreshold)
        return l * kSrgbLinearScale;
    return kSrgbGammaBase * std::pow(l, 1.0 / kSrgbGammaExponent) - kSrgbGammaOffset;
}

double pq_eotf(double v) {
    double vp = std::pow(v, 1.0 / kPqM2);
    double num = std::max(vp - kPqC1, 0.0);
    double den = kPqC2 - kPqC3 * vp;
    if (den <= 0.0) return 0.0;
    return kPqMaxNits * std::pow(num / den, 1.0 / kPqM1);
}

double pq_inv_eotf(double nits) {
    double y = std::pow(nits / kPqMaxNits, kPqM1);
    return std::pow((kPqC1 + kPqC2 * y) / (1.0 + kPqC3 * y), kPqM2);
}

double gamma_eotf(double v, double gamma) {
    return std::pow(v, gamma);
}

double gamma_inv_eotf(double l, double gamma) {
    return std::pow(l, 1.0 / gamma);
}

} // namespace hdrfixer::color
```

**Step 4: Run test to verify it passes**

Run: `cmake --build build --config Debug --target hdrfixer_tests && build\tests\Debug\hdrfixer_tests.exe -tc="sRGB*,PQ*,Gamma*,sRGB brighter*"`
Expected: All PASS

**Step 5: Commit**

```bash
git add src/core/color/transfer_functions.h src/core/color/transfer_functions.cpp tests/test_transfer_functions.cpp
git commit -m "feat: add transfer functions (sRGB, PQ ST.2084, gamma power law)"
```

---

## Task 3: Core — Gamma Correction LUT Generation

**Files:**
- Create: `src/core/color/gamma_lut.h`
- Create: `src/core/color/gamma_lut.cpp`
- Create: `tests/test_gamma_lut.cpp`

**Step 1: Write the failing tests**

```cpp
#include "doctest.h"
#include "core/color/gamma_lut.h"

using namespace hdrfixer::color;

TEST_CASE("SDR LUT basics") {
    auto lut = generate_sdr_lut(1024);
    CHECK(lut.size() == 1024);
    CHECK(lut.front() == doctest::Approx(0.0).epsilon(0.001));
    CHECK(lut.back() == doctest::Approx(1.0).epsilon(0.001));
}

TEST_CASE("SDR LUT monotonically increasing") {
    auto lut = generate_sdr_lut(1024);
    for (size_t i = 1; i < lut.size(); ++i) {
        CHECK(lut[i] >= lut[i - 1]);
    }
}

TEST_CASE("SDR LUT midpoint darker than linear") {
    auto lut = generate_sdr_lut(1024);
    CHECK(lut[512] < 0.5);
}

TEST_CASE("HDR LUT basics") {
    auto lut = generate_hdr_lut(4096, 200.0, 0.0);
    CHECK(lut.size() == 4096);
    CHECK(lut.front() == doctest::Approx(0.0).epsilon(0.001));
}

TEST_CASE("HDR LUT passthrough above white level") {
    auto lut = generate_hdr_lut(4096, 200.0, 0.0);
    // Values above SDR white should be passthrough (lut[i] == input)
    // The last entry maps PQ=1.0 (10000 nits) which is above 200 nits
    CHECK(lut.back() == doctest::Approx(1.0).epsilon(0.001));
}

TEST_CASE("HDR LUT monotonically increasing") {
    auto lut = generate_hdr_lut(4096, 200.0, 0.0);
    for (size_t i = 1; i < lut.size(); ++i) {
        CHECK(lut[i] >= lut[i - 1]);
    }
}
```

**Step 2: Run test to verify it fails**

Run: `cmake --build build --config Debug --target hdrfixer_tests && build\tests\Debug\hdrfixer_tests.exe -tc="SDR LUT*,HDR LUT*"`
Expected: FAIL — header not found

**Step 3: Write the implementation**

```cpp
// gamma_lut.h
#pragma once
#include <vector>

namespace hdrfixer::color {

std::vector<double> generate_sdr_lut(int size = 1024);
std::vector<double> generate_hdr_lut(int size = 4096, double white_nits = 200.0, double black_nits = 0.0);

} // namespace hdrfixer::color
```

```cpp
// gamma_lut.cpp
#include "gamma_lut.h"
#include "transfer_functions.h"

namespace hdrfixer::color {

std::vector<double> generate_sdr_lut(int size) {
    std::vector<double> lut(size);
    for (int i = 0; i < size; ++i) {
        double input = static_cast<double>(i) / (size - 1);
        double linear = srgb_eotf(input);
        lut[i] = gamma_inv_eotf(linear, 2.2);
    }
    return lut;
}

std::vector<double> generate_hdr_lut(int size, double white_nits, double black_nits) {
    std::vector<double> lut(size);
    for (int i = 0; i < size; ++i) {
        double pq_input = static_cast<double>(i) / (size - 1);
        double nits = pq_eotf(pq_input);

        if (nits > white_nits) {
            lut[i] = pq_input; // passthrough above SDR range
            continue;
        }

        double normalized = (white_nits > 0.0) ? nits / white_nits : 0.0;
        double srgb_signal = srgb_inv_eotf(normalized);
        double corrected_nits = (white_nits - black_nits) * std::pow(srgb_signal, 2.2) + black_nits;
        lut[i] = pq_inv_eotf(corrected_nits);
    }
    return lut;
}

} // namespace hdrfixer::color
```

**Step 4: Run test to verify it passes**

Run: `cmake --build build --config Debug --target hdrfixer_tests && build\tests\Debug\hdrfixer_tests.exe -tc="SDR LUT*,HDR LUT*"`
Expected: All PASS

**Step 5: Commit**

```bash
git add src/core/color/gamma_lut.h src/core/color/gamma_lut.cpp tests/test_gamma_lut.cpp
git commit -m "feat: add gamma correction LUT generation (SDR and HDR)"
```

---

## Task 4: Core — Display Detection (DXGI + DisplayConfig)

**Files:**
- Create: `src/core/display/display_info.h`
- Create: `src/core/display/dxgi_detector.h`
- Create: `src/core/display/dxgi_detector.cpp`
- Create: `src/core/display/display_config.h`
- Create: `src/core/display/display_config.cpp`
- Create: `tests/test_display_info.cpp`
- Create: `tests/test_sdr_white_level.cpp`

**Step 1: Write the failing tests**

```cpp
// test_display_info.cpp
#include "doctest.h"
#include "core/display/display_info.h"

using namespace hdrfixer::display;

TEST_CASE("DisplayInfo defaults") {
    DisplayInfo info{};
    CHECK(info.device_name.empty());
    CHECK(info.is_hdr_enabled == false);
    CHECK(info.bits_per_color == 0);
}

TEST_CASE("DisplayInfo HDR capable threshold") {
    DisplayInfo info{};
    info.max_luminance = 249.0f;
    CHECK(info.is_hdr_capable() == false);
    info.max_luminance = 251.0f;
    CHECK(info.is_hdr_capable() == true);
}

TEST_CASE("GpuVendor from ID") {
    CHECK(gpu_vendor_from_id(0x10DE) == GpuVendor::Nvidia);
    CHECK(gpu_vendor_from_id(0x1002) == GpuVendor::Amd);
    CHECK(gpu_vendor_from_id(0x8086) == GpuVendor::Intel);
    CHECK(gpu_vendor_from_id(0x0000) == GpuVendor::Unknown);
}
```

```cpp
// test_sdr_white_level.cpp
#include "doctest.h"
#include "core/display/display_config.h"

using namespace hdrfixer::display;

TEST_CASE("SDR white level raw to nits") {
    CHECK(raw_to_nits(1000) == doctest::Approx(80.0f));
    CHECK(raw_to_nits(2500) == doctest::Approx(200.0f));
    CHECK(raw_to_nits(5000) == doctest::Approx(400.0f));
}

TEST_CASE("SDR white level nits to raw") {
    CHECK(nits_to_raw(80.0f) == 1000);
    CHECK(nits_to_raw(200.0f) == 2500);
    CHECK(nits_to_raw(400.0f) == 5000);
}
```

**Step 2: Run test to verify it fails**

Run: `cmake --build build --config Debug --target hdrfixer_tests && build\tests\Debug\hdrfixer_tests.exe -tc="DisplayInfo*,SDR white*,GpuVendor*"`
Expected: FAIL

**Step 3: Write the implementation**

```cpp
// display_info.h
#pragma once
#include <string>
#include <cstdint>
#include <windows.h>

namespace hdrfixer::display {

enum class GpuVendor : uint32_t {
    Unknown = 0,
    Nvidia = 0x10DE,
    Amd = 0x1002,
    Intel = 0x8086,
};

inline GpuVendor gpu_vendor_from_id(uint32_t id) {
    switch (id) {
        case 0x10DE: return GpuVendor::Nvidia;
        case 0x1002: return GpuVendor::Amd;
        case 0x8086: return GpuVendor::Intel;
        default:     return GpuVendor::Unknown;
    }
}

struct DisplayInfo {
    std::wstring device_name;
    std::wstring monitor_name;
    std::wstring gpu_name;
    GpuVendor gpu_vendor = GpuVendor::Unknown;
    bool is_hdr_enabled = false;
    uint32_t bits_per_color = 0;
    float min_luminance = 0.0f;
    float max_luminance = 0.0f;
    float max_full_frame_luminance = 0.0f;
    float sdr_white_level_nits = 80.0f;
    float red_primary[2] = {};
    float green_primary[2] = {};
    float blue_primary[2] = {};
    float white_point[2] = {};
    LUID adapter_luid = {};
    uint32_t source_id = 0;
    uint32_t target_id = 0;

    bool is_hdr_capable() const { return max_luminance > 250.0f; }
};

} // namespace hdrfixer::display
```

```cpp
// display_config.h
#pragma once
#include "display_info.h"
#include <vector>
#include <expected>

namespace hdrfixer::display {

inline float raw_to_nits(uint32_t raw) {
    return static_cast<float>(raw) / 1000.0f * 80.0f;
}

inline uint32_t nits_to_raw(float nits) {
    return static_cast<uint32_t>(std::round(nits / 80.0f * 1000.0f));
}

struct DisplayPath {
    LUID adapter_id;
    uint32_t source_id;
    uint32_t target_id;
    float sdr_white_level_nits;
};

// Query active display paths and their SDR white levels
std::expected<std::vector<DisplayPath>, std::string> query_display_paths();

// Get SDR white level for a specific target
std::expected<float, std::string> get_sdr_white_level(LUID adapter_id, uint32_t target_id);

} // namespace hdrfixer::display
```

```cpp
// display_config.cpp
#include "display_config.h"
#include <vector>

namespace hdrfixer::display {

std::expected<std::vector<DisplayPath>, std::string> query_display_paths() {
    UINT32 path_count = 0, mode_count = 0;
    LONG result = GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, &path_count, &mode_count);
    if (result != ERROR_SUCCESS)
        return std::unexpected(std::format("GetDisplayConfigBufferSizes failed: {}", result));

    std::vector<DISPLAYCONFIG_PATH_INFO> paths(path_count);
    std::vector<DISPLAYCONFIG_MODE_INFO> modes(mode_count);
    result = QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS,
        &path_count, paths.data(), &mode_count, modes.data(), nullptr);
    if (result != ERROR_SUCCESS)
        return std::unexpected(std::format("QueryDisplayConfig failed: {}", result));

    std::vector<DisplayPath> display_paths;
    for (UINT32 i = 0; i < path_count; ++i) {
        DisplayPath dp{};
        dp.adapter_id = paths[i].targetInfo.adapterId;
        dp.source_id = paths[i].sourceInfo.id;
        dp.target_id = paths[i].targetInfo.id;

        auto nits = get_sdr_white_level(dp.adapter_id, dp.target_id);
        dp.sdr_white_level_nits = nits.value_or(80.0f);

        display_paths.push_back(dp);
    }
    return display_paths;
}

std::expected<float, std::string> get_sdr_white_level(LUID adapter_id, uint32_t target_id) {
    DISPLAYCONFIG_SDR_WHITE_LEVEL white_level = {};
    white_level.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_SDR_WHITE_LEVEL;
    white_level.header.size = sizeof(white_level);
    white_level.header.adapterId = adapter_id;
    white_level.header.id = target_id;

    LONG result = DisplayConfigGetDeviceInfo(&white_level.header);
    if (result != ERROR_SUCCESS)
        return std::unexpected(std::format("DisplayConfigGetDeviceInfo failed: {}", result));

    return raw_to_nits(white_level.SDRWhiteLevel);
}

} // namespace hdrfixer::display
```

```cpp
// dxgi_detector.h
#pragma once
#include "display_info.h"
#include <vector>
#include <expected>

namespace hdrfixer::display {

std::expected<std::vector<DisplayInfo>, std::string> detect_displays();

} // namespace hdrfixer::display
```

```cpp
// dxgi_detector.cpp
#include "dxgi_detector.h"
#include "display_config.h"
#include <dxgi1_6.h>
#include <wrl/client.h>

using Microsoft::WRL::ComPtr;

namespace hdrfixer::display {

std::expected<std::vector<DisplayInfo>, std::string> detect_displays() {
    ComPtr<IDXGIFactory6> factory;
    HRESULT hr = CreateDXGIFactory1(IID_PPV_ARGS(&factory));
    if (FAILED(hr))
        return std::unexpected(std::format("CreateDXGIFactory1 failed: 0x{:08X}", static_cast<unsigned>(hr)));

    // Query display paths for LUID/source/target mapping
    auto paths_result = query_display_paths();
    auto& paths = paths_result.has_value() ? paths_result.value() : std::vector<DisplayPath>{};

    std::vector<DisplayInfo> displays;

    ComPtr<IDXGIAdapter1> adapter;
    for (UINT ai = 0; factory->EnumAdapters1(ai, &adapter) != DXGI_ERROR_NOT_FOUND; ++ai) {
        DXGI_ADAPTER_DESC1 adapter_desc{};
        adapter->GetDesc1(&adapter_desc);

        if (adapter_desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE) {
            adapter.Reset();
            continue;
        }

        ComPtr<IDXGIOutput> output;
        for (UINT oi = 0; adapter->EnumOutputs(oi, &output) != DXGI_ERROR_NOT_FOUND; ++oi) {
            ComPtr<IDXGIOutput6> output6;
            if (SUCCEEDED(output.As(&output6))) {
                DXGI_OUTPUT_DESC1 desc{};
                output6->GetDesc1(&desc);

                DisplayInfo info{};
                info.device_name = desc.DeviceName;
                info.gpu_name = adapter_desc.Description;
                info.gpu_vendor = gpu_vendor_from_id(adapter_desc.VendorId);
                info.is_hdr_enabled = (desc.ColorSpace == DXGI_COLOR_SPACE_RGB_FULL_G2084_NONE_P2020);
                info.bits_per_color = desc.BitsPerColor;
                info.min_luminance = desc.MinLuminance;
                info.max_luminance = desc.MaxLuminance;
                info.max_full_frame_luminance = desc.MaxFullFrameLuminance;
                info.red_primary[0] = desc.RedPrimary[0];
                info.red_primary[1] = desc.RedPrimary[1];
                info.green_primary[0] = desc.GreenPrimary[0];
                info.green_primary[1] = desc.GreenPrimary[1];
                info.blue_primary[0] = desc.BluePrimary[0];
                info.blue_primary[1] = desc.BluePrimary[1];
                info.white_point[0] = desc.WhitePoint[0];
                info.white_point[1] = desc.WhitePoint[1];

                // Match with DisplayConfig path for LUID + source/target
                info.adapter_luid = adapter_desc.AdapterLuid;
                for (const auto& path : paths) {
                    if (path.adapter_id.LowPart == adapter_desc.AdapterLuid.LowPart &&
                        path.adapter_id.HighPart == adapter_desc.AdapterLuid.HighPart) {
                        info.source_id = path.source_id;
                        info.target_id = path.target_id;
                        info.sdr_white_level_nits = path.sdr_white_level_nits;
                        break;
                    }
                }

                displays.push_back(std::move(info));
            }
            output.Reset();
        }
        adapter.Reset();
    }

    return displays;
}

} // namespace hdrfixer::display
```

**Step 4: Run test to verify it passes**

Run: `cmake --build build --config Debug --target hdrfixer_tests && build\tests\Debug\hdrfixer_tests.exe -tc="DisplayInfo*,SDR white*,GpuVendor*"`
Expected: All PASS

**Step 5: Commit**

```bash
git add src/core/display/ tests/test_display_info.cpp tests/test_sdr_white_level.cpp
git commit -m "feat: add display detection (DXGI 1.6 + DisplayConfig + SDR white level)"
```

---

## Task 5: Core — EDID Reader

**Files:**
- Create: `src/core/display/edid_reader.h`
- Create: `src/core/display/edid_reader.cpp`
- Create: `tests/test_edid_reader.cpp`

**Step 1: Write the failing tests**

```cpp
#include "doctest.h"
#include "core/display/edid_reader.h"

using namespace hdrfixer::display;

TEST_CASE("EDID manufacturer decode") {
    // DEL = Dell: D=4, E=5, L=12 -> (4<<10)|(5<<5)|12 = 0x10AC
    uint8_t edid[128] = {};
    edid[8] = 0x10; edid[9] = 0xAC;
    auto info = parse_edid(edid, 128);
    CHECK(info.has_value());
    CHECK(info->manufacturer == "DEL");
}

TEST_CASE("EDID product code little-endian") {
    uint8_t edid[128] = {};
    edid[10] = 0x34; edid[11] = 0x12;
    auto info = parse_edid(edid, 128);
    CHECK(info.has_value());
    CHECK(info->product_code == 0x1234);
}

TEST_CASE("EDID monitor name from descriptor") {
    uint8_t edid[128] = {};
    // Descriptor at offset 54: type 0xFC = monitor name
    edid[54] = 0; edid[55] = 0; edid[56] = 0; edid[57] = 0xFC; edid[58] = 0;
    const char* name = "Test Monitor";
    std::memcpy(&edid[59], name, strlen(name));
    edid[59 + strlen(name)] = '\n';
    auto info = parse_edid(edid, 128);
    CHECK(info.has_value());
    CHECK(info->monitor_name == "Test Monitor");
}

TEST_CASE("EDID rejects short data") {
    uint8_t edid[64] = {};
    auto info = parse_edid(edid, 64);
    CHECK(!info.has_value());
}
```

**Step 2: Run test to verify it fails**

Run: `cmake --build build --config Debug --target hdrfixer_tests && build\tests\Debug\hdrfixer_tests.exe -tc="EDID*"`
Expected: FAIL

**Step 3: Write the implementation**

```cpp
// edid_reader.h
#pragma once
#include <cstdint>
#include <string>
#include <optional>
#include <span>

namespace hdrfixer::display {

struct EdidInfo {
    std::string manufacturer;
    uint16_t product_code = 0;
    std::string monitor_name;
    uint32_t serial_number = 0;
};

std::optional<EdidInfo> parse_edid(const uint8_t* data, size_t length);

} // namespace hdrfixer::display
```

```cpp
// edid_reader.cpp
#include "edid_reader.h"
#include <cstring>
#include <algorithm>

namespace hdrfixer::display {

std::optional<EdidInfo> parse_edid(const uint8_t* data, size_t length) {
    if (!data || length < 128)
        return std::nullopt;

    EdidInfo info{};

    // Manufacturer ID (bytes 8-9, compressed 3-letter ASCII)
    uint16_t mfg = (static_cast<uint16_t>(data[8]) << 8) | data[9];
    char c1 = static_cast<char>(((mfg >> 10) & 0x1F) + 'A' - 1);
    char c2 = static_cast<char>(((mfg >> 5) & 0x1F) + 'A' - 1);
    char c3 = static_cast<char>((mfg & 0x1F) + 'A' - 1);
    info.manufacturer = {c1, c2, c3};

    // Product code (bytes 10-11, little-endian)
    info.product_code = data[10] | (static_cast<uint16_t>(data[11]) << 8);

    // Serial number (bytes 12-15, little-endian)
    info.serial_number = data[12] | (data[13] << 8) | (data[14] << 16) | (data[15] << 24);

    // Monitor name from descriptor blocks (starting at byte 54, each 18 bytes)
    for (int offset = 54; offset <= 108; offset += 18) {
        if (data[offset] == 0 && data[offset + 1] == 0 && data[offset + 3] == 0xFC) {
            char name_buf[14] = {};
            std::memcpy(name_buf, &data[offset + 5], 13);
            std::string name(name_buf);
            // Trim trailing whitespace and control chars
            while (!name.empty() && (name.back() == '\n' || name.back() == '\r' ||
                                      name.back() == ' ' || name.back() == '\0')) {
                name.pop_back();
            }
            info.monitor_name = name;
            break;
        }
    }

    return info;
}

} // namespace hdrfixer::display
```

**Step 4: Run test to verify it passes**

Run: `cmake --build build --config Debug --target hdrfixer_tests && build\tests\Debug\hdrfixer_tests.exe -tc="EDID*"`
Expected: All PASS

**Step 5: Commit**

```bash
git add src/core/display/edid_reader.h src/core/display/edid_reader.cpp tests/test_edid_reader.cpp
git commit -m "feat: add EDID binary parser (manufacturer, product code, monitor name)"
```

---

## Task 6: Core — MHC2 ICC Profile Writer

**Files:**
- Create: `src/core/profile/icc_binary.h`
- Create: `src/core/profile/mhc2_writer.h`
- Create: `src/core/profile/mhc2_writer.cpp`
- Create: `tests/test_mhc2_writer.cpp`

**Step 1: Write the failing tests**

```cpp
#include "doctest.h"
#include "core/profile/mhc2_writer.h"
#include "core/profile/icc_binary.h"
#include <cstring>

using namespace hdrfixer::profile;

TEST_CASE("S15Fixed16 encoding") {
    CHECK(to_s15f16(1.0) == 65536);
    CHECK(to_s15f16(0.5) == 32768);
    CHECK(to_s15f16(-1.0) == -65536);
    CHECK(to_s15f16(0.0) == 0);
}

TEST_CASE("ICC profile header") {
    auto lut = std::vector<double>(64, 0.0);
    for (int i = 0; i < 64; ++i) lut[i] = static_cast<double>(i) / 63.0;

    Mhc2Params params{};
    params.lut = lut;
    params.min_nits = 0.0;
    params.max_nits = 1000.0;

    auto data = generate_mhc2_profile(params);
    REQUIRE(data.size() >= 128);

    // Profile size in first 4 bytes (big-endian) matches actual size
    uint32_t profile_size = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
    CHECK(profile_size == data.size());

    // "acsp" at offset 36
    CHECK(data[36] == 'a'); CHECK(data[37] == 'c');
    CHECK(data[38] == 's'); CHECK(data[39] == 'p');

    // "mntr" at offset 12
    CHECK(data[12] == 'm'); CHECK(data[13] == 'n');
    CHECK(data[14] == 't'); CHECK(data[15] == 'r');

    // "RGB " at offset 16
    CHECK(data[16] == 'R'); CHECK(data[17] == 'G');
    CHECK(data[18] == 'B'); CHECK(data[19] == ' ');
}

TEST_CASE("ICC profile contains MHC2 tag") {
    auto lut = std::vector<double>(64, 0.0);
    for (int i = 0; i < 64; ++i) lut[i] = static_cast<double>(i) / 63.0;

    Mhc2Params params{};
    params.lut = lut;
    params.min_nits = 0.0;
    params.max_nits = 1000.0;

    auto data = generate_mhc2_profile(params);

    // Find MHC2 tag in tag table
    uint32_t tag_count = (data[128] << 24) | (data[129] << 16) | (data[130] << 8) | data[131];
    CHECK(tag_count == 11);

    bool found_mhc2 = false;
    for (uint32_t i = 0; i < tag_count; ++i) {
        uint32_t offset = 132 + i * 12;
        if (data[offset] == 'M' && data[offset+1] == 'H' &&
            data[offset+2] == 'C' && data[offset+3] == '2') {
            found_mhc2 = true;
            break;
        }
    }
    CHECK(found_mhc2);
}

TEST_CASE("Profile writes to file and reads back") {
    auto lut = std::vector<double>(64, 0.0);
    for (int i = 0; i < 64; ++i) lut[i] = static_cast<double>(i) / 63.0;

    Mhc2Params params{};
    params.lut = lut;
    params.min_nits = 0.0;
    params.max_nits = 1000.0;

    auto data = generate_mhc2_profile(params);
    auto path = std::filesystem::temp_directory_path() / L"test_profile.icm";
    CHECK(write_profile_to_file(data, path));
    CHECK(std::filesystem::exists(path));
    CHECK(std::filesystem::file_size(path) == data.size());
    std::filesystem::remove(path);
}
```

**Step 2: Run test to verify it fails**

Run: `cmake --build build --config Debug --target hdrfixer_tests && build\tests\Debug\hdrfixer_tests.exe -tc="S15Fixed16*,ICC profile*,Profile writes*"`
Expected: FAIL

**Step 3: Write the implementation**

```cpp
// icc_binary.h
#pragma once
#include <cstdint>
#include <vector>
#include <cmath>
#include <string>

namespace hdrfixer::profile {

inline int32_t to_s15f16(double v) {
    return static_cast<int32_t>(std::round(v * 65536.0));
}

inline void write_be32(std::vector<uint8_t>& buf, uint32_t v) {
    buf.push_back((v >> 24) & 0xFF);
    buf.push_back((v >> 16) & 0xFF);
    buf.push_back((v >> 8) & 0xFF);
    buf.push_back(v & 0xFF);
}

inline void write_be32_signed(std::vector<uint8_t>& buf, int32_t v) {
    write_be32(buf, static_cast<uint32_t>(v));
}

inline void write_be16(std::vector<uint8_t>& buf, uint16_t v) {
    buf.push_back((v >> 8) & 0xFF);
    buf.push_back(v & 0xFF);
}

inline void write_tag_sig(std::vector<uint8_t>& buf, const char sig[4]) {
    buf.push_back(sig[0]); buf.push_back(sig[1]);
    buf.push_back(sig[2]); buf.push_back(sig[3]);
}

inline void pad_to_4(std::vector<uint8_t>& buf) {
    while (buf.size() % 4 != 0) buf.push_back(0);
}

} // namespace hdrfixer::profile
```

```cpp
// mhc2_writer.h
#pragma once
#include "icc_binary.h"
#include <vector>
#include <filesystem>

namespace hdrfixer::profile {

struct Mhc2Params {
    std::vector<double> lut;
    double min_nits = 0.0;
    double max_nits = 1000.0;
    double gamma = 2.2;
    std::string description = "HDRFixer Gamma 2.2 Correction";
};

std::vector<uint8_t> generate_mhc2_profile(const Mhc2Params& params);
bool write_profile_to_file(const std::vector<uint8_t>& data, const std::filesystem::path& path);

} // namespace hdrfixer::profile
```

```cpp
// mhc2_writer.cpp
#include "mhc2_writer.h"
#include <fstream>

namespace hdrfixer::profile {

namespace {

// BT.709 / sRGB primaries in XYZ
constexpr double kRedX = 0.4361, kRedY = 0.2225, kRedZ = 0.0139;
constexpr double kGreenX = 0.3851, kGreenY = 0.7169, kGreenZ = 0.0971;
constexpr double kBlueX = 0.1431, kBlueY = 0.0606, kBlueZ = 0.7141;
constexpr double kWhiteX = 0.9505, kWhiteY = 1.0000, kWhiteZ = 1.0890;
// D50 illuminant for PCS
constexpr double kD50X = 0.9642, kD50Y = 1.0000, kD50Z = 0.8249;

std::vector<uint8_t> build_xyz_tag(double x, double y, double z) {
    std::vector<uint8_t> tag;
    write_tag_sig(tag, "XYZ ");
    write_be32(tag, 0); // reserved
    write_be32_signed(tag, to_s15f16(x));
    write_be32_signed(tag, to_s15f16(y));
    write_be32_signed(tag, to_s15f16(z));
    return tag;
}

std::vector<uint8_t> build_curv_tag(double gamma) {
    std::vector<uint8_t> tag;
    write_tag_sig(tag, "curv");
    write_be32(tag, 0); // reserved
    write_be32(tag, 1); // count = 1 (parametric gamma)
    write_be16(tag, static_cast<uint16_t>(gamma * 256.0));
    write_be16(tag, 0); // padding
    return tag;
}

std::vector<uint8_t> build_mluc_tag(const std::string& text) {
    std::vector<uint8_t> tag;
    write_tag_sig(tag, "mluc");
    write_be32(tag, 0); // reserved
    write_be32(tag, 1); // record count
    write_be32(tag, 12); // record size
    // Language: en-US
    write_be16(tag, 'e'); write_be16(tag, 'n');
    write_be16(tag, 'U'); write_be16(tag, 'S');

    // String length and offset
    uint32_t str_bytes = static_cast<uint32_t>(text.size() * 2);
    write_be32(tag, str_bytes);
    write_be32(tag, 28); // offset to string data

    // UTF-16BE encoded string
    for (char c : text) {
        tag.push_back(0);
        tag.push_back(static_cast<uint8_t>(c));
    }
    pad_to_4(tag);
    return tag;
}

std::vector<uint8_t> build_mhc2_tag(const Mhc2Params& params) {
    std::vector<uint8_t> tag;
    int lut_size = static_cast<int>(params.lut.size());

    write_tag_sig(tag, "MHC2");
    write_be32(tag, 0); // reserved
    write_be32(tag, static_cast<uint32_t>(lut_size));
    write_be32_signed(tag, to_s15f16(params.min_nits));
    write_be32_signed(tag, to_s15f16(params.max_nits));

    // Offsets: matrix at 36, lut0 at 84
    uint32_t matrix_offset = 36;
    uint32_t lut0_offset = 84;
    uint32_t lut_data_size = 8 + lut_size * 4; // sf32 sig + reserved + entries
    uint32_t lut1_offset = lut0_offset + lut_data_size;
    uint32_t lut2_offset = lut1_offset + lut_data_size;

    write_be32(tag, matrix_offset);
    write_be32(tag, lut0_offset);
    write_be32(tag, lut1_offset);
    write_be32(tag, lut2_offset);

    // 3x4 identity matrix (12 S15.16 values)
    double matrix[12] = {1,0,0,0, 0,1,0,0, 0,0,1,0};
    for (double v : matrix) {
        write_be32_signed(tag, to_s15f16(v));
    }

    // 3 identical LUTs (R, G, B)
    for (int ch = 0; ch < 3; ++ch) {
        write_tag_sig(tag, "sf32");
        write_be32(tag, 0); // reserved
        for (int i = 0; i < lut_size; ++i) {
            write_be32_signed(tag, to_s15f16(params.lut[i]));
        }
    }

    return tag;
}

} // anonymous namespace

std::vector<uint8_t> generate_mhc2_profile(const Mhc2Params& params) {
    // Build all tag data blobs
    auto desc_tag = build_mluc_tag(params.description);
    auto cprt_tag = build_mluc_tag("Generated by HDRFixer");
    auto rxyz_tag = build_xyz_tag(kRedX, kRedY, kRedZ);
    auto gxyz_tag = build_xyz_tag(kGreenX, kGreenY, kGreenZ);
    auto bxyz_tag = build_xyz_tag(kBlueX, kBlueY, kBlueZ);
    auto wtpt_tag = build_xyz_tag(kWhiteX, kWhiteY, kWhiteZ);
    auto lumi_tag = build_xyz_tag(0, params.max_nits, 0);
    auto trc_tag = build_curv_tag(params.gamma);
    auto mhc2_tag = build_mhc2_tag(params);

    // 11 tags: desc, cprt, rXYZ, gXYZ, bXYZ, wtpt, lumi, rTRC, gTRC, bTRC, MHC2
    // gTRC and bTRC share the same data as rTRC
    constexpr uint32_t tag_count = 11;
    uint32_t tag_table_size = 4 + tag_count * 12; // count + entries
    uint32_t data_start = 128 + tag_table_size;

    // Calculate offsets for each tag's data
    struct TagEntry { const char sig[5]; const std::vector<uint8_t>* data; uint32_t offset; };
    std::vector<std::pair<std::vector<uint8_t>*, uint32_t>> unique_tags;

    uint32_t current_offset = data_start;

    auto align4 = [](uint32_t v) -> uint32_t { return (v + 3) & ~3u; };

    uint32_t desc_off = current_offset; current_offset += align4(static_cast<uint32_t>(desc_tag.size()));
    uint32_t cprt_off = current_offset; current_offset += align4(static_cast<uint32_t>(cprt_tag.size()));
    uint32_t rxyz_off = current_offset; current_offset += align4(static_cast<uint32_t>(rxyz_tag.size()));
    uint32_t gxyz_off = current_offset; current_offset += align4(static_cast<uint32_t>(gxyz_tag.size()));
    uint32_t bxyz_off = current_offset; current_offset += align4(static_cast<uint32_t>(bxyz_tag.size()));
    uint32_t wtpt_off = current_offset; current_offset += align4(static_cast<uint32_t>(wtpt_tag.size()));
    uint32_t lumi_off = current_offset; current_offset += align4(static_cast<uint32_t>(lumi_tag.size()));
    uint32_t trc_off = current_offset; current_offset += align4(static_cast<uint32_t>(trc_tag.size()));
    // gTRC and bTRC share trc_off
    uint32_t mhc2_off = current_offset; current_offset += align4(static_cast<uint32_t>(mhc2_tag.size()));

    uint32_t profile_size = current_offset;

    // Build the profile
    std::vector<uint8_t> profile;
    profile.reserve(profile_size);

    // === ICC HEADER (128 bytes) ===
    write_be32(profile, profile_size);
    write_be32(profile, 0); // preferred CMM
    write_be32(profile, 0x04400000); // version 4.4
    write_tag_sig(profile, "mntr");
    write_tag_sig(profile, "RGB ");
    write_tag_sig(profile, "XYZ ");
    // Date/time (12 bytes)
    write_be16(profile, 2026); write_be16(profile, 2); write_be16(profile, 21);
    write_be16(profile, 0); write_be16(profile, 0); write_be16(profile, 0);
    write_tag_sig(profile, "acsp");
    write_tag_sig(profile, "MSFT");
    write_be32(profile, 0); // flags
    write_be32(profile, 0); // manufacturer
    write_be32(profile, 0); // model
    for (int i = 0; i < 4; ++i) write_be32(profile, 0); // attributes + intent
    // D50 PCS illuminant
    write_be32_signed(profile, to_s15f16(kD50X));
    write_be32_signed(profile, to_s15f16(kD50Y));
    write_be32_signed(profile, to_s15f16(kD50Z));
    write_be32(profile, 0); // creator
    for (int i = 0; i < 4; ++i) write_be32(profile, 0); // profile ID
    for (int i = 0; i < 6; ++i) write_be32(profile, 0); // reserved (24 bytes)

    // === TAG TABLE ===
    write_be32(profile, tag_count);

    auto write_tag_entry = [&](const char* sig, uint32_t offset, uint32_t size) {
        write_tag_sig(profile, sig);
        write_be32(profile, offset);
        write_be32(profile, size);
    };

    write_tag_entry("desc", desc_off, static_cast<uint32_t>(desc_tag.size()));
    write_tag_entry("cprt", cprt_off, static_cast<uint32_t>(cprt_tag.size()));
    write_tag_entry("rXYZ", rxyz_off, static_cast<uint32_t>(rxyz_tag.size()));
    write_tag_entry("gXYZ", gxyz_off, static_cast<uint32_t>(gxyz_tag.size()));
    write_tag_entry("bXYZ", bxyz_off, static_cast<uint32_t>(bxyz_tag.size()));
    write_tag_entry("wtpt", wtpt_off, static_cast<uint32_t>(wtpt_tag.size()));
    write_tag_entry("lumi", lumi_off, static_cast<uint32_t>(lumi_tag.size()));
    write_tag_entry("rTRC", trc_off, static_cast<uint32_t>(trc_tag.size()));
    write_tag_entry("gTRC", trc_off, static_cast<uint32_t>(trc_tag.size())); // shared
    write_tag_entry("bTRC", trc_off, static_cast<uint32_t>(trc_tag.size())); // shared
    write_tag_entry("MHC2", mhc2_off, static_cast<uint32_t>(mhc2_tag.size()));

    // === TAG DATA ===
    auto write_tag_data = [&](const std::vector<uint8_t>& tag, uint32_t expected_off) {
        while (profile.size() < expected_off) profile.push_back(0);
        profile.insert(profile.end(), tag.begin(), tag.end());
    };

    write_tag_data(desc_tag, desc_off);
    write_tag_data(cprt_tag, cprt_off);
    write_tag_data(rxyz_tag, rxyz_off);
    write_tag_data(gxyz_tag, gxyz_off);
    write_tag_data(bxyz_tag, bxyz_off);
    write_tag_data(wtpt_tag, wtpt_off);
    write_tag_data(lumi_tag, lumi_off);
    write_tag_data(trc_tag, trc_off);
    write_tag_data(mhc2_tag, mhc2_off);

    // Pad to final size
    while (profile.size() < profile_size) profile.push_back(0);

    return profile;
}

bool write_profile_to_file(const std::vector<uint8_t>& data, const std::filesystem::path& path) {
    std::ofstream file(path, std::ios::binary);
    if (!file) return false;
    file.write(reinterpret_cast<const char*>(data.data()), data.size());
    return file.good();
}

} // namespace hdrfixer::profile
```

**Step 4: Run test to verify it passes**

Run: `cmake --build build --config Debug --target hdrfixer_tests && build\tests\Debug\hdrfixer_tests.exe -tc="S15Fixed16*,ICC profile*,Profile writes*"`
Expected: All PASS

**Step 5: Commit**

```bash
git add src/core/profile/ tests/test_mhc2_writer.cpp
git commit -m "feat: add MHC2 ICC profile writer with binary generation and validation"
```

---

## Task 7: Core — WCS Profile Installer

**Files:**
- Create: `src/core/profile/wcs_installer.h`
- Create: `src/core/profile/wcs_installer.cpp`

**Step 1: Write the implementation**

Note: WCS installation requires admin privileges and actual hardware, so we test this via integration tests later. Unit tests verify the API wrapper compiles and error paths work.

```cpp
// wcs_installer.h
#pragma once
#include <string>
#include <expected>
#include <filesystem>
#include <windows.h>

namespace hdrfixer::profile {

struct InstallParams {
    std::filesystem::path profile_path;
    LUID adapter_luid;
    uint32_t source_id;
    bool set_as_default = true;
};

std::expected<void, std::string> install_profile(const InstallParams& params);
std::expected<void, std::string> uninstall_profile(const std::wstring& filename, LUID adapter_luid, uint32_t source_id);

} // namespace hdrfixer::profile
```

```cpp
// wcs_installer.cpp
#include "wcs_installer.h"
#include <icm.h>
#include <format>

// ColorProfileAddDisplayAssociation is available on Windows 11+
// Dynamically load to avoid hard dependency on specific SDK version
typedef HRESULT (WINAPI *PFN_ColorProfileAddDisplayAssociation)(
    WCS_PROFILE_MANAGEMENT_SCOPE scope,
    PCWSTR profileName,
    LUID targetAdapterID,
    UINT sourceID,
    BOOL setAsDefault,
    BOOL associateAsAdvancedColor
);

typedef HRESULT (WINAPI *PFN_ColorProfileRemoveDisplayAssociation)(
    WCS_PROFILE_MANAGEMENT_SCOPE scope,
    PCWSTR profileName,
    LUID targetAdapterID,
    UINT sourceID,
    BOOL dissociateAdvancedColor
);

namespace hdrfixer::profile {

std::expected<void, std::string> install_profile(const InstallParams& params) {
    // Step 1: Install profile into system color directory
    if (!InstallColorProfileW(nullptr, params.profile_path.c_str())) {
        DWORD err = GetLastError();
        return std::unexpected(std::format("InstallColorProfileW failed: error {}", err));
    }

    // Step 2: Associate with display via ColorProfileAddDisplayAssociation
    auto filename = params.profile_path.filename().wstring();

    HMODULE mscms = GetModuleHandleW(L"Mscms.dll");
    if (!mscms) mscms = LoadLibraryW(L"Mscms.dll");

    auto pfn = reinterpret_cast<PFN_ColorProfileAddDisplayAssociation>(
        GetProcAddress(mscms, "ColorProfileAddDisplayAssociation"));

    if (pfn) {
        HRESULT hr = pfn(
            WCS_PROFILE_MANAGEMENT_SCOPE_CURRENT_USER,
            filename.c_str(),
            params.adapter_luid,
            params.source_id,
            params.set_as_default ? TRUE : FALSE,
            TRUE // associateAsAdvancedColor
        );
        if (FAILED(hr))
            return std::unexpected(std::format("ColorProfileAddDisplayAssociation failed: 0x{:08X}", static_cast<unsigned>(hr)));
    } else {
        // Fallback: legacy WcsAssociateColorProfileWithDevice (needs device name)
        return std::unexpected("ColorProfileAddDisplayAssociation not available; Windows 11 required");
    }

    return {};
}

std::expected<void, std::string> uninstall_profile(const std::wstring& filename, LUID adapter_luid, uint32_t source_id) {
    // Step 1: Remove display association
    HMODULE mscms = GetModuleHandleW(L"Mscms.dll");
    if (!mscms) mscms = LoadLibraryW(L"Mscms.dll");

    auto pfn = reinterpret_cast<PFN_ColorProfileRemoveDisplayAssociation>(
        GetProcAddress(mscms, "ColorProfileRemoveDisplayAssociation"));

    if (pfn) {
        pfn(WCS_PROFILE_MANAGEMENT_SCOPE_CURRENT_USER,
            filename.c_str(), adapter_luid, source_id, TRUE);
    }

    // Step 2: Uninstall and delete profile
    if (!UninstallColorProfileW(nullptr, filename.c_str(), TRUE)) {
        DWORD err = GetLastError();
        return std::unexpected(std::format("UninstallColorProfileW failed: error {}", err));
    }

    return {};
}

} // namespace hdrfixer::profile
```

**Step 2: Verify build compiles**

Run: `cmake --build build --config Debug`
Expected: Compiles successfully

**Step 3: Commit**

```bash
git add src/core/profile/wcs_installer.h src/core/profile/wcs_installer.cpp
git commit -m "feat: add WCS profile installer with ColorProfileAddDisplayAssociation"
```

---

## Task 8: Core — Registry Manager & Backup

**Files:**
- Create: `src/core/registry/hdr_registry.h`
- Create: `src/core/registry/hdr_registry.cpp`
- Create: `src/core/registry/registry_backup.h`
- Create: `src/core/registry/registry_backup.cpp`
- Create: `tests/test_registry.cpp`

**Step 1: Write the failing tests**

```cpp
#include "doctest.h"
#include "core/registry/hdr_registry.h"

using namespace hdrfixer::registry;

TEST_CASE("Registry path constants") {
    CHECK(kGraphicsDrivers == L"SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers");
    CHECK(kDirectXUserPrefs == L"Software\\Microsoft\\DirectX\\UserGpuPreferences");
}

TEST_CASE("DirectX settings parse") {
    auto settings = parse_dx_settings(L"SwapEffectUpgradeEnable=1;AutoHDREnable=1;");
    CHECK(settings.size() == 2);
    CHECK(settings[L"SwapEffectUpgradeEnable"] == L"1");
    CHECK(settings[L"AutoHDREnable"] == L"1");
}

TEST_CASE("DirectX settings serialize") {
    std::map<std::wstring, std::wstring> settings;
    settings[L"AutoHDREnable"] = L"1";
    settings[L"SwapEffectUpgradeEnable"] = L"1";
    auto result = serialize_dx_settings(settings);
    CHECK(result.find(L"AutoHDREnable=1;") != std::wstring::npos);
    CHECK(result.find(L"SwapEffectUpgradeEnable=1;") != std::wstring::npos);
}

TEST_CASE("DirectX settings empty string") {
    auto settings = parse_dx_settings(L"");
    CHECK(settings.empty());
}
```

**Step 2-5: Implement, test, commit** (follows same TDD pattern)

Implementation provides: `kGraphicsDrivers`, `kDirectXUserPrefs`, `kVideoSettings` path constants, `parse_dx_settings()`, `serialize_dx_settings()`, `read_hdr_enabled()`, `write_hdr_enabled()`, `read_auto_hdr_enabled()`, `write_auto_hdr_enabled()`.

Registry backup provides: `backup_key()`, `restore_backup()`, `list_backups()` saving JSON to `%APPDATA%\HDRFixer\backups\`.

**Commit:**
```bash
git commit -m "feat: add HDR registry manager and backup system"
```

---

## Task 9: Core — Settings & Logging

**Files:**
- Create: `src/core/config/settings.h`
- Create: `src/core/config/settings.cpp`
- Create: `src/core/log/logger.h`
- Create: `src/core/log/logger.cpp`
- Create: `tests/test_settings.cpp`

Settings: JSON file at `%LOCALAPPDATA%\HDRFixer\settings.json`. Uses a minimal JSON writer/reader (no external dependency) or raw Win32 file I/O with simple key=value format.

Logger: File-based logging to `%LOCALAPPDATA%\HDRFixer\hdrfixer.log` with levels (debug, info, warn, error). Thread-safe via mutex. Includes timestamp and source location.

**Commit:**
```bash
git commit -m "feat: add settings persistence and file logger"
```

---

## Task 10: Fix Engine — Core Framework

**Files:**
- Create: `src/app/fixes/fix_engine.h`
- Create: `src/app/fixes/fix_engine.cpp`
- Create: `tests/test_fix_engine.cpp`

**Step 1: Write the failing tests**

```cpp
#include "doctest.h"
#include "app/fixes/fix_engine.h"

using namespace hdrfixer::fixes;

struct MockFix : public IFix {
    std::string name() const override { return "MockFix"; }
    std::string description() const override { return "Test fix"; }
    FixCategory category() const override { return FixCategory::ToneCurve; }
    FixResult apply() override { applied = true; return {true, "Applied"}; }
    FixResult revert() override { applied = false; return {true, "Reverted"}; }
    FixStatus diagnose() override { return {applied ? FixState::Applied : FixState::NotApplied, ""}; }
    bool applied = false;
};

TEST_CASE("FixEngine register and count") {
    FixEngine engine;
    engine.register_fix(std::make_unique<MockFix>());
    CHECK(engine.fix_count() == 1);
}

TEST_CASE("FixEngine apply all") {
    FixEngine engine;
    auto fix = std::make_unique<MockFix>();
    auto* ptr = fix.get();
    engine.register_fix(std::move(fix));
    engine.apply_all();
    CHECK(ptr->applied == true);
}

TEST_CASE("FixEngine revert all") {
    FixEngine engine;
    auto fix = std::make_unique<MockFix>();
    auto* ptr = fix.get();
    engine.register_fix(std::move(fix));
    engine.apply_all();
    engine.revert_all();
    CHECK(ptr->applied == false);
}
```

**Step 2-5: Implement, test, commit**

```cpp
// fix_engine.h
#pragma once
#include <string>
#include <vector>
#include <memory>

namespace hdrfixer::fixes {

enum class FixCategory { ToneCurve, SdrBrightness, PixelFormat, AutoHdr, IccConflict, EdidValidation, OledProtection };
enum class FixState { NotApplied, Applied, Error, NotNeeded };

struct FixResult { bool success; std::string message; };
struct FixStatus { FixState state; std::string message; };

struct IFix {
    virtual ~IFix() = default;
    virtual std::string name() const = 0;
    virtual std::string description() const = 0;
    virtual FixCategory category() const = 0;
    virtual FixResult apply() = 0;
    virtual FixResult revert() = 0;
    virtual FixStatus diagnose() = 0;
};

class FixEngine {
public:
    void register_fix(std::unique_ptr<IFix> fix);
    size_t fix_count() const;
    void apply_all();
    void revert_all();
    std::vector<FixStatus> diagnose_all();
    IFix* get_fix(const std::string& name);
private:
    std::vector<std::unique_ptr<IFix>> fixes_;
};

} // namespace hdrfixer::fixes
```

**Commit:**
```bash
git commit -m "feat: add fix engine with IFix interface and apply/revert/diagnose lifecycle"
```

---

## Task 11: Fixes — Gamma Correction Fix

**Files:**
- Create: `src/app/fixes/gamma_fix.h`
- Create: `src/app/fixes/gamma_fix.cpp`

Implements the main gamma correction fix: generates MHC2 profile using HDR LUT, writes to temp, installs via WCS, associates with the correct display. Revert removes the profile.

**Commit:**
```bash
git commit -m "feat: add gamma correction fix (MHC2 profile generation + WCS installation)"
```

---

## Task 12: Fixes — SDR Brightness, Pixel Format, Screen-Share Helper

**Files:**
- Create: `src/app/fixes/sdr_brightness_fix.h`
- Create: `src/app/fixes/sdr_brightness_fix.cpp`
- Create: `src/app/fixes/pixel_format_fix.h`
- Create: `src/app/fixes/pixel_format_fix.cpp`
- Create: `src/app/fixes/share_helper.h`
- Create: `src/app/fixes/share_helper.cpp`

SDR brightness: reads current level, calculates optimal (OLED>=800→200nits, HDR600→250, HDR400→280, default→200), applies via DisplayConfig API.

Pixel format: queries DXGI output color space and bit depth, returns diagnostic warning if not 10-bit RGB.

Share helper: global hotkey (Ctrl+Shift+S) toggles SDR white level between current and 80 nits across all displays.

**Commit:**
```bash
git commit -m "feat: add SDR brightness fix, pixel format detection, and screen-share helper"
```

---

## Task 13: Fixes — Watchdog & Hotplug

**Files:**
- Create: `src/app/fixes/watchdog.h`
- Create: `src/app/fixes/watchdog.cpp`
- Create: `src/app/fixes/hotplug.h`
- Create: `src/app/fixes/hotplug.cpp`

Watchdog: background thread with `RegNotifyChangeKeyValue` on GraphicsDrivers key. When change detected, diagnose all fixes and re-apply any that were reverted. Also runs on a 60-second timer as fallback.

Hotplug: registers for `WM_DISPLAYCHANGE` and `GUID_DEVINTERFACE_MONITOR` device notifications. On display change, re-enumerates displays and re-applies fixes.

**Commit:**
```bash
git commit -m "feat: add registry watchdog and display hotplug detection"
```

---

## Task 14: UI — System Tray & Settings Window

**Files:**
- Create: `src/app/ui/tray.h`
- Create: `src/app/ui/tray.cpp`
- Create: `src/app/ui/settings_wnd.h`
- Create: `src/app/ui/settings_wnd.cpp`
- Create: `src/app/resource.h`
- Create: `src/app/resource.rc`

Tray: invisible message window, Shell_NotifyIcon with context menu (Apply All, Revert All, Share Mode, Settings, Exit). Balloon notifications for state changes. Global hotkeys: Ctrl+Shift+H (toggle fixes), Ctrl+Shift+S (share mode).

Settings window: Win32 dialog with controls for SDR brightness slider, auto-start toggle, watchdog enable, log level. Reads/writes via Settings manager.

**Commit:**
```bash
git commit -m "feat: add system tray UI with context menu, hotkeys, and settings dialog"
```

---

## Task 15: Integration — Wire Everything Together

**Files:**
- Modify: `src/app/main.cpp`

Wire up: display detection → fix engine → tray UI → watchdog → hotplug. Single-instance check, admin elevation prompt if needed, COM initialization, create all components, run message loop.

**Commit:**
```bash
git commit -m "feat: wire up all components in main entry point"
```

---

## Task 16: Final Build & Verification

**Step 1:** Full clean build: `cmake --build build --config Release`
**Step 2:** Run all tests: `build\tests\Release\hdrfixer_tests.exe`
**Step 3:** Verify exe size and dependencies: `dumpbin /dependents build\src\app\Release\HDRFixer.exe`
**Step 4:** Manual testing on system with HDR display
**Step 5:** Update README.md with new build instructions

**Commit:**
```bash
git commit -m "docs: update README for v2 C++ native rebuild"
```

---

## Parallelization Strategy

Tasks that can run in parallel (no dependencies between them):
- **Group A (Core — independent modules):** Task 2, Task 3, Task 5 can run in parallel
- **Group B (Core — depends on Group A):** Task 4 (depends on nothing from A), Task 6 (depends on Task 3 for LUT)
- **Group C (Framework):** Task 7, Task 8, Task 9 can run in parallel after Task 1
- **Group D (Fix Engine):** Task 10 after Group A+B+C
- **Group E (Fixes):** Task 11, Task 12 can run in parallel after Task 10
- **Group F (Infrastructure):** Task 13 after Task 10
- **Group G (UI):** Task 14 after Task 10
- **Group H (Integration):** Task 15 after everything

Maximum parallel agents at any time: **3** (Group A has 3 independent tasks)
