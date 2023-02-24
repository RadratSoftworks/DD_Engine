add_library(sisinstaller SHARED
    buffer.h
    sis_fields.cpp
    sis_fields.h
    sis_installer_entry.cpp
    sis_installer.cpp
    sis_installer.h)

target_link_libraries(sisinstaller PRIVATE fileutils miniz logging)

if (NOT ANDROID)
	target_link_libraries(sisinstaller PRIVATE nfd)
endif()