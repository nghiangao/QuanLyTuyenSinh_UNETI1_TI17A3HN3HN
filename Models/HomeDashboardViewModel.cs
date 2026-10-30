// Họ và tên: Mã Đức Thịnh
// Mã sinh viên: 23103100314
// Nội dung thực hiện: ViewModel tổng hợp dữ liệu Trang chủ (Home) phân chia theo vai trò người dùng (Admin, Cán bộ, Thí sinh, Khách).

using System.Collections.Generic;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

public class HomeDashboardViewModel
{
    // ==========================================
    // THÔNG TIN PHÂN QUYỀN & SESSION
    // ==========================================
    public bool IsAuthenticated { get; set; }
    public int? MaTaiKhoan { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string VaiTro { get; set; } = string.Empty;

    // ==========================================
    // DÀNH CHO KHÁCH (CHƯA ĐĂNG NHẬP)
    // ==========================================
    public List<NganhTuyenSinh> NganhDangTuyen { get; set; } = new();
    public int TongSoNganhDangTuyen { get; set; }
    public int TongSoKhoa { get; set; }

    // ==========================================
    // DÀNH CHO ADMIN & CÁN BỘ TUYỂN SINH
    // ==========================================
    public int TongSoThiSinh { get; set; }
    public int TongSoHoSo { get; set; }
    public int SoHoSoChoDuyet { get; set; }
    public int SoHoSoDatSoTuyen { get; set; }
    public int SoLichPhongVanSapToi { get; set; }
    public int SoHoSoTrungTuyen { get; set; }

    // ==========================================
    // DÀNH RIÊNG CHO THÍ SINH
    // ==========================================
    public ThiSinh? ThongTinThiSinh { get; set; }
    public List<HoSoXetTuyen> DanhSachHoSoCuaToi { get; set; } = new();
}

