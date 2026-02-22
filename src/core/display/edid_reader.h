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
