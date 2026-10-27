
// Họ và tên: Giang Mạnh Tường
// Mã sinh viên: 23103100155
// Module 3: M3.1 - Hồ sơ cá nhân; M3.2 - Danh sách và chi tiết thí sinh.
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;


namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Controllers
{
    public class ThiSinhController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThiSinhController(ApplicationDbContext context)
        {
            _context = context;
        }

        // M3.1: Hiển thị hồ sơ cá nhân của thí sinh đang đăng nhập.
        [Authorize(Roles = VaiTro.ThiSinh)]
        [HttpGet]
        public async Task<IActionResult> ThongTinCaNhan()
        {
            int? maTaiKhoan = LayMaTaiKhoanDangNhap();

            if (maTaiKhoan == null)
                return Forbid();

            var thiSinh = await _context.ThiSinhs
                .FirstOrDefaultAsync(ts => ts.MaTaiKhoan == maTaiKhoan.Value);

            if (thiSinh == null)
            {
                var taiKhoan = await _context.TaiKhoans
                    .FindAsync(maTaiKhoan.Value);

                if (taiKhoan == null)
                    return Forbid();

                // Tạo dữ liệu ban đầu để hiển thị biểu mẫu.
                // Chưa lưu vào database cho đến khi người dùng gửi form.
                thiSinh = new ThiSinh
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    HoTen = taiKhoan.HoTen,
                    Email = taiKhoan.Email,
                    NgaySinh = DateTime.Today.AddYears(-18),
                    GioiTinh = GioiTinhConst.Nam,
                    TrangThai = true
                };
            }

            return View(thiSinh);
        }

        // M3.1: Lưu thông tin cá nhân.
        [Authorize(Roles = VaiTro.ThiSinh)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThongTinCaNhan(
            [Bind("HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,ToHopXetTuyen,TruongTHPT,DiemThiTHPT,ThanhTichNoiBat")]
            ThiSinh input)
        {
            int? maTaiKhoan = LayMaTaiKhoanDangNhap();

            if (maTaiKhoan == null)
                return Forbid();

            if (input.NgaySinh.Date >= DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(input.NgaySinh),
                    "Ngày sinh phải trước ngày hiện tại.");
            }

            var taiKhoan = await _context.TaiKhoans
                .FindAsync(maTaiKhoan.Value);

            if (taiKhoan == null || taiKhoan.VaiTro != VaiTro.ThiSinh)
                return Forbid();

            // Tìm hồ sơ theo tài khoản đang đăng nhập.
            // Không nhận MaThiSinh hoặc MaTaiKhoan từ form.
            var thiSinh = await _context.ThiSinhs
                .FirstOrDefaultAsync(ts => ts.MaTaiKhoan == maTaiKhoan.Value);

            if (!ModelState.IsValid)
                return View(input);

            if (thiSinh == null)
            {
                thiSinh = new ThiSinh
                {
                    MaTaiKhoan = maTaiKhoan.Value,
                    TrangThai = true
                };

                _context.ThiSinhs.Add(thiSinh);
            }

            // Chỉ cập nhật các thuộc tính được phép.
            thiSinh.HoTen = input.HoTen.Trim();
            thiSinh.NgaySinh = input.NgaySinh;
            thiSinh.GioiTinh = input.GioiTinh;
            thiSinh.SoDienThoai = input.SoDienThoai?.Trim();
            thiSinh.Email = input.Email?.Trim();
            thiSinh.DiaChi = input.DiaChi?.Trim();
            thiSinh.ToHopXetTuyen = input.ToHopXetTuyen?.Trim();
            thiSinh.TruongTHPT = input.TruongTHPT?.Trim();
            thiSinh.DiemThiTHPT = input.DiemThiTHPT;
            thiSinh.ThanhTichNoiBat = input.ThanhTichNoiBat?.Trim();

            // Đồng bộ họ tên với tài khoản.
            taiKhoan.HoTen = thiSinh.HoTen;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Đã lưu hồ sơ cá nhân thành công.";

            return RedirectToAction(nameof(ThongTinCaNhan));
        }

        // M3.2: Danh sách thí sinh dành cho Admin/Cán bộ tuyển sinh.
        [Authorize(Roles = VaiTro.Admin + "," + VaiTro.CanBoTuyenSinh)]
        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.ThiSinhs
                .AsNoTracking()
                .Include(ts => ts.TaiKhoan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(ts =>
                    ts.HoTen.Contains(search)
                    || (ts.Email != null && ts.Email.Contains(search))
                    || (ts.SoDienThoai != null
                        && ts.SoDienThoai.Contains(search))
                    || (ts.TaiKhoan != null
                        && ts.TaiKhoan.TenDangNhap.Contains(search)));
            }

            ViewBag.Search = search;

            var danhSach = await query
                .OrderBy(ts => ts.HoTen)
                .ToListAsync();

            return View(danhSach);
        }

        // M3.2: Chi tiết một thí sinh.
        [Authorize(Roles = VaiTro.Admin + "," + VaiTro.CanBoTuyenSinh)]
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var thiSinh = await _context.ThiSinhs
                .AsNoTracking()
                .Include(ts => ts.TaiKhoan)
                .FirstOrDefaultAsync(ts => ts.MaThiSinh == id.Value);

            if (thiSinh == null)
                return NotFound();

            return View(thiSinh);
        }

        // Lấy mã tài khoản từ thông tin đăng nhập.
        private int? LayMaTaiKhoanDangNhap()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(value, out int id) ? id : null;
        }
    }
}
