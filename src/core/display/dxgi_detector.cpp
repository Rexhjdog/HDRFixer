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
