#include "doctest.h"
#include "core/display/display_info.h"

using namespace hdrfixer::display;

TEST_CASE("DisplayInfo defaults") {
    DisplayInfo info{};
    CHECK(info.device_name.empty());
    CHECK(info.is_hdr_enabled == false);
    CHECK(info.bits_per_color == 0);
}

TEST_CASE("DisplayInfo HDR capable threshold") {
    DisplayInfo info{};
    info.max_luminance = 249.0f;
    CHECK(info.is_hdr_capable() == false);
    info.max_luminance = 251.0f;
    CHECK(info.is_hdr_capable() == true);
}

TEST_CASE("GpuVendor from ID") {
    CHECK(gpu_vendor_from_id(0x10DE) == GpuVendor::Nvidia);
    CHECK(gpu_vendor_from_id(0x1002) == GpuVendor::Amd);
    CHECK(gpu_vendor_from_id(0x8086) == GpuVendor::Intel);
    CHECK(gpu_vendor_from_id(0x0000) == GpuVendor::Unknown);
}
