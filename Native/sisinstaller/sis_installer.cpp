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

#include "sis_installer.h"
#include "sis_fields.h"
#include "fileutils.h"

#include <miniz.h>
#include <logging.h>

namespace eka2l1 {
    namespace loader {
        #define CHUNK_SIZE 0x2000
        #define CHUNK_MAX_INFLATED_SIZE 0x200000

        static bool inflate_data(mz_stream *stream, void *in, void *out, uint32_t in_size, uint32_t *out_size = nullptr) {
            stream->avail_in = in_size;
            stream->next_in = static_cast<const unsigned char *>(in);
            stream->next_out = static_cast<unsigned char *>(out);
            stream->avail_out = CHUNK_MAX_INFLATED_SIZE;

            auto res = inflate(stream, Z_NO_FLUSH);

            if (res != MZ_OK) {
                if (res == MZ_STREAM_END) {
                    if (out_size)
                        *out_size = CHUNK_MAX_INFLATED_SIZE - stream->avail_out;

                    return true;
                }

                LOG_ERROR(COMMON, "Inflate failed description: {}", mz_error(res));

                if (out_size)
                    *out_size = CHUNK_MAX_INFLATED_SIZE - stream->avail_out;

                return false;
            };

            if (out_size)
                *out_size = CHUNK_MAX_INFLATED_SIZE - stream->avail_out;
            return true;
        }

        bool sis_dd_data_installer::appprop(sis_uid uid, sis_property prop) {
            return false;
        }

        bool sis_dd_data_installer::package(sis_uid uid) {
            return false;
        }

        sis_dd_data_installer::sis_dd_data_installer(common::ro_stream *stream,
            sis_controller *main_controller,
            sis_data *inst_data,
            const std::string &destination_folder)
            : main_controller(main_controller)
            , install_data(inst_data)
            , extract_target_accumulated_size(0)
            , extract_target_decomped_size(0)
            , destination_folder(destination_folder)
            , data_stream(stream) {
        }

        std::vector<uint8_t> sis_dd_data_installer::get_small_file_buf(uint32_t data_idx, uint16_t crr_blck_idx) {
            sis_file_data *data = reinterpret_cast<sis_file_data *>(
                reinterpret_cast<sis_data_unit *>(install_data->data_units.fields[crr_blck_idx].get())->data_unit.fields[data_idx].get());
            sis_compressed compressed = data->raw_data;

            std::uint64_t us = ((compressed.len_low) | (static_cast<uint64_t>(compressed.len_high) << 32)) - 12;

            compressed.compressed_data.resize(us);

            data_stream->seek(compressed.offset, common::seek_where::beg);
            data_stream->read(&compressed.compressed_data[0], us);

            if (compressed.algorithm == sis_compressed_algorithm::none) {
                return compressed.compressed_data;
            }

            compressed.uncompressed_data.resize(compressed.uncompressed_size);
            mz_stream stream;

            stream.zalloc = nullptr;
            stream.zfree = nullptr;

            if (inflateInit(&stream) != MZ_OK) {
                LOG_ERROR(PACKAGE, "Can not intialize inflate stream");
            }

            inflate_data(&stream, compressed.compressed_data.data(),
                compressed.uncompressed_data.data(), static_cast<std::uint32_t>(us));

            inflateEnd(&stream);

            return compressed.uncompressed_data;
        }

