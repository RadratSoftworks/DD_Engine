#pragma once

#include <iostream>
#include <fmt/format.h>

#define LOG_INFO(category, formatstring, ...) std::cout << fmt::format(formatstring, ##__VA_ARGS__) << "\n";
#define LOG_WARN(category, formatstring, ...) std::cout << fmt::format(formatstring, ##__VA_ARGS__) << "\n";
#define LOG_ERROR(category, formatstring, ...) std::cerr << fmt::format(formatstring, ##__VA_ARGS__) << "\n";