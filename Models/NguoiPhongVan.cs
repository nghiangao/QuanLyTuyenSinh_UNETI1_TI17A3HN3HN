using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models
{
    // Entity bổ sung (đề cho phép): nhờ có khóa riêng nên kiểm tra
    // "người phỏng vấn trùng lịch" chính xác, không phụ thuộc việc gõ đúng tên.
    public class NguoiPhongVan
    {
        [Key]
        public int MaNguoiPhongVan { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Người phỏng vấn")]
        public string HoTen { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Display(Name = "Hoạt động")]
        public bool TrangThai { get; set; } = true;

        public ICollection<LichPhongVan> LichPhongVans { get; set; } = new List<LichPhongVan>();
    }
}