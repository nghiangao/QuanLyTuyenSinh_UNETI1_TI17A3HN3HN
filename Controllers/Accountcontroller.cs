// Họ và tên: Đào Văn Khánh
// Mã sinh viên: 23103100125
// Nội dung thực hiện: Quản lý ngành tuyển sinh, Tìm kiếm, Lọc, Sắp xếp và Phân trang.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Helpers;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Controllers
{
    /// <summary>
    /// Quản lý ngành tuyển sinh.
    /// - Index / Details: ai cũng xem được (người chưa đăng nhập và thí sinh chỉ thấy ngành đang tuyển).
    /// - Thêm / Sửa / Xóa / Đổi trạng thái: chỉ Admin.
    /// </summary>
    [Authorize(Roles = VaiTro.Admin)]
    public class NganhTuyenSinhController : Controller
    {
        private const int DefaultPageSize = 10;
        private static readonly int[] AllowedPageSizes = { 5, 10, 20, 50 };

        private readonly ApplicationDbContext _context;

        public NganhTuyenSinhController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================================
        // INDEX: tìm kiếm + lọc + sắp xếp + phân trang
        // GET: NganhTuyenSinh?search=&maKhoa=&trangThai=&dangNhanHoSo=&sortOrder=&page=&pageSize=
        // =====================================================================
        [AllowAnonymous]
        public async Task<IActionResult> Index(
            string? search,
            int? maKhoa,
            string? trangThai,
            bool? dangNhanHoSo,
            string? sortOrder,
            int page = 1,
            int pageSize = DefaultPageSize)
        {
            if (!AllowedPageSizes.Contains(pageSize)) pageSize = DefaultPageSize;

            var isAdmin = User.IsInRole(VaiTro.Admin);
            var today = DateTime.Today;   // dùng biến cục bộ để EF dịch được sang SQL

            var query = _context.NganhTuyenSinhs
                .Include(n => n.Khoa)
                .AsNoTracking()
                .AsQueryable();

            // Người không phải Admin chỉ xem ngành đang tuyển
            if (!isAdmin)
            {
                query = query.Where(n => n.TrangThai == TrangThaiNganh.DangTuyen);
            }
            else if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(n => n.TrangThai == trangThai);
            }

            // ----- Tìm kiếm -----
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(n => n.TenNganh.Contains(search)
                                      || n.ToHopXetTuyen.Contains(search)
                                      || (n.Khoa != null && n.Khoa.TenKhoa.Contains(search)));
            }

            // ----- Lọc theo khoa -----
            if (maKhoa.HasValue)
                query = query.Where(n => n.MaKhoa == maKhoa.Value);

            // ----- Lọc "đang nhận hồ sơ" -----
            // KHÔNG dùng n.DangNhanHoSo (NotMapped) trong LINQ to EF -> viết điều kiện đầy đủ
            if (dangNhanHoSo == true)
            {
                query = query.Where(n => n.TrangThai == TrangThaiNganh.DangTuyen
                                      && n.NgayBatDauNhanHoSo <= today
                                      && n.HanNopHoSo >= today);
            }

            // ----- Sắp xếp -----
            query = sortOrder switch
            {
                "ten_desc"     => query.OrderByDescending(n => n.TenNganh),
                "khoa"         => query.OrderBy(n => n.Khoa!.TenKhoa).ThenBy(n => n.TenNganh),
                "khoa_desc"    => query.OrderByDescending(n => n.Khoa!.TenKhoa).ThenBy(n => n.TenNganh),
                "chitieu"      => query.OrderBy(n => n.ChiTieu).ThenBy(n => n.TenNganh),
                "chitieu_desc" => query.OrderByDescending(n => n.ChiTieu).ThenBy(n => n.TenNganh),
                "diem"         => query.OrderBy(n => n.DiemXetTuyenToiThieu).ThenBy(n => n.TenNganh),
                "diem_desc"    => query.OrderByDescending(n => n.DiemXetTuyenToiThieu).ThenBy(n => n.TenNganh),
                "han"          => query.OrderBy(n => n.HanNopHoSo).ThenBy(n => n.TenNganh),
                "han_desc"     => query.OrderByDescending(n => n.HanNopHoSo).ThenBy(n => n.TenNganh),
                _              => query.OrderBy(n => n.TenNganh)   // mặc định: tên A-Z
            };

            // ----- Dữ liệu cho bộ lọc trên View -----
            ViewData["Search"] = search;
            ViewData["MaKhoa"] = maKhoa;
            ViewData["TrangThai"] = trangThai;
            ViewData["DangNhanHoSo"] = dangNhanHoSo;
            ViewData["SortOrder"] = sortOrder;
            ViewData["PageSize"] = pageSize;
            ViewData["PageSizes"] = AllowedPageSizes;

            // Đảo chiều sắp xếp cho tiêu đề cột (bấm lần nữa để đảo)
            ViewData["TenSort"]     = sortOrder == "ten_desc" ? "" : "ten_desc";
            ViewData["KhoaSort"]    = sortOrder == "khoa" ? "khoa_desc" : "khoa";
            ViewData["ChiTieuSort"] = sortOrder == "chitieu" ? "chitieu_desc" : "chitieu";
            ViewData["DiemSort"]    = sortOrder == "diem" ? "diem_desc" : "diem";
            ViewData["HanSort"]     = sortOrder == "han" ? "han_desc" : "han";

            await LoadFilterListsAsync(maKhoa, trangThai);

            var model = await PaginatedList<NganhTuyenSinh>.CreateAsync(query, page, pageSize);
            return View(model);
        }

        // =====================================================================
        // DETAILS
        // =====================================================================
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var nganh = await _context.NganhTuyenSinhs
                .Include(n => n.Khoa)
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.MaNganh == id);

            if (nganh == null) return NotFound();

            // Ngành chưa mở chỉ Admin được xem
            if (!User.IsInRole(VaiTro.Admin) && nganh.TrangThai != TrangThaiNganh.DangTuyen)
                return NotFound();

            if (User.IsInRole(VaiTro.Admin))
            {
                ViewBag.SoHoSo = await _context.NganhTuyenSinhs
                    .Where(n => n.MaNganh == id)
                    .SelectMany(n => n.HoSoXetTuyens)
                    .CountAsync();
            }

            return View(nganh);
        }

        // =====================================================================
        // CREATE
        // =====================================================================
        public async Task<IActionResult> Create()
        {
            await LoadKhoaSelectListAsync();
            return View(new NganhTuyenSinh());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TenNganh,MaKhoa,ChiTieu,ToHopXetTuyen,DiemXetTuyenToiThieu," +
                  "NgayBatDauNhanHoSo,HanNopHoSo,MoTaNganh,YeuCauThiSinh,TrangThai")]
            NganhTuyenSinh nganh)
        {
            nganh.TenNganh = nganh.TenNganh?.Trim() ?? string.Empty;
            nganh.ToHopXetTuyen = nganh.ToHopXetTuyen?.Trim() ?? string.Empty;

            await KiemTraNghiepVuAsync(nganh);

            if (!ModelState.IsValid)
            {
                await LoadKhoaSelectListAsync(nganh.MaKhoa);
                return View(nganh);
            }

            _context.Add(nganh);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm ngành tuyển sinh thành công";
            return RedirectToAction(nameof(Index));
        }

        // =====================================================================
        // EDIT
        // =====================================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var nganh = await _context.NganhTuyenSinhs.FindAsync(id);
            if (nganh == null) return NotFound();

            await LoadKhoaSelectListAsync(nganh.MaKhoa);
            return View(nganh);
        }

        [HttpPost, ActionName("Edit"), ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null) return NotFound();

            var nganh = await _context.NganhTuyenSinhs.FirstOrDefaultAsync(n => n.MaNganh == id);
            if (nganh == null) return NotFound();

            // Chỉ cập nhật các trường cho phép (chống overposting, không đụng vào HoSoXetTuyens/Khoa)
            var ok = await TryUpdateModelAsync(nganh, "",
                n => n.TenNganh, n => n.MaKhoa, n => n.ChiTieu, n => n.ToHopXetTuyen,
                n => n.DiemXetTuyenToiThieu, n => n.NgayBatDauNhanHoSo, n => n.HanNopHoSo,
                n => n.MoTaNganh, n => n.YeuCauThiSinh, n => n.TrangThai);

            nganh.TenNganh = nganh.TenNganh?.Trim() ?? string.Empty;
            nganh.ToHopXetTuyen = nganh.ToHopXetTuyen?.Trim() ?? string.Empty;

            await KiemTraNghiepVuAsync(nganh);

            if (!ok || !ModelState.IsValid)
            {
                await LoadKhoaSelectListAsync(nganh.MaKhoa);
                return View(nganh);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.NganhTuyenSinhs.AnyAsync(n => n.MaNganh == id)) return NotFound();
                throw;
            }

            TempData["Success"] = "Cập nhật ngành tuyển sinh thành công";
            return RedirectToAction(nameof(Index));
        }

        // =====================================================================
        // ĐỔI TRẠNG THÁI nhanh: Chưa mở <-> Đang tuyển
        // =====================================================================
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id, string? returnUrl = null)
        {
            var nganh = await _context.NganhTuyenSinhs.FindAsync(id);
            if (nganh == null) return NotFound();

            nganh.TrangThai = nganh.TrangThai == TrangThaiNganh.DangTuyen
                ? TrangThaiNganh.ChuaMo
                : TrangThaiNganh.DangTuyen;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Ngành \"{nganh.TenNganh}\" chuyển sang trạng thái: {nganh.TrangThai}";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        // =====================================================================
        // DELETE
        // =====================================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var nganh = await _context.NganhTuyenSinhs
                .Include(n => n.Khoa)
                .Include(n => n.HoSoXetTuyens)
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.MaNganh == id);

            return nganh == null ? NotFound() : View(nganh);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nganh = await _context.NganhTuyenSinhs
                .Include(n => n.HoSoXetTuyens)
                .FirstOrDefaultAsync(n => n.MaNganh == id);
            if (nganh == null) return NotFound();

            // Đã có hồ sơ xét tuyển -> không xóa để giữ toàn vẹn dữ liệu
            if (nganh.HoSoXetTuyens.Any())
            {
                TempData["Error"] = $"Ngành \"{nganh.TenNganh}\" đã có {nganh.HoSoXetTuyens.Count} hồ sơ xét tuyển nên không thể xóa. " +
                                    "Bạn có thể chuyển ngành về trạng thái chưa mở.";
                return RedirectToAction(nameof(Index));
            }

            _context.NganhTuyenSinhs.Remove(nganh);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa ngành tuyển sinh thành công";
            return RedirectToAction(nameof(Index));
        }

        // =====================================================================
        // HỖ TRỢ
        // =====================================================================

        /// <summary>Kiểm tra khoa hợp lệ + không trùng tên ngành trong cùng khoa.</summary>
        private async Task KiemTraNghiepVuAsync(NganhTuyenSinh nganh)
        {
            // Navigation "Khoa" không đến từ form -> bỏ khỏi validation
            ModelState.Remove(nameof(NganhTuyenSinh.Khoa));

            var khoa = await _context.Khoas.AsNoTracking()
                .FirstOrDefaultAsync(k => k.MaKhoa == nganh.MaKhoa);

            if (khoa == null)
                ModelState.AddModelError(nameof(nganh.MaKhoa), "Vui lòng chọn khoa hợp lệ");

            var trung = await _context.NganhTuyenSinhs.AnyAsync(n =>
                n.MaNganh != nganh.MaNganh
                && n.MaKhoa == nganh.MaKhoa
                && n.TenNganh == nganh.TenNganh);

            if (trung)
                ModelState.AddModelError(nameof(nganh.TenNganh), "Khoa này đã có ngành cùng tên");
        }

        /// <summary>Dropdown khoa cho form Thêm/Sửa (chỉ khoa đang hoạt động + khoa hiện tại của ngành).</summary>
        private async Task LoadKhoaSelectListAsync(int? selected = null)
        {
            var khoas = await _context.Khoas.AsNoTracking()
                .Where(k => k.TrangThai || k.MaKhoa == selected)
                .OrderBy(k => k.TenKhoa)
                .ToListAsync();

            ViewBag.MaKhoa = new SelectList(khoas, nameof(Khoa.MaKhoa), nameof(Khoa.TenKhoa), selected);
        }

        /// <summary>Dropdown lọc trên trang Index.</summary>
        private async Task LoadFilterListsAsync(int? maKhoa, string? trangThai)
        {
            var khoas = await _context.Khoas.AsNoTracking()
                .OrderBy(k => k.TenKhoa)
                .ToListAsync();
            ViewBag.DsKhoa = new SelectList(khoas, nameof(Khoa.MaKhoa), nameof(Khoa.TenKhoa), maKhoa);

            // Lấy danh sách trạng thái đang có trong DB -> không phụ thuộc vào hằng số nào
            var dsTrangThai = await _context.NganhTuyenSinhs.AsNoTracking()
                .Select(n => n.TrangThai)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
            ViewBag.DsTrangThai = new SelectList(dsTrangThai, trangThai);
        }
    }
}