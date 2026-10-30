using System.ComponentModel.DataAnnotations;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

public class TaiKhoanFormViewModel
{
    public int MaTaiKhoan { get; set; }

    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [StringLength(50)]
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Mật khẩu không được vượt quá 255 ký tự")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string? MatKhau { get; set; }

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100)]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(100)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn vai trò")]
    [StringLength(30)]
    [Display(Name = "Vai trò")]
    public string VaiTro { get; set; } = Models.VaiTro.ThiSinh;

    [Display(Name = "Hoạt động")]
    public bool TrangThai { get; set; } = true;
}

public class TaiKhoanListItemViewModel
{
    public int MaTaiKhoan { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string VaiTro { get; set; } = string.Empty;
    public bool TrangThai { get; set; }
}
