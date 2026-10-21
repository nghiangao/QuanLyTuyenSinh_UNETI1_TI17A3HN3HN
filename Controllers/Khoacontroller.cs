// Họ và tên: Đinh Văn Thành
// Mã sinh viên: 23103100180
// Nội dung thực hiện: Tài Khoản, Đăng Nhập, Phân Quyền, Quản Lý Khoa.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Controllers
{
    /// <summary>Quản lý Khoa (chỉ Admin).</summary>
    [Authorize(Roles = VaiTro.Admin)]
    public class KhoaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KhoaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Khoa?search=&trangThai=
        public async Task<IActionResult> Index(string? search, bool? trangThai)
        {
            var query = _context.Khoas
                .Include(k => k.NganhTuyenSinhs)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(k => k.TenKhoa.Contains(search)
                                      || (k.EmailLienHe != null && k.EmailLienHe.Contains(search)));
            }

            if (trangThai.HasValue)
                query = query.Where(k => k.TrangThai == trangThai.Value);

            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;

            return View(await query.OrderBy(k => k.TenKhoa).ToListAsync());
        }

        // GET: Khoa/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var khoa = await _context.Khoas
                .Include(k => k.NganhTuyenSinhs)
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.MaKhoa == id);

            return khoa == null ? NotFound() : View(khoa);
        }

        // GET: Khoa/Create
        public IActionResult Create() => View(new Khoa());

        // POST: Khoa/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TenKhoa,MoTa,EmailLienHe,TrangThai")] Khoa khoa)
        {
            khoa.TenKhoa = khoa.TenKhoa?.Trim() ?? string.Empty;

            if (await _context.Khoas.AnyAsync(k => k.TenKhoa == khoa.TenKhoa))
                ModelState.AddModelError(nameof(khoa.TenKhoa), "Tên khoa đã tồn tại");

            if (!ModelState.IsValid) return View(khoa);

            _context.Add(khoa);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm khoa thành công";
            return RedirectToAction(nameof(Index));
        }

        // GET: Khoa/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var khoa = await _context.Khoas.FindAsync(id);
            return khoa == null ? NotFound() : View(khoa);
        }

        // POST: Khoa/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("MaKhoa,TenKhoa,MoTa,EmailLienHe,TrangThai")] Khoa khoa)
        {
            if (id != khoa.MaKhoa) return NotFound();

            khoa.TenKhoa = khoa.TenKhoa?.Trim() ?? string.Empty;

            if (await _context.Khoas.AnyAsync(k => k.TenKhoa == khoa.TenKhoa && k.MaKhoa != khoa.MaKhoa))
                ModelState.AddModelError(nameof(khoa.TenKhoa), "Tên khoa đã tồn tại");

            if (!ModelState.IsValid) return View(khoa);

            try
            {
                _context.Update(khoa);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Khoas.AnyAsync(k => k.MaKhoa == id)) return NotFound();
                throw;
            }

            TempData["Success"] = "Cập nhật khoa thành công";
            return RedirectToAction(nameof(Index));
        }

        // POST: Khoa/DoiTrangThai/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var khoa = await _context.Khoas.FindAsync(id);
            if (khoa == null) return NotFound();

            khoa.TrangThai = !khoa.TrangThai;
            await _context.SaveChangesAsync();

            TempData["Success"] = khoa.TrangThai ? "Đã kích hoạt khoa" : "Đã ngừng hoạt động khoa";
            return RedirectToAction(nameof(Index));
        }

        // GET: Khoa/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var khoa = await _context.Khoas
                .Include(k => k.NganhTuyenSinhs)
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.MaKhoa == id);

            return khoa == null ? NotFound() : View(khoa);
        }

        // POST: Khoa/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var khoa = await _context.Khoas
                .Include(k => k.NganhTuyenSinhs)
                .FirstOrDefaultAsync(k => k.MaKhoa == id);
            if (khoa == null) return NotFound();

            // Khoa còn ngành tuyển sinh -> không cho xóa
            if (khoa.NganhTuyenSinhs.Any())
            {
                TempData["Error"] = $"Khoa \"{khoa.TenKhoa}\" đang có {khoa.NganhTuyenSinhs.Count} ngành tuyển sinh, không thể xóa. Bạn có thể chuyển sang trạng thái ngừng hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            _context.Khoas.Remove(khoa);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa khoa thành công";
            return RedirectToAction(nameof(Index));
        }
    }
}