        bool sis_dd_data_installer::extract_file(const std::string &path, const uint32_t idx, uint16_t crr_blck_idx) {
            const std::string file_ext = common::get_path_extension(path);

            // Check if it's game extension that we actually care about!
            if ((file_ext != ".opes") && (file_ext != ".ngdat")) {
                // Skip this file
                return true;
            }

            const std::string file_path = destination_folder + common::get_separator() + common::get_filename(path);

            // Delete the file, starts over
            if (common::exists(file_path)) {
                if (!common::remove(file_path)) {
                    LOG_WARN(PACKAGE, "Unable to remove {} to extract new file", file_path);
                }
            }

            sis_data_unit *data_unit = reinterpret_cast<sis_data_unit *>(install_data->data_units.fields[crr_blck_idx].get());

            if (data_unit->data_unit.fields.empty()) {
                // Stub sis without file data
                return true;
            }

            sis_file_data *data = reinterpret_cast<sis_file_data *>(data_unit->data_unit.fields[idx].get());

            sis_compressed compressed = data->raw_data;

            std::uint64_t left = ((compressed.len_low) | (static_cast<std::uint64_t>(compressed.len_high) << 32)) - 12;
            data_stream->seek(compressed.offset, common::seek_where::beg);

            std::vector<unsigned char> temp_chunk;
            temp_chunk.resize(CHUNK_SIZE);

            std::vector<unsigned char> temp_inflated_chunk;
            temp_inflated_chunk.resize(CHUNK_MAX_INFLATED_SIZE);

            mz_stream stream;

            stream.zalloc = nullptr;
            stream.zfree = nullptr;

            if (compressed.algorithm == sis_compressed_algorithm::deflated) {
                if (inflateInit(&stream) != MZ_OK) {
                    LOG_ERROR(PACKAGE, "Can not intialize inflate stream");
                }
            }

            std::uint32_t total_inflated_size = 0;

            {
                common::wo_std_file_stream std_fstream(file_path, true);

                while (left > 0) {
                    std::fill(temp_chunk.begin(), temp_chunk.end(), 0);
                    int grab = static_cast<int>(left < CHUNK_SIZE ? left : CHUNK_SIZE);

                    data_stream->read(&temp_chunk[0], grab);

                    if (!data_stream->valid()) {
                        LOG_ERROR(PACKAGE, "Stream fail, skipping this file, should report to developers.");
                        return false;
                    }

                    if (compressed.algorithm == sis_compressed_algorithm::deflated) {
                        uint32_t inflated_size = 0;

                        auto res = inflate_data(&stream, temp_chunk.data(), temp_inflated_chunk.data(), grab, &inflated_size);

                        if (!res) {
                            LOG_ERROR(PACKAGE, "Decompress failed! Report to developers");
                            return false;
                        }

                        std_fstream.write(temp_inflated_chunk.data(), inflated_size);

                        total_inflated_size += inflated_size;
                        extract_target_decomped_size += inflated_size;
                    } else {
                        std_fstream.write(temp_chunk.data(), grab);
                        extract_target_decomped_size += grab;
                    }

                    left -= grab;
                }

                if (compressed.algorithm == sis_compressed_algorithm::deflated) {
                    if (total_inflated_size != compressed.uncompressed_size) {
                        LOG_ERROR(PACKAGE, "Sanity check failed: Total inflated size not equal to specified uncompress size "
                                           "in SISCompressed ({} vs {})!",
                            total_inflated_size, compressed.uncompressed_size);
                    }

                    inflateEnd(&stream);
                }
            }

            return true;
        }

        int sis_dd_data_installer::gasp_true_form_of_integral_expression(const sis_expression &expr) {
            switch (expr.op) {
            case ss_expr_op::EPrimTypeVariable: {
                switch (expr.int_val) {
                // Language variable. We choosen upper
                case 0x1000: {
                    return static_cast<int>(current_controllers.top()->chosen_lang);
                }

                default:
                    break;
                }

                return 0;
            }

            default:
                break;
            }

            return expr.int_val;
        }

        int sis_dd_data_installer::condition_passed(sis_expression *expr) {
            if (!expr || expr->type != sis_field_type::SISExpression) {
                return -1;
            }

            int pass = 0;

            if ((expr->left_expr && (expr->left_expr->op == ss_expr_op::EPrimTypeString)) || (expr->right_expr && (expr->right_expr->op == ss_expr_op::EPrimTypeString))) {
                if (expr->left_expr->op != expr->right_expr->op) {
                    LOG_ERROR(PACKAGE, "String expression can only be compared with string expression");
                    return -1;
                }

                switch (expr->op) {
                case ss_expr_op::EBinOpEqual: {
                    pass = (expr->left_expr->val.unicode_string == expr->right_expr->val.unicode_string);
                    break;
                }

                case ss_expr_op::EBinOpNotEqual: {
                    pass = (expr->left_expr->val.unicode_string != expr->right_expr->val.unicode_string);
                    break;
                }

                case ss_expr_op::EBinOpGreaterThan: {
                    pass = (expr->left_expr->val.unicode_string > expr->right_expr->val.unicode_string);
                    break;
                }

                case ss_expr_op::EBinOpLessThan: {
                    pass = (expr->left_expr->val.unicode_string < expr->right_expr->val.unicode_string);
                    break;
                }

                case ss_expr_op::EBinOpGreaterThanOrEqual: {
                    pass = (expr->left_expr->val.unicode_string >= expr->right_expr->val.unicode_string);
                    break;
                }

                case ss_expr_op::EBinOpLessOrEqual: {
                    pass = (expr->left_expr->val.unicode_string <= expr->right_expr->val.unicode_string);
                    break;
                }

                default: {
                    LOG_WARN(PACKAGE, "Unhandled string op type: {}", static_cast<int>(expr->op));
                    pass = -1;
                    break;
                }
                }

                return pass;
            }

            const int lhs = condition_passed(expr->left_expr.get());
            const int rhs = condition_passed(expr->right_expr.get());

            switch (expr->op) {
            case ss_expr_op::EBinOpEqual: {
                pass = (lhs == rhs);
                break;
            }

            case ss_expr_op::EBinOpNotEqual: {
                pass = (lhs != rhs);
                break;
            }

            case ss_expr_op::EBinOpGreaterThan: {
                pass = (lhs > rhs);
                break;
            }

            case ss_expr_op::EBinOpLessThan: {
                pass = (lhs < rhs);
                break;
            }

            case ss_expr_op::EBinOpGreaterThanOrEqual: {
                pass = (lhs >= rhs);
                break;
            }

            case ss_expr_op::EBinOpLessOrEqual: {
                pass = (lhs <= rhs);
                break;
            }

            case ss_expr_op::ELogOpAnd: {
                pass = static_cast<std::uint32_t>(lhs) & static_cast<std::uint32_t>(rhs);
                break;
            }

            case ss_expr_op::ELogOpOr: {
                pass = static_cast<std::uint32_t>(lhs) | static_cast<std::uint32_t>(rhs);
                break;
            }

            case ss_expr_op::EPrimTypeNumber:
            case ss_expr_op::EPrimTypeVariable: {
                pass = gasp_true_form_of_integral_expression(*expr);
                break;
            }

            case ss_expr_op::EUnaryOpNot: {
                pass = !lhs;
                break;
            }

            case ss_expr_op::EFuncExists: {
                pass = false;
                break;
            }

            default: {
                pass = -1;
                LOG_WARN(PACKAGE, "Unimplemented operation {} for expression", static_cast<int>(expr->op));
                break;
            }
            }

            return pass;
        }

        bool sis_dd_data_installer::interpret(sis_controller *controller, const std::uint16_t base_data_idx) {
            // Set current controller
            current_controllers.push(controller);

            // Ask for language. If we can't choose the first one, or none
            controller->chosen_lang = reinterpret_cast<sis_language *>(controller->langs.langs.fields[0].get())->language;

            const bool result = interpret(controller->install_block, base_data_idx + controller->idx.data_index);
            current_controllers.pop();

            return result;
        }

        bool sis_dd_data_installer::interpret(sis_install_block &install_block, std::uint16_t crr_blck_idx) {
            // Process file
            for (auto &wrap_file : install_block.files.fields) {
                sis_file_des *file = reinterpret_cast<sis_file_des *>(wrap_file.get());

                switch (file->op) {
                case ss_op::text: {
                    break;
                }

                case ss_op::undefined:
                case ss_op::install: {
                    if ((file->op == ss_op::undefined) && (!file->len || !file->uncompressed_len)) {
                        break;
                    }

                    bool lowered = false;

                    if (!install_data->data_units.fields.empty()) {
                        extract_target_info info;
                        info.file_path_ = common::ucs2_to_utf8(file->target.unicode_string);
                        info.data_unit_block_index_ = file->idx;
                        info.data_unit_index_ = crr_blck_idx;

                        extract_targets.push_back(info);
                        extract_target_accumulated_size += file->uncompressed_len;
                    }

                    break;
                }

                default:
                    break;
                }
            }

            for (auto &wrap_mini_pkg : install_block.controllers.fields) {
                interpret(reinterpret_cast<sis_controller *>(wrap_mini_pkg.get()), crr_blck_idx);
            }

            // Parse if blocks
            for (auto &wrap_if_statement : install_block.if_blocks.fields) {
                sis_if *if_stmt = reinterpret_cast<sis_if *>(wrap_if_statement.get());
                auto result = condition_passed(&if_stmt->expr);

                if (result) {
                    interpret(if_stmt->install_block, crr_blck_idx);
                } else {
                    for (auto &wrap_else_branch : if_stmt->else_if.fields) {
                        sis_else_if *else_branch = reinterpret_cast<sis_else_if *>(wrap_else_branch.get());

                        if (condition_passed(&else_branch->expr)) {
                            interpret(else_branch->install_block, crr_blck_idx);
                            break;
                        }
                    }
                }
            }

            return true;
        }

        bool sis_dd_data_installer::run() {
            if (install_data->data_units.fields.empty()) {
                LOG_INFO(PACKAGE, "Interpreting a stub SIS");
            }

            extract_targets.clear();

            extract_target_accumulated_size = 0;
            extract_target_decomped_size = 0;

            if (!interpret(main_controller, 0)) {
                return false;
            }

            if (extract_targets.empty()) {
                return false;
            } else {
                std::size_t n = 0;
                bool cancel_requested = false;

                for (; n < extract_targets.size(); n++) {
                    extract_target_info &target = extract_targets[n];
                    if (!extract_file(target.file_path_, target.data_unit_block_index_, target.data_unit_index_)) {
                        cancel_requested = true;
                        break;
                    }
                }

                if (cancel_requested) {
                    for (std::size_t i = 0; i < n; i++) {
                        common::remove(extract_targets[i].file_path_);
                    }

                    return false;
                }
            }

            return true;
        }
    }
}