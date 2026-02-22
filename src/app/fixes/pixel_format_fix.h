#pragma once
#include "core/fixes/fix_engine.h"
#include "core/display/display_info.h"

namespace hdrfixer::fixes {

// Detect-and-warn fix: checks whether the display is using 10-bit (or higher)
// pixel format.  If 8-bit is detected, advises the user to change the setting
// in their GPU driver control panel.  apply()/revert() are intentional no-ops
// because the pixel format can only be changed through the GPU driver UI.
class PixelFormatFix : public IFix {
public:
    explicit PixelFormatFix(const display::DisplayInfo& display);

    std::string name() const override;
    std::string description() const override;
    FixCategory category() const override;

    FixStatus diagnose() override;
    FixResult apply() override;
    FixResult revert() override;

private:
    display::DisplayInfo display_;
};

} // namespace hdrfixer::fixes
