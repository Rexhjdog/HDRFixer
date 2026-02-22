add_library(hdrfixer_core STATIC
    color/transfer_functions.cpp
    color/gamma_lut.cpp
    display/dxgi_detector.cpp
    display/display_config.cpp
    display/edid_reader.cpp
    profile/mhc2_writer.cpp
    profile/wcs_installer.cpp
    registry/hdr_registry.cpp
    registry/registry_backup.cpp
    config/settings.cpp
    log/logger.cpp
)

target_include_directories(hdrfixer_core PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/..)
target_link_libraries(hdrfixer_core PUBLIC
    dxgi.lib
    user32.lib
    shell32.lib
    advapi32.lib
    mscms.lib
    ole32.lib
)
target_precompile_headers(hdrfixer_core PUBLIC core.h)
