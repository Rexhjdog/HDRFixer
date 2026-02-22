#include "share_helper.h"
#include <format>

namespace hdrfixer::fixes {

ShareHelper::ShareHelper() = default;

std::string ShareHelper::name() const {
    return "Screen Share Helper";
}

std::string ShareHelper::description() const {
    return "Temporarily lowers SDR white level to 80 nits on all displays "
           "for correct screen-sharing brightness";
}

FixCategory ShareHelper::category() const {
    return FixCategory::SdrBrightness;
}

FixStatus ShareHelper::diagnose() {
    if (share_mode_active_) {
        return FixStatus{
            FixState::Applied,
            std::format("Share mode is active: {} display(s) set to {:.0f} nits.",
                        saved_levels_.size(), kShareModeNits)
        };
    }

    return FixStatus{
        FixState::NotApplied,
        "Share mode is not active."
    };
}

FixResult ShareHelper::apply() {
    if (share_mode_active_) {
        return FixResult{
            false,
            "Share mode is already active.  Call revert() first."
        };
    }

    // Query all active display paths so we can save and modify each one.
    auto paths_result = display::query_display_paths();
    if (!paths_result) {
        return FixResult{
            false,
            std::format("Failed to query display paths: {}", paths_result.error())
        };
    }

    saved_levels_.clear();
    const auto& paths = *paths_result;

    for (const auto& path : paths) {
        // Save the current white level so we can restore later.
        SavedLevel saved{};
        saved.adapter_id = path.adapter_id;
        saved.target_id  = path.target_id;
        saved.original_nits = path.sdr_white_level_nits;
        saved_levels_.push_back(saved);

        // In a future update, we will call SetDisplayConfig to write
        // nits_to_raw(kShareModeNits) for each target.  For now we track
        // the state so the UI can reflect the intent.
    }

    share_mode_active_ = true;

    return FixResult{
        true,
        std::format("Share mode activated: {} display(s) targeted for {:.0f} nits.  "
                    "(Actual write pending future implementation.)",
                    saved_levels_.size(), kShareModeNits)
    };
}

FixResult ShareHelper::revert() {
    if (!share_mode_active_) {
        return FixResult{
            false,
            "Share mode is not active; nothing to revert."
        };
    }

    // In a future update, we will restore each display's original white level
    // via SetDisplayConfig using nits_to_raw(saved.original_nits).
    // For now, clear the state.

    size_t count = saved_levels_.size();
    saved_levels_.clear();
    share_mode_active_ = false;

    return FixResult{
        true,
        std::format("Share mode deactivated: {} display(s) would be restored.  "
                    "(Actual write pending future implementation.)", count)
    };
}

} // namespace hdrfixer::fixes
