//Họ và tên: Nguyễn Đức Nghĩa
//Mã sinh viên: 23103100307
//Nội dung thực hiện: ViewModel màn hình sơ tuyển hồ sơ, đối chiếu tiêu chí và duyệt/từ chối (Task M4.3, M4.4, M4.5).

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.ViewModels
{
    public class SoTuyenViewModel
    {
        // Thông tin hồ sơ hiển thị
        public int MaHoSo { get; set; }
        public DateTime NgayNop { get; set; }
        public string TrangThaiHienTai { get; set; } = string.Empty;
        public string NguyenVongXetTuyen { get; set; } = string.Empty;
        public string? GhiChu { get; set; }

        // Thông tin Thí sinh
        public int MaThiSinh { get; set; }
        public string HoTenThiSinh { get; set; } = string.Empty;
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

        // Thông tin Ngành tuyển sinh
        public int MaNganh { get; set; }
        public string TenNganh { get; set; } = string.Empty;
        public string TenKhoa { get; set; } = string.Empty;
        public int ChiTieu { get; set; }
        public string ToHopYeuCauNganh { get; set; } = string.Empty;
        public decimal DiemXetTuyenToiThieu { get; set; }
        public DateTime HanNopHoSo { get; set; }
        public string TrangThaiNganh { get; set; } = string.Empty;
        public string? MoTaNganh { get; set; }
        public string? YeuCauThiSinh { get; set; }

        // M4.3: Đối chiếu & So sánh tiêu chí tham khảo (hệ thống không tự suy diễn kết quả)
        public bool? ToHopPhuHop { get; set; }
        public string? ThongTinSoSanhToHop { get; set; }

        public bool? DiemDatChuanToiThieu { get; set; }
        public string? ThongTinSoSanhDiem { get; set; }

        // M4.4: Kiểm tra 4 điều kiện hợp lệ trước khi duyệt
        public bool DieuKien1_HoSoTonTaiVaChuaHuy { get; set; }
        public bool DieuKien2_NganhTonTai { get; set; }
        public bool DieuKien3_ThiSinhHoatDong { get; set; }
        public bool DieuKien4_ChuaSoTuyen { get; set; }

        public bool DuDieuKienSoTuyen =>
            DieuKien1_HoSoTonTaiVaChuaHuy &&
            DieuKien2_NganhTonTai &&
            DieuKien3_ThiSinhHoatDong &&
            DieuKien4_ChuaSoTuyen;

        public List<string> DanhSachLoiDieuKien { get; set; } = new();

        // M4.5: Dữ liệu Form xử lý sơ tuyển do Cán bộ nhập
        [Required(ErrorMessage = "Vui lòng chọn kết quả sơ tuyển (Đạt sơ tuyển hoặc Không đạt sơ tuyển).")]
        [Display(Name = "Kết quả sơ tuyển")]
        public string KetQuaSoTuyen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nhận xét / đánh giá sơ tuyển.")]
        [StringLength(1000, ErrorMessage = "Nhận xét không được vượt quá 1000 ký tự.")]
        [Display(Name = "Nhận xét sơ tuyển")]
        public string NhanXetSoTuyen { get; set; } = string.Empty;
    }
}

