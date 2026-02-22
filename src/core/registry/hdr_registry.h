#pragma once
#include <string>
#include <map>
#include <optional>
#include <expected>
#include <windows.h>

namespace hdrfixer::registry {

// Registry path constants
inline constexpr wchar_t kGraphicsDrivers[] = L"SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers";
inline constexpr wchar_t kMonitorDataStore[] = L"SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers\\MonitorDataStore";
inline constexpr wchar_t kDirectXUserPrefs[] = L"Software\\Microsoft\\DirectX\\UserGpuPreferences";
inline constexpr wchar_t kDirect3D[] = L"Software\\Microsoft\\Direct3D";
inline constexpr wchar_t kVideoSettings[] = L"Software\\Microsoft\\Windows\\CurrentVersion\\VideoSettings";

// Parse DirectX settings string "Key1=Value1;Key2=Value2;" into map
std::map<std::wstring, std::wstring> parse_dx_settings(const std::wstring& raw);

// Serialize map back to "Key1=Value1;Key2=Value2;" format
std::wstring serialize_dx_settings(const std::map<std::wstring, std::wstring>& settings);

// Read/write HDR enabled state
std::expected<bool, std::string> read_hdr_enabled();
std::expected<void, std::string> write_hdr_enabled(bool enabled);

// Read/write Auto HDR state
std::expected<bool, std::string> read_auto_hdr_enabled();
std::expected<void, std::string> write_auto_hdr_enabled(bool enabled);

// Read a DWORD value from registry
std::expected<DWORD, std::string> read_dword(HKEY root, const wchar_t* subkey, const wchar_t* value_name);

// Write a DWORD value to registry
std::expected<void, std::string> write_dword(HKEY root, const wchar_t* subkey, const wchar_t* value_name, DWORD value);

// Read a string value from registry
std::expected<std::wstring, std::string> read_string(HKEY root, const wchar_t* subkey, const wchar_t* value_name);

} // namespace hdrfixer::registry
