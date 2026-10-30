// Họ và tên: Giang Mạnh Tường
// Mã sinh viên: 23103100155
// Nội dung thực hiện: Module 3.3 - Nộp hồ sơ,
// M3.4 - Hủy hồ sơ, M3.5 - Theo dõi hồ sơ.

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Controllers
{
    [Authorize(Roles = VaiTro.ThiSinh)]
    public class HoSoThiSinhController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HoSoThiSinhController(ApplicationDbContext context)
        {
            _context = context;
        }

        // M3.3: Hiển thị biểu mẫu nộp hồ sơ.
        [HttpGet]
        public async Task<IActionResult> NopHoSo(int maNganh)
        {
            var thiSinh = await LayThiSinhDangNhapAsync();

            if (thiSinh == null)
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy hồ sơ thí sinh đang hoạt động. Vui lòng kiểm tra thông tin cá nhân.";
                return RedirectToAction("ThongTinCaNhan", "ThiSinh");
            }

            var nganh = await _context.NganhTuyenSinhs
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.MaNganh == maNganh);

            if (nganh == null)
                return NotFound("Không tìm thấy ngành tuyển sinh.");

            if (!DangNhanHoSo(nganh))
            {
                TempData["ErrorMessage"] =
                    "Ngành này hiện không trong thời gian nhận hồ sơ.";
                return RedirectToAction(
                    "Details", "NganhTuyenSinh",
                    new { id = maNganh });
            }

            ViewBag.MaNganh = nganh.MaNganh;
            ViewBag.TenNganh = nganh.TenNganh;

            return View(new HoSoXetTuyen());
        }

        // M3.3: Kiểm tra điều kiện và lưu hồ sơ.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NopHoSo(
            int maNganh,
            [Bind("NguyenVongXetTuyen,GhiChu")]
            HoSoXetTuyen input)
        {
            var thiSinh = await LayThiSinhDangNhapAsync();

            if (thiSinh == null)
            {
                TempData["ErrorMessage"] =
                    "Tài khoản hoặc hồ sơ thí sinh không còn hoạt động.";
                return RedirectToAction("ThongTinCaNhan", "ThiSinh");
            }

            var nganh = await _context.NganhTuyenSinhs
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.MaNganh == maNganh);

            if (nganh == null)
                return NotFound("Không tìm thấy ngành tuyển sinh.");

            if (!DangNhanHoSo(nganh))
            {
                TempData["ErrorMessage"] =
                    "Ngành không còn nhận hồ sơ hoặc đã hết hạn.";
                return RedirectToAction(
                    "Details", "NganhTuyenSinh",
                    new { id = maNganh });
            }

            // Kiểm tra thông tin bắt buộc trong hồ sơ cá nhân.
            if (string.IsNullOrWhiteSpace(thiSinh.HoTen))
                ModelState.AddModelError("", "Bạn chưa khai báo họ tên.");

            if (thiSinh.NgaySinh.Date >= DateTime.Today)
                ModelState.AddModelError("", "Ngày sinh chưa hợp lệ.");

            if (string.IsNullOrWhiteSpace(thiSinh.Email))
                ModelState.AddModelError("", "Bạn cần khai báo email.");

            if (string.IsNullOrWhiteSpace(thiSinh.SoDienThoai))
                ModelState.AddModelError("", "Bạn cần khai báo số điện thoại.");

            if (!thiSinh.DiemThiTHPT.HasValue)
                ModelState.AddModelError("", "Bạn cần khai báo điểm thi THPT.");

            if (string.IsNullOrWhiteSpace(thiSinh.ToHopXetTuyen))
                ModelState.AddModelError("", "Bạn cần khai báo tổ hợp xét tuyển.");

            // Không cho nộp hai hồ sơ đang xử lý cùng một ngành.
            var trangThaiDangXuLy = new[]
            {
                TrangThaiHoSo.ChoDuyet,
                TrangThaiHoSo.DatSoTuyen,
                TrangThaiHoSo.ChoPhongVan,
                TrangThaiHoSo.DaPhongVan
            };

            bool biTrung = await _context.HoSoXetTuyens.AnyAsync(h =>
                h.MaThiSinh == thiSinh.MaThiSinh
                && h.MaNganh == maNganh
                && trangThaiDangXuLy.Contains(h.TrangThai));

            if (biTrung)
            {
                ModelState.AddModelError(
                    "",
                    "Bạn đã có hồ sơ đang xử lý cho ngành này.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MaNganh = nganh.MaNganh;
                ViewBag.TenNganh = nganh.TenNganh;
                return View(input);
            }

            // Các trường quan trọng do hệ thống tự gán.
            var hoSo = new HoSoXetTuyen
            {
                MaThiSinh = thiSinh.MaThiSinh,
                MaNganh = nganh.MaNganh,
                NgayNop = DateTime.Now,
                NguyenVongXetTuyen = input.NguyenVongXetTuyen.Trim(),
                GhiChu = input.GhiChu?.Trim(),
                TrangThai = TrangThaiHoSo.ChoDuyet
            };

            _context.HoSoXetTuyens.Add(hoSo);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Nộp hồ sơ xét tuyển thành công. Hồ sơ đang chờ duyệt.";

            return RedirectToAction(nameof(TheoDoi));
        }

        // M3.4: Thí sinh hủy hồ sơ của chính mình.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            var thiSinh = await LayThiSinhDangNhapAsync();

            if (thiSinh == null)
                return Forbid();

            var hoSo = await _context.HoSoXetTuyens
                .Include(h => h.LichPhongVans)
                .FirstOrDefaultAsync(h =>
                    h.MaHoSo == id
                    && h.MaThiSinh == thiSinh.MaThiSinh);

            // Không tìm thấy hồ sơ thuộc chính thí sinh này.
            if (hoSo == null)
                return NotFound();

            bool coLichPhongVan = hoSo.LichPhongVans.Any(l =>
                l.TrangThai != TrangThaiLich.DaHuy);

            bool duocHuy =
                hoSo.TrangThai == TrangThaiHoSo.ChoDuyet
                || (hoSo.TrangThai == TrangThaiHoSo.DatSoTuyen
                    && !coLichPhongVan);

            if (!duocHuy)
            {
                TempData["ErrorMessage"] =
                    "Không thể hủy hồ sơ ở trạng thái hiện tại hoặc hồ sơ đã có lịch phỏng vấn.";

                return RedirectToAction(nameof(ChiTiet), new { id });
            }

            hoSo.TrangThai = TrangThaiHoSo.DaHuy;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Đã hủy hồ sơ xét tuyển.";

            return RedirectToAction(nameof(TheoDoi));
        }

        // M3.5: Danh sách hồ sơ của thí sinh đang đăng nhập.
        [HttpGet]
        public async Task<IActionResult> TheoDoi()
        {
            var thiSinh = await LayThiSinhDangNhapAsync();

            if (thiSinh == null)
                return Forbid();

            var danhSach = await _context.HoSoXetTuyens
                .AsNoTracking()
                .Where(h => h.MaThiSinh == thiSinh.MaThiSinh)
                .Include(h => h.NganhTuyenSinh)
                    .ThenInclude(n => n!.Khoa)
                .Include(h => h.LichPhongVans)
                .Include(h => h.KetQuaXetTuyen)
                .OrderByDescending(h => h.NgayNop)
                .ToListAsync();

            return View(danhSach);
        }

        // M3.5: Chi tiết hồ sơ thuộc thí sinh đang đăng nhập.
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var thiSinh = await LayThiSinhDangNhapAsync();

            if (thiSinh == null)
                return Forbid();

            var hoSo = await _context.HoSoXetTuyens
                .AsNoTracking()
                .Where(h =>
                    h.MaHoSo == id
                    && h.MaThiSinh == thiSinh.MaThiSinh)
                .Include(h => h.NganhTuyenSinh)
                    .ThenInclude(n => n!.Khoa)
                .Include(h => h.LichPhongVans)
                .Include(h => h.KetQuaXetTuyen)
                .FirstOrDefaultAsync();

            if (hoSo == null)
                return NotFound();

            return View(hoSo);
        }

        // Lấy hồ sơ theo tài khoản đang đăng nhập, không nhận mã thí sinh từ URL.
        private async Task<ThiSinh?> LayThiSinhDangNhapAsync()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(value, out int maTaiKhoan))
                return null;

            var thiSinh = await _context.ThiSinhs
                .Include(ts => ts.TaiKhoan)
                .FirstOrDefaultAsync(ts => ts.MaTaiKhoan == maTaiKhoan);

            if (thiSinh == null
                || !thiSinh.TrangThai
                || thiSinh.TaiKhoan == null
                || !thiSinh.TaiKhoan.TrangThai
                || thiSinh.TaiKhoan.VaiTro != VaiTro.ThiSinh)
            {
                return null;
            }

            return thiSinh;
        }

        private static bool DangNhanHoSo(NganhTuyenSinh nganh)
        {
            var homNay = DateTime.Today;

            return nganh.TrangThai == TrangThaiNganh.DangTuyen
                && homNay >= nganh.NgayBatDauNhanHoSo.Date
                && homNay <= nganh.HanNopHoSo.Date;
        }
    }
}