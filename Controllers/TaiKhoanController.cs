// Họ và tên: Đinh Văn Thành
// Mã sinh viên: 23103100180
// Nội dung thực hiện: Tài Khoản, Đăng Nhập, Phân Quyền, Quản Lý Khoa.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Controllers;

public class TaiKhoanController : Controller
{
    private readonly ApplicationDbContext _context;
    private static readonly string[] VaiTroHopLe =
    {
        VaiTro.Admin,
        VaiTro.CanBoTuyenSinh,
        VaiTro.ThiSinh
    };

    public TaiKhoanController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult DangNhap(string? returnUrl = null)
    {
        return View(new DangNhapViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(DangNhapViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var tenDangNhap = model.TenDangNhap.Trim();
        var taiKhoan = await _context.TaiKhoans
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);

        if (taiKhoan == null)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản không tồn tại.");
            return View(model);
        }

        if (!taiKhoan.TrangThai)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa.");
            return View(model);
        }

        if (!string.Equals(taiKhoan.MatKhau, model.MatKhau, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "Mật khẩu không chính xác.");
            return View(model);
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, taiKhoan.MaTaiKhoan.ToString()),
            new Claim(ClaimTypes.Name, taiKhoan.HoTen),
            new Claim(ClaimTypes.Role, taiKhoan.VaiTro)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32(nameof(SessionKeys.MaTaiKhoan), taiKhoan.MaTaiKhoan);
        HttpContext.Session.SetString(nameof(SessionKeys.HoTen), taiKhoan.HoTen);
        HttpContext.Session.SetString(nameof(SessionKeys.VaiTro), taiKhoan.VaiTro);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangXuat()
    {
        HttpContext.Session.Clear();
        await HttpContext.Session.CommitAsync();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [Authorize(Roles = VaiTro.Admin)]
    public async Task<IActionResult> Index(string? search, string? vaiTro, bool? trangThai)
    {
        var query = _context.TaiKhoans.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(t => t.TenDangNhap.Contains(search)
                || t.HoTen.Contains(search)
                || t.Email.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(vaiTro) && VaiTroHopLe.Contains(vaiTro, StringComparer.Ordinal))
            query = query.Where(t => t.VaiTro == vaiTro);

        if (trangThai.HasValue)
            query = query.Where(t => t.TrangThai == trangThai.Value);

        ViewBag.Search = search;
        ViewBag.VaiTro = vaiTro;
        ViewBag.TrangThai = trangThai;

        var taiKhoans = await query
            .OrderBy(t => t.TenDangNhap)
            .Select(t => new TaiKhoanListItemViewModel
            {
                MaTaiKhoan = t.MaTaiKhoan,
                TenDangNhap = t.TenDangNhap,
                HoTen = t.HoTen,
                Email = t.Email,
                VaiTro = t.VaiTro,
                TrangThai = t.TrangThai
            })
            .ToListAsync();

        return View(taiKhoans);
    }

    [Authorize(Roles = VaiTro.Admin)]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var account = await _context.TaiKhoans
            .AsNoTracking()
            .Where(t => t.MaTaiKhoan == id.Value)
            .Select(t => new TaiKhoanFormViewModel
            {
                MaTaiKhoan = t.MaTaiKhoan,
                TenDangNhap = t.TenDangNhap,
                HoTen = t.HoTen,
                Email = t.Email,
                VaiTro = t.VaiTro,
                TrangThai = t.TrangThai
            })
            .FirstOrDefaultAsync();

        return account == null ? NotFound() : View(account);
    }

    [Authorize(Roles = VaiTro.Admin)]
    public IActionResult Create()
    {
        GanDanhSachVaiTro(VaiTro.ThiSinh);
        return View(new TaiKhoanFormViewModel());
    }

    [Authorize(Roles = VaiTro.Admin)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaiKhoanFormViewModel model)
    {
        model.TenDangNhap = model.TenDangNhap?.Trim() ?? string.Empty;
        model.HoTen = model.HoTen?.Trim() ?? string.Empty;
        model.Email = model.Email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(model.MatKhau))
            ModelState.AddModelError(nameof(model.MatKhau), "Mật khẩu là bắt buộc");

        if (!VaiTroHopLe.Contains(model.VaiTro, StringComparer.Ordinal))
            ModelState.AddModelError(nameof(model.VaiTro), "Vai trò không hợp lệ");

        if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap))
            ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại");

        if (!ModelState.IsValid)
        {
            GanDanhSachVaiTro(model.VaiTro);
            return View(model);
        }

        _context.TaiKhoans.Add(new TaiKhoan
        {
            TenDangNhap = model.TenDangNhap,
            MatKhau = model.MatKhau!,
            HoTen = model.HoTen,
            Email = model.Email,
            VaiTro = model.VaiTro,
            TrangThai = model.TrangThai
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = "Thêm tài khoản thành công.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = VaiTro.Admin)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var account = await _context.TaiKhoans
            .AsNoTracking()
            .Where(t => t.MaTaiKhoan == id.Value)
            .Select(t => new TaiKhoanFormViewModel
            {
                MaTaiKhoan = t.MaTaiKhoan,
                TenDangNhap = t.TenDangNhap,
                HoTen = t.HoTen,
                Email = t.Email,
                VaiTro = t.VaiTro,
                TrangThai = t.TrangThai
            })
            .FirstOrDefaultAsync();

        if (account == null)
            return NotFound();

        GanDanhSachVaiTro(account.VaiTro);
        return View(account);
    }

    [Authorize(Roles = VaiTro.Admin)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TaiKhoanFormViewModel model)
    {
        if (id != model.MaTaiKhoan)
            return NotFound();

        model.TenDangNhap = model.TenDangNhap?.Trim() ?? string.Empty;
        model.HoTen = model.HoTen?.Trim() ?? string.Empty;
        model.Email = model.Email?.Trim() ?? string.Empty;

        if (!VaiTroHopLe.Contains(model.VaiTro, StringComparer.Ordinal))
            ModelState.AddModelError(nameof(model.VaiTro), "Vai trò không hợp lệ");

        if (await _context.TaiKhoans.AnyAsync(t =>
            t.TenDangNhap == model.TenDangNhap && t.MaTaiKhoan != id))
            ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại");

        var currentAccountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentAccountId == id.ToString()
            && (!model.TrangThai || model.VaiTro != VaiTro.Admin))
        {
            ModelState.AddModelError(string.Empty, "Không thể khóa hoặc hạ quyền tài khoản đang đăng nhập.");
        }

        if (!ModelState.IsValid)
        {
            GanDanhSachVaiTro(model.VaiTro);
            return View(model);
        }

        var account = await _context.TaiKhoans.FindAsync(id);
        if (account == null)
            return NotFound();

        account.TenDangNhap = model.TenDangNhap;
        account.HoTen = model.HoTen;
        account.Email = model.Email;
        account.VaiTro = model.VaiTro;
        account.TrangThai = model.TrangThai;
        if (!string.IsNullOrWhiteSpace(model.MatKhau))
            account.MatKhau = model.MatKhau;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Cập nhật tài khoản thành công.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = VaiTro.Admin)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var account = await _context.TaiKhoans
            .AsNoTracking()
            .Where(t => t.MaTaiKhoan == id.Value)
            .Select(t => new TaiKhoanFormViewModel
            {
                MaTaiKhoan = t.MaTaiKhoan,
                TenDangNhap = t.TenDangNhap,
                HoTen = t.HoTen,
                Email = t.Email,
                VaiTro = t.VaiTro,
                TrangThai = t.TrangThai
            })
            .FirstOrDefaultAsync();

        return account == null ? NotFound() : View(account);
    }

    [Authorize(Roles = VaiTro.Admin)]
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var account = await _context.TaiKhoans
            .Include(t => t.ThiSinh)
            .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
        if (account == null)
            return NotFound();

        if (User.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString())
        {
            TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index));
        }

        if (account.ThiSinh != null)
        {
            TempData["Error"] = "Không thể xóa tài khoản đã gắn với hồ sơ thí sinh để bảo toàn dữ liệu.";
            return RedirectToAction(nameof(Index));
        }

        _context.TaiKhoans.Remove(account);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Xóa tài khoản thành công.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = VaiTro.Admin)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiTrangThai(int id)
    {
        var account = await _context.TaiKhoans.FindAsync(id);
        if (account == null)
            return NotFound();

        if (User.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString())
        {
            TempData["Error"] = "Không thể khóa tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index));
        }

        account.TrangThai = !account.TrangThai;
        await _context.SaveChangesAsync();

        TempData["Success"] = account.TrangThai
            ? "Đã mở khóa tài khoản."
            : "Đã khóa tài khoản.";
        return RedirectToAction(nameof(Index));
    }

    private void GanDanhSachVaiTro(string? selectedRole)
    {
        ViewBag.VaiTroList = TaoDanhSachVaiTro(selectedRole);
    }

    private static SelectList TaoDanhSachVaiTro(string? selectedRole)
    {
        return new SelectList(VaiTroHopLe.Select(role => new { Value = role, Text = role }), "Value", "Text", selectedRole);
    }
}
