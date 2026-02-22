#include "doctest.h"
#include "core/color/transfer_functions.h"

using namespace hdrfixer::color;

TEST_CASE("sRGB EOTF") {
    CHECK(srgb_eotf(0.0) == doctest::Approx(0.0));
    CHECK(srgb_eotf(1.0) == doctest::Approx(1.0));
    CHECK(srgb_eotf(0.5) == doctest::Approx(0.214041).epsilon(0.001));
    // Below linear threshold
    CHECK(srgb_eotf(0.04045) == doctest::Approx(0.04045 / 12.92).epsilon(0.0001));
}

TEST_CASE("sRGB round-trip") {
    for (double v = 0.0; v <= 1.0; v += 0.1) {
        CHECK(srgb_inv_eotf(srgb_eotf(v)) == doctest::Approx(v).epsilon(0.0001));
    }
}

TEST_CASE("PQ EOTF") {
    CHECK(pq_eotf(0.0) == doctest::Approx(0.0).epsilon(0.01));
    CHECK(pq_eotf(1.0) == doctest::Approx(10000.0).epsilon(1.0));
    // 500 nits round-trip
    double pq500 = pq_inv_eotf(500.0);
    CHECK(pq_eotf(pq500) == doctest::Approx(500.0).epsilon(0.1));
}

TEST_CASE("Gamma power law") {
    CHECK(gamma_eotf(0.5, 2.2) == doctest::Approx(std::pow(0.5, 2.2)).epsilon(0.0001));
    CHECK(gamma_inv_eotf(0.5, 2.2) == doctest::Approx(std::pow(0.5, 1.0/2.2)).epsilon(0.0001));
}

TEST_CASE("sRGB brighter than gamma 2.2 in deep shadows") {
    // sRGB's linear segment lifts deep shadows above pure gamma 2.2
    double srgb_shadow = srgb_eotf(0.05);
    double g22_shadow = gamma_eotf(0.05, 2.2);
    CHECK(srgb_shadow > g22_shadow);
}
