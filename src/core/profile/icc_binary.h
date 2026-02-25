#pragma once
#include <cstdint>
#include <vector>
#include <algorithm>
#include <cmath>
#include <string>

namespace hdrfixer::profile {

inline int32_t to_s15f16(double v) {
    v = std::clamp(v, -32768.0, 32767.0 + (65535.0 / 65536.0));
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
