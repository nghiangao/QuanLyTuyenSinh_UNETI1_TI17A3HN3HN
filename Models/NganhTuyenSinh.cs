// Họ và tên: Đào Văn Khánh
// Mã sinh viên: 23103100125
// Nội dung thực hiện: Quản lý ngành tuyển sinh, Tìm kiếm, Lọc, Sắp xếp và Phân trang.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models
{
    // Bổ sung IValidatableObject để hỗ trợ hàm Validate kiểm tra ngày tháng
    public class NganhTuyenSinh : IValidatableObject
    {
        [Key]
        public int MaNganh { get; set; }

        [Required(ErrorMessage = "Tên ngành là bắt buộc")]
        [StringLength(150)]
        [Display(Name = "Tên ngành")]
        public string TenNganh { get; set; } = string.Empty;

        [Display(Name = "Khoa")]
        public int MaKhoa { get; set; }
        public Khoa? Khoa { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Chỉ tiêu phải lớn hơn 0")]
        [Display(Name = "Chỉ tiêu")]
        public int ChiTieu { get; set; }

        [Required(ErrorMessage = "Tổ hợp xét tuyển là bắt buộc")]   
        [StringLength(100)]
        [Display(Name = "Tổ hợp xét tuyển")]
        public string ToHopXetTuyen { get; set; } = string.Empty;   // ví dụ: "A00, A01, D01"

        [Range(0, 30, ErrorMessage = "Điểm xét tuyển tối thiểu từ 0 đến 30")]
        [Display(Name = "Điểm xét tuyển tối thiểu")]
        public decimal DiemXetTuyenToiThieu { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu nhận hồ sơ")]
        public DateTime NgayBatDauNhanHoSo { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Hạn nộp hồ sơ")]
        public DateTime HanNopHoSo { get; set; } = DateTime.Today.AddMonths(1);

        [StringLength(2000)]
        [Display(Name = "Mô tả ngành")]
        public string? MoTaNganh { get; set; }

        [StringLength(2000)]
        [Display(Name = "Yêu cầu thí sinh")]
        public string? YeuCauThiSinh { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = TrangThaiNganh.ChuaMo;

        public ICollection<HoSoXetTuyen> HoSoXetTuyens { get; set; } = new List<HoSoXetTuyen>();

        // Kiểm tra liên trường (hạn nộp >= ngày bắt đầu)
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HanNopHoSo.Date < NgayBatDauNhanHoSo.Date)
                yield return new ValidationResult(
                    "Hạn nộp hồ sơ phải sau hoặc bằng ngày bắt đầu nhận hồ sơ",
                    new[] { nameof(HanNopHoSo) });
        }

        // CHỈ dùng trong View/C#. KHÔNG dùng trong truy vấn LINQ to EF (thuộc tính NotMapped
        // không dịch được sang SQL) - trong truy vấn hãy viết điều kiện đầy đủ.
        [NotMapped]
        public bool DangNhanHoSo =>
            TrangThai == TrangThaiNganh.DangTuyen
            && DateTime.Today >= NgayBatDauNhanHoSo.Date
            && DateTime.Today <= HanNopHoSo.Date;

        [NotMapped]
        public bool HetHan => DateTime.Today > HanNopHoSo.Date;
    }
}