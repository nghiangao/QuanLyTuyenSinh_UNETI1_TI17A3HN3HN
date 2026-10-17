using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models
{
    public class HoSoXetTuyen
    {
        [Key]
        public int MaHoSo { get; set; }

        public int MaThiSinh { get; set; }
        public ThiSinh? ThiSinh { get; set; }

        public int MaNganh { get; set; }
        public NganhTuyenSinh? NganhTuyenSinh { get; set; }

        [Display(Name = "Ngày nộp")]
        public DateTime NgayNop { get; set; }          // hệ thống gán = DateTime.Now, không cho nhập

        [Required(ErrorMessage = "Vui lòng nhập nguyện vọng xét tuyển")]
        [StringLength(1000)]
        [Display(Name = "Nguyện vọng xét tuyển")]
        public string NguyenVongXetTuyen { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = TrangThaiHoSo.ChoDuyet;

        [Display(Name = "Ngày xử lý")]
        public DateTime? NgayXuLy { get; set; }

        [StringLength(1000)]
        [Display(Name = "Nhận xét sơ tuyển")]
        public string? NhanXetSoTuyen { get; set; }

        public ICollection<LichPhongVan> LichPhongVans { get; set; } = new List<LichPhongVan>();   // 1 - 0..n
        public KetQuaXetTuyen? KetQuaXetTuyen { get; set; }                                       // 1 - 0..1
    }
}