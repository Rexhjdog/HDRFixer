#include "fix_engine.h"

namespace hdrfixer::fixes {

void FixEngine::register_fix(std::unique_ptr<IFix> fix) {
    fixes_.push_back(std::move(fix));
}

size_t FixEngine::fix_count() const {
    return fixes_.size();
}

void FixEngine::apply_all() {
    for (auto& fix : fixes_) {
        auto status = fix->diagnose();
        if (status.state == FixState::NotApplied) {
            (void)fix->apply();
        }
    }
}

void FixEngine::revert_all() {
    for (auto& fix : fixes_) {
        auto status = fix->diagnose();
        if (status.state == FixState::Applied) {
            (void)fix->revert();
        }
    }
}

std::vector<FixStatus> FixEngine::diagnose_all() {
    std::vector<FixStatus> results;
    results.reserve(fixes_.size());
    for (auto& fix : fixes_) {
        results.push_back(fix->diagnose());
    }
    return results;
}

IFix* FixEngine::get_fix(const std::string& name) {
    for (auto& fix : fixes_) {
        if (fix->name() == name) {
            return fix.get();
        }
    }
    return nullptr;
}

} // namespace hdrfixer::fixes
