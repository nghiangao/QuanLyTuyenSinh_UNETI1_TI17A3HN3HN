// Họ và tên: Đào Văn Khánh
// Mã sinh viên: 23103100125
// Nội dung thực hiện: Quản lý ngành tuyển sinh, Tìm kiếm, Lọc, Sắp xếp và Phân trang.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models
{
    public class NganhTuyenSinh
{
    [Key]
    [Required(ErrorMessage = "Mã ngành là bắt buộc nhập.")]
    [StringLength(50, ErrorMessage = "Mã ngành không được vượt quá 50 ký tự.")]
    [Display(Name = "Mã ngành")]
    public string MaNganh { get; set; } 

    [Required(ErrorMessage = "Tên ngành tuyển sinh là bắt buộc nhập.")]
    [StringLength(200, ErrorMessage = "Tên ngành không được vượt quá 200 ký tự.")]
    [Display(Name = "Tên ngành tuyển sinh")]
    public string TenNganh { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn khoa.")]
    [Display(Name = "Khoa")]
    public int MaKhoa { get; set; } // Giả sử bảng Khoa vẫn dùng MaKhoa kiểu int

    [Required(ErrorMessage = "Chỉ tiêu là bắt buộc nhập.")]
    [Range(1, int.MaxValue, ErrorMessage = "Chỉ tiêu phải lớn hơn 0.")]
    [Display(Name = "Chỉ tiêu")]
    public int ChiTieu { get; set; }

    [Required(ErrorMessage = "Tổ hợp xét tuyển là bắt buộc nhập (VD: A00, A01...).")]
    [StringLength(50)]
    [Display(Name = "Tổ hợp xét tuyển")]
    public string ToHopXetTuyen { get; set; }

    [Required(ErrorMessage = "Điểm xét tuyển tối thiểu là bắt buộc nhập.")]
    [Range(0, 30, ErrorMessage = "Điểm xét tuyển tối thiểu phải nằm trong khoảng từ 0 đến 30.")]
    [Display(Name = "Điểm xét tuyển tối thiểu")]
    public double DiemXetTuyenToiThieu { get; set; }

    [Required(ErrorMessage = "Ngày bắt đầu nhận hồ sơ là bắt buộc.")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày bắt đầu nhận hồ sơ")]
    public DateTime NgayBatDauNhanHoSo { get; set; }

    [Required(ErrorMessage = "Hạn nộp hồ sơ là bắt buộc.")]
    [DataType(DataType.Date)]
    [Display(Name = "Hạn nộp hồ sơ")]
    public DateTime HanNopHoSo { get; set; }

    [DataType(DataType.MultilineText)]
    [Display(Name = "Mô tả ngành")]
    public string MoTaNganh { get; set; }

    [DataType(DataType.MultilineText)]
    [Display(Name = "Yêu cầu thí sinh")]
    public string YeuCauThiSinh { get; set; }

    [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
    [StringLength(50)]
    [Display(Name = "Trạng thái")]
    // Trạng thái gồm: Chưa mở, Đang tuyển, Tạm dừng, Đã đóng
    public string TrangThai { get; set; }

    // ==========================================
    // NAVIGATION PROPERTIES (LIÊN KẾT KHÓA NGOẠI)
    // ==========================================

    // Liên kết N-1 với Entity Khoa
    [ForeignKey("MaKhoa")]
    public virtual Khoa Khoa { get; set; }

    // Liên kết 1-N với Entity HoSoXetTuyen
    public virtual ICollection<HoSoXetTuyen> HoSoXetTuyens { get; set; }
}
}