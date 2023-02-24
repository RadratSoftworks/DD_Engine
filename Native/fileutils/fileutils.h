#pragma once

#include <string>
#include <functional>

#ifndef _WIN32
#include <unistd.h>
#include <sys/stat.h> 
#else
#include <Windows.h>
#endif

#include <locale>
#include <codecvt>

namespace eka2l1::common {
    std::wstring utf8_to_wstr(const std::string &str) {
        std::wstring_convert<std::codecvt_utf8<wchar_t>, wchar_t> converter;
        auto wstr = converter.from_bytes(reinterpret_cast<const char*>(&str[0]),
                        reinterpret_cast<const char*>(&str[0] + str.size()));

        if (wstr.back() == L'\0') {
            wstr.pop_back();
        }

        return wstr;
    }
    
    // VS2017 bug: https://stackoverflow.com/questions/32055357/visual-studio-c-2015-stdcodecvt-with-char16-t-or-char32-t
    std::string ucs2_to_utf8(const std::u16string &str) {
        if (str.empty()) {
            return "";
        }

        std::wstring_convert<std::codecvt_utf8_utf16<char16_t>, char16_t> convert;
        auto p = reinterpret_cast<const char16_t *>(str.data());

        return convert.to_bytes(p, p + str.size());
    }

    bool exists(std::string path) {
#ifdef _WIN32
        const std::wstring wpath = utf8_to_wstr(path);
        DWORD dw_attrib = GetFileAttributesW(wpath.c_str());
        return (dw_attrib != INVALID_FILE_ATTRIBUTES);
#else
        struct stat st;
        auto res = stat(path.c_str(), &st);

        return res != -1;
#endif
    }

    char get_separator(bool symbian_use = false) {
        if (symbian_use) {
            return '\\';
        }

#ifdef _WIN32
        return '\\';
#else
        return '/';
#endif
    }

    char16_t get_separator_16(bool symbian_use = false) {
        if (symbian_use) {
            return u'\\';
        }

#ifdef _WIN32
        return u'\\';
#else
        return u'/';
#endif
    }

    bool is_separator(const char sep) {
        if (sep == '/' || sep == '\\') {
            return true;
        }

        return false;
    }

    bool is_separator(const char16_t sep) {
        return (sep == '/' || sep == '\\');
    }

    bool remove(const std::string &path) {
#ifdef _WIN32
        const std::wstring path_w = utf8_to_wstr(path);

        if (path_w.back() == L'\\' || path_w.back() == L'/') {
            return RemoveDirectoryW(path_w.c_str());
        }

        return DeleteFileW(path_w.c_str());
#else
        return (::remove(path.c_str()) == 0);
#endif
    }
    
    template <typename T>
    std::basic_string<T> get_path_extension(const std::basic_string<T> &path) {
        std::size_t last_dot_pos = path.find_last_of(static_cast<T>('.'));

        if (last_dot_pos == std::string::npos) {
            return std::basic_string<T>{};
        }

        return path.substr(last_dot_pos, path.length() - last_dot_pos);
    }

    template <typename T>
    std::basic_string<T> get_filename(std::basic_string<T> path, bool symbian_use = false,
        std::function<T(bool)> separator_func = nullptr) {
        using generic_string = decltype(path);
        generic_string fn;

        if (path.length() < 1) {
            return generic_string{};
        }

        if (is_separator(path[path.length() - 1])) {
            // It's directory
            return fn;
        }

        for (int64_t i = path.length(); i >= 0; --i) {
            if (is_separator(path[i]) || (static_cast<char>(path[i]) == ':')) {
                break;
            }

            fn = path[i] + fn;
        }

        while (!fn.empty() && static_cast<char>(fn.back()) == '\0') {
            fn.pop_back();
        }

        return fn;
    }
}