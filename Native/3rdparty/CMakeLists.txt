add_library(miniz STATIC
    miniz/miniz.c
    miniz/miniz.h)

target_include_directories(miniz PUBLIC miniz)

add_subdirectory(fmt)

if (NOT ANDROID)
    add_subdirectory(nativefiledialog)
endif()