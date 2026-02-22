#pragma once
#include "fix_engine.h"
#include "core/display/display_info.h"
#include <filesystem>

namespace hdrfixer::fixes {

class GammaFix : public IFix {
public:
    explicit GammaFix(const hdrfixer::display::DisplayInfo& display);

    std::string name() const override;
    std::string description() const override;
    FixCategory category() const override;
    FixResult apply() override;
    FixResult revert() override;
    FixStatus diagnose() override;

private:
    std::filesystem::path profile_path() const;
    std::wstring profile_filename() const;

    hdrfixer::display::DisplayInfo display_;
    static constexpr double kDefaultSdrWhiteNits = 200.0;
    static constexpr int kLutSize = 4096;
    static constexpr const char* kProfileBaseName = "HDRFixer_Gamma22";
};

} // namespace hdrfixer::fixes
