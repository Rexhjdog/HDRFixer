#define DOCTEST_CONFIG_IMPLEMENT_WITH_MAIN
#include "doctest.h"
#include "../src/core/display/edid_reader.h"
#include <cstring>
#include <vector>

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
    std::memcpy(&edid[59], name, std::strlen(name));
    edid[59 + std::strlen(name)] = '\n';
    auto info = parse_edid(edid, 128);
    CHECK(info.has_value());
    CHECK(info->monitor_name == "Test Monitor");
}

// Edge Cases

TEST_CASE("EDID rejects null pointer") {
    auto info = parse_edid(nullptr, 128);
    CHECK(!info.has_value());
}

TEST_CASE("EDID rejects short data") {
    uint8_t edid[127] = {};
    auto info = parse_edid(edid, 127);
    CHECK(!info.has_value());
}

TEST_CASE("EDID rejects empty data") {
    auto info = parse_edid(nullptr, 0);
    CHECK(!info.has_value());
}

TEST_CASE("EDID ignores garbage descriptors") {
    uint8_t edid[128] = {};
    // Invalid descriptor type
    edid[54] = 0; edid[55] = 0; edid[56] = 0; edid[57] = 0xFF; edid[58] = 0;
    auto info = parse_edid(edid, 128);
    CHECK(info.has_value());
    CHECK(info->monitor_name.empty());
}

TEST_CASE("EDID handles multiple descriptors") {
    uint8_t edid[128] = {};
    // First descriptor is NOT name (e.g. Serial Number 0xFF)
    edid[54] = 0; edid[55] = 0; edid[56] = 0; edid[57] = 0xFF; edid[58] = 0;

    // Second descriptor (offset 72) IS name
    int offset = 72;
    edid[offset] = 0; edid[offset+1] = 0; edid[offset+2] = 0; edid[offset+3] = 0xFC; edid[offset+4] = 0;
    const char* name = "Second Desc";
    std::memcpy(&edid[offset+5], name, std::strlen(name));

    auto info = parse_edid(edid, 128);
    CHECK(info.has_value());
    CHECK(info->monitor_name == "Second Desc");
}
