#include "sdr_brightness_fix.h"
#include "core/display/display_config.h"
#include <cmath>
#include <format>

namespace hdrfixer::fixes {

SdrBrightnessFix::SdrBrightnessFix(const display::DisplayInfo& display)
    : display_(display)
{
}

std::string SdrBrightnessFix::name() const {
    return "SDR Brightness";
}

std::string SdrBrightnessFix::description() const {
    return "Adjusts SDR content white level to match display capabilities";
}

FixCategory SdrBrightnessFix::category() const {
    return FixCategory::SdrBrightness;
}

float SdrBrightnessFix::optimal_white_level() const {
    float max_lum = display_.max_luminance;
    if (max_lum >= 800.0f) return 200.0f;
    if (max_lum >= 600.0f) return 250.0f;
    if (max_lum >= 400.0f) return 280.0f;
    return 200.0f;
}

FixStatus SdrBrightnessFix::diagnose() {
    // Read the live SDR white level from the OS for this display target.
    auto current_result = display::get_sdr_white_level(
        display_.adapter_luid, display_.target_id);

    float current_nits = current_result.value_or(display_.sdr_white_level_nits);
    float optimal_nits = optimal_white_level();
    float diff = std::abs(current_nits - optimal_nits);

    if (diff < 30.0f) {
        return FixStatus{
            FixState::NotNeeded,
            std::format("SDR white level is near optimal ({:.0f} nits, "
                        "recommended {:.0f} nits)", current_nits, optimal_nits)
        };
    }

    return FixStatus{
        FixState::NotApplied,
        std::format("SDR white level is {:.0f} nits, recommended {:.0f} nits "
                    "(diff {:.0f})", current_nits, optimal_nits, diff)
    };
}

FixResult SdrBrightnessFix::apply() {
    // Read current value for reporting.
    auto current_result = display::get_sdr_white_level(
        display_.adapter_luid, display_.target_id);

    float current_nits = current_result.value_or(display_.sdr_white_level_nits);
    float optimal_nits = optimal_white_level();

    // Writing the SDR white level via SetDisplayConfig requires an
    // undocumented / elevated path.  For now we report the recommended
    // value and let a future spike implement the actual write.
    return FixResult{
        true,
        std::format("Recommend setting SDR white level from {:.0f} to {:.0f} nits "
                    "(max luminance: {:.0f} nits).  "
                    "Automatic adjustment will be available in a future update.",
                    current_nits, optimal_nits, display_.max_luminance)
    };
}

FixResult SdrBrightnessFix::revert() {
    // Nothing to revert since apply() does not modify the system yet.
    return FixResult{
        true,
        "No changes were made; nothing to revert."
    };
}

} // namespace hdrfixer::fixes
