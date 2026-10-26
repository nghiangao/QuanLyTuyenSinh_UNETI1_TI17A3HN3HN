//Họ và tên: Nguyễn Đức Nghĩa
//Mã sinh viên: 23103100307
//Nội dung thực hiện: Module 4 - Service triển khai nghiệp vụ tiếp nhận hồ sơ, sơ tuyển và kiểm soát chỉ tiêu theo kiến trúc N-Tier.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Repositories;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.ViewModels;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Services
{
    public class HoSoXetTuyenService : IHoSoXetTuyenService
    {
        private readonly IHoSoXetTuyenRepository _hoSoRepository;
        private readonly IHoSoTrangThaiService _hoSoTrangThaiService;

        public HoSoXetTuyenService(
            IHoSoXetTuyenRepository hoSoRepository,
            IHoSoTrangThaiService hoSoTrangThaiService)
        {
            _hoSoRepository = hoSoRepository;
            _hoSoTrangThaiService = hoSoTrangThaiService;
        }

        public async Task<HoSoXetTuyenIndexViewModel> GetDanhSachHoSoAsync(HoSoXetTuyenIndexViewModel filter)
        {
            var query = _hoSoRepository.GetAllWithDetailsQueryable();

            // Thống kê nhanh toàn bộ hồ sơ trong hệ thống
            filter.TotalAll = await query.CountAsync();
            filter.TotalChoDuyet = await query.CountAsync(h => h.TrangThai == TrangThaiHoSo.ChoDuyet);
            filter.TotalDatSoTuyen = await query.CountAsync(h => h.TrangThai == TrangThaiHoSo.DatSoTuyen);
            filter.TotalTrungTuyen = await query.CountAsync(h => h.TrangThai == TrangThaiHoSo.TrungTuyen);

            // M4.1: Tìm kiếm theo họ tên thí sinh hoặc tên ngành tuyển sinh
            if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
            {
                var keyword = filter.SearchKeyword.Trim();
                query = query.Where(h =>
                    (h.ThiSinh != null && EF.Functions.Like(h.ThiSinh.HoTen, $"%{keyword}%")) ||
                    (h.NganhTuyenSinh != null && EF.Functions.Like(h.NganhTuyenSinh.TenNganh, $"%{keyword}%")));
            }

            // M4.1: Lọc theo Khoa
            if (filter.MaKhoa.HasValue && filter.MaKhoa.Value > 0)
            {
                query = query.Where(h => h.NganhTuyenSinh != null && h.NganhTuyenSinh.MaKhoa == filter.MaKhoa.Value);
            }

            // M4.1: Lọc theo Trạng thái hồ sơ
            if (!string.IsNullOrWhiteSpace(filter.TrangThai))
            {
                var tt = filter.TrangThai.Trim();
                query = query.Where(h => h.TrangThai == tt);
            }

            // M4.1: Lọc theo Ngày nộp (Từ ngày - Đến ngày)
            if (filter.NgayNopTu.HasValue)
            {
                var ngayTu = filter.NgayNopTu.Value.Date;
                query = query.Where(h => h.NgayNop >= ngayTu);
            }

            if (filter.NgayNopDen.HasValue)
            {
                var ngayDen = filter.NgayNopDen.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(h => h.NgayNop <= ngayDen);
            }

            // M4.1: Sắp xếp theo ngày nộp hoặc tên thí sinh
            filter.SortOrder = string.IsNullOrWhiteSpace(filter.SortOrder) ? "ngaynop_desc" : filter.SortOrder.Trim();
            query = filter.SortOrder switch
            {
                "ngaynop_asc" => query.OrderBy(h => h.NgayNop),
                "hoten_asc" => query.OrderBy(h => h.ThiSinh != null ? h.ThiSinh.HoTen : string.Empty),
                "hoten_desc" => query.OrderByDescending(h => h.ThiSinh != null ? h.ThiSinh.HoTen : string.Empty),
                _ => query.OrderByDescending(h => h.NgayNop)
            };

            // M4.1: Phân trang LINQ server-side với Skip và Take
            filter.TotalItems = await query.CountAsync();
            filter.PageNumber = Math.Max(1, filter.PageNumber);
            filter.PageSize = filter.PageSize > 0 ? filter.PageSize : 10;

            var pagedData = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(h => new HoSoXetTuyenItemViewModel
                {
                    MaHoSo = h.MaHoSo,
                    MaThiSinh = h.MaThiSinh,
                    HoTenThiSinh = h.ThiSinh != null ? h.ThiSinh.HoTen : string.Empty,
                    NgaySinh = h.ThiSinh != null ? h.ThiSinh.NgaySinh : DateTime.MinValue,
                    GioiTinh = h.ThiSinh != null ? h.ThiSinh.GioiTinh : string.Empty,
                    SoDienThoai = h.ThiSinh != null ? h.ThiSinh.SoDienThoai : null,
                    Email = h.ThiSinh != null ? h.ThiSinh.Email : null,
                    TruongTHPT = h.ThiSinh != null ? h.ThiSinh.TruongTHPT : null,
                    DiemThiTHPT = h.ThiSinh != null ? h.ThiSinh.DiemThiTHPT : null,
                    ToHopThiSinh = h.ThiSinh != null ? h.ThiSinh.ToHopXetTuyen : null,
                    MaNganh = h.MaNganh,
                    TenNganh = h.NganhTuyenSinh != null ? h.NganhTuyenSinh.TenNganh : string.Empty,
                    TenKhoa = h.NganhTuyenSinh != null && h.NganhTuyenSinh.Khoa != null ? h.NganhTuyenSinh.Khoa.TenKhoa : string.Empty,
                    NgayNop = h.NgayNop,
                    TrangThai = h.TrangThai,
                    NgayXuLy = h.NgayXuLy,
                    NhanXetSoTuyen = h.NhanXetSoTuyen
                })
                .ToListAsync();

            filter.Items = pagedData;

            // Nạp dữ liệu danh sách Dropdown
            var dsKhoa = await _hoSoRepository.GetDanhSachKhoaAsync();
            filter.KhoaList = new SelectList(dsKhoa, "MaKhoa", "TenKhoa", filter.MaKhoa);

            var dsTrangThai = new List<string>
            {
                TrangThaiHoSo.ChoDuyet,
                TrangThaiHoSo.DatSoTuyen,
                TrangThaiHoSo.KhongDatSoTuyen,
                TrangThaiHoSo.ChoPhongVan,
                TrangThaiHoSo.DaPhongVan,
                TrangThaiHoSo.TrungTuyen,
                TrangThaiHoSo.KhongTrungTuyen,
                TrangThaiHoSo.DaHuy
            };
            filter.TrangThaiList = new SelectList(dsTrangThai, filter.TrangThai);

            return filter;
        }

        public async Task<HoSoXetTuyenDetailsViewModel?> GetChiTietHoSoAsync(int maHoSo)
        {
            var hoSo = await _hoSoRepository.GetByIdWithDetailsAsync(maHoSo);
            if (hoSo == null)
                return null;

            // Kiểm soát chỉ tiêu (M4.6)
            var countTrungTuyen = await _hoSoRepository.CountTrungTuyenByNganhAsync(hoSo.MaNganh);
            var chiTieu = hoSo.NganhTuyenSinh?.ChiTieu ?? 0;
            var daDatChiTieu = chiTieu > 0 && countTrungTuyen >= chiTieu;
            string? canhBao = null;
            if (daDatChiTieu)
            {
                canhBao = $"CẢNH BÁO: Ngành \"{hoSo.NganhTuyenSinh?.TenNganh}\" đã đạt hoặc vượt chỉ tiêu tuyển sinh ({countTrungTuyen}/{chiTieu} chỉ tiêu). Cán bộ có thể cân nhắc đóng nhận thêm hồ sơ cho ngành này.";
            }

            // Kiểm tra điều kiện sơ tuyển (M4.4)
            var (duDieuKien, dsLoi) = await KiemTraDieuKienSoTuyenAsync(maHoSo);

            var vm = new HoSoXetTuyenDetailsViewModel
            {
                MaHoSo = hoSo.MaHoSo,
                NgayNop = hoSo.NgayNop,
                NguyenVongXetTuyen = hoSo.NguyenVongXetTuyen,
                GhiChu = hoSo.GhiChu,
                TrangThai = hoSo.TrangThai,
                NgayXuLy = hoSo.NgayXuLy,
                NhanXetSoTuyen = hoSo.NhanXetSoTuyen,

                // Thí sinh
                MaThiSinh = hoSo.MaThiSinh,
                HoTen = hoSo.ThiSinh?.HoTen ?? string.Empty,
                NgaySinh = hoSo.ThiSinh?.NgaySinh ?? DateTime.MinValue,
                GioiTinh = hoSo.ThiSinh?.GioiTinh ?? string.Empty,
                SoDienThoai = hoSo.ThiSinh?.SoDienThoai,
                Email = hoSo.ThiSinh?.Email,
                DiaChi = hoSo.ThiSinh?.DiaChi,
                TruongTHPT = hoSo.ThiSinh?.TruongTHPT,
                ToHopThiSinh = hoSo.ThiSinh?.ToHopXetTuyen,
                DiemThiTHPT = hoSo.ThiSinh?.DiemThiTHPT,
                ThanhTichNoiBat = hoSo.ThiSinh?.ThanhTichNoiBat,
                ThiSinhHoatDong = hoSo.ThiSinh?.TrangThai ?? false,
                TenDangNhap = hoSo.ThiSinh?.TaiKhoan?.TenDangNhap,

                // Ngành & Khoa
                MaNganh = hoSo.MaNganh,
                TenNganh = hoSo.NganhTuyenSinh?.TenNganh ?? string.Empty,
                MaKhoa = hoSo.NganhTuyenSinh?.MaKhoa ?? 0,
                TenKhoa = hoSo.NganhTuyenSinh?.Khoa?.TenKhoa ?? string.Empty,
                ChiTieu = chiTieu,
                ToHopXetTuyenNganh = hoSo.NganhTuyenSinh?.ToHopXetTuyen ?? string.Empty,
                DiemXetTuyenToiThieu = hoSo.NganhTuyenSinh?.DiemXetTuyenToiThieu ?? 0,
                NgayBatDauNhanHoSo = hoSo.NganhTuyenSinh?.NgayBatDauNhanHoSo ?? DateTime.MinValue,
                HanNopHoSo = hoSo.NganhTuyenSinh?.HanNopHoSo ?? DateTime.MinValue,
                TrangThaiNganh = hoSo.NganhTuyenSinh?.TrangThai ?? string.Empty,
                MoTaNganh = hoSo.NganhTuyenSinh?.MoTaNganh,
                YeuCauThiSinh = hoSo.NganhTuyenSinh?.YeuCauThiSinh,

                // Chỉ tiêu
                SoLuongTrungTuyen = countTrungTuyen,
                CanhBaoChiTieu = canhBao,

                // Sơ tuyển
                DuDieuKienSoTuyen = duDieuKien,
                ThongBaoDieuKien = dsLoi,

                // Lịch phỏng vấn
                LichPhongVans = hoSo.LichPhongVans.Select(l => new LichPhongVanItemViewModel
                {
                    MaLichPhongVan = l.MaLichPhongVan,
                    ThoiGianBatDau = l.ThoiGianBatDau,
                    ThoiGianKetThuc = l.ThoiGianKetThuc,
                    DiaDiemHoacLienKet = l.DiaDiemHoacLienKet,
                    HinhThucPhongVan = l.HinhThucPhongVan,
                    TrangThai = l.TrangThai
                }).ToList(),

                // Kết quả
                KetQuaXetTuyen = hoSo.KetQuaXetTuyen != null ? new KetQuaXetTuyenItemViewModel
                {
                    MaKetQuaXetTuyen = hoSo.KetQuaXetTuyen.MaKetQuaXetTuyen,
                    DiemDanhGia = hoSo.KetQuaXetTuyen.DiemDanhGia,
                    KetQua = hoSo.KetQuaXetTuyen.KetQua,
                    NhanXet = hoSo.KetQuaXetTuyen.NhanXet,
                    NgayCapNhat = hoSo.KetQuaXetTuyen.NgayCapNhat
                } : null
            };

            return vm;
        }

        public async Task<SoTuyenViewModel?> GetThongTinSoTuyenAsync(int maHoSo)
        {
            var hoSo = await _hoSoRepository.GetByIdWithDetailsAsync(maHoSo);
            if (hoSo == null)
                return null;

            var vm = new SoTuyenViewModel
            {
                MaHoSo = hoSo.MaHoSo,
                NgayNop = hoSo.NgayNop,
                TrangThaiHienTai = hoSo.TrangThai,
                NguyenVongXetTuyen = hoSo.NguyenVongXetTuyen,
                GhiChu = hoSo.GhiChu,

                // Thí sinh
                MaThiSinh = hoSo.MaThiSinh,
                HoTenThiSinh = hoSo.ThiSinh?.HoTen ?? string.Empty,
                NgaySinh = hoSo.ThiSinh?.NgaySinh ?? DateTime.MinValue,
                GioiTinh = hoSo.ThiSinh?.GioiTinh ?? string.Empty,
                SoDienThoai = hoSo.ThiSinh?.SoDienThoai,
                Email = hoSo.ThiSinh?.Email,
                DiaChi = hoSo.ThiSinh?.DiaChi,
                TruongTHPT = hoSo.ThiSinh?.TruongTHPT,
                ToHopThiSinh = hoSo.ThiSinh?.ToHopXetTuyen,
                DiemThiTHPT = hoSo.ThiSinh?.DiemThiTHPT,
                ThanhTichNoiBat = hoSo.ThiSinh?.ThanhTichNoiBat,
                ThiSinhHoatDong = hoSo.ThiSinh?.TrangThai ?? false,

                // Ngành tuyển sinh
                MaNganh = hoSo.MaNganh,
                TenNganh = hoSo.NganhTuyenSinh?.TenNganh ?? string.Empty,
                TenKhoa = hoSo.NganhTuyenSinh?.Khoa?.TenKhoa ?? string.Empty,
                ChiTieu = hoSo.NganhTuyenSinh?.ChiTieu ?? 0,
                ToHopYeuCauNganh = hoSo.NganhTuyenSinh?.ToHopXetTuyen ?? string.Empty,
                DiemXetTuyenToiThieu = hoSo.NganhTuyenSinh?.DiemXetTuyenToiThieu ?? 0,
                HanNopHoSo = hoSo.NganhTuyenSinh?.HanNopHoSo ?? DateTime.MinValue,
                TrangThaiNganh = hoSo.NganhTuyenSinh?.TrangThai ?? string.Empty,
                MoTaNganh = hoSo.NganhTuyenSinh?.MoTaNganh,
                YeuCauThiSinh = hoSo.NganhTuyenSinh?.YeuCauThiSinh
            };

            // M4.3: Đối chiếu tổ hợp xét tuyển (chỉ mang tính tham khảo)
            if (!string.IsNullOrWhiteSpace(vm.ToHopThiSinh) && !string.IsNullOrWhiteSpace(vm.ToHopYeuCauNganh))
            {
                var dsToHopYeuCau = vm.ToHopYeuCauNganh
                    .Split(new[] { ',', ';', '/' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToUpperInvariant())
                    .ToList();

                var toHopThiSinhChuan = vm.ToHopThiSinh.Trim().ToUpperInvariant();
                vm.ToHopPhuHop = dsToHopYeuCau.Contains(toHopThiSinhChuan);

                vm.ThongTinSoSanhToHop = vm.ToHopPhuHop.Value
                    ? $"Tổ hợp của thí sinh ({toHopThiSinhChuan}) trùng khớp với danh mục tổ hợp ngành chấp nhận ({vm.ToHopYeuCauNganh})."
                    : $"Tổ hợp của thí sinh ({toHopThiSinhChuan}) không nằm trong danh mục tổ hợp ngành tuyển ({vm.ToHopYeuCauNganh}).";
            }
            else
            {
                vm.ToHopPhuHop = null;
                vm.ThongTinSoSanhToHop = "Chưa đủ dữ liệu tổ hợp để so sánh đối chiếu.";
            }

            // M4.3: Đối chiếu điểm thi THPT với điểm sàn (chỉ mang tính tham khảo)
            if (vm.DiemThiTHPT.HasValue)
            {
                var diff = vm.DiemThiTHPT.Value - vm.DiemXetTuyenToiThieu;
                vm.DiemDatChuanToiThieu = diff >= 0;

                vm.ThongTinSoSanhDiem = vm.DiemDatChuanToiThieu.Value
                    ? $"Điểm thi THPT của thí sinh ({vm.DiemThiTHPT.Value:F2}) đạt chuẩn tối thiểu của ngành ({vm.DiemXetTuyenToiThieu:F2}) [Cao hơn: +{diff:F2} điểm]."
                    : $"Điểm thi THPT của thí sinh ({vm.DiemThiTHPT.Value:F2}) thấp hơn điểm sàn tối thiểu ({vm.DiemXetTuyenToiThieu:F2}) [Thiếu: {Math.Abs(diff):F2} điểm].";
            }
            else
            {
                vm.DiemDatChuanToiThieu = null;
                vm.ThongTinSoSanhDiem = "Thí sinh chưa cập nhật điểm thi THPT.";
            }

            // M4.4: Kiểm tra 4 điều kiện nghiệp vụ
            vm.DieuKien1_HoSoTonTaiVaChuaHuy = hoSo != null && !string.Equals(hoSo.TrangThai, TrangThaiHoSo.DaHuy, StringComparison.OrdinalIgnoreCase);
            vm.DieuKien2_NganhTonTai = hoSo.NganhTuyenSinh != null;
            vm.DieuKien3_ThiSinhHoatDong = hoSo.ThiSinh != null && hoSo.ThiSinh.TrangThai;
            vm.DieuKien4_ChuaSoTuyen = string.Equals(hoSo.TrangThai, TrangThaiHoSo.ChoDuyet, StringComparison.OrdinalIgnoreCase);

            if (!vm.DieuKien1_HoSoTonTaiVaChuaHuy)
                vm.DanhSachLoiDieuKien.Add("Điều kiện 1 vi phạm: Hồ sơ không tồn tại hoặc đã bị hủy.");
            if (!vm.DieuKien2_NganhTonTai)
                vm.DanhSachLoiDieuKien.Add("Điều kiện 2 vi phạm: Ngành tuyển sinh liên quan không tồn tại trong hệ thống.");
            if (!vm.DieuKien3_ThiSinhHoatDong)
                vm.DanhSachLoiDieuKien.Add("Điều kiện 3 vi phạm: Tài khoản thí sinh đang bị khóa hoặc không ở trạng thái hoạt động.");
            if (!vm.DieuKien4_ChuaSoTuyen)
                vm.DanhSachLoiDieuKien.Add($"Điều kiện 4 vi phạm: Hồ sơ đã được sơ tuyển hoặc chuyển trạng thái trước đó (Hiện tại: \"{hoSo.TrangThai}\").");

            return vm;
        }

        public async Task<(bool HopLe, List<string> DanhSachLoi)> KiemTraDieuKienSoTuyenAsync(int maHoSo)
        {
            var danhSachLoi = new List<string>();
            var hoSo = await _hoSoRepository.GetByIdWithDetailsAsync(maHoSo);

            // Điều kiện 1: Hồ sơ tồn tại và chưa bị hủy
            if (hoSo == null)
            {
                danhSachLoi.Add("Hồ sơ xét tuyển không tồn tại trong hệ thống.");
                return (false, danhSachLoi);
            }

            if (string.Equals(hoSo.TrangThai, TrangThaiHoSo.DaHuy, StringComparison.OrdinalIgnoreCase))
            {
                danhSachLoi.Add("Hồ sơ xét tuyển đã bị hủy, không thể thực hiện sơ tuyển.");
            }

            // Điều kiện 2: Ngành tuyển sinh liên quan tồn tại
            if (hoSo.NganhTuyenSinh == null)
            {
                danhSachLoi.Add("Ngành tuyển sinh đăng ký xét tuyển không tồn tại.");
            }

            // Điều kiện 3: Thí sinh còn hoạt động
            if (hoSo.ThiSinh == null || !hoSo.ThiSinh.TrangThai)
            {
                danhSachLoi.Add("Thí sinh nộp hồ sơ hiện không còn hoạt động hoặc hồ sơ thí sinh đã bị khóa.");
            }

            // Điều kiện 4: Hồ sơ chưa có kết quả sơ tuyển trước đó (phải ở trạng thái Chờ duyệt)
            if (!string.Equals(hoSo.TrangThai, TrangThaiHoSo.ChoDuyet, StringComparison.OrdinalIgnoreCase))
            {
                danhSachLoi.Add($"Hồ sơ đã qua vòng sơ tuyển hoặc đang ở trạng thái \"{hoSo.TrangThai}\", không thể sơ tuyển lại.");
            }

            return (danhSachLoi.Count == 0, danhSachLoi);
        }

        public async Task<(bool ThanhCong, string ThongBao)> XuLySoTuyenAsync(int maHoSo, string ketQua, string nhanXet)
        {
            // Kiểm tra tính hợp lệ của kết quả nhập vào
            if (string.IsNullOrWhiteSpace(ketQua) ||
                (!string.Equals(ketQua, TrangThaiHoSo.DatSoTuyen, StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(ketQua, TrangThaiHoSo.KhongDatSoTuyen, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "Kết quả sơ tuyển không hợp lệ. Chỉ chấp nhận \"Đạt sơ tuyển\" hoặc \"Không đạt sơ tuyển\".");
            }

            if (string.IsNullOrWhiteSpace(nhanXet))
            {
                return (false, "Vui lòng nhập nhận xét / lý do đánh giá sơ tuyển.");
            }

            // M4.4: Kiểm tra đầy đủ 4 điều kiện
            var (hopLe, danhSachLoi) = await KiemTraDieuKienSoTuyenAsync(maHoSo);
            if (!hopLe)
            {
                return (false, string.Join(" ", danhSachLoi));
            }

            var hoSo = await _hoSoRepository.GetByIdAsync(maHoSo);
            if (hoSo == null)
            {
                return (false, $"Không tìm thấy hồ sơ #{maHoSo}.");
            }

            // M4.2 & M4.5: Kiểm tra ma trận chuyển trạng thái
            var (choPhep, lyDo) = _hoSoTrangThaiService.CoTheSoTuyen(hoSo.TrangThai, ketQua);
            if (!choPhep)
            {
                return (false, lyDo ?? "Không đủ điều kiện để chuyển trạng thái sơ tuyển.");
            }

            // M4.5: Ghi nhận trạng thái mới, NgayXuLy = DateTime.Now và NhanXetSoTuyen
            hoSo.TrangThai = ketQua;
            hoSo.NgayXuLy = DateTime.Now;
            hoSo.NhanXetSoTuyen = nhanXet.Trim();

            await _hoSoRepository.UpdateAsync(hoSo);
            await _hoSoRepository.SaveChangesAsync();

            return (true, $"Đã cập nhật sơ tuyển hồ sơ #{maHoSo} thành công: \"{ketQua}\".");
        }

        public async Task<(int SoLuongTrungTuyen, int ChiTieu, bool DaDatChiTieu, string CanhBao)> KiemTraCanhBaoChiTieuAsync(int maNganh)
        {
            var countTrungTuyen = await _hoSoRepository.CountTrungTuyenByNganhAsync(maNganh);
            var nganh = await _hoSoRepository.GetNganhByIdAsync(maNganh);
            var chiTieu = nganh?.ChiTieu ?? 0;
            var daDat = chiTieu > 0 && countTrungTuyen >= chiTieu;

            var canhBao = daDat
                ? $"CẢNH BÁO: Ngành \"{nganh?.TenNganh}\" đã đạt hoặc vượt chỉ tiêu tuyển sinh ({countTrungTuyen}/{chiTieu})."
                : $"Chỉ tiêu ngành \"{nganh?.TenNganh}\": Hiện có {countTrungTuyen}/{chiTieu} thí sinh trúng tuyển.";

            return (countTrungTuyen, chiTieu, daDat, canhBao);
        }

        public async Task<(bool ThanhCong, string ThongBao)> DongNganhTuyenSinhAsync(int maNganh)
        {
            var nganh = await _hoSoRepository.GetNganhByIdAsync(maNganh);
            if (nganh == null)
            {
                return (false, "Không tìm thấy ngành tuyển sinh tương ứng.");
            }

            if (string.Equals(nganh.TrangThai, TrangThaiNganh.DaDong, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Ngành tuyển sinh này đã được đóng trước đó.");
            }

            await _hoSoRepository.DongNganhTuyenSinhAsync(maNganh);

            return (true, $"Đã đóng thành công ngành \"{nganh.TenNganh}\". Tất cả các hồ sơ hiện có vẫn được bảo toàn nguyên vẹn.");
        }
    }
}
