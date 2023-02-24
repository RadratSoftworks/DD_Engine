#include <exception>

struct sis_uncompressed_data_too_large_exception : public std::exception {
public:
    const char* what() const noexcept override {
        return "The uncompressed data is too large to extract! This file is probably corrupted or wrong file!";
    }
};