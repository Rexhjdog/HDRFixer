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
