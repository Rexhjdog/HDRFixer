#include "pixel_format_fix.h"
#include <format>

namespace hdrfixer::fixes {

PixelFormatFix::PixelFormatFix(const display::DisplayInfo& display)
    : display_(display)
{
}

std::string PixelFormatFix::name() const {
    return "Pixel Format";
}

std::string PixelFormatFix::description() const {
    return "Checks whether 10-bit color output is enabled in the GPU driver";
}

FixCategory PixelFormatFix::category() const {
    return FixCategory::PixelFormat;
}

FixStatus PixelFormatFix::diagnose() {
    uint32_t bpc = display_.bits_per_color;

    if (bpc >= 10) {
        return FixStatus{
            FixState::NotNeeded,
            std::format("{}-bit color output detected; no action needed.", bpc)
        };
    }

    // 8-bit (or unknown) -- recommend the user enable 10-bit in their
    // GPU driver control panel.
    std::string vendor_hint;
    switch (display_.gpu_vendor) {
        case display::GpuVendor::Nvidia:
            vendor_hint = "Open NVIDIA Control Panel > Change Resolution > "
                          "set Output color depth to 10 bpc.";
            break;
        case display::GpuVendor::Amd:
            vendor_hint = "Open AMD Software > Display > "
                          "set Color Depth to 10 bpc.";
            break;
        case display::GpuVendor::Intel:
            vendor_hint = "Open Intel Graphics Command Center > Display > "
                          "Color > set to 10-bit.";
            break;
        default:
            vendor_hint = "Open your GPU driver settings and set color depth "
                          "to 10 bpc.";
            break;
    }

    return FixStatus{
        FixState::NotNeeded,
        std::format("{}-bit color output detected.  Recommend enabling 10-bit "
                    "in GPU driver settings.  {}", bpc, vendor_hint)
    };
}

FixResult PixelFormatFix::apply() {
    // Pixel format can only be changed through the GPU driver UI; there is
    // no programmatic API available for this setting.
    return FixResult{
        false,
        "Pixel format must be changed manually in your GPU driver control "
        "panel.  Use diagnose() for vendor-specific instructions."
    };
}

FixResult PixelFormatFix::revert() {
    // Nothing to revert since apply() never changes the system.
    return FixResult{
        false,
        "Pixel format is managed by the GPU driver; nothing to revert."
    };
}

} // namespace hdrfixer::fixes
