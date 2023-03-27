#include "sis_fields.h"
#include "sis_installer.h"
#include "sis_exceptions.h"

#include <logging.h>
#include <cstdint>

#define MOBILE_COMPILATION (__ANDROID__ != 0)

#if !MOBILE_COMPILATION
#include <nfd.hpp>
#endif

#ifdef _MSC_VER
#define DDENGINE_EXPORT __declspec(dllexport)
#else
#define DDENGINE_EXPORT
#endif

using namespace eka2l1::loader;
using namespace eka2l1::common;

#if !MOBILE_COMPILATION
nfdu8char_t *current_path = nullptr;
#endif

extern "C" {
    enum dd_game_data_install_error_code
    {
        DD_GAME_DATA_NO_ERROR = 0,
        DD_GAME_DATA_CANT_OPEN_INSTALL_FILE = -1,
        DD_GAME_DATA_FAILED_TO_INSTALL = -2,
        DD_GAME_DATA_CORRUPTED = -3,
        DD_GAME_DATA_TOO_LARGE = -4,
        DD_GAME_DATA_NOT_DIRK_DAGGER_FILE = -5
    };

    static constexpr std::uint32_t SIS_UID1 = 0x10201A7A;
    static constexpr std::uint32_t DIRK_DAGGER_UID = 0x2000AFC3;

    DDENGINE_EXPORT int install_dd_game_data(const char *path, const char *dest_path) {
        sis_parser parser(path);
        if (!parser.valid()) {
            LOG_ERROR(PACKAGE, "Can't open file: {} to extract game data!", path);
            return DD_GAME_DATA_CANT_OPEN_INSTALL_FILE;
        }

        sis_header header = parser.parse_header();
        if ((header.uid1 != SIS_UID1) || !parser.valid()) {
            return DD_GAME_DATA_CORRUPTED;
        }

        sis_contents content;

        try {
            content = parser.parse_contents();
        }
        catch (sis_uncompressed_data_too_large_exception &exception) {
            return DD_GAME_DATA_TOO_LARGE;
        }
        catch (...) {
            return DD_GAME_DATA_CORRUPTED;
        }

        if (content.controller.info.uid.uid != DIRK_DAGGER_UID) {
            return DD_GAME_DATA_NOT_DIRK_DAGGER_FILE;
        }

        ro_std_file_stream data_ref_stream(path, true);
        sis_dd_data_installer data_installer(reinterpret_cast<ro_stream *>(&data_ref_stream),
                                             &content.controller, &content.data, dest_path);

        if (!data_installer.run()) {
            return DD_GAME_DATA_FAILED_TO_INSTALL;
        }

        return DD_GAME_DATA_NO_ERROR;
    }

#if !MOBILE_COMPILATION
    DDENGINE_EXPORT const char *open_fallback_pick_ngage_game_window() {
        nfdu8filteritem_t filter_items[1] = { { "N-Gage 2.0 install file", "n-gage" } };
        nfdu8char_t *out_path = nullptr;

        nfdresult_t result = NFD::OpenDialog(out_path, filter_items, 1, nullptr);
        if (result == NFD_OKAY) {
            if (current_path != nullptr) {
                NFD::FreePath(current_path);
            }
            
            current_path = out_path;
            return out_path;
        }

        return nullptr;
    }

    __declspec(dllexport) void free_fallback_pick_ngage_game_window_path() {
        if (current_path != nullptr) {
            NFD::FreePath(current_path);
            current_path = nullptr;
        }
    }
#endif
}