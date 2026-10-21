// Họ và tên: Đinh Văn Thành
// Mã sinh viên: 23103100180
// Nội dung thực hiện: Tài Khoản, Đăng Nhập, Phân Quyền, Quản Lý Khoa.


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models
{
    public class Khoa
    {
        [Key]
        public int MaKhoa { get; set; }

        [Required(ErrorMessage = "Tên khoa là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Tên khoa")]
        public string TenKhoa { get; set; } = string.Empty;   // unique

        [StringLength(1000)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        [Display(Name = "Email liên hệ")]
        public string? EmailLienHe { get; set; }

        [Display(Name = "Hoạt động")]
        public bool TrangThai { get; set; } = true;

        public ICollection<NganhTuyenSinh> NganhTuyenSinhs { get; set; } = new List<NganhTuyenSinh>();
    }
}