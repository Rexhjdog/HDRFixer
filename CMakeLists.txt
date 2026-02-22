cmake_minimum_required(VERSION 3.20)
project(HDRFixerTests VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Download doctest if needed
if(NOT EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/tests/doctest.h")
    file(DOWNLOAD
        "https://raw.githubusercontent.com/doctest/doctest/v2.4.11/doctest/doctest.h"
        "${CMAKE_CURRENT_SOURCE_DIR}/tests/doctest.h"
        EXPECTED_HASH SHA256=44faa038e9c3f9728efbda143748d01124ea0a27f4bf78f35a15d8fab2e039fb
    )
endif()

# Core library (subset)
add_library(hdrfixer_core STATIC
    src/core/display/edid_reader.cpp
)
target_include_directories(hdrfixer_core PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/src)

# Tests
add_executable(hdrfixer_tests
    tests/test_edid_reader.cpp
)
target_link_libraries(hdrfixer_tests PRIVATE hdrfixer_core)
target_include_directories(hdrfixer_tests PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/tests)
