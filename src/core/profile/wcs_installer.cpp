#include "wcs_installer.h"
#include <icm.h>
#include <format>

// ColorProfileAddDisplayAssociation is available on Windows 11+
// Dynamically load to avoid hard dependency on specific SDK version
typedef HRESULT (WINAPI *PFN_ColorProfileAddDisplayAssociation)(
    WCS_PROFILE_MANAGEMENT_SCOPE scope,
    PCWSTR profileName,
    LUID targetAdapterID,
    UINT sourceID,
    BOOL setAsDefault,
    BOOL associateAsAdvancedColor
);

typedef HRESULT (WINAPI *PFN_ColorProfileRemoveDisplayAssociation)(
    WCS_PROFILE_MANAGEMENT_SCOPE scope,
    PCWSTR profileName,
    LUID targetAdapterID,
    UINT sourceID,
    BOOL dissociateAdvancedColor
);

namespace hdrfixer::profile {

std::expected<void, std::string> install_profile(const InstallParams& params) {
    // Step 1: Install profile into system color directory
    if (!InstallColorProfileW(nullptr, params.profile_path.c_str())) {
        DWORD err = GetLastError();
        return std::unexpected(std::format("InstallColorProfileW failed: error {}", err));
    }

    // Step 2: Associate with display via ColorProfileAddDisplayAssociation
    auto filename = params.profile_path.filename().wstring();

    HMODULE mscms = GetModuleHandleW(L"Mscms.dll");
    if (!mscms) mscms = LoadLibraryW(L"Mscms.dll");

    auto pfn = reinterpret_cast<PFN_ColorProfileAddDisplayAssociation>(
        GetProcAddress(mscms, "ColorProfileAddDisplayAssociation"));

    if (pfn) {
        HRESULT hr = pfn(
            WCS_PROFILE_MANAGEMENT_SCOPE_CURRENT_USER,
            filename.c_str(),
            params.adapter_luid,
            params.source_id,
            params.set_as_default ? TRUE : FALSE,
            TRUE // associateAsAdvancedColor
        );
        if (FAILED(hr))
            return std::unexpected(std::format("ColorProfileAddDisplayAssociation failed: 0x{:08X}", static_cast<unsigned>(hr)));
    } else {
        // Fallback: legacy WcsAssociateColorProfileWithDevice (needs device name)
        return std::unexpected("ColorProfileAddDisplayAssociation not available; Windows 11 required");
    }

    return {};
}

std::expected<void, std::string> uninstall_profile(const std::wstring& filename, LUID adapter_luid, uint32_t source_id) {
    // Step 1: Remove display association
    HMODULE mscms = GetModuleHandleW(L"Mscms.dll");
    if (!mscms) mscms = LoadLibraryW(L"Mscms.dll");

    auto pfn = reinterpret_cast<PFN_ColorProfileRemoveDisplayAssociation>(
        GetProcAddress(mscms, "ColorProfileRemoveDisplayAssociation"));

    if (pfn) {
        pfn(WCS_PROFILE_MANAGEMENT_SCOPE_CURRENT_USER,
            filename.c_str(), adapter_luid, source_id, TRUE);
    }

    // Step 2: Uninstall and delete profile
    if (!UninstallColorProfileW(nullptr, filename.c_str(), TRUE)) {
        DWORD err = GetLastError();
        return std::unexpected(std::format("UninstallColorProfileW failed: error {}", err));
    }

    return {};
}

} // namespace hdrfixer::profile
