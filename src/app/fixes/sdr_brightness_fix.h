#pragma once
#include "core/fixes/fix_engine.h"
#include "core/display/display_info.h"

namespace hdrfixer::fixes {

// Detects sub-optimal SDR content brightness (white level) and recommends
// the correct value based on the display's peak luminance capability.
class SdrBrightnessFix : public IFix {
public:
    explicit SdrBrightnessFix(const display::DisplayInfo& display);

    std::string name() const override;
    std::string description() const override;
    FixCategory category() const override;

    FixStatus diagnose() override;
    FixResult apply() override;
    FixResult revert() override;

private:
    // Calculate the optimal SDR white level based on panel max luminance.
    float optimal_white_level() const;

    const display::DisplayInfo& display_;
};

} // namespace hdrfixer::fixes
