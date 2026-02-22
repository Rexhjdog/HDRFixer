#pragma once
#include "icc_binary.h"
#include <vector>
#include <filesystem>

namespace hdrfixer::profile {

struct Mhc2Params {
    std::vector<double> lut;
    double min_nits = 0.0;
    double max_nits = 1000.0;
    double gamma = 2.2;
    std::string description = "HDRFixer Gamma 2.2 Correction";
};

std::vector<uint8_t> generate_mhc2_profile(const Mhc2Params& params);
bool write_profile_to_file(const std::vector<uint8_t>& data, const std::filesystem::path& path);

} // namespace hdrfixer::profile
