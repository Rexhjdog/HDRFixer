# Pure C++ tests (no Windows deps)
add_executable(hdrfixer_tests_pure
    test_main.cpp
    test_transfer_functions.cpp
    test_gamma_lut.cpp
    test_edid_reader.cpp
    test_mhc2_writer.cpp
    test_fix_engine.cpp
)
target_include_directories(hdrfixer_tests_pure PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})
target_link_libraries(hdrfixer_tests_pure PRIVATE hdrfixer_core_testable)
add_test(NAME pure_tests COMMAND hdrfixer_tests_pure)

if(NOT WIN32)
    # Tests that need Windows mocks on Linux
    add_executable(hdrfixer_tests_mocked
        test_main.cpp
        test_display_info.cpp
        test_sdr_white_level.cpp
        test_registry.cpp
        test_settings.cpp
    )
    target_include_directories(hdrfixer_tests_mocked PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})
    target_include_directories(hdrfixer_tests_mocked SYSTEM BEFORE PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/mocks)
    target_link_libraries(hdrfixer_tests_mocked PRIVATE hdrfixer_core_mocked)
    target_compile_definitions(hdrfixer_tests_mocked PRIVATE _MOCK_WINDOWS=1)
    add_test(NAME mocked_tests COMMAND hdrfixer_tests_mocked)
endif()

if(WIN32)
    # Full test suite on Windows
    add_executable(hdrfixer_tests
        test_main.cpp
        test_transfer_functions.cpp
        test_gamma_lut.cpp
        test_edid_reader.cpp
        test_mhc2_writer.cpp
        test_fix_engine.cpp
        test_display_info.cpp
        test_sdr_white_level.cpp
        test_registry.cpp
        test_settings.cpp
    )
    target_include_directories(hdrfixer_tests PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})
    target_link_libraries(hdrfixer_tests PRIVATE hdrfixer_core)
    add_test(NAME unit_tests COMMAND hdrfixer_tests)
endif()
