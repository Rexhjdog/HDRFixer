#include "doctest.h"
#include "core/color/gamma_lut.h"

using namespace hdrfixer::color;

TEST_CASE("SDR LUT basics") {
    auto lut = generate_sdr_lut(1024);
    CHECK(lut.size() == 1024);
    CHECK(lut.front() == doctest::Approx(0.0).epsilon(0.001));
    CHECK(lut.back() == doctest::Approx(1.0).epsilon(0.001));
}

TEST_CASE("SDR LUT monotonically increasing") {
    auto lut = generate_sdr_lut(1024);
    for (size_t i = 1; i < lut.size(); ++i) {
        CHECK(lut[i] >= lut[i - 1]);
    }
}

TEST_CASE("SDR LUT midpoint darker than linear") {
    auto lut = generate_sdr_lut(1024);
    CHECK(lut[512] < 0.5);
}

TEST_CASE("HDR LUT basics") {
    auto lut = generate_hdr_lut(4096, 200.0, 0.0);
    CHECK(lut.size() == 4096);
    CHECK(lut.front() == doctest::Approx(0.0).epsilon(0.001));
}

TEST_CASE("HDR LUT passthrough above white level") {
    auto lut = generate_hdr_lut(4096, 200.0, 0.0);
    // Values above SDR white should be passthrough (lut[i] == input)
    // The last entry maps PQ=1.0 (10000 nits) which is above 200 nits
    CHECK(lut.back() == doctest::Approx(1.0).epsilon(0.001));
}

TEST_CASE("HDR LUT monotonically increasing") {
    auto lut = generate_hdr_lut(4096, 200.0, 0.0);
    for (size_t i = 1; i < lut.size(); ++i) {
        CHECK(lut[i] >= lut[i - 1]);
    }
}
