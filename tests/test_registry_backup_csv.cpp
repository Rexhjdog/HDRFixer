#include "../src/core/registry/registry_backup.cpp" // Include cpp to test static/internal functions if needed, or just link. Since no build system, include cpp is easier.
#include <cassert>
#include <iostream>

void test_parse_csv_line() {
    using namespace hdrfixer::registry;

    // Test case 1: Normal
    auto parts = parse_csv_line(L"A|B|C");
    assert(parts.size() == 3);
    assert(parts[0] == L"A");
    assert(parts[1] == L"B");
    assert(parts[2] == L"C");

    // Test case 2: Trailing empty
    parts = parse_csv_line(L"A|B|");
    assert(parts.size() == 3);
    assert(parts[0] == L"A");
    assert(parts[1] == L"B");
    assert(parts[2] == L"");

    // Test case 3: Empty string
    parts = parse_csv_line(L"");
    assert(parts.size() == 1);
    assert(parts[0] == L"");

    // Test case 4: Only separator
    parts = parse_csv_line(L"|");
    assert(parts.size() == 2);
    assert(parts[0] == L"");
    assert(parts[1] == L"");

    std::cout << "All tests passed!" << std::endl;
}

int main() {
    test_parse_csv_line();
    return 0;
}
