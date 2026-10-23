// Họ và tên: Nguyễn Đức Nghĩa
// Mã sinh viên: 23103100307
// Nội dung thực hiện: Module 4 - Task M4.2: Implementation Service xử lý quy tắc chuyển trạng thái hồ sơ, tích hợp IHoSoXetTuyenRepository theo kiến trúc N-Tier.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Repositories;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Services
{
    public class HoSoTrangThaiService : IHoSoTrangThaiService
    {
        private readonly IHoSoXetTuyenRepository _hoSoRepository;

        public HoSoTrangThaiService(IHoSoXetTuyenRepository hoSoRepository)
        {
            _hoSoRepository = hoSoRepository;
        }

        /// <summary>
        /// Bảng luồng chuyển trạng thái hợp lệ theo quy định T1.3 &amp; Mục 5.2.
        /// </summary>
        private static readonly Dictionary<string, string[]> LuongTrangThaiHopLe = new(StringComparer.OrdinalIgnoreCase)
        {
            // 1. Chờ duyệt: có thể sơ tuyển Đạt, Không đạt, hoặc Thí sinh tự Hủy
            [TrangThaiHoSo.ChoDuyet] = new[]
            {
                TrangThaiHoSo.DatSoTuyen,
                TrangThaiHoSo.KhongDatSoTuyen,
                TrangThaiHoSo.DaHuy
            },

            // 2. Đạt sơ tuyển: có thể Lập lịch phỏng vấn (-> Chờ phỏng vấn) hoặc Thí sinh Hủy (nếu chưa có lịch)
            [TrangThaiHoSo.DatSoTuyen] = new[]
            {
                TrangThaiHoSo.ChoPhongVan,
                TrangThaiHoSo.DaHuy
            },

            // 3. Không đạt sơ tuyển: nhánh kết thúc của hồ sơ bị loại
            [TrangThaiHoSo.KhongDatSoTuyen] = Array.Empty<string>(),

            // 4. Chờ phỏng vấn: chuyển sang Đã phỏng vấn khi hoàn thành lịch, hoặc quay về Đạt sơ tuyển khi hủy lịch
            [TrangThaiHoSo.ChoPhongVan] = new[]
            {
                TrangThaiHoSo.DaPhongVan,
                TrangThaiHoSo.DatSoTuyen
            },

            // 5. Đã phỏng vấn: cán bộ ghi nhận kết quả cuối cùng
            [TrangThaiHoSo.DaPhongVan] = new[]
            {
                TrangThaiHoSo.TrungTuyen,
                TrangThaiHoSo.KhongTrungTuyen
            },

            // 6. Các trạng thái kết thúc (Terminal states)
            [TrangThaiHoSo.TrungTuyen] = Array.Empty<string>(),
            [TrangThaiHoSo.KhongTrungTuyen] = Array.Empty<string>(),
            [TrangThaiHoSo.DaHuy] = Array.Empty<string>()
        };

        public IReadOnlyList<string> LayDanhSachTrangThaiHopLe(string? trangThaiHienTai)
        {
            if (string.IsNullOrWhiteSpace(trangThaiHienTai))
                return Array.Empty<string>();

            return LuongTrangThaiHopLe.TryGetValue(trangThaiHienTai.Trim(), out var ds)
                ? ds
                : Array.Empty<string>();
        }

        public bool CoTheChuyenTrangThai(string? trangThaiHienTai, string? trangThaiMoi)
        {
            if (string.IsNullOrWhiteSpace(trangThaiHienTai) || string.IsNullOrWhiteSpace(trangThaiMoi))
                return false;

            var hienTai = trangThaiHienTai.Trim();
            var moi = trangThaiMoi.Trim();

            if (string.Equals(hienTai, moi, StringComparison.OrdinalIgnoreCase))
                return false;

            return LayDanhSachTrangThaiHopLe(hienTai).Any(s => string.Equals(s, moi, StringComparison.OrdinalIgnoreCase));
        }

        public (bool HopLe, string? ThongBaoLoi) KiemTraChuyenTrangThai(string? trangThaiHienTai, string? trangThaiMoi)
        {
            if (string.IsNullOrWhiteSpace(trangThaiHienTai))
                return (false, "Trạng thái hiện tại của hồ sơ không hợp lệ hoặc để trống.");

            if (string.IsNullOrWhiteSpace(trangThaiMoi))
                return (false, "Vui lòng chọn trạng thái mới cần cập nhật.");

            var hienTai = trangThaiHienTai.Trim();
            var moi = trangThaiMoi.Trim();

            if (string.Equals(hienTai, moi, StringComparison.OrdinalIgnoreCase))
                return (false, $"Hồ sơ hiện tại đã ở trạng thái \"{hienTai}\".");

            // Chặn sửa trạng thái đã kết thúc
            if (string.Equals(hienTai, TrangThaiHoSo.DaHuy, StringComparison.OrdinalIgnoreCase))
                return (false, "Hồ sơ đã bị hủy, không thể thay đổi trạng thái nữa.");

            if (string.Equals(hienTai, TrangThaiHoSo.KhongDatSoTuyen, StringComparison.OrdinalIgnoreCase))
                return (false, "Hồ sơ \"Không đạt sơ tuyển\" không thể chuyển sang trạng thái khác.");

            if (string.Equals(hienTai, TrangThaiHoSo.TrungTuyen, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(hienTai, TrangThaiHoSo.KhongTrungTuyen, StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"Hồ sơ đã có kết quả xét tuyển cuối cùng (\"{hienTai}\"), không thể cập nhật thêm.");
            }

            // Chặn chuyển nhảy cóc từ Chờ duyệt trực tiếp sang Trúng tuyển / Không trúng tuyển
            if (string.Equals(hienTai, TrangThaiHoSo.ChoDuyet, StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(moi, TrangThaiHoSo.TrungTuyen, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(moi, TrangThaiHoSo.KhongTrungTuyen, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "Không thể chuyển trực tiếp từ \"Chờ duyệt\" sang kết quả tuyển sinh cuối cùng (nhảy cóc quy trình nghiệp vụ).");
            }

            // Chặn lập lịch khi chưa Đạt sơ tuyển
            if (string.Equals(moi, TrangThaiHoSo.ChoPhongVan, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(hienTai, TrangThaiHoSo.DatSoTuyen, StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"Chỉ hồ sơ \"{TrangThaiHoSo.DatSoTuyen}\" mới được lập lịch phỏng vấn (hồ sơ này đang \"{hienTai}\").");
            }

            // Chặn ghi kết quả khi chưa Đã phỏng vấn
            if ((string.Equals(moi, TrangThaiHoSo.TrungTuyen, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(moi, TrangThaiHoSo.KhongTrungTuyen, StringComparison.OrdinalIgnoreCase)) &&
                !string.Equals(hienTai, TrangThaiHoSo.DaPhongVan, StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"Chỉ hồ sơ \"{TrangThaiHoSo.DaPhongVan}\" mới được ghi nhận kết quả cuối cùng (hồ sơ này đang \"{hienTai}\").");
            }

            // Kiểm tra ma trận luồng chuyển trạng thái
            if (!CoTheChuyenTrangThai(hienTai, moi))
            {
                return (false, $"Chuyển trạng thái không hợp lệ: không thể chuyển từ \"{hienTai}\" sang \"{moi}\".");
            }

            return (true, null);
        }

        public async Task<(bool HopLe, string? ThongBaoLoi)> KiemTraChuyenTrangThaiHoSoAsync(int maHoSo, string trangThaiMoi)
        {
            var hoSo = await _hoSoRepository.GetByIdAsync(maHoSo);
            if (hoSo == null)
            {
                return (false, $"Không tìm thấy hồ sơ xét tuyển có mã #{maHoSo}.");
            }

            return KiemTraChuyenTrangThai(hoSo.TrangThai, trangThaiMoi);
        }

        public (bool ChoPhep, string? LyDo) CoTheSoTuyen(string? trangThaiHienTai, string ketQuaSoTuyen)
        {
            if (string.IsNullOrWhiteSpace(trangThaiHienTai))
                return (false, "Hồ sơ không tồn tại.");

            if (!string.Equals(trangThaiHienTai.Trim(), TrangThaiHoSo.ChoDuyet, StringComparison.OrdinalIgnoreCase))
                return (false, $"Chỉ hồ sơ \"{TrangThaiHoSo.ChoDuyet}\" mới được sơ tuyển (hồ sơ này đang \"{trangThaiHienTai}\").");

            if (!string.Equals(ketQuaSoTuyen, TrangThaiHoSo.DatSoTuyen, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(ketQuaSoTuyen, TrangThaiHoSo.KhongDatSoTuyen, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Kết quả sơ tuyển chỉ có thể là \"Đạt sơ tuyển\" hoặc \"Không đạt sơ tuyển\".");
            }

            return (true, null);
        }

        public (bool ChoPhep, string? LyDo) CoTheHuyHoSo(string? trangThaiHienTai, bool daCoLichPhongVan = false)
        {
            if (string.IsNullOrWhiteSpace(trangThaiHienTai))
                return (false, "Hồ sơ không tồn tại.");

            var hienTai = trangThaiHienTai.Trim();

            if (string.Equals(hienTai, TrangThaiHoSo.DaHuy, StringComparison.OrdinalIgnoreCase))
                return (false, "Hồ sơ này đã được hủy trước đó.");

            if (string.Equals(hienTai, TrangThaiHoSo.DaPhongVan, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(hienTai, TrangThaiHoSo.TrungTuyen, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(hienTai, TrangThaiHoSo.KhongTrungTuyen, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Hồ sơ đã phỏng vấn hoặc đã có kết quả cuối cùng, không được phép hủy.");
            }

            if (string.Equals(hienTai, TrangThaiHoSo.ChoPhongVan, StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(hienTai, TrangThaiHoSo.DatSoTuyen, StringComparison.OrdinalIgnoreCase) && daCoLichPhongVan))
            {
                return (false, "Hồ sơ đã được xếp lịch phỏng vấn, không thể tự ý hủy.");
            }

            if (string.Equals(hienTai, TrangThaiHoSo.ChoDuyet, StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(hienTai, TrangThaiHoSo.DatSoTuyen, StringComparison.OrdinalIgnoreCase) && !daCoLichPhongVan))
            {
                return (true, null);
            }

            return (false, $"Không thể hủy hồ sơ đang ở trạng thái \"{hienTai}\".");
        }

        public (bool ChoPhep, string? LyDo) CoTheLapLichPhongVan(string? trangThaiHienTai)
        {
            if (string.IsNullOrWhiteSpace(trangThaiHienTai))
                return (false, "Vui lòng chọn hồ sơ xét tuyển.");

            var hienTai = trangThaiHienTai.Trim();

            if (string.Equals(hienTai, TrangThaiHoSo.DatSoTuyen, StringComparison.OrdinalIgnoreCase))
                return (true, null);

            if (string.Equals(hienTai, TrangThaiHoSo.KhongDatSoTuyen, StringComparison.OrdinalIgnoreCase))
                return (false, "Hồ sơ \"Không đạt sơ tuyển\" không được phép lập lịch phỏng vấn.");

            if (string.Equals(hienTai, TrangThaiHoSo.ChoDuyet, StringComparison.OrdinalIgnoreCase))
                return (false, "Hồ sơ chưa qua vòng sơ tuyển (đang \"Chờ duyệt\"), chưa thể lập lịch.");

            if (string.Equals(hienTai, TrangThaiHoSo.ChoPhongVan, StringComparison.OrdinalIgnoreCase))
                return (false, "Hồ sơ này đã có lịch phỏng vấn đang hiệu lực.");

            if (string.Equals(hienTai, TrangThaiHoSo.DaPhongVan, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(hienTai, TrangThaiHoSo.TrungTuyen, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(hienTai, TrangThaiHoSo.KhongTrungTuyen, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Hồ sơ đã hoàn thành giai đoạn phỏng vấn.");
            }

            if (string.Equals(hienTai, TrangThaiHoSo.DaHuy, StringComparison.OrdinalIgnoreCase))
                return (false, "Hồ sơ đã bị hủy.");

            return (false, $"Hồ sơ đang ở trạng thái \"{hienTai}\", không thể lập lịch phỏng vấn.");
        }

        public (bool ChoPhep, string? LyDo) CoTheChuyenSangDaPhongVan(string? trangThaiHienTai, bool lichHopLe = true)
        {
            if (!lichHopLe)
                return (false, "Lịch phỏng vấn chưa tồn tại hoặc đã bị hủy, không thể đánh dấu Đã phỏng vấn.");

            if (string.IsNullOrWhiteSpace(trangThaiHienTai))
                return (false, "Hồ sơ không tồn tại.");

            var hienTai = trangThaiHienTai.Trim();

            if (string.Equals(hienTai, TrangThaiHoSo.ChoPhongVan, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(hienTai, TrangThaiHoSo.DatSoTuyen, StringComparison.OrdinalIgnoreCase))
            {
                return (true, null);
            }

            return (false, $"Hồ sơ đang ở trạng thái \"{hienTai}\", không thể chuyển sang \"{TrangThaiHoSo.DaPhongVan}\".");
        }

        public (bool ChoPhep, string? LyDo) CoTheGhiNhanKetQua(string? trangThaiHienTai, string ketQuaMoi)
        {
            if (string.IsNullOrWhiteSpace(trangThaiHienTai))
                return (false, "Hồ sơ không tồn tại.");

            var hienTai = trangThaiHienTai.Trim();

            if (!string.Equals(hienTai, TrangThaiHoSo.DaPhongVan, StringComparison.OrdinalIgnoreCase))
                return (false, $"Chỉ hồ sơ \"{TrangThaiHoSo.DaPhongVan}\" mới được ghi nhận kết quả cuối cùng (hồ sơ này đang \"{hienTai}\").");

            if (!string.Equals(ketQuaMoi, TrangThaiHoSo.TrungTuyen, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(ketQuaMoi, TrangThaiHoSo.KhongTrungTuyen, StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"Kết quả cuối cùng chỉ được phép là \"{TrangThaiHoSo.TrungTuyen}\" hoặc \"{TrangThaiHoSo.KhongTrungTuyen}\".");
            }

            return (true, null);
        }
    }
}

