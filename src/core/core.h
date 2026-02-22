#pragma once

#define WINVER 0x0A00
#define _WIN32_WINNT 0x0A00
#define NOMINMAX
#define WIN32_LEAN_AND_MEAN

#include <windows.h>
#include <dxgi1_6.h>
#include <wrl/client.h>
#include <icm.h>
#include <shellapi.h>

#include <cstdint>
#include <cmath>
#include <cstring>
#include <string>
#include <vector>
#include <optional>
#include <expected>
#include <format>
#include <fstream>
#include <filesystem>
#include <functional>
#include <memory>
#include <mutex>
#include <thread>
#include <chrono>
#include <algorithm>
#include <array>
#include <map>
#include <span>
