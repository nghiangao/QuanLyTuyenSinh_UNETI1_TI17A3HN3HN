// Họ và tên: Đinh Văn Thành
// Mã sinh viên: 23103100180
// Nội dung thực hiện: Tài Khoản, Đăng Nhập, Phân Quyền, Quản Lý Khoa.

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.ViewModels;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Controllers
{
    /// <summary>Quản lý tài khoản + phân quyền (chỉ Admin).</summary>
    [Authorize(Roles = VaiTro.Admin)]
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<TaiKhoan> _hasher = new();

        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: TaiKhoan?search=&vaiTro=
        public async Task<IActionResult> Index(string? search, string? vaiTro)
        {
            var query = _context.TaiKhoans.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(t => t.TenDangNhap.Contains(search)
                                      || t.HoTen.Contains(search)
                                      || t.Email.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(vaiTro))
                query = query.Where(t => t.VaiTro == vaiTro);

            ViewBag.Search = search;
            ViewBag.VaiTro = vaiTro;

            return View(await query.OrderBy(t => t.TenDangNhap).ToListAsync());
        }

        // GET: TaiKhoan/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tk = await _context.TaiKhoans
                .Include(t => t.ThiSinh)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

            return tk == null ? NotFound() : View(tk);
        }

        // GET: TaiKhoan/Create
        public IActionResult Create()
        {
            return View(new TaiKhoan());
        }

        // POST: TaiKhoan/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TenDangNhap,MatKhau,HoTen,Email,VaiTro,TrangThai")] TaiKhoan tk)
        {
            tk.TenDangNhap = tk.TenDangNhap?.Trim() ?? string.Empty;

            if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == tk.TenDangNhap))
                ModelState.AddModelError(nameof(tk.TenDangNhap), "Tên đăng nhập đã tồn tại");

            if (tk.MatKhau?.Length < 6)
                ModelState.AddModelError(nameof(tk.MatKhau), "Mật khẩu tối thiểu 6 ký tự");

            if (!ModelState.IsValid) return View(tk);

            tk.MatKhau = _hasher.HashPassword(tk, tk.MatKhau);
            _context.Add(tk);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm tài khoản thành công";
            return RedirectToAction(nameof(Index));
        }

        // GET: TaiKhoan/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tk = await _context.TaiKhoans.FindAsync(id);
            return tk == null ? NotFound() : View(tk);
        }

        // POST: TaiKhoan/Edit/5  (không sửa mật khẩu ở đây -> dùng DatLaiMatKhau)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("MaTaiKhoan,HoTen,Email,VaiTro,TrangThai")] TaiKhoan model)
        {
            if (id != model.MaTaiKhoan) return NotFound();

            // Các trường không bind -> bỏ khỏi validation
            ModelState.Remove(nameof(TaiKhoan.TenDangNhap));
            ModelState.Remove(nameof(TaiKhoan.MatKhau));

            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk == null) return NotFound();

            // Không cho admin tự hạ quyền / tự khóa chính mình
            if (tk.MaTaiKhoan == CurrentUserId && (model.VaiTro != VaiTro.Admin || !model.TrangThai))
                ModelState.AddModelError(string.Empty, "Bạn không thể tự hạ quyền hoặc tự khóa tài khoản của mình");

            if (!ModelState.IsValid)
            {
                model.TenDangNhap = tk.TenDangNhap;
                return View(model);
            }

            tk.HoTen = model.HoTen.Trim();
            tk.Email = model.Email.Trim();
            tk.VaiTro = model.VaiTro;
            tk.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật tài khoản thành công";
            return RedirectToAction(nameof(Index));
        }

        // GET: TaiKhoan/DatLaiMatKhau/5
        public async Task<IActionResult> DatLaiMatKhau(int? id)
        {
            if (id == null) return NotFound();

            var tk = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (tk == null) return NotFound();

            return View(new DatLaiMatKhauViewModel { MaTaiKhoan = tk.MaTaiKhoan, TenDangNhap = tk.TenDangNhap });
        }

        // POST: TaiKhoan/DatLaiMatKhau
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DatLaiMatKhau(DatLaiMatKhauViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var tk = await _context.TaiKhoans.FindAsync(model.MaTaiKhoan);
            if (tk == null) return NotFound();

            tk.MatKhau = _hasher.HashPassword(tk, model.MatKhauMoi);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã đặt lại mật khẩu cho \"{tk.TenDangNhap}\"";
            return RedirectToAction(nameof(Index));
        }

        // POST: TaiKhoan/DoiTrangThai/5  (khóa / mở khóa)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            if (id == CurrentUserId)
            {
                TempData["Error"] = "Bạn không thể khóa tài khoản của chính mình";
                return RedirectToAction(nameof(Index));
            }

            var tk = await _context.TaiKhoans.FindAsync(id);
            if (tk == null) return NotFound();

            tk.TrangThai = !tk.TrangThai;
            await _context.SaveChangesAsync();

            TempData["Success"] = tk.TrangThai ? "Đã mở khóa tài khoản" : "Đã khóa tài khoản";
            return RedirectToAction(nameof(Index));
        }

        // GET: TaiKhoan/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tk = await _context.TaiKhoans
                .Include(t => t.ThiSinh)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

            return tk == null ? NotFound() : View(tk);
        }

        // POST: TaiKhoan/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id == CurrentUserId)
            {
                TempData["Error"] = "Bạn không thể xóa tài khoản của chính mình";
                return RedirectToAction(nameof(Index));
            }

            var tk = await _context.TaiKhoans
                .Include(t => t.ThiSinh)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (tk == null) return NotFound();

            // Đã có hồ sơ thí sinh -> không xóa cứng, chỉ khóa để giữ toàn vẹn dữ liệu
            if (tk.ThiSinh != null)
            {
                tk.TrangThai = false;
                await _context.SaveChangesAsync();
                TempData["Error"] = "Tài khoản đã có hồ sơ thí sinh nên không thể xóa, hệ thống đã khóa tài khoản này";
                return RedirectToAction(nameof(Index));
            }

            _context.TaiKhoans.Remove(tk);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa tài khoản thành công";
            return RedirectToAction(nameof(Index));
        }
    }
}