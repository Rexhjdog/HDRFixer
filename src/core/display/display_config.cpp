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
