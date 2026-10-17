using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models
{
    public class KetQuaXetTuyen
    {
        [Key]
        public int MaKetQuaXetTuyen { get; set; }

        public int MaHoSo { get; set; }                // unique, 1 - 0..1
        public HoSoXetTuyen? HoSoXetTuyen { get; set; }

        [Range(0, 100, ErrorMessage = "Điểm đánh giá từ 0 đến 100")]
        [Display(Name = "Điểm đánh giá")]
        public decimal? DiemDanhGia { get; set; }

        [StringLength(1000)]
        [Display(Name = "Nhận xét")]
        public string? NhanXet { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Kết quả")]
        public string KetQua { get; set; } = KetQuaTuyen.KhongTrungTuyen;

        [Display(Name = "Ngày cập nhật")]
        public DateTime NgayCapNhat { get; set; }       // hệ thống gán
    }
}