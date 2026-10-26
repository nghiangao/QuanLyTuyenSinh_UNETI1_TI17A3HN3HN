//Họ và tên: Nguyễn Đức Nghĩa
//Mã sinh viên: 23103100307
//Nội dung thực hiện: ViewModel chi tiết hồ sơ xét tuyển và kiểm soát chỉ tiêu ngành (Task M4.1, M4.6).

using System;
using System.Collections.Generic;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.ViewModels
{
    public class HoSoXetTuyenDetailsViewModel
    {
        // Thông tin hồ sơ xét tuyển
        public int MaHoSo { get; set; }
        public DateTime NgayNop { get; set; }
        public string NguyenVongXetTuyen { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public DateTime? NgayXuLy { get; set; }
        public string? NhanXetSoTuyen { get; set; }

        // Thông tin Thí sinh
        public int MaThiSinh { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public string? TruongTHPT { get; set; }
        public string? ToHopThiSinh { get; set; }
        public decimal? DiemThiTHPT { get; set; }
        public string? ThanhTichNoiBat { get; set; }
        public bool ThiSinhHoatDong { get; set; }
        public string? TenDangNhap { get; set; }

        // Thông tin Ngành tuyển sinh & Khoa
        public int MaNganh { get; set; }
        public string TenNganh { get; set; } = string.Empty;
        public int MaKhoa { get; set; }
        public string TenKhoa { get; set; } = string.Empty;
        public int ChiTieu { get; set; }
        public string ToHopXetTuyenNganh { get; set; } = string.Empty;
        public decimal DiemXetTuyenToiThieu { get; set; }
        public DateTime NgayBatDauNhanHoSo { get; set; }
        public DateTime HanNopHoSo { get; set; }
        public string TrangThaiNganh { get; set; } = string.Empty;
        public string? MoTaNganh { get; set; }
        public string? YeuCauThiSinh { get; set; }

        // Kiểm soát chỉ tiêu (Task M4.6)
        public int SoLuongTrungTuyen { get; set; }
        public bool DaDatChiTieu => SoLuongTrungTuyen >= ChiTieu && ChiTieu > 0;
        public string? CanhBaoChiTieu { get; set; }
        public bool ChoPhepDongNganh => DaDatChiTieu && TrangThaiNganh != "Đã đóng";

        // Kiểm tra điều kiện sơ tuyển (Task M4.4)
        public bool DuDieuKienSoTuyen { get; set; }
        public List<string> ThongBaoDieuKien { get; set; } = new();

        // Danh sách Lịch phỏng vấn liên quan (nếu có)
        public List<LichPhongVanItemViewModel> LichPhongVans { get; set; } = new();

        // Kết quả xét tuyển (nếu có)
        public KetQuaXetTuyenItemViewModel? KetQuaXetTuyen { get; set; }
    }

    public class LichPhongVanItemViewModel
    {
        public int MaLichPhongVan { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public string? DiaDiemHoacLienKet { get; set; }
        public string HinhThucPhongVan { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
    }

    public class KetQuaXetTuyenItemViewModel
    {
        public int MaKetQuaXetTuyen { get; set; }
        public decimal? DiemDanhGia { get; set; }
        public string KetQua { get; set; } = string.Empty;
        public string? NhanXet { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }
}

