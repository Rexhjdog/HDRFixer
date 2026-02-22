#include "doctest.h"
#include "core/profile/mhc2_writer.h"
#include "core/profile/icc_binary.h"
#include <cstring>

using namespace hdrfixer::profile;

TEST_CASE("S15Fixed16 encoding") {
    CHECK(to_s15f16(1.0) == 65536);
    CHECK(to_s15f16(0.5) == 32768);
    CHECK(to_s15f16(-1.0) == -65536);
    CHECK(to_s15f16(0.0) == 0);
}

TEST_CASE("ICC profile header") {
    auto lut = std::vector<double>(64, 0.0);
    for (int i = 0; i < 64; ++i) lut[i] = static_cast<double>(i) / 63.0;

    Mhc2Params params{};
    params.lut = lut;
    params.min_nits = 0.0;
    params.max_nits = 1000.0;

    auto data = generate_mhc2_profile(params);
    REQUIRE(data.size() >= 128);

    // Profile size in first 4 bytes (big-endian) matches actual size
    uint32_t profile_size = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
    CHECK(profile_size == data.size());

    // "acsp" at offset 36
    CHECK(data[36] == 'a'); CHECK(data[37] == 'c');
    CHECK(data[38] == 's'); CHECK(data[39] == 'p');

    // "mntr" at offset 12
    CHECK(data[12] == 'm'); CHECK(data[13] == 'n');
    CHECK(data[14] == 't'); CHECK(data[15] == 'r');

    // "RGB " at offset 16
    CHECK(data[16] == 'R'); CHECK(data[17] == 'G');
    CHECK(data[18] == 'B'); CHECK(data[19] == ' ');
}

TEST_CASE("ICC profile contains MHC2 tag") {
    auto lut = std::vector<double>(64, 0.0);
    for (int i = 0; i < 64; ++i) lut[i] = static_cast<double>(i) / 63.0;

    Mhc2Params params{};
    params.lut = lut;
    params.min_nits = 0.0;
    params.max_nits = 1000.0;

    auto data = generate_mhc2_profile(params);

    // Find MHC2 tag in tag table
    uint32_t tag_count = (data[128] << 24) | (data[129] << 16) | (data[130] << 8) | data[131];
    CHECK(tag_count == 11);

    bool found_mhc2 = false;
    for (uint32_t i = 0; i < tag_count; ++i) {
        uint32_t offset = 132 + i * 12;
        if (data[offset] == 'M' && data[offset+1] == 'H' &&
            data[offset+2] == 'C' && data[offset+3] == '2') {
            found_mhc2 = true;
            break;
        }
    }
    CHECK(found_mhc2);
}

TEST_CASE("Profile writes to file and reads back") {
    auto lut = std::vector<double>(64, 0.0);
    for (int i = 0; i < 64; ++i) lut[i] = static_cast<double>(i) / 63.0;

    Mhc2Params params{};
    params.lut = lut;
    params.min_nits = 0.0;
    params.max_nits = 1000.0;

    auto data = generate_mhc2_profile(params);
    auto path = std::filesystem::temp_directory_path() / L"test_profile.icm";
    CHECK(write_profile_to_file(data, path));
    CHECK(std::filesystem::exists(path));
    CHECK(std::filesystem::file_size(path) == data.size());
    std::filesystem::remove(path);
}
