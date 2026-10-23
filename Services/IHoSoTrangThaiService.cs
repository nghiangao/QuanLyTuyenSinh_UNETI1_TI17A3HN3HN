// Họ và tên: Nguyễn Đức Nghĩa
// Mã sinh viên: 23103100307
// Nội dung thực hiện: Module 4 - Task M4.2: Interface Service kiểm tra luồng chuyển trạng thái hồ sơ xét tuyển dùng chung cho SV3, SV4, SV5 theo kiến trúc N-Tier.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Services
{
    /// <summary>
    /// Service quản lý và kiểm tra tính hợp lệ của luồng chuyển trạng thái hồ sơ xét tuyển (Task M4.2).
    /// Đảm bảo tuân thủ nghiêm ngặt bảng trạng thái tại T1.3 và Mục 5.2:
    /// Chờ duyệt → Đạt sơ tuyển → Chờ phỏng vấn → Đã phỏng vấn → Trúng tuyển / Không trúng tuyển
    /// </summary>
    public interface IHoSoTrangThaiService
    {
        /// <summary>
        /// Lấy danh sách các trạng thái tiếp theo hợp lệ từ trạng thái hiện tại.
        /// </summary>
        IReadOnlyList<string> LayDanhSachTrangThaiHopLe(string? trangThaiHienTai);

        /// <summary>
        /// Kiểm tra nhanh xem việc chuyển từ trạng thái hiện tại sang trạng thái mới có hợp lệ hay không.
        /// </summary>
        bool CoTheChuyenTrangThai(string? trangThaiHienTai, string? trangThaiMoi);

        /// <summary>
        /// Kiểm tra chi tiết và trả về thông báo lỗi nếu vi phạm nghiệp vụ (dùng khi Controller xử lý ModelState/TempData).
        /// </summary>
        (bool HopLe, string? ThongBaoLoi) KiemTraChuyenTrangThai(string? trangThaiHienTai, string? trangThaiMoi);

        /// <summary>
        /// Kiểm tra luồng trạng thái của một hồ sơ cụ thể bằng cách truy vấn DB thông qua Repository.
        /// </summary>
        Task<(bool HopLe, string? ThongBaoLoi)> KiemTraChuyenTrangThaiHoSoAsync(int maHoSo, string trangThaiMoi);

        /// <summary>
        /// Kiểm tra hồ sơ có đủ điều kiện để Cán bộ sơ tuyển hay không (Task M4.5).
        /// </summary>
        (bool ChoPhep, string? LyDo) CoTheSoTuyen(string? trangThaiHienTai, string ketQuaSoTuyen);

        /// <summary>
        /// Kiểm tra thí sinh có được phép hủy hồ sơ của mình hay không (Bàn giao cho SV3 - Task M3.4).
        /// </summary>
        (bool ChoPhep, string? LyDo) CoTheHuyHoSo(string? trangThaiHienTai, bool daCoLichPhongVan = false);

        /// <summary>
        /// Kiểm tra hồ sơ có đủ điều kiện để lập lịch phỏng vấn hay không (Bàn giao cho SV5 - Task M5.1).
        /// </summary>
        (bool ChoPhep, string? LyDo) CoTheLapLichPhongVan(string? trangThaiHienTai);

        /// <summary>
        /// Kiểm tra điều kiện khi hoàn thành lịch phỏng vấn để chuyển sang Đã phỏng vấn (Bàn giao cho SV5 - Task M5.2).
        /// </summary>
        (bool ChoPhep, string? LyDo) CoTheChuyenSangDaPhongVan(string? trangThaiHienTai, bool lichHopLe = true);

        /// <summary>
        /// Kiểm tra hồ sơ có đủ điều kiện để ghi nhận kết quả cuối cùng hay không (Bàn giao cho SV5 - Task M5.3).
        /// </summary>
        (bool ChoPhep, string? LyDo) CoTheGhiNhanKetQua(string? trangThaiHienTai, string ketQuaMoi);
    }
}

