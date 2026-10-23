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
