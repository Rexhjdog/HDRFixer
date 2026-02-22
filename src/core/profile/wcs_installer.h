#pragma once
#include <string>
#include <expected>
#include <filesystem>
#include <windows.h>

namespace hdrfixer::profile {

struct InstallParams {
    std::filesystem::path profile_path;
    LUID adapter_luid;
    uint32_t source_id;
    bool set_as_default = true;
};

std::expected<void, std::string> install_profile(const InstallParams& params);
std::expected<void, std::string> uninstall_profile(const std::wstring& filename, LUID adapter_luid, uint32_t source_id);

} // namespace hdrfixer::profile
