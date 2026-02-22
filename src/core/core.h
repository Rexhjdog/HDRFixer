#pragma once

#ifndef WINVER
#define WINVER 0x0A00
#endif
#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0A00
#endif
#ifndef NOMINMAX
#define NOMINMAX
#endif
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

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
