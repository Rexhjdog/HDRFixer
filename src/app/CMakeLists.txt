add_executable(HDRFixer WIN32
    main.cpp
    resource.rc
    fixes/gamma_fix.cpp
    fixes/sdr_brightness_fix.cpp
    fixes/pixel_format_fix.cpp
    fixes/share_helper.cpp
    fixes/watchdog.cpp
    fixes/hotplug.cpp
    ui/tray.cpp
    ui/settings_wnd.cpp
)

target_include_directories(HDRFixer PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/..)
target_link_libraries(HDRFixer PRIVATE hdrfixer_core comctl32.lib)
