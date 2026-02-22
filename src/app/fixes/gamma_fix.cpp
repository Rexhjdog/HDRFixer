#include "gamma_fix.h"
#include "core/color/gamma_lut.h"
#include "core/profile/mhc2_writer.h"
#include "core/profile/wcs_installer.h"
#include "core/display/display_info.h"
#include <format>
#include <icm.h>

namespace hdrfixer::fixes {

GammaFix::GammaFix(const hdrfixer::display::DisplayInfo& display)
    : display_(display) {}

std::string GammaFix::name() const {
    return "GammaCorrection";
}

std::string GammaFix::description() const {
    return "Applies gamma 2.2 correction via MHC2 ICC profile to fix washed-out HDR colors";
}

FixCategory GammaFix::category() const {
    return FixCategory::ToneCurve;
}

std::filesystem::path GammaFix::profile_path() const {
    wchar_t temp_dir[MAX_PATH] = {};
    GetTempPathW(MAX_PATH, temp_dir);
    return std::filesystem::path(temp_dir) / profile_filename();
}

std::wstring GammaFix::profile_filename() const {
    return L"HDRFixer_Gamma22.icm";
}

FixResult GammaFix::apply() {
    // Step 1: Determine SDR white level for this display
    double white_nits = static_cast<double>(display_.sdr_white_level_nits);
    if (white_nits <= 0.0) {
        white_nits = kDefaultSdrWhiteNits;
    }

    // Step 2: Generate HDR gamma correction LUT
    auto lut = hdrfixer::color::generate_hdr_lut(kLutSize, white_nits, 0.0);

    // Step 3: Build MHC2 ICC profile
    hdrfixer::profile::Mhc2Params params{};
    params.lut = std::move(lut);
    params.min_nits = 0.0;
    params.max_nits = static_cast<double>(display_.max_luminance);
    params.gamma = 2.2;
    params.description = "HDRFixer Gamma 2.2 Correction";

    auto profile_data = hdrfixer::profile::generate_mhc2_profile(params);

    // Step 4: Write profile to temp directory
    auto path = profile_path();
    if (!hdrfixer::profile::write_profile_to_file(profile_data, path)) {
        return {false, std::format("Failed to write profile to {}", path.string())};
    }

    // Step 5: Install profile via WCS and associate with this display
    hdrfixer::profile::InstallParams install{};
    install.profile_path = path;
    install.adapter_luid = display_.adapter_luid;
    install.source_id = display_.source_id;
    install.set_as_default = true;

    auto result = hdrfixer::profile::install_profile(install);
    if (!result.has_value()) {
        return {false, std::format("Failed to install profile: {}", result.error())};
    }

    return {true, "Gamma 2.2 correction profile applied successfully"};
}

FixResult GammaFix::revert() {
    auto result = hdrfixer::profile::uninstall_profile(
        profile_filename(),
        display_.adapter_luid,
        display_.source_id
    );

    if (!result.has_value()) {
        return {false, std::format("Failed to uninstall profile: {}", result.error())};
    }

    // Clean up temp file
    auto path = profile_path();
    std::error_code ec;
    std::filesystem::remove(path, ec);

    return {true, "Gamma 2.2 correction profile removed"};
}

FixStatus GammaFix::diagnose() {
    // Check if the profile file exists in the system color directory
    // The WCS API installs profiles to the system color directory,
    // so we check if our profile filename is installed
    wchar_t color_dir[MAX_PATH] = {};
    DWORD size = MAX_PATH;
    if (GetColorDirectoryW(nullptr, color_dir, &size)) {
        auto system_profile = std::filesystem::path(color_dir) / profile_filename();

        if (std::filesystem::exists(system_profile)) {
            return {FixState::Applied, "Gamma 2.2 correction profile is installed"};
        }
    }

    // Also check if the temp file exists (profile was generated but maybe not installed)
    auto temp_profile = profile_path();
    if (std::filesystem::exists(temp_profile)) {
        return {FixState::Error, "Profile file exists but is not installed"};
    }

    return {FixState::NotApplied, "Gamma 2.2 correction profile is not installed"};
}

} // namespace hdrfixer::fixes
