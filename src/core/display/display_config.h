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
