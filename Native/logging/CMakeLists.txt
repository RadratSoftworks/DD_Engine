add_library(logging INTERFACE)

target_include_directories(logging INTERFACE .)
target_link_libraries(logging INTERFACE fmt::fmt)