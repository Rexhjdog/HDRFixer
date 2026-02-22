import sys

target_file = "src/app/fixes/gamma_fix.cpp"

with open(target_file, "r") as f:
    content = f.read()

search_str = """    GetSystemDirectoryW(color_dir, MAX_PATH);
    auto system_profile = std::filesystem::path(color_dir) / L"spool" / L"drivers" / L"color" / profile_filename();

    if (std::filesystem::exists(system_profile)) {
        return {FixState::Applied, "Gamma 2.2 correction profile is installed"};
    }"""

replace_str = """    DWORD size = MAX_PATH;
    if (GetColorDirectoryW(nullptr, color_dir, &size)) {
        auto system_profile = std::filesystem::path(color_dir) / profile_filename();

        if (std::filesystem::exists(system_profile)) {
            return {FixState::Applied, "Gamma 2.2 correction profile is installed"};
        }
    }"""

if search_str in content:
    new_content = content.replace(search_str, replace_str)
    with open(target_file, "w") as f:
        f.write(new_content)
    print("Replacement successful")
else:
    print("Search string not found")
    start_idx = content.find("wchar_t color_dir[MAX_PATH] = {};")
    if start_idx != -1:
        print("Found nearby context:")
        print(content[start_idx:start_idx+500])
