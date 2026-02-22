#pragma once
#include "core/fixes/fix_engine.h"
#include "core/display/display_info.h"
#include "core/display/display_config.h"
#include <vector>
#include <utility>

namespace hdrfixer::fixes {

// Manages a "screen-share mode" toggle.  When activated, all displays have
// their SDR white level lowered to 80 nits so that screen-sharing software
// shows a correct brightness.  When deactivated the original white levels
// are restored.
class ShareHelper : public IFix {
public:
    ShareHelper();

    std::string name() const override;
    std::string description() const override;
    FixCategory category() const override;

    FixStatus diagnose() override;
    FixResult apply() override;
    FixResult revert() override;

    bool is_share_mode_active() const { return share_mode_active_; }

private:
    static constexpr float kShareModeNits = 80.0f;

    bool share_mode_active_ = false;

    // Saved (adapter_luid, target_id, original_nits) for each display so we
    // can restore on revert.
    struct SavedLevel {
        LUID adapter_id;
        uint32_t target_id;
        float original_nits;
    };
    std::vector<SavedLevel> saved_levels_;
};

} // namespace hdrfixer::fixes
