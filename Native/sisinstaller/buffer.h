/*
 * Copyright (c) 2018 EKA2L1 Team.
 * 
 * This file is part of EKA2L1 project 
 * (see bentokun.github.com/EKA2L1).
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

#pragma once

#include <cstdint>
#include <cstdio>
#include <cstring>
#include <fstream>
#include <memory>
#include <string>

#if _WIN32
#include <locale>
#include <codecvt>
#endif

namespace eka2l1 {
    namespace common {
        enum file_mode {
            text_mode = 0,
            binary_mode = 1 << 0,
            read_mode = 0,
            write_mode = 1 << 1
        };

        enum seek_where {
            beg,
            cur,
            end
        };

        class basic_stream {
        public:
            virtual void seek(const std::int64_t amount, seek_where wh) = 0;
            virtual bool valid() = 0;
            virtual std::uint64_t left() = 0;
            virtual uint64_t tell() = 0;
            virtual uint64_t size() = 0;
        };

        class wo_stream : public basic_stream {
        public:
            virtual std::uint64_t write(const void *buf, const std::uint64_t write_size) = 0;

            template <typename T>
            void write_string(const std::basic_string<T> &str) {
                const std::size_t len = str.length();

                write(&len, sizeof(len));
                write(&str[0], static_cast<std::uint32_t>(len) * sizeof(T));
            }

            virtual bool write_text(const std::string &str) {
                write(&str[0], static_cast<std::uint32_t>(str.length()));
                return valid();
            }
        };

        class ro_stream : public basic_stream {
        public:
            virtual std::uint64_t read(void *buf, const std::uint64_t read_size) = 0;

            template <typename T>
            std::basic_string<T> read_string() {
                std::basic_string<T> str;
                std::size_t len;

                read(&len, sizeof(len));
                str.resize(len);

                read(&str[0], static_cast<std::uint32_t>(len) * sizeof(T));

                return str;
            }

            std::uint64_t read(const std::uint64_t pos, void *buf, const std::uint64_t size) {
                seek(pos, seek_where::beg);
                return read(buf, size);
            }
        };

        class buffer_stream_base {
        protected:
            uint8_t *beg;
            uint8_t *end;

            std::uint64_t crr_pos;

        public:
            buffer_stream_base()
                : beg(nullptr)
                , end(nullptr)
                , crr_pos(0) {}

            buffer_stream_base(uint8_t *data, uint64_t size)
                : beg(data)
                , end(beg + size)
                , crr_pos(0) {}

            std::uint8_t *get_current() {
                return beg + crr_pos;
            }
        };

        class ro_buf_stream : public buffer_stream_base, public ro_stream {
        public:
            ro_buf_stream(uint8_t *beg, uint64_t size)
                : buffer_stream_base(beg, size) {}

            bool valid() override {
                return beg + crr_pos < end;
            }

            std::uint64_t size() override {
                return end - beg;
            }

            uint64_t tell() override {
                return crr_pos;
            }

            uint64_t left() override {
                return end - beg - crr_pos;
            }

            void seek(const std::int64_t amount, seek_where wh) override {
                if (wh == seek_where::beg) {
                    crr_pos = amount;
                    return;
                }

                if (wh == seek_where::cur) {
                    crr_pos += amount;
                    return;
                }

                crr_pos = (end - beg) + amount;
            }

            std::uint64_t read(void *buf, const std::uint64_t read_size) override {
                std::uint64_t actual_read_size = std::min<std::uint64_t>(read_size,
                    static_cast<std::uint64_t>(end - beg - crr_pos));

                if (actual_read_size == 0) {
                    return 0;
                }

                memcpy(buf, beg + crr_pos, actual_read_size);
                crr_pos += actual_read_size;

                return actual_read_size;
            }
        };

        class ro_std_file_stream : public common::ro_stream {
            mutable FILE *fi_;

        public:
            explicit ro_std_file_stream(const std::string &path, const bool binary) {
#ifdef _WIN32
                std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
                const std::wstring path_u16 = converter.from_bytes(path);

                fi_ = _wfopen(path_u16.c_str(), binary ? L"rb" : L"r");
#else
                fi_ = fopen(path.c_str(), binary ? "rb" : "r");
#endif
            }

            ~ro_std_file_stream() {
                if (fi_) {
                    fclose(fi_);
                }
            }

            bool valid() override {
                return fi_;
            }

            std::uint64_t read(void *buf, const std::uint64_t read_size) override {
                return fread(buf, 1, read_size, fi_);
            }

            void seek(const std::int64_t amount, common::seek_where wh) override {
                int flags = 0;
                switch (wh) {
                case common::seek_where::beg: {
                    flags = SEEK_SET;
                    break;
                }

                case common::seek_where::cur: {
                    flags = SEEK_CUR;
                    break;
                }

                default: {
                    flags = SEEK_END;
                    break;
                }
                }

                fseek(fi_, static_cast<long>(amount), flags);
            }

            std::uint64_t left() override {
                return size() - tell();
            }

            std::uint64_t tell() override {
                return ftell(fi_);
            }

            std::uint64_t size() override {
                const std::uint64_t cur_pos = tell();
                seek(0, common::end);
                const std::uint64_t s = tell();

                seek(cur_pos, common::beg);
                return s;
            }
        };

        class wo_std_file_stream : public common::wo_stream {
            mutable FILE *fo_;

        public:
            explicit wo_std_file_stream(const std::string &path, const bool binary)
                : fo_(nullptr) {
#ifdef _WIN32
                std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
                const std::wstring path_u16 = converter.from_bytes(path);

                fo_ = _wfopen(path_u16.c_str(), binary ? L"wb" : L"w");
#else
                fo_ = fopen(path.c_str(), binary ? "wb" : "w");
#endif
            }

            ~wo_std_file_stream() {
                if (fo_) {
                    fclose(fo_);
                }
            }

            std::uint64_t write(const void *buf, const std::uint64_t size) override {
                if (!fo_) {
                    return 0;
                }

                const std::uint64_t pos = ftell(fo_);
                fwrite(buf, 1, size, fo_);
                const std::uint64_t current_pos = ftell(fo_);

                return current_pos - pos;
            }

            bool write_text(const std::string &str) override {
                if (!fo_) {
                    return false;
                }

                fwrite(str.data(), 1, str.size(), fo_);
                return true;
            }

            void seek(const std::int64_t amount, common::seek_where wh) override {
                if (!fo_) {
                    return;
                }

                fseek(fo_, static_cast<long>(amount), common::beg ? SEEK_SET : (common::cur ? SEEK_CUR : SEEK_END));
            }

            bool valid() override {
                return fo_;
            }

            std::uint64_t left() override {
                return 0xFFFFFFFF;
            }

            std::uint64_t tell() override {
                if (!fo_) {
                    return 0;
                }
                return ftell(fo_);
            }

            std::uint64_t size() override {
                if (!fo_) {
                    return 0;
                }
                const std::uint64_t cur_pos = tell();
                seek(0, common::end);
                const std::uint64_t s = tell();

                seek(cur_pos, common::beg);
                return s;
            }
        };

        class ro_window_ref_stream: public ro_stream {
        private:
            ro_stream &ref_;
            std::size_t start_;
            std::size_t size_;

        public:
            explicit ro_window_ref_stream(ro_stream &ref, std::size_t start, std::size_t size)
                : ref_(ref)
                , start_(start)
                , size_(size) {
                ref_.seek(start, common::seek_where::beg);
            }

            bool valid() override {
                return ref_.tell() < start_ + size_;
            }

            std::uint64_t read(void *buf, const std::uint64_t read_size) override {
                return ref_.read(buf, read_size);
            }

            void seek(const std::int64_t amount, common::seek_where wh) override {
                std::int64_t real_amount = amount;

                switch (wh) {
                case common::seek_where::beg: {
                    real_amount += start_;
                    break;
                }

                case common::seek_where::cur: {
                    real_amount += ref_.tell();
                    break;
                }

                default: {
                    real_amount += start_ + size_;
                    break;
                }
                }

                ref_.seek(real_amount, common::seek_where::beg);
            }

            std::uint64_t left() override {
                return start_ + size_ - ref_.tell();
            }

            std::uint64_t tell() override {
                return ref_.tell();
            }

            std::uint64_t size() override {
                return size_;
            }
        };
    }
}