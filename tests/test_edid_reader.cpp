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

TEST_CASE("EDID rejects null data") {
    auto info = parse_edid(nullptr, 128);
    CHECK(!info.has_value());
}

TEST_CASE("EDID rejects zero length") {
    uint8_t edid[128] = {};
    auto info = parse_edid(edid, 0);
    CHECK(!info.has_value());
}

TEST_CASE("EDID skips descriptor with non-zero byte 2") {
    uint8_t edid[128] = {};
    // Descriptor at offset 54 with byte 2 = 0x01 (not valid per EDID spec)
    edid[54] = 0; edid[55] = 0; edid[56] = 0x01; edid[57] = 0xFC; edid[58] = 0;
    const char* name = "Bad Desc";
    std::memcpy(&edid[59], name, strlen(name));
    auto info = parse_edid(edid, 128);
    CHECK(info.has_value());
    CHECK(info->monitor_name.empty()); // Should not parse invalid descriptor
}

TEST_CASE("EDID invalid manufacturer ID produces '?'") {
    uint8_t edid[128] = {};
    edid[8] = 0x00; edid[9] = 0x00; // All zeros = invalid
    auto info = parse_edid(edid, 128);
    CHECK(info.has_value());
    CHECK(info->manufacturer == "???");
}

TEST_CASE("EDID monitor name in second descriptor block") {
    uint8_t edid[128] = {};
    // First descriptor at 54: not a monitor name
    edid[54] = 0; edid[55] = 0; edid[56] = 0; edid[57] = 0xFF; edid[58] = 0;
    // Second descriptor at 72: monitor name
    edid[72] = 0; edid[73] = 0; edid[74] = 0; edid[75] = 0xFC; edid[76] = 0;
    const char* name = "Second Block";
    std::memcpy(&edid[77], name, strlen(name));
    edid[77 + strlen(name)] = '\n';
    auto info = parse_edid(edid, 128);
    CHECK(info.has_value());
    CHECK(info->monitor_name == "Second Block");
}

TEST_CASE("EDID serial number little-endian") {
    uint8_t edid[128] = {};
    edid[12] = 0x78; edid[13] = 0x56; edid[14] = 0x34; edid[15] = 0x12;
    auto info = parse_edid(edid, 128);
    CHECK(info.has_value());
    CHECK(info->serial_number == 0x12345678);
}
