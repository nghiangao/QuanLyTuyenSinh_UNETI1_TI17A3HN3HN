// Họ và tên: Mã Đức Thịnh
// Mã sinh viên: 23103100314
// Nội dung thực hiện: Module 5 - Các ViewModel phục vụ Dashboard (Đề mục 9.5) và Thống kê LINQ (Đề mục 9.6).

using System;
using System.Collections.Generic;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models
{
    // =========================================================================
    // VIEWMODEL CHO DASHBOARD (Đề mục 9.5: 9 chỉ số tổng quan)
    // Liên kết dữ liệu:
    //   - [Module 1]: Khoa
    //   - [Module 2]: NganhTuyenSinh (tổng số ngành, ngành đang tuyển)
    //   - [Module 3]: ThiSinh (tổng số thí sinh), HoSoXetTuyen
    //   - [Module 4]: Hồ sơ chờ duyệt, Đạt sơ tuyển
    //   - [Module 5]: Lịch phỏng vấn sắp tới, Hồ sơ trúng tuyển
    // =========================================================================
    public class DashboardViewModel
    {
        public int TongSoKhoa { get; set; }                    // 1. Tổng số khoa
        public int TongSoNganh { get; set; }                   // 2. Tổng số ngành tuyển sinh
        public int SoNganhDangTuyen { get; set; }              // 3. Số ngành tuyển sinh trạng thái Đang tuyển
        public int TongSoThiSinh { get; set; }                 // 4. Tổng số thí sinh
        public int TongSoHoSo { get; set; }                    // 5. Tổng số hồ sơ
        public int SoHoSoChoDuyet { get; set; }                // 6. Số hồ sơ Chờ duyệt
        public int SoHoSoDatSoTuyen { get; set; }              // 7. Số hồ sơ Đạt sơ tuyển
        public int SoLichPhongVanSapToi { get; set; }          // 8. Số lịch phỏng vấn sắp tới (chưa diễn ra, chưa hủy)
        public int SoHoSoTrungTuyen { get; set; }              // 9. Số hồ sơ Trúng tuyển
    }

    // =========================================================================
    // VIEWMODEL CHO THỐNG KÊ BẰNG LINQ (Đề mục 9.6: 10 báo cáo thống kê)
    // =========================================================================
    public class ThongKeViewModel
    {
        // 1. Số ngành tuyển sinh theo khoa [LINQ: GroupBy(Khoa) -> Count()]
        public List<ThongKeNganhTheoKhoaItem> NganhTheoKhoa { get; set; } = new();

        // 2. Số hồ sơ theo từng ngành tuyển sinh [LINQ: GroupBy(Nganh) -> Count()]
        public List<ThongKeHoSoTheoNganhItem> HoSoTheoNganh { get; set; } = new();

        // 3. Số hồ sơ theo trạng thái [LINQ: GroupBy(TrangThai) -> Count()]
        public List<ThongKeHoSoTheoTrangThaiItem> HoSoTheoTrangThai { get; set; } = new();

        // 4. Số hồ sơ đạt sơ tuyển theo ngành tuyển sinh [LINQ: Where(DatSoTuyen) -> GroupBy(Nganh)]
        public List<ThongKeDatSoTuyenTheoNganhItem> DatSoTuyenTheoNganh { get; set; } = new();

        // 5. Ngành tuyển sinh nhận nhiều hồ sơ nhất [LINQ: GroupBy -> OrderByDescending -> FirstOrDefault]
        public NganhNhieuHoSoNhatItem? NganhNhieuHoSoNhat { get; set; }

        // 6. Số hồ sơ trúng tuyển theo khoa [LINQ: Where(TrungTuyen) -> GroupBy(Khoa)]
        public List<ThongKeTrungTuyenTheoKhoaItem> TrungTuyenTheoKhoa { get; set; } = new();

        // 7. Số hồ sơ nộp theo tháng [LINQ: GroupBy(NgayNop.Month)]
        public List<ThongKeTheoThangItem> HoSoTheoThang { get; set; } = new();

        // 8. Số lịch phỏng vấn theo tháng [LINQ: GroupBy(ThoiGianBatDau.Month)]
        public List<ThongKeTheoThangItem> LichTheoThang { get; set; } = new();

        // 9. Tỷ lệ hồ sơ qua sơ tuyển theo từng ngành tuyển sinh (% = Đạt sơ tuyển / Tổng hồ sơ ngành)
        public List<TyLeQuaSoTuyenItem> TyLeQuaSoTuyenTheoNganh { get; set; } = new();

        // 10. Tỷ lệ hồ sơ trúng tuyển trên số hồ sơ đã phỏng vấn
        public TyLeTrungTuyenSauPVItem TyLeTrungTuyenTrenDaPV { get; set; } = new();
    }

    public class ThongKeNganhTheoKhoaItem
    {
        public string TenKhoa { get; set; } = string.Empty;
        public int SoLuongNganh { get; set; }
        public int TongChiTieu { get; set; }
    }

    public class ThongKeHoSoTheoNganhItem
    {
        public string TenNganh { get; set; } = string.Empty;
        public int ChiTieu { get; set; }
        public int SoLuongHoSo { get; set; }
    }

    public class ThongKeHoSoTheoTrangThaiItem
    {
        public string TrangThai { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public double TyLePhanTram { get; set; }
    }

    public class ThongKeDatSoTuyenTheoNganhItem
    {
        public string TenNganh { get; set; } = string.Empty;
        public int SoHoSoDatSoTuyen { get; set; }
    }

    public class NganhNhieuHoSoNhatItem
    {
        public string TenNganh { get; set; } = string.Empty;
        public string TenKhoa { get; set; } = string.Empty;
        public int ChiTieu { get; set; }
        public int SoLuongHoSo { get; set; }
    }

    public class ThongKeTrungTuyenTheoKhoaItem
    {
        public string TenKhoa { get; set; } = string.Empty;
        public int SoLuongTrungTuyen { get; set; }
    }

    public class ThongKeTheoThangItem
    {
        public int Nam { get; set; }
        public int Thang { get; set; }
        public int SoLuong { get; set; }
    }

    public class TyLeQuaSoTuyenItem
    {
        public string TenNganh { get; set; } = string.Empty;
        public int TongHoSo { get; set; }
        public int SoDatSoTuyen { get; set; }
        public double TyLePhanTram { get; set; }
    }

    public class TyLeTrungTuyenSauPVItem
    {
        public int TongHoSoDaPhongVan { get; set; }
        public int SoHoSoTrungTuyen { get; set; }
        public int SoHoSoKhongTrungTuyen { get; set; }
        public double TyLeTrungTuyenPhanTram { get; set; }
    }

    // =========================================================================
    // VIEWMODEL CHO THÍ SINH TRA CỨU LỊCH VÀ KẾT QUẢ (Đề mục M5.4)
    // =========================================================================
    public class TraCuuThiSinhViewModel
    {
        public string? TuKhoaTraCuu { get; set; }
        public ThiSinh? ThongTinThiSinh { get; set; }
        public List<HoSoXetTuyen> DanhSachHoSo { get; set; } = new();
    }
}

