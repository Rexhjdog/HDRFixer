if(WIN32)
    add_executable(hdrfixer_tests
        test_main.cpp
        test_transfer_functions.cpp
        test_gamma_lut.cpp
        test_edid_reader.cpp
        test_mhc2_writer.cpp
        test_sdr_white_level.cpp
        test_display_info.cpp
        test_fix_engine.cpp
        test_registry.cpp
        test_settings.cpp
    )
    target_include_directories(hdrfixer_tests PRIVATE
        ${CMAKE_CURRENT_SOURCE_DIR}
        ${CMAKE_CURRENT_SOURCE_DIR}/../src
    )
    target_link_libraries(hdrfixer_tests PRIVATE hdrfixer_core)
else()
    # On non-Windows, we test only what we can mock or is platform independent
    add_executable(hdrfixer_tests
        test_main.cpp
        test_settings.cpp
        ../src/core/config/settings.cpp
    )
    target_include_directories(hdrfixer_tests PRIVATE
        ${CMAKE_CURRENT_SOURCE_DIR}/mocks
        ${CMAKE_CURRENT_SOURCE_DIR}
        ${CMAKE_CURRENT_SOURCE_DIR}/../src
    )
    # No hdrfixer_core link
endif()

add_test(NAME unit_tests COMMAND hdrfixer_tests)
