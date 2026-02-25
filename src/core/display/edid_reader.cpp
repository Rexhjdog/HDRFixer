#include "edid_reader.h"
#include <cstring>
#include <algorithm>

namespace hdrfixer::display {

std::optional<EdidInfo> parse_edid(const uint8_t* data, size_t length) {
    if (!data || length < 128)
        return std::nullopt;

    EdidInfo info{};

    // Manufacturer ID (bytes 8-9, big-endian compressed 3-letter ASCII)
    // EDID spec: A=1..Z=26 encoded in 5-bit fields
    uint16_t mfg = (static_cast<uint16_t>(data[8]) << 8) | data[9];
    int v1 = (mfg >> 10) & 0x1F;
    int v2 = (mfg >> 5) & 0x1F;
    int v3 = mfg & 0x1F;
    char c1 = (v1 >= 1 && v1 <= 26) ? static_cast<char>(v1 + 'A' - 1) : '?';
    char c2 = (v2 >= 1 && v2 <= 26) ? static_cast<char>(v2 + 'A' - 1) : '?';
    char c3 = (v3 >= 1 && v3 <= 26) ? static_cast<char>(v3 + 'A' - 1) : '?';
    info.manufacturer = {c1, c2, c3};

    // Product code (bytes 10-11, little-endian)
    info.product_code = data[10] | (static_cast<uint16_t>(data[11]) << 8);

    // Serial number (bytes 12-15, little-endian)
    info.serial_number = data[12] | (data[13] << 8) | (data[14] << 16) | (data[15] << 24);

    // Monitor name from descriptor blocks (starting at byte 54, each 18 bytes)
    for (int offset = 54; offset <= 108; offset += 18) {
        if (data[offset] == 0 && data[offset + 1] == 0 &&
            data[offset + 2] == 0 && data[offset + 3] == 0xFC) {
            char name_buf[14] = {};
            std::memcpy(name_buf, &data[offset + 5], 13);
            std::string name(name_buf, strnlen(name_buf, 13));
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
