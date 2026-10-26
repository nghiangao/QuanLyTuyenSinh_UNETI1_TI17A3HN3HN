//Họ và tên: Nguyễn Đức Nghĩa
//Mã sinh viên: 23103100307
//Nội dung thực hiện: ViewModel danh sách hồ sơ cần xử lý, tìm kiếm, lọc, sắp xếp và phân trang (Task M4.1).

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.ViewModels
{
    public class HoSoXetTuyenItemViewModel
    {
        public int MaHoSo { get; set; }
        public int MaThiSinh { get; set; }
        public string HoTenThiSinh { get; set; } = string.Empty;
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? TruongTHPT { get; set; }
        public decimal? DiemThiTHPT { get; set; }
        public string? ToHopThiSinh { get; set; }
        public int MaNganh { get; set; }
        public string TenNganh { get; set; } = string.Empty;
        public string TenKhoa { get; set; } = string.Empty;
        public DateTime NgayNop { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public DateTime? NgayXuLy { get; set; }
        public string? NhanXetSoTuyen { get; set; }
    }

    public class HoSoXetTuyenIndexViewModel
    {
        // Bộ lọc & Tìm kiếm
        public string? SearchKeyword { get; set; }
        public int? MaKhoa { get; set; }
        public string? TrangThai { get; set; }
        public DateTime? NgayNopTu { get; set; }
        public DateTime? NgayNopDen { get; set; }
        public string? SortOrder { get; set; }

        // Phân trang
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 0;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        // Dữ liệu danh sách
        public List<HoSoXetTuyenItemViewModel> Items { get; set; } = new();

        // Thống kê nhanh trên tiêu đề
        public int TotalAll { get; set; }
        public int TotalChoDuyet { get; set; }
        public int TotalDatSoTuyen { get; set; }
        public int TotalTrungTuyen { get; set; }

        // Danh sách Dropdown cho bộ lọc
        public SelectList? KhoaList { get; set; }
        public SelectList? TrangThaiList { get; set; }
    }
}

