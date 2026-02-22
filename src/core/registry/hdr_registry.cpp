#include "core/registry/hdr_registry.h"
#include <format>
#include <sstream>

namespace hdrfixer::registry {

// ---------------------------------------------------------------------------
// DirectX settings parsing / serialization
// ---------------------------------------------------------------------------

std::map<std::wstring, std::wstring> parse_dx_settings(const std::wstring& raw) {
    std::map<std::wstring, std::wstring> result;
    if (raw.empty()) return result;

    std::wstring::size_type pos = 0;
    while (pos < raw.size()) {
        // Find the next semicolon (or end of string)
        auto semi = raw.find(L';', pos);
        std::wstring token;
        if (semi == std::wstring::npos) {
            token = raw.substr(pos);
            pos = raw.size();
        } else {
            token = raw.substr(pos, semi - pos);
            pos = semi + 1;
        }

        if (token.empty()) continue;

        // Split on '='
        auto eq = token.find(L'=');
        if (eq != std::wstring::npos) {
            std::wstring key = token.substr(0, eq);
            std::wstring value = token.substr(eq + 1);
            if (!key.empty()) {
                result[key] = value;
            }
        }
    }

    return result;
}

std::wstring serialize_dx_settings(const std::map<std::wstring, std::wstring>& settings) {
    std::wstring result;
    for (const auto& [key, value] : settings) {
        result += key;
        result += L'=';
        result += value;
        result += L';';
    }
    return result;
}

// ---------------------------------------------------------------------------
// Low-level registry helpers
// ---------------------------------------------------------------------------

static std::string format_win_error(LSTATUS status) {
    char buf[256]{};
    FormatMessageA(
        FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
        nullptr,
        static_cast<DWORD>(status),
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        buf,
        sizeof(buf),
        nullptr);

    // Trim trailing \r\n
    std::string msg(buf);
    while (!msg.empty() && (msg.back() == '\r' || msg.back() == '\n'))
        msg.pop_back();
    return msg;
}

std::expected<DWORD, std::string> read_dword(HKEY root, const wchar_t* subkey, const wchar_t* value_name) {
    HKEY hkey = nullptr;
    LSTATUS status = RegOpenKeyExW(root, subkey, 0, KEY_READ, &hkey);
    if (status != ERROR_SUCCESS) {
        return std::unexpected(
            std::format("Failed to open registry key: {}", format_win_error(status)));
    }

    DWORD data = 0;
    DWORD size = sizeof(data);
    DWORD type = 0;
    status = RegQueryValueExW(hkey, value_name, nullptr, &type,
                              reinterpret_cast<BYTE*>(&data), &size);
    RegCloseKey(hkey);

    if (status != ERROR_SUCCESS) {
        return std::unexpected(
            std::format("Failed to read registry value: {}", format_win_error(status)));
    }

    if (type != REG_DWORD) {
        return std::unexpected("Registry value is not a DWORD");
    }

    return data;
}

std::expected<void, std::string> write_dword(HKEY root, const wchar_t* subkey,
                                              const wchar_t* value_name, DWORD value) {
    HKEY hkey = nullptr;
    DWORD disposition = 0;
    LSTATUS status = RegCreateKeyExW(root, subkey, 0, nullptr,
                                     REG_OPTION_NON_VOLATILE, KEY_WRITE,
                                     nullptr, &hkey, &disposition);
    if (status != ERROR_SUCCESS) {
        return std::unexpected(
            std::format("Failed to create/open registry key: {}", format_win_error(status)));
    }

    status = RegSetValueExW(hkey, value_name, 0, REG_DWORD,
                            reinterpret_cast<const BYTE*>(&value), sizeof(value));
    RegCloseKey(hkey);

    if (status != ERROR_SUCCESS) {
        return std::unexpected(
            std::format("Failed to write registry value: {}", format_win_error(status)));
    }

    return {};
}

std::expected<std::wstring, std::string> read_string(HKEY root, const wchar_t* subkey,
                                                      const wchar_t* value_name) {
    HKEY hkey = nullptr;
    LSTATUS status = RegOpenKeyExW(root, subkey, 0, KEY_READ, &hkey);
    if (status != ERROR_SUCCESS) {
        return std::unexpected(
            std::format("Failed to open registry key: {}", format_win_error(status)));
    }

    // First query to get the required buffer size
    DWORD size = 0;
    DWORD type = 0;
    status = RegQueryValueExW(hkey, value_name, nullptr, &type, nullptr, &size);
    if (status != ERROR_SUCCESS) {
        RegCloseKey(hkey);
        return std::unexpected(
            std::format("Failed to query registry value size: {}", format_win_error(status)));
    }

    if (type != REG_SZ && type != REG_EXPAND_SZ) {
        RegCloseKey(hkey);
        return std::unexpected("Registry value is not a string type");
    }

    // Allocate buffer and read the value
    std::wstring data;
    data.resize(size / sizeof(wchar_t));
    status = RegQueryValueExW(hkey, value_name, nullptr, &type,
                              reinterpret_cast<BYTE*>(data.data()), &size);
    RegCloseKey(hkey);

    if (status != ERROR_SUCCESS) {
        return std::unexpected(
            std::format("Failed to read registry string value: {}", format_win_error(status)));
    }

    // Remove trailing null character(s) if present
    while (!data.empty() && data.back() == L'\0') {
        data.pop_back();
    }

    return data;
}

// ---------------------------------------------------------------------------
// HDR state accessors
// ---------------------------------------------------------------------------

std::expected<bool, std::string> read_hdr_enabled() {
    auto result = read_dword(HKEY_LOCAL_MACHINE, kGraphicsDrivers, L"AutoHDREnabled");
    if (!result.has_value()) {
        return std::unexpected(result.error());
    }
    return result.value() != 0;
}

std::expected<void, std::string> write_hdr_enabled(bool enabled) {
    return write_dword(HKEY_LOCAL_MACHINE, kGraphicsDrivers, L"AutoHDREnabled",
                       enabled ? 1u : 0u);
}

std::expected<bool, std::string> read_auto_hdr_enabled() {
    // Auto HDR is typically stored in the DirectX user GPU preferences
    // as AutoHDREnable=1 in the settings string, or as a separate DWORD.
    // We first try reading a DWORD value from the VideoSettings key.
    auto result = read_dword(HKEY_CURRENT_USER, kVideoSettings, L"AutoHDR");
    if (result.has_value()) {
        return result.value() != 0;
    }

    // Fallback: try the GraphicsDrivers key
    auto fallback = read_dword(HKEY_LOCAL_MACHINE, kGraphicsDrivers, L"AutoHDREnabled");
    if (fallback.has_value()) {
        return fallback.value() != 0;
    }

    return std::unexpected(fallback.error());
}

std::expected<void, std::string> write_auto_hdr_enabled(bool enabled) {
    // Write to VideoSettings for the current user
    auto result = write_dword(HKEY_CURRENT_USER, kVideoSettings, L"AutoHDR",
                              enabled ? 1u : 0u);
    if (!result.has_value()) {
        return result;
    }

    // Also update the GraphicsDrivers system-wide setting
    return write_dword(HKEY_LOCAL_MACHINE, kGraphicsDrivers, L"AutoHDREnabled",
                       enabled ? 1u : 0u);
}

} // namespace hdrfixer::registry
