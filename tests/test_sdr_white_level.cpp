#include "doctest.h"
#include "core/display/display_config.h"

using namespace hdrfixer::display;

TEST_CASE("SDR white level raw to nits") {
    CHECK(raw_to_nits(1000) == doctest::Approx(80.0f));
    CHECK(raw_to_nits(2500) == doctest::Approx(200.0f));
    CHECK(raw_to_nits(5000) == doctest::Approx(400.0f));
}

TEST_CASE("SDR white level nits to raw") {
    CHECK(nits_to_raw(80.0f) == 1000);
    CHECK(nits_to_raw(200.0f) == 2500);
    CHECK(nits_to_raw(400.0f) == 5000);
}
