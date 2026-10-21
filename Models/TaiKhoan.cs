// Họ và tên: Đinh Văn Thành
// Mã sinh viên: 23103100180
// Nội dung thực hiện: Tài Khoản, Đăng Nhập, Phân Quyền, Quản Lý Khoa.


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;   // unique (xem DbContext)

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(255)]   // để dài sẵn, sau này nâng cấp mã hóa mật khẩu không phải đổi cột
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = Models.VaiTro.ThiSinh;

        [Display(Name = "Hoạt động")]
        public bool TrangThai { get; set; } = true;   // true = hoạt động, false = bị khóa

        public ThiSinh? ThiSinh { get; set; }          // 1 - 0..1
    }
}