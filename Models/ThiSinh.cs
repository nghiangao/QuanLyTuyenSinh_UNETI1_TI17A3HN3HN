using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models
{
    public class ThiSinh
    {
        [Key]
        public int MaThiSinh { get; set; }

        public int MaTaiKhoan { get; set; }            // unique, 1 - 0..1
        public TaiKhoan? TaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime NgaySinh { get; set; }

        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; } = GioiTinhConst.Nam;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        // NULLABLE có chủ ý: null = "chưa khai báo" -> dùng để chặn nộp hồ sơ
        [StringLength(20)]
        [Display(Name = "Tổ hợp xét tuyển")]
        public string? ToHopXetTuyen { get; set; }

        [StringLength(150)]
        [Display(Name = "Trường THPT")]
        public string? TruongTHPT { get; set; }

        [Range(0, 30, ErrorMessage = "Điểm thi THPT từ 0 đến 30")]
        [Display(Name = "Điểm thi THPT")]
        public decimal? DiemThiTHPT { get; set; }     // NULLABLE có chủ ý (như trên)

        [StringLength(1000)]
        [Display(Name = "Thành tích nổi bật")]
        public string? ThanhTichNoiBat { get; set; }

        [Display(Name = "Hoạt động")]
        public bool TrangThai { get; set; } = true;

        public ICollection<HoSoXetTuyen> HoSoXetTuyens { get; set; } = new List<HoSoXetTuyen>();
    }
}