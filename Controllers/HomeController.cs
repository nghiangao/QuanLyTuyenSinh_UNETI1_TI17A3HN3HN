// Họ và tên: Mã Đức Thịnh
// Mã sinh viên: 23103100314
// Nội dung thực hiện: Controller Trang chủ hệ thống - Tự động nhận diện Session / Vai trò người dùng và phân luồng giao diện (Admin, Cán bộ tuyển sinh, Thí sinh, Khách).

using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================================================================
    // GET: / hoặc /Home hoặc /Home/Index
    // =========================================================================
    public async Task<IActionResult> Index()
    {
        var vm = new HomeDashboardViewModel();

        // 1. Kiểm tra trạng thái đăng nhập qua Claims và Session
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        var sessionMaTaiKhoan = HttpContext.Session.GetInt32(nameof(SessionKeys.MaTaiKhoan));
        var sessionVaiTro = HttpContext.Session.GetString(nameof(SessionKeys.VaiTro));
        var sessionHoTen = HttpContext.Session.GetString(nameof(SessionKeys.HoTen));

        int? maTaiKhoan = sessionMaTaiKhoan;
        string? vaiTro = sessionVaiTro;
        string? hoTen = sessionHoTen;

        // Nếu có Cookie Auth nhưng Session bị mất, khôi phục lại Session từ Claims
        if (isAuthenticated)
        {
            if (!maTaiKhoan.HasValue)
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(idStr, out int parsedId))
                {
                    maTaiKhoan = parsedId;
                    HttpContext.Session.SetInt32(nameof(SessionKeys.MaTaiKhoan), parsedId);
                }
            }

            if (string.IsNullOrEmpty(vaiTro))
            {
                vaiTro = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
                HttpContext.Session.SetString(nameof(SessionKeys.VaiTro), vaiTro);
            }

            if (string.IsNullOrEmpty(hoTen))
            {
                hoTen = User.Identity?.Name ?? string.Empty;
                HttpContext.Session.SetString(nameof(SessionKeys.HoTen), hoTen);
            }
        }

        vm.IsAuthenticated = isAuthenticated || maTaiKhoan.HasValue;
        vm.MaTaiKhoan = maTaiKhoan;
        vm.VaiTro = vaiTro ?? string.Empty;
        vm.HoTen = hoTen ?? string.Empty;

        // =====================================================================
        // PHÂN CHIA DỮ LIỆU THEO VAI TRÒ (SESSION / ROLE)
        // =====================================================================

        if (!vm.IsAuthenticated)
        {
            // --- TRƯỜNG HỢP 1: KHÁCH VÃNG LAI (CHƯA ĐĂNG NHẬP) ---
            var today = DateTime.Today;
            vm.NganhDangTuyen = await _context.NganhTuyenSinhs
                .Include(n => n.Khoa)
                .AsNoTracking()
                .Where(n => n.TrangThai == TrangThaiNganh.DangTuyen &&
                            today >= n.NgayBatDauNhanHoSo.Date &&
                            today <= n.HanNopHoSo.Date)
                .OrderBy(n => n.TenNganh)
                .Take(6)
                .ToListAsync();

            vm.TongSoNganhDangTuyen = await _context.NganhTuyenSinhs
                .CountAsync(n => n.TrangThai == TrangThaiNganh.DangTuyen);

            vm.TongSoKhoa = await _context.Khoas.CountAsync(k => k.TrangThai);
        }
        else if (vm.VaiTro == VaiTro.Admin || vm.VaiTro == VaiTro.CanBoTuyenSinh)
        {
            // --- TRƯỜNG HỢP 2: ADMIN HOẶC CÁN BỘ TUYỂN SINH ---
            var now = DateTime.Now;

            vm.TongSoThiSinh = await _context.ThiSinhs.CountAsync();
            vm.TongSoHoSo = await _context.HoSoXetTuyens.CountAsync();
            vm.SoHoSoChoDuyet = await _context.HoSoXetTuyens.CountAsync(h => h.TrangThai == TrangThaiHoSo.ChoDuyet);
            vm.SoHoSoDatSoTuyen = await _context.HoSoXetTuyens.CountAsync(h => h.TrangThai == TrangThaiHoSo.DatSoTuyen);
            vm.SoLichPhongVanSapToi = await _context.LichPhongVans.CountAsync(l => l.TrangThai == TrangThaiLich.DaLenLich && l.ThoiGianBatDau >= now);
            vm.SoHoSoTrungTuyen = await _context.HoSoXetTuyens.CountAsync(h => h.TrangThai == TrangThaiHoSo.TrungTuyen);
            vm.TongSoKhoa = await _context.Khoas.CountAsync();
            vm.TongSoNganhDangTuyen = await _context.NganhTuyenSinhs.CountAsync(n => n.TrangThai == TrangThaiNganh.DangTuyen);
        }
        else if (vm.VaiTro == VaiTro.ThiSinh)
        {
            // --- TRƯỜNG HỢP 3: THÍ SINH ĐÃ ĐĂNG NHẬP ---
            if (maTaiKhoan.HasValue)
            {
                var thiSinh = await _context.ThiSinhs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan.Value);

                vm.ThongTinThiSinh = thiSinh;

                if (thiSinh != null)
                {
                    vm.DanhSachHoSoCuaToi = await _context.HoSoXetTuyens
                        .AsNoTracking()
                        .Where(h => h.MaThiSinh == thiSinh.MaThiSinh)
                        .Include(h => h.NganhTuyenSinh)
                            .ThenInclude(n => n!.Khoa)
                        .Include(h => h.LichPhongVans)
                            .ThenInclude(l => l.NguoiPhongVan)
                        .Include(h => h.KetQuaXetTuyen)
                        .OrderByDescending(h => h.NgayNop)
                        .ToListAsync();
                }
            }
        }

        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

