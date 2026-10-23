using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Controllers;

public class TaiKhoanController : Controller
{
    private readonly ApplicationDbContext _context;

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
}
