//Họ và tên: Nguyễn Đức Nghĩa
//Mã sinh viên: 23103100307
//Nội dung thực hiện: Module 4 - Interface Service xử lý nghiệp vụ tiếp nhận hồ sơ, sơ tuyển, kiểm soát chỉ tiêu theo kiến trúc N-Tier.

using System.Collections.Generic;
using System.Threading.Tasks;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.ViewModels;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Services
{
    public interface IHoSoXetTuyenService
    {
        /// <summary>
        /// M4.1: Tìm kiếm, lọc theo khoa/trạng thái/ngày nộp, sắp xếp và phân trang hồ sơ xét tuyển.
        /// </summary>
        Task<HoSoXetTuyenIndexViewModel> GetDanhSachHoSoAsync(HoSoXetTuyenIndexViewModel filter);

        /// <summary>
        /// M4.1 & M4.6: Lấy chi tiết hồ sơ xét tuyển đầy đủ thông tin thí sinh, ngành, khoa, lịch phỏng vấn và cảnh báo chỉ tiêu.
        /// </summary>
        Task<HoSoXetTuyenDetailsViewModel?> GetChiTietHoSoAsync(int maHoSo);

        /// <summary>
        /// M4.3: Lấy thông tin màn hình sơ tuyển, chuẩn bị dữ liệu đối chiếu tổ hợp và điểm số tham khảo.
        /// </summary>
        Task<SoTuyenViewModel?> GetThongTinSoTuyenAsync(int maHoSo);

        /// <summary>
        /// M4.4: Kiểm tra 4 điều kiện nghiêm ngặt trước khi duyệt sơ tuyển:
        /// 1. Hồ sơ tồn tại và chưa bị hủy.
        /// 2. Ngành tuyển sinh liên quan tồn tại.
        /// 3. Thí sinh còn hoạt động.
        /// 4. Hồ sơ chưa có kết quả sơ tuyển trước đó (đang ở trạng thái Chờ duyệt).
        /// </summary>
        Task<(bool HopLe, List<string> DanhSachLoi)> KiemTraDieuKienSoTuyenAsync(int maHoSo);

        /// <summary>
        /// M4.5: Xử lý duyệt / từ chối sơ tuyển, cập nhật trạng thái, ngày xử lý và nhận xét.
        /// </summary>
        Task<(bool ThanhCong, string ThongBao)> XuLySoTuyenAsync(int maHoSo, string ketQua, string nhanXet);

        /// <summary>
        /// M4.6: LINQ đếm số hồ sơ Trúng tuyển của từng ngành, kiểm soát và cảnh báo khi đạt chỉ tiêu.
        /// </summary>
        Task<(int SoLuongTrungTuyen, int ChiTieu, bool DaDatChiTieu, string CanhBao)> KiemTraCanhBaoChiTieuAsync(int maNganh);

        /// <summary>
        /// M4.6: Đóng ngành tuyển sinh khi đã đạt chỉ tiêu tuyển sinh mà không xóa các hồ sơ còn lại.
        /// </summary>
        Task<(bool ThanhCong, string ThongBao)> DongNganhTuyenSinhAsync(int maNganh);
    }
}

