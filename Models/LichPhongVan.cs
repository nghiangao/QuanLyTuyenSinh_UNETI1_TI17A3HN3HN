using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models
{
    public class LichPhongVan : IValidatableObject
    {
        [Key]
        public int MaLichPhongVan { get; set; }

        public int MaHoSo { get; set; }
        public HoSoXetTuyen? HoSoXetTuyen { get; set; }

        [Display(Name = "Bắt đầu")]
        public DateTime ThoiGianBatDau { get; set; }

        [Display(Name = "Kết thúc")]
        public DateTime ThoiGianKetThuc { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình thức phỏng vấn")]
        [StringLength(30)]
        [Display(Name = "Hình thức")]
        public string HinhThucPhongVan { get; set; } = "Trực tiếp";   // Trực tiếp / Trực tuyến

        [StringLength(250)]
        [Display(Name = "Địa điểm / Liên kết")]
        public string? DiaDiemHoacLienKet { get; set; }

        [Display(Name = "Người phỏng vấn")]
        public int MaNguoiPhongVan { get; set; }
        public NguoiPhongVan? NguoiPhongVan { get; set; }

        [StringLength(1000)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = TrangThaiLich.DaLenLich;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ThoiGianKetThuc <= ThoiGianBatDau)
                yield return new ValidationResult(
                    "Thời gian kết thúc phải sau thời gian bắt đầu",
                    new[] { nameof(ThoiGianKetThuc) });
        }
    }
}