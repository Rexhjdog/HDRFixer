#include "core/log/logger.h"
#include <filesystem>
#include <iostream>
#include <windows.h>
#include <shlobj.h>

namespace hdrfixer::log {

namespace {

const char* level_string(Level level) {
    switch (level) {
        case Level::Debug: return "DEBUG";
        case Level::Info:  return "INFO";
        case Level::Warn:  return "WARN";
        case Level::Error: return "ERROR";
        default:           return "UNKNOWN";
    }
}

std::filesystem::path default_log_path() {
    wchar_t* appdata = nullptr;
    if (SUCCEEDED(SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, nullptr, &appdata))) {
        std::filesystem::path dir = std::filesystem::path(appdata) / L"HDRFixer";
        CoTaskMemFree(appdata);
        return dir / "hdrfixer.log";
    }
    // Fallback: use %LOCALAPPDATA% environment variable
    const char* env = std::getenv("LOCALAPPDATA");
    if (env) {
        std::filesystem::path dir = std::filesystem::path(env) / "HDRFixer";
        return dir / "hdrfixer.log";
    }
    // Last resort fallback
    return std::filesystem::path("hdrfixer.log");
}

} // anonymous namespace

Logger::Logger() {
    auto path = default_log_path();
    auto dir = path.parent_path();

    // Create directory if it doesn't exist
    std::error_code ec;
    if (!dir.empty() && !std::filesystem::exists(dir)) {
        std::filesystem::create_directories(dir, ec);
    }

    file_.open(path, std::ios::app);
    if (!file_.is_open()) {
        // If we can't open the default path, try current directory
        file_.open("hdrfixer.log", std::ios::app);
    }
}

Logger& Logger::instance() {
    static Logger logger;
    return logger;
}

void Logger::set_file(const std::filesystem::path& path) {
    std::lock_guard<std::mutex> lock(mutex_);

    auto dir = path.parent_path();

    // Create directory if it doesn't exist
    std::error_code ec;
    if (!dir.empty() && !std::filesystem::exists(dir)) {
        std::filesystem::create_directories(dir, ec);
    }

    if (file_.is_open()) {
        file_.close();
    }

    file_.open(path, std::ios::app);
}

void Logger::log(Level level, const std::string& message, const std::source_location& loc) {
    if (level < level_) {
        return;
    }

    // Get current timestamp
    auto now = std::chrono::system_clock::now();
    auto time_t_now = std::chrono::system_clock::to_time_t(now);
    std::tm tm_buf{};
    localtime_s(&tm_buf, &time_t_now);

    // Extract just the filename from the full path
    std::string_view file_path = loc.file_name();
    auto last_sep = file_path.find_last_of("/\\");
    std::string_view filename = (last_sep != std::string_view::npos)
        ? file_path.substr(last_sep + 1)
        : file_path;

    // Format: [2026-02-21 12:34:56] [INFO] [file.cpp:42] message
    auto formatted = std::format("[{:04d}-{:02d}-{:02d} {:02d}:{:02d}:{:02d}] [{}] [{}:{}] {}",
        tm_buf.tm_year + 1900, tm_buf.tm_mon + 1, tm_buf.tm_mday,
        tm_buf.tm_hour, tm_buf.tm_min, tm_buf.tm_sec,
        level_string(level),
        filename,
        loc.line(),
        message);

    std::lock_guard<std::mutex> lock(mutex_);

    if (file_.is_open()) {
        file_ << formatted << "\n";
        file_.flush();
    }

    // Also output to debug console in debug builds
#ifdef _DEBUG
    OutputDebugStringA((formatted + "\n").c_str());
#endif
}

} // namespace hdrfixer::log
