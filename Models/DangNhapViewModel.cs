// Họ và tên: Đinh Văn Thành
// Mã sinh viên: 23103100180
// Nội dung thực hiện: Tài Khoản, Đăng Nhập, Phân Quyền, Quản Lý Khoa.
using System.ComponentModel.DataAnnotations;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

public class DangNhapViewModel
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
