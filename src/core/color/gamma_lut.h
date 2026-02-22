#pragma once
#include <vector>

namespace hdrfixer::color {

std::vector<double> generate_sdr_lut(int size = 1024);
std::vector<double> generate_hdr_lut(int size = 4096, double white_nits = 200.0, double black_nits = 0.0);

} // namespace hdrfixer::color
