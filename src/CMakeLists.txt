# Core library - testable parts (cross-platform)
add_library(hdrfixer_core_testable STATIC
    core/color/transfer_functions.cpp
    core/color/gamma_lut.cpp
    core/display/edid_reader.cpp
    core/fixes/fix_engine.cpp
    core/profile/mhc2_writer.cpp
)
target_include_directories(hdrfixer_core_testable PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})

if(WIN32)
    # Full core library with Windows-specific code
    add_library(hdrfixer_core STATIC
        core/color/transfer_functions.cpp
        core/color/gamma_lut.cpp
        core/display/dxgi_detector.cpp
        core/display/display_config.cpp
        core/display/edid_reader.cpp
        core/profile/mhc2_writer.cpp
        core/profile/wcs_installer.cpp
        core/registry/hdr_registry.cpp
        core/registry/registry_backup.cpp
        core/config/settings.cpp
        core/log/logger.cpp
        core/fixes/fix_engine.cpp
    )
    target_include_directories(hdrfixer_core PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})
    target_link_libraries(hdrfixer_core PUBLIC
        dxgi.lib user32.lib shell32.lib advapi32.lib mscms.lib ole32.lib
    )

    # Main application
    add_executable(HDRFixer WIN32
        app/main.cpp
        app/ui/tray.cpp
        app/ui/settings_wnd.cpp
        app/fixes/gamma_fix.cpp
        app/fixes/sdr_brightness_fix.cpp
        app/fixes/pixel_format_fix.cpp
        app/fixes/share_helper.cpp
        app/fixes/watchdog.cpp
        app/fixes/hotplug.cpp
    )
    target_include_directories(HDRFixer PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})
    target_link_libraries(HDRFixer PRIVATE hdrfixer_core comctl32.lib)
else()
    # On Linux, also build mockable Windows-dependent source files for testing
    add_library(hdrfixer_core_mocked STATIC
        core/color/transfer_functions.cpp
        core/color/gamma_lut.cpp
        core/display/edid_reader.cpp
        core/display/display_config.cpp
        core/fixes/fix_engine.cpp
        core/profile/mhc2_writer.cpp
        core/registry/hdr_registry.cpp
        core/registry/registry_backup.cpp
        core/config/settings.cpp
        core/log/logger.cpp
    )
    target_include_directories(hdrfixer_core_mocked PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})
    # Mock headers must come FIRST to override <windows.h> etc.
    target_include_directories(hdrfixer_core_mocked SYSTEM BEFORE PUBLIC ${CMAKE_SOURCE_DIR}/tests/mocks)
    target_compile_definitions(hdrfixer_core_mocked PUBLIC _MOCK_WINDOWS=1)
endif()
