#include "doctest.h"
#include "core/display/edid_reader.h"
#include <cstring>

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
