#pragma once
#include "display_info.h"
#include <vector>
#include <expected>

namespace hdrfixer::display {

std::expected<std::vector<DisplayInfo>, std::string> detect_displays();

} // namespace hdrfixer::display
