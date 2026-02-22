#pragma once
#include <string>
#include <filesystem>
#include <mutex>
#include <fstream>
#include <format>
#include <chrono>
#include <source_location>

namespace hdrfixer::log {

enum class Level { Debug, Info, Warn, Error };

class Logger {
public:
    static Logger& instance();

    void set_level(Level level) { level_ = level; }
    void set_file(const std::filesystem::path& path);

    void log(Level level, const std::string& message,
             const std::source_location& loc = std::source_location::current());

    void debug(const std::string& msg, const std::source_location& loc = std::source_location::current()) { log(Level::Debug, msg, loc); }
    void info(const std::string& msg, const std::source_location& loc = std::source_location::current()) { log(Level::Info, msg, loc); }
    void warn(const std::string& msg, const std::source_location& loc = std::source_location::current()) { log(Level::Warn, msg, loc); }
    void error(const std::string& msg, const std::source_location& loc = std::source_location::current()) { log(Level::Error, msg, loc); }

private:
    Logger();
    Level level_ = Level::Info;
    std::ofstream file_;
    std::mutex mutex_;
};

// Convenience macros
#define LOG_DEBUG(msg) hdrfixer::log::Logger::instance().debug(msg)
#define LOG_INFO(msg) hdrfixer::log::Logger::instance().info(msg)
#define LOG_WARN(msg) hdrfixer::log::Logger::instance().warn(msg)
#define LOG_ERROR(msg) hdrfixer::log::Logger::instance().error(msg)

} // namespace hdrfixer::log
