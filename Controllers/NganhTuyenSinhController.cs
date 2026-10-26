// Họ và tên: Đào Văn Khánh
// Mã sinh viên: 23103100125
// Đã thực hiện: M2.1-M2.6 CRUD ngành, kiểm tra dữ liệu/trạng thái, tìm kiếm,
// lọc kết hợp, sắp xếp và phân trang; M2.7 danh mục công khai, chi tiết ngành
// và hiển thị tình trạng nhận hồ sơ.
// Cần làm ở M3.3 để hoàn tất M2.7: kiểm tra phía máy chủ khi nộp hồ sơ rằng
// thí sinh đã đăng nhập và còn hoạt động; ngành đang tuyển, đã đến ngày bắt đầu
// và chưa quá hạn; dữ liệu bắt buộc, điểm THPT và tổ hợp hợp lệ; không có hồ sơ
// đang xử lý trùng ngành. Chỉ tạo hồ sơ khi mọi điều kiện đều đạt.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Controllers;

public class NganhTuyenSinhController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    public NganhTuyenSinhController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Danh sách dành cho cán bộ/admin: hỗ trợ kết hợp tìm kiếm, lọc, sắp xếp và phân trang.
    [Authorize(Roles = VaiTro.Admin + "," + VaiTro.CanBoTuyenSinh)]
    public async Task<IActionResult> Index(
        string? search,
        int? maKhoa,
        string? toHop,
        string? trangThai,
        decimal? diemToiThieu,
        string? tinhTrangNhanHoSo,
        string? sort,
        int page = 1,
        int pageSize = DefaultPageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var today = DateTime.Today;
        var query = _context.NganhTuyenSinhs
            .Include(n => n.Khoa)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(n => n.TenNganh.Contains(search)
                || (n.Khoa != null && n.Khoa.TenKhoa.Contains(search)));
        }

        if (maKhoa.HasValue)
            query = query.Where(n => n.MaKhoa == maKhoa.Value);

        if (!string.IsNullOrWhiteSpace(toHop))
        {
            toHop = toHop.Trim();
            query = query.Where(n => n.ToHopXetTuyen.Contains(toHop));
        }

        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(n => n.TrangThai == trangThai);

        if (diemToiThieu.HasValue)
            query = query.Where(n => n.DiemXetTuyenToiThieu >= diemToiThieu.Value);

        if (tinhTrangNhanHoSo == "conHan")
        {
            query = query.Where(n => n.TrangThai == TrangThaiNganh.DangTuyen
                && today >= n.NgayBatDauNhanHoSo.Date
                && today <= n.HanNopHoSo.Date);
        }
        else if (tinhTrangNhanHoSo == "hetHan")
        {
            query = query.Where(n => today > n.HanNopHoSo.Date);
        }

        query = sort switch
        {
            "ten_desc" => query.OrderByDescending(n => n.TenNganh),
            "han_asc" => query.OrderBy(n => n.HanNopHoSo).ThenBy(n => n.TenNganh),
            "han_desc" => query.OrderByDescending(n => n.HanNopHoSo).ThenBy(n => n.TenNganh),
            "chiTieu_asc" => query.OrderBy(n => n.ChiTieu).ThenBy(n => n.TenNganh),
            "chiTieu_desc" => query.OrderByDescending(n => n.ChiTieu).ThenBy(n => n.TenNganh),
            _ => query.OrderBy(n => n.TenNganh)
        };

        var totalItems = await query.CountAsync();
        var totalPages = Math.Max((int)Math.Ceiling(totalItems / (double)pageSize), 1);
        page = Math.Min(page, totalPages);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Search = search;
        ViewBag.MaKhoa = maKhoa;
        ViewBag.ToHop = toHop;
        ViewBag.TrangThai = trangThai;
        ViewBag.DiemToiThieu = diemToiThieu;
        ViewBag.TinhTrangNhanHoSo = tinhTrangNhanHoSo;
        ViewBag.Sort = sort;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;
        ViewBag.Khoas = new SelectList(await _context.Khoas.AsNoTracking().OrderBy(k => k.TenKhoa).ToListAsync(), "MaKhoa", "TenKhoa", maKhoa);

        return View(items);
    }

    // Danh sách công khai để Module 3 dùng khi thí sinh chọn ngành nộp hồ sơ.
    [AllowAnonymous]
    public async Task<IActionResult> CongKhai(
        string? search,
        int? maKhoa,
        string? toHop,
        string? sort,
        int page = 1,
        int pageSize = DefaultPageSize)
    {
        var query = _context.NganhTuyenSinhs
            .Include(n => n.Khoa)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(n => n.TenNganh.Contains(search)
                || (n.Khoa != null && n.Khoa.TenKhoa.Contains(search)));
        }

        if (maKhoa.HasValue)
            query = query.Where(n => n.MaKhoa == maKhoa.Value);

        if (!string.IsNullOrWhiteSpace(toHop))
        {
            toHop = toHop.Trim();
            query = query.Where(n => n.ToHopXetTuyen.Contains(toHop));
        }

        query = sort switch
        {
            "ten_desc" => query.OrderByDescending(n => n.TenNganh).ThenBy(n => n.MaNganh),
            "han_asc" => query.OrderBy(n => n.HanNopHoSo).ThenBy(n => n.TenNganh),
            "han_desc" => query.OrderByDescending(n => n.HanNopHoSo).ThenBy(n => n.TenNganh),
            "chiTieu_asc" => query.OrderBy(n => n.ChiTieu).ThenBy(n => n.TenNganh),
            "chiTieu_desc" => query.OrderByDescending(n => n.ChiTieu).ThenBy(n => n.TenNganh),
            _ => query.OrderBy(n => n.TenNganh).ThenBy(n => n.MaNganh)
        };

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var totalItems = await query.CountAsync();
        var totalPages = Math.Max((int)Math.Ceiling(totalItems / (double)pageSize), 1);
        page = Math.Min(page, totalPages);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Search = search;
        ViewBag.MaKhoa = maKhoa;
        ViewBag.ToHop = toHop;
        ViewBag.Sort = sort;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;
        ViewBag.Khoas = new SelectList(await _context.Khoas.AsNoTracking().OrderBy(k => k.TenKhoa).ToListAsync(), "MaKhoa", "TenKhoa", maKhoa);

        return View("CongKhai", items);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var nganh = await _context.NganhTuyenSinhs
            .Include(n => n.Khoa)
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.MaNganh == id.Value);

        if (nganh != null)
            ViewBag.DangNhanHoSo = nganh.DangNhanHoSo;

        return nganh == null ? NotFound() : View(nganh);
    }

    [Authorize(Roles = VaiTro.Admin + "," + VaiTro.CanBoTuyenSinh)]
    public async Task<IActionResult> Create()
    {
        await NapDanhSachKhoaAsync();
        return View(new NganhTuyenSinh());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = VaiTro.Admin + "," + VaiTro.CanBoTuyenSinh)]
    public async Task<IActionResult> Create([Bind("TenNganh,MaKhoa,ChiTieu,ToHopXetTuyen,DiemXetTuyenToiThieu,NgayBatDauNhanHoSo,HanNopHoSo,MoTaNganh,YeuCauThiSinh,TrangThai")] NganhTuyenSinh nganh)
    {
        await ValidateNganhAsync(nganh);
        if (!ModelState.IsValid)
        {
            await NapDanhSachKhoaAsync(nganh.MaKhoa);
            return View(nganh);
        }

        _context.NganhTuyenSinhs.Add(nganh);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Thêm ngành tuyển sinh thành công.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = VaiTro.Admin + "," + VaiTro.CanBoTuyenSinh)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var nganh = await _context.NganhTuyenSinhs.FindAsync(id.Value);
        if (nganh == null) return NotFound();
        await NapDanhSachKhoaAsync(nganh.MaKhoa);
        return View(nganh);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = VaiTro.Admin + "," + VaiTro.CanBoTuyenSinh)]
    public async Task<IActionResult> Edit(int id, [Bind("MaNganh,TenNganh,MaKhoa,ChiTieu,ToHopXetTuyen,DiemXetTuyenToiThieu,NgayBatDauNhanHoSo,HanNopHoSo,MoTaNganh,YeuCauThiSinh,TrangThai")] NganhTuyenSinh nganh)
    {
        if (id != nganh.MaNganh) return NotFound();
        await ValidateNganhAsync(nganh);
        if (!ModelState.IsValid)
        {
            await NapDanhSachKhoaAsync(nganh.MaKhoa);
            return View(nganh);
        }

        try
        {
            _context.Update(nganh);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.NganhTuyenSinhs.AnyAsync(n => n.MaNganh == id)) return NotFound();
            throw;
        }

        TempData["Success"] = "Cập nhật ngành tuyển sinh thành công.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = VaiTro.Admin + "," + VaiTro.CanBoTuyenSinh)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var nganh = await _context.NganhTuyenSinhs
            .Include(n => n.Khoa)
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.MaNganh == id.Value);

        return nganh == null ? NotFound() : View(nganh);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = VaiTro.Admin + "," + VaiTro.CanBoTuyenSinh)]
    public async Task<IActionResult> Delete(int id)
    {
        var nganh = await _context.NganhTuyenSinhs
            .Include(n => n.HoSoXetTuyens)
            .FirstOrDefaultAsync(n => n.MaNganh == id);
        if (nganh == null) return NotFound();

        if (nganh.HoSoXetTuyens.Count > 0)
        {
            TempData["Error"] = "Ngành đã có hồ sơ xét tuyển nên không thể xóa để bảo toàn lịch sử. Hãy chuyển trạng thái sang Đã đóng.";
            return RedirectToAction(nameof(Index));
        }

        _context.NganhTuyenSinhs.Remove(nganh);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Xóa ngành tuyển sinh thành công.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateNganhAsync(NganhTuyenSinh nganh)
    {
        var trangThaiHopLe = nganh.TrangThai == TrangThaiNganh.ChuaMo
            || nganh.TrangThai == TrangThaiNganh.DangTuyen
            || nganh.TrangThai == TrangThaiNganh.TamDung
            || nganh.TrangThai == TrangThaiNganh.DaDong;

        if (!trangThaiHopLe)
            ModelState.AddModelError(nameof(nganh.TrangThai), "Trạng thái ngành không hợp lệ.");

        if (!await _context.Khoas.AnyAsync(k => k.MaKhoa == nganh.MaKhoa && k.TrangThai))
            ModelState.AddModelError(nameof(nganh.MaKhoa), "Khoa không tồn tại hoặc đã ngừng hoạt động.");

        if (nganh.TrangThai == TrangThaiNganh.DangTuyen
            && (string.IsNullOrWhiteSpace(nganh.TenNganh)
                || string.IsNullOrWhiteSpace(nganh.ToHopXetTuyen)
                || nganh.ChiTieu <= 0
                || nganh.DiemXetTuyenToiThieu < 0
                || nganh.DiemXetTuyenToiThieu > 30
                || nganh.HanNopHoSo.Date < nganh.NgayBatDauNhanHoSo.Date))
        {
            ModelState.AddModelError(nameof(nganh.TrangThai), "Không thể chuyển sang Đang tuyển khi dữ liệu ngành chưa hợp lệ.");
        }
    }

    private async Task NapDanhSachKhoaAsync(int? selectedId = null)
    {
        ViewBag.Khoas = new SelectList(
            await _context.Khoas.AsNoTracking().Where(k => k.TrangThai).OrderBy(k => k.TenKhoa).ToListAsync(),
            "MaKhoa", "TenKhoa", selectedId);
    }
}
