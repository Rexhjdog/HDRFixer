#include "doctest.h"
#include "core/registry/hdr_registry.h"

using namespace hdrfixer::registry;

TEST_CASE("Registry path constants") {
    CHECK(std::wstring(kGraphicsDrivers) == L"SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers");
    CHECK(std::wstring(kDirectXUserPrefs) == L"Software\\Microsoft\\DirectX\\UserGpuPreferences");
}

TEST_CASE("DirectX settings parse") {
    auto settings = parse_dx_settings(L"SwapEffectUpgradeEnable=1;AutoHDREnable=1;");
    CHECK(settings.size() == 2);
    CHECK(settings[L"SwapEffectUpgradeEnable"] == L"1");
    CHECK(settings[L"AutoHDREnable"] == L"1");
}

TEST_CASE("DirectX settings serialize") {
    std::map<std::wstring, std::wstring> settings;
    settings[L"AutoHDREnable"] = L"1";
    settings[L"SwapEffectUpgradeEnable"] = L"1";
    auto result = serialize_dx_settings(settings);
    CHECK(result.find(L"AutoHDREnable=1;") != std::wstring::npos);
    CHECK(result.find(L"SwapEffectUpgradeEnable=1;") != std::wstring::npos);
}

TEST_CASE("DirectX settings empty string") {
    auto settings = parse_dx_settings(L"");
    CHECK(settings.empty());
}

TEST_CASE("DirectX settings trailing semicolon") {
    auto settings = parse_dx_settings(L"Key=Value;");
    CHECK(settings.size() == 1);
    CHECK(settings[L"Key"] == L"Value");
}

TEST_CASE("DirectX settings no trailing semicolon") {
    auto settings = parse_dx_settings(L"Key=Value");
    CHECK(settings.size() == 1);
    CHECK(settings[L"Key"] == L"Value");
}
