// Họ và tên: Mã Đức Thịnh
// Mã sinh viên: 23103100314
// Nội dung thực hiện: MODULE 5 - LỊCH PHỎNG VẤN, KẾT QUẢ XÉT TUYỂN, DASHBOARD VÀ THỐNG KÊ.
//
// Phân chia nhiệm vụ và liên kết các Module:
//   - [Module 1 - TaiKhoan/Khoa]: Kiểm tra quyền Admin/Cán bộ tuyển sinh tại Controller;
//                                 Sử dụng dữ liệu Khoa và Tài khoản để xác thực và thống kê.
//   - [Module 2 - NganhTuyenSinh]: Hiển thị tên ngành, chỉ tiêu; Thống kê ngành theo khoa, ngành nhiều hồ sơ nhất.
//   - [Module 3 - ThiSinh/HoSoXetTuyen]: Lấy hồ sơ của thí sinh; Kiểm tra trùng lịch thí sinh;
//                                        Cung cấp chức năng cho thí sinh xem lịch & kết quả của chính mình (M5.4).
//   - [Module 4 - SoTuyen]: Chỉ những hồ sơ ở trạng thái "Đạt sơ tuyển" mới được lập lịch phỏng vấn.
//   - [Module 5 - LichPhongVan/KetQuaXetTuyen]: Quản lý lịch phỏng vấn, giải thuật kiểm tra trùng khoảng thời gian,
//                                              ghi nhận điểm đánh giá (0-100) và kết quả xét tuyển.
//   - [Đề mục 9.5 & 9.6]: Tính toán 9 chỉ số Dashboard và 10 báo cáo thống kê bằng LINQ.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Controllers
{
    public class LichPhongVanController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Hình thức phỏng vấn hợp lệ (Đề mục 9.1 & form lập lịch)
        private static readonly string[] HinhThucHopLe = ["Trực tiếp", "Trực tuyến"];

        public LichPhongVanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // M5.1: DANH SÁCH LỊCH PHỎNG VẤN (Cán bộ / Admin)
        // =========================================================================
        // GET: LichPhongVan?trangThai=...
        public async Task<IActionResult> Index(string? trangThai)
        {
            // [Module 1]: Kiểm tra quyền hạn (nếu dự án bật session auth thủ công)
            // if (!KiemTraQuyenCanBo()) return RedirectToAction("DangNhap", "TaiKhoan");

            var query = _context.LichPhongVans
                .Include(l => l.HoSoXetTuyen)
                    .ThenInclude(h => h!.ThiSinh)          // [Module 3]: tên thí sinh
                .Include(l => l.HoSoXetTuyen)
                    .ThenInclude(h => h!.NganhTuyenSinh)   // [Module 2]: tên ngành
                .Include(l => l.NguoiPhongVan)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(l => l.TrangThai == trangThai);
            }

            ViewBag.TrangThai = trangThai;
            var danhSach = await query.OrderByDescending(l => l.ThoiGianBatDau).ToListAsync();
            return View(danhSach);
        }

        // =========================================================================
        // M5.1: LẬP LỊCH PHỎNG VẤN (Create)
        // =========================================================================
        // GET: LichPhongVan/Create?maHoSo=...
        public async Task<IActionResult> Create(int? maHoSo)
        {
            var thoiGianBatDau = DateTime.Today.AddDays(1).AddHours(8);
            var lich = new LichPhongVan
            {
                MaHoSo = maHoSo ?? 0,
                ThoiGianBatDau = thoiGianBatDau,
                ThoiGianKetThuc = thoiGianBatDau.AddMinutes(45),
                HinhThucPhongVan = "Trực tiếp"
            };

            await NapDropdownAsync(lich);
            return View(lich);
        }

        // POST: LichPhongVan/Create
        // Bảo mật Binding: TrangThai không cho người dùng gửi qua form để tránh gian lận trạng thái
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaHoSo,ThoiGianBatDau,ThoiGianKetThuc,HinhThucPhongVan,DiaDiemHoacLienKet,MaNguoiPhongVan,GhiChu")]
            LichPhongVan lich)
        {
            lich.TrangThai = TrangThaiLich.DaLenLich; // Hệ thống tự gán

            // [Module 4 & Module 5]: Kiểm tra các ràng buộc nghiệp vụ
            var hoSo = await KiemTraNghiepVuAsync(lich);

            if (!ModelState.IsValid || hoSo == null)
            {
                await NapDropdownAsync(lich);
                return View(lich);
            }

            // [Module 3 & 4]: Khi lập lịch thành công, hồ sơ chuyển từ "Đạt sơ tuyển" sang "Chờ phỏng vấn"
            hoSo.TrangThai = TrangThaiHoSo.ChoPhongVan;
            _context.LichPhongVans.Add(lich);

            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Đã lập lịch phỏng vấn thành công!";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // M5.2: HỦY LỊCH PHỎNG VẤN
        // =========================================================================
        // POST: LichPhongVan/Huy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            var lich = await _context.LichPhongVans
                .Include(l => l.HoSoXetTuyen)
                .FirstOrDefaultAsync(l => l.MaLichPhongVan == id);

            if (lich == null) return NotFound();

            if (lich.TrangThai != TrangThaiLich.DaLenLich)
            {
                TempData["Loi"] = $"Chỉ được hủy lịch đang ở trạng thái \"{TrangThaiLich.DaLenLich}\".";
                return RedirectToAction(nameof(Index));
            }

            lich.TrangThai = TrangThaiLich.DaHuy;

            // [Module 3 & 4]: Hồ sơ đang chờ phỏng vấn được hoàn tác về "Đạt sơ tuyển" để có thể sắp lịch khác
            if (lich.HoSoXetTuyen != null && lich.HoSoXetTuyen.TrangThai == TrangThaiHoSo.ChoPhongVan)
            {
                lich.HoSoXetTuyen.TrangThai = TrangThaiHoSo.DatSoTuyen;
            }

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Đã hủy lịch phỏng vấn thành công.";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // M5.2: HOÀN THÀNH LỊCH PHỎNG VẤN
        // =========================================================================
        // POST: LichPhongVan/HoanThanh/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanThanh(int id)
        {
            var lich = await _context.LichPhongVans
                .Include(l => l.HoSoXetTuyen)
                .FirstOrDefaultAsync(l => l.MaLichPhongVan == id);

            if (lich == null) return NotFound();

            if (lich.TrangThai == TrangThaiLich.DaHuy)
            {
                TempData["Loi"] = "Lịch đã bị hủy, không thể đánh dấu hoàn thành.";
                return RedirectToAction(nameof(Index));
            }

            if (lich.TrangThai == TrangThaiLich.DaHoanThanh)
            {
                TempData["Loi"] = "Lịch phỏng vấn này đã được hoàn thành trước đó.";
                return RedirectToAction(nameof(Index));
            }

            var hoSo = lich.HoSoXetTuyen;
            bool hoSoHopLe = hoSo != null &&
                (hoSo.TrangThai == TrangThaiHoSo.ChoPhongVan || hoSo.TrangThai == TrangThaiHoSo.DatSoTuyen);

            if (!hoSoHopLe)
            {
                TempData["Loi"] = $"Hồ sơ đang ở trạng thái \"{hoSo?.TrangThai}\", không thể chuyển sang \"{TrangThaiHoSo.DaPhongVan}\".";
                return RedirectToAction(nameof(Index));
            }

            lich.TrangThai = TrangThaiLich.DaHoanThanh;
            // [Module 3 & 6]: Chuyển trạng thái hồ sơ sang "Đã phỏng vấn", sẵn sàng nhập kết quả
            hoSo!.TrangThai = TrangThaiHoSo.DaPhongVan;

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Đã hoàn thành phỏng vấn. Hồ sơ đã chuyển sang trạng thái \"Đã phỏng vấn\".";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // CHI TIẾT LỊCH PHỎNG VẤN
        // =========================================================================
        // GET: LichPhongVan/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var lich = await _context.LichPhongVans
                .Include(l => l.HoSoXetTuyen)
                    .ThenInclude(h => h!.ThiSinh)         // [Module 3]
                .Include(l => l.HoSoXetTuyen)
                    .ThenInclude(h => h!.NganhTuyenSinh)  // [Module 2]
                .Include(l => l.HoSoXetTuyen)
                    .ThenInclude(h => h!.KetQuaXetTuyen)  // [Module 5/6]: Kết quả xét tuyển
                .Include(l => l.NguoiPhongVan)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.MaLichPhongVan == id);

            if (lich == null) return NotFound();
            return View(lich);
        }

        // =========================================================================
        // M5.3: GHI NHẬN KẾT QUẢ XÉT TUYỂN (Điểm đánh giá & Trúng/Không trúng)
        // =========================================================================
        // GET: LichPhongVan/NhapKetQua/5
        public async Task<IActionResult> NhapKetQua(int id)
        {
            var lich = await _context.LichPhongVans
                .Include(l => l.HoSoXetTuyen)
                    .ThenInclude(h => h!.ThiSinh)
                .Include(l => l.HoSoXetTuyen)
                    .ThenInclude(h => h!.NganhTuyenSinh)
                .Include(l => l.HoSoXetTuyen)
                    .ThenInclude(h => h!.KetQuaXetTuyen)
                .FirstOrDefaultAsync(l => l.MaLichPhongVan == id);

            if (lich == null) return NotFound();

            if (lich.TrangThai != TrangThaiLich.DaHoanThanh)
            {
                TempData["Loi"] = "Chỉ được nhập kết quả cho lịch phỏng vấn đã hoàn thành.";
                return RedirectToAction(nameof(Index));
            }

            var trangThaiHoSo = lich.HoSoXetTuyen?.TrangThai;
            bool hopLe = trangThaiHoSo == TrangThaiHoSo.DaPhongVan ||
                         trangThaiHoSo == TrangThaiHoSo.TrungTuyen ||
                         trangThaiHoSo == TrangThaiHoSo.KhongTrungTuyen;

            if (!hopLe)
            {
                TempData["Loi"] = $"Hồ sơ đang ở trạng thái \"{trangThaiHoSo}\", chưa đủ điều kiện nhập kết quả.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu đã có kết quả trước đó thì hiển thị để cán bộ chỉnh sửa
            var ketQua = lich.HoSoXetTuyen!.KetQuaXetTuyen ?? new KetQuaXetTuyen
            {
                MaHoSo = lich.MaHoSo,
                KetQua = KetQuaTuyen.TrungTuyen
            };

            ViewBag.MaLichPhongVan = id;
            ViewBag.ThongTinLich = lich;
            return View(ketQua);
        }

        // POST: LichPhongVan/NhapKetQua/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NhapKetQua(
            int id,
            [Bind("MaKetQuaXetTuyen,MaHoSo,DiemDanhGia,NhanXet,KetQua")] KetQuaXetTuyen ketQua)
        {
            var lich = await _context.LichPhongVans
                .Include(l => l.HoSoXetTuyen)
                    .ThenInclude(h => h!.KetQuaXetTuyen)
                .FirstOrDefaultAsync(l => l.MaLichPhongVan == id);

            if (lich == null) return NotFound();

            // Ràng buộc kết quả hợp lệ
            if (ketQua.KetQua != KetQuaTuyen.TrungTuyen && ketQua.KetQua != KetQuaTuyen.KhongTrungTuyen)
            {
                ModelState.AddModelError(nameof(ketQua.KetQua), "Kết quả phải là Trúng tuyển hoặc Không trúng tuyển.");
            }

            // Điểm đánh giá (Đề mục 9.4: trong khoảng 0 - 100)
            if (ketQua.DiemDanhGia.HasValue && (ketQua.DiemDanhGia.Value < 0 || ketQua.DiemDanhGia.Value > 100))
            {
                ModelState.AddModelError(nameof(ketQua.DiemDanhGia), "Điểm đánh giá phải nằm trong khoảng từ 0 đến 100.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MaLichPhongVan = id;
                ViewBag.ThongTinLich = lich;
                return View(ketQua);
            }

            ketQua.NgayCapNhat = DateTime.Now; // Hệ thống tự động gán ngày cập nhật
            ketQua.MaHoSo = lich.MaHoSo;       // Đảm bảo không bị sửa mã hồ sơ

            var hoSo = lich.HoSoXetTuyen!;

            if (hoSo.KetQuaXetTuyen == null)
            {
                _context.KetQuaXetTuyens.Add(ketQua);
            }
            else
            {
                var existing = hoSo.KetQuaXetTuyen;
                existing.DiemDanhGia = ketQua.DiemDanhGia;
                existing.NhanXet = ketQua.NhanXet;
                existing.KetQua = ketQua.KetQua;
                existing.NgayCapNhat = ketQua.NgayCapNhat;
            }

            // [Module 3 & 4]: Cập nhật trạng thái cuối cùng của hồ sơ
            hoSo.TrangThai = (ketQua.KetQua == KetQuaTuyen.TrungTuyen)
                ? TrangThaiHoSo.TrungTuyen
                : TrangThaiHoSo.KhongTrungTuyen;

            await _context.SaveChangesAsync();

            TempData["ThongBao"] = $"Đã cập nhật kết quả: {ketQua.KetQua}. Trạng thái hồ sơ chuyển sang \"{hoSo.TrangThai}\".";
            return RedirectToAction(nameof(Details), new { id });
        }

        // =========================================================================
        // M5.4: THÍ SINH TRA CỨU LỊCH PHỎNG VẤN VÀ KẾT QUẢ CỦA CHÍNH MÌNH
        // =========================================================================
        // GET: LichPhongVan/LichCuaToi?tuKhoa=...
        public async Task<IActionResult> LichCuaToi(string? tuKhoa)
        {
            var model = new TraCuuThiSinhViewModel
            {
                TuKhoaTraCuu = tuKhoa
            };

            // 1. Kiểm tra nếu Thí sinh đã đăng nhập (Module 1 lưu Session MaTaiKhoan)
            var sessionMaTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            int? maThiSinhXacDinh = null;

            if (sessionMaTaiKhoan.HasValue)
            {
                var tsDangNhap = await _context.ThiSinhs
                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == sessionMaTaiKhoan.Value);
                if (tsDangNhap != null)
                {
                    maThiSinhXacDinh = tsDangNhap.MaThiSinh;
                    model.ThongTinThiSinh = tsDangNhap;
                }
            }

            // 2. Nếu chưa đăng nhập hoặc tra cứu công khai theo từ khóa (SĐT, Email, Họ tên)
            if (!maThiSinhXacDinh.HasValue && !string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                var ts = await _context.ThiSinhs
                    .FirstOrDefaultAsync(t => t.SoDienThoai == tuKhoa || t.Email == tuKhoa);

                if (ts != null)
                {
                    maThiSinhXacDinh = ts.MaThiSinh;
                    model.ThongTinThiSinh = ts;
                }
                else
                {
                    ViewBag.ThongBaoLoi = "Không tìm thấy thông tin thí sinh với số điện thoại hoặc email trên.";
                }
            }

            // 3. Nếu tìm thấy thí sinh, lấy danh sách hồ sơ kèm lịch phỏng vấn và kết quả
            if (maThiSinhXacDinh.HasValue)
            {
                model.DanhSachHoSo = await _context.HoSoXetTuyens
                    .Where(h => h.MaThiSinh == maThiSinhXacDinh.Value)
                    .Include(h => h.NganhTuyenSinh)
                        .ThenInclude(n => n!.Khoa)
                    .Include(h => h.LichPhongVans)
                        .ThenInclude(l => l.NguoiPhongVan)
                    .Include(h => h.KetQuaXetTuyen)
                    .OrderByDescending(h => h.NgayNop)
                    .ToListAsync();
            }

            return View(model);
        }

        // =========================================================================
        // M5.5: DASHBOARD 9 CHỈ SỐ TỔNG QUAN (Đề mục 9.5)
        // =========================================================================
        // GET: LichPhongVan/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var now = DateTime.Now;

            // Truy vấn tối ưu bằng LINQ CountAsync độc lập
            var vm = new DashboardViewModel
            {
                // 1. Tổng số khoa [Module 1]
                TongSoKhoa = await _context.Khoas.CountAsync(),

                // 2. Tổng số ngành tuyển sinh [Module 2]
                TongSoNganh = await _context.NganhTuyenSinhs.CountAsync(),

                // 3. Số ngành tuyển sinh trạng thái "Đang tuyển" [Module 2]
                SoNganhDangTuyen = await _context.NganhTuyenSinhs
                    .CountAsync(n => n.TrangThai == TrangThaiNganh.DangTuyen),

                // 4. Tổng số thí sinh [Module 3]
                TongSoThiSinh = await _context.ThiSinhs.CountAsync(),

                // 5. Tổng số hồ sơ [Module 3]
                TongSoHoSo = await _context.HoSoXetTuyens.CountAsync(),

                // 6. Số hồ sơ "Chờ duyệt" [Module 4]
                SoHoSoChoDuyet = await _context.HoSoXetTuyens
                    .CountAsync(h => h.TrangThai == TrangThaiHoSo.ChoDuyet),

                // 7. Số hồ sơ "Đạt sơ tuyển" [Module 4]
                SoHoSoDatSoTuyen = await _context.HoSoXetTuyens
                    .CountAsync(h => h.TrangThai == TrangThaiHoSo.DatSoTuyen),

                // 8. Số lịch phỏng vấn sắp tới (Đã lên lịch & chưa hết thời gian bắt đầu) [Module 5]
                SoLichPhongVanSapToi = await _context.LichPhongVans
                    .CountAsync(l => l.TrangThai == TrangThaiLich.DaLenLich && l.ThoiGianBatDau >= now),

                // 9. Số hồ sơ trúng tuyển [Module 5 & 6]
                SoHoSoTrungTuyen = await _context.HoSoXetTuyens
                    .CountAsync(h => h.TrangThai == TrangThaiHoSo.TrungTuyen)
            };

            return View(vm);
        }

        // =========================================================================
        // M5.6: THỐNG KÊ BẰNG LINQ (Đề mục 9.6: 10 nghiệp vụ thống kê chuyên sâu)
        // Áp dụng: Count, Sum, GroupBy, OrderBy, OrderByDescending, Any, Select.
        // =========================================================================
        // GET: LichPhongVan/ThongKe
        public async Task<IActionResult> ThongKe()
        {
            var vm = new ThongKeViewModel();

            // -----------------------------------------------------------------
            // 1. Số ngành tuyển sinh theo khoa [LINQ: GroupBy theo Khoa + Count/Sum]
            // Ý nghĩa: Đánh giá quy mô phân bổ ngành và chỉ tiêu của từng khoa
            // -----------------------------------------------------------------
            vm.NganhTheoKhoa = await _context.Khoas
                .Select(k => new ThongKeNganhTheoKhoaItem
                {
                    TenKhoa = k.TenKhoa,
                    SoLuongNganh = k.NganhTuyenSinhs.Count(),
                    TongChiTieu = k.NganhTuyenSinhs.Sum(n => n.ChiTieu)
                })
                .OrderByDescending(k => k.SoLuongNganh)
                .ToListAsync();

            // -----------------------------------------------------------------
            // 2. Số hồ sơ theo từng ngành tuyển sinh [LINQ: Select/GroupBy + Count]
            // Ý nghĩa: Đo lường độ thu hút hồ sơ của từng ngành học
            // -----------------------------------------------------------------
            vm.HoSoTheoNganh = await _context.NganhTuyenSinhs
                .Select(n => new ThongKeHoSoTheoNganhItem
                {
                    TenNganh = n.TenNganh,
                    ChiTieu = n.ChiTieu,
                    SoLuongHoSo = n.HoSoXetTuyens.Count()
                })
                .OrderByDescending(n => n.SoLuongHoSo)
                .ToListAsync();

            // -----------------------------------------------------------------
            // 3. Số hồ sơ theo trạng thái [LINQ: GroupBy(TrangThai) + Count]
            // Ý nghĩa: Kiểm soát luồng xử lý hồ sơ đang nằm ở giai đoạn nào
            // -----------------------------------------------------------------
            var tongHoSo = await _context.HoSoXetTuyens.CountAsync();
            var nhomTrangThai = await _context.HoSoXetTuyens
                .GroupBy(h => h.TrangThai)
                .Select(g => new
                {
                    TrangThai = g.Key,
                    SoLuong = g.Count()
                })
                .ToListAsync();

            vm.HoSoTheoTrangThai = nhomTrangThai
                .Select(x => new ThongKeHoSoTheoTrangThaiItem
                {
                    TrangThai = x.TrangThai,
                    SoLuong = x.SoLuong,
                    TyLePhanTram = tongHoSo > 0 ? Math.Round((double)x.SoLuong * 100 / tongHoSo, 1) : 0
                })
                .OrderByDescending(x => x.SoLuong)
                .ToList();

            // -----------------------------------------------------------------
            // 4. Số hồ sơ đạt sơ tuyển theo ngành tuyển sinh
            // [LINQ: Where(DatSoTuyen || bước sau) + Count]
            // Ý nghĩa: Xem ngành nào có nhiều thí sinh đáp ứng điều kiện đầu vào sơ bộ
            // -----------------------------------------------------------------
            vm.DatSoTuyenTheoNganh = await _context.NganhTuyenSinhs
                .Select(n => new ThongKeDatSoTuyenTheoNganhItem
                {
                    TenNganh = n.TenNganh,
                    // Hồ sơ đạt sơ tuyển là hồ sơ có trạng thái DatSoTuyen hoặc các trạng thái tiếp theo
                    SoHoSoDatSoTuyen = n.HoSoXetTuyens.Count(h =>
                        h.TrangThai == TrangThaiHoSo.DatSoTuyen ||
                        h.TrangThai == TrangThaiHoSo.ChoPhongVan ||
                        h.TrangThai == TrangThaiHoSo.DaPhongVan ||
                        h.TrangThai == TrangThaiHoSo.TrungTuyen ||
                        h.TrangThai == TrangThaiHoSo.KhongTrungTuyen)
                })
                .OrderByDescending(n => n.SoHoSoDatSoTuyen)
                .ToListAsync();

            // -----------------------------------------------------------------
            // 5. Ngành tuyển sinh nhận nhiều hồ sơ nhất
            // [LINQ: OrderByDescending(Count) + FirstOrDefaultAsync]
            // Ý nghĩa: Xác định ngành "hot" nhất mùa tuyển sinh
            // -----------------------------------------------------------------
            vm.NganhNhieuHoSoNhat = await _context.NganhTuyenSinhs
                .Select(n => new NganhNhieuHoSoNhatItem
                {
                    TenNganh = n.TenNganh,
                    TenKhoa = n.Khoa != null ? n.Khoa.TenKhoa : "—",
                    ChiTieu = n.ChiTieu,
                    SoLuongHoSo = n.HoSoXetTuyens.Count()
                })
                .OrderByDescending(n => n.SoLuongHoSo)
                .FirstOrDefaultAsync();

            // -----------------------------------------------------------------
            // 6. Số hồ sơ trúng tuyển theo khoa [LINQ: GroupBy Khoa + Count Trúng tuyển]
            // Ý nghĩa: Thống kê số lượng sinh viên tương lai nhập học theo từng khoa
            // -----------------------------------------------------------------
            vm.TrungTuyenTheoKhoa = await _context.Khoas
                .Select(k => new ThongKeTrungTuyenTheoKhoaItem
                {
                    TenKhoa = k.TenKhoa,
                    SoLuongTrungTuyen = k.NganhTuyenSinhs
                        .SelectMany(n => n.HoSoXetTuyens)
                        .Count(h => h.TrangThai == TrangThaiHoSo.TrungTuyen)
                })
                .OrderByDescending(k => k.SoLuongTrungTuyen)
                .ToListAsync();

            // -----------------------------------------------------------------
            // 7. Số hồ sơ nộp theo tháng [LINQ: GroupBy(NgayNop.Year, Month) + Count]
            // Ý nghĩa: Phân tích xu hướng thời gian nộp hồ sơ của thí sinh
            // -----------------------------------------------------------------
            var dsHoSoTheoThang = await _context.HoSoXetTuyens
                .Select(h => new { h.NgayNop.Year, h.NgayNop.Month })
                .ToListAsync();

            vm.HoSoTheoThang = dsHoSoTheoThang
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => new ThongKeTheoThangItem
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    SoLuong = g.Count()
                })
                .OrderBy(x => x.Nam)
                .ThenBy(x => x.Thang)
                .ToList();

            // -----------------------------------------------------------------
            // 8. Số lịch phỏng vấn theo tháng [LINQ: GroupBy(ThoiGianBatDau.Year, Month)]
            // Ý nghĩa: Phân tích khối lượng công việc phỏng vấn của cán bộ theo tháng
            // -----------------------------------------------------------------
            var dsLichTheoThang = await _context.LichPhongVans
                .Select(l => new { l.ThoiGianBatDau.Year, l.ThoiGianBatDau.Month })
                .ToListAsync();

            vm.LichTheoThang = dsLichTheoThang
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => new ThongKeTheoThangItem
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    SoLuong = g.Count()
                })
                .OrderBy(x => x.Nam)
                .ThenBy(x => x.Thang)
                .ToList();

            // -----------------------------------------------------------------
            // 9. Tỷ lệ hồ sơ qua sơ tuyển theo từng ngành tuyển sinh (%)
            // Ý nghĩa: Đánh giá chất lượng đầu vào sơ bộ của từng ngành
            // -----------------------------------------------------------------
            var dsNganhVaHoSo = await _context.NganhTuyenSinhs
                .Select(n => new
                {
                    n.TenNganh,
                    TongHoSo = n.HoSoXetTuyens.Count(),
                    SoDat = n.HoSoXetTuyens.Count(h =>
                        h.TrangThai == TrangThaiHoSo.DatSoTuyen ||
                        h.TrangThai == TrangThaiHoSo.ChoPhongVan ||
                        h.TrangThai == TrangThaiHoSo.DaPhongVan ||
                        h.TrangThai == TrangThaiHoSo.TrungTuyen ||
                        h.TrangThai == TrangThaiHoSo.KhongTrungTuyen)
                })
                .ToListAsync();

            vm.TyLeQuaSoTuyenTheoNganh = dsNganhVaHoSo
                .Select(x => new TyLeQuaSoTuyenItem
                {
                    TenNganh = x.TenNganh,
                    TongHoSo = x.TongHoSo,
                    SoDatSoTuyen = x.SoDat,
                    TyLePhanTram = x.TongHoSo > 0 ? Math.Round((double)x.SoDat * 100 / x.TongHoSo, 1) : 0
                })
                .OrderByDescending(x => x.TyLePhanTram)
                .ToList();

            // -----------------------------------------------------------------
            // 10. Tỷ lệ hồ sơ trúng tuyển trên số hồ sơ đã phỏng vấn (%)
            // Ý nghĩa: Đánh giá tỷ lệ chọn lọc tại vòng phỏng vấn
            // -----------------------------------------------------------------
            // Các hồ sơ đã qua phỏng vấn gồm: DaPhongVan, TrungTuyen, KhongTrungTuyen
            var soDaPhongVan = await _context.HoSoXetTuyens
                .CountAsync(h => h.TrangThai == TrangThaiHoSo.DaPhongVan ||
                                 h.TrangThai == TrangThaiHoSo.TrungTuyen ||
                                 h.TrangThai == TrangThaiHoSo.KhongTrungTuyen);

            var soTrungTuyen = await _context.HoSoXetTuyens
                .CountAsync(h => h.TrangThai == TrangThaiHoSo.TrungTuyen);

            var soKhongTrung = await _context.HoSoXetTuyens
                .CountAsync(h => h.TrangThai == TrangThaiHoSo.KhongTrungTuyen);

            vm.TyLeTrungTuyenTrenDaPV = new TyLeTrungTuyenSauPVItem
            {
                TongHoSoDaPhongVan = soDaPhongVan,
                SoHoSoTrungTuyen = soTrungTuyen,
                SoHoSoKhongTrungTuyen = soKhongTrung,
                TyLeTrungTuyenPhanTram = soDaPhongVan > 0
                    ? Math.Round((double)soTrungTuyen * 100 / soDaPhongVan, 1)
                    : 0
            };

            return View(vm);
        }

        // =========================================================================
        // HÀM HỖ TRỢ KIỂM TRA NGHIỆP VỤ LẬP LỊCH (Đề mục 9.2)
        // =========================================================================
        private async Task<HoSoXetTuyen?> KiemTraNghiepVuAsync(LichPhongVan lich)
        {
            // 1. Kiểm tra hình thức phỏng vấn
            if (!HinhThucHopLe.Contains(lich.HinhThucPhongVan))
            {
                ModelState.AddModelError(nameof(lich.HinhThucPhongVan), "Hình thức phỏng vấn phải là Trực tiếp hoặc Trực tuyến.");
            }

            // 2. Địa điểm hoặc liên kết bắt buộc
            if (string.IsNullOrWhiteSpace(lich.DiaDiemHoacLienKet))
            {
                ModelState.AddModelError(nameof(lich.DiaDiemHoacLienKet),
                    "Vui lòng nhập địa điểm (phòng thi trực tiếp) hoặc liên kết họp (Google Meet/Zoom trực tuyến).");
            }

            // 3. Thời gian phỏng vấn không được trong quá khứ
            if (lich.ThoiGianBatDau < DateTime.Now)
            {
                ModelState.AddModelError(nameof(lich.ThoiGianBatDau), "Thời gian bắt đầu phỏng vấn không được nằm trong quá khứ.");
            }

            // 4. Hồ sơ phải tồn tại và đang ở trạng thái "Đạt sơ tuyển" [Module 4]
            var hoSo = await _context.HoSoXetTuyens.FirstOrDefaultAsync(h => h.MaHoSo == lich.MaHoSo);
            if (hoSo == null)
            {
                ModelState.AddModelError(nameof(lich.MaHoSo), "Vui lòng chọn hồ sơ xét tuyển.");
                return null;
            }

            if (hoSo.TrangThai != TrangThaiHoSo.DatSoTuyen)
            {
                ModelState.AddModelError(nameof(lich.MaHoSo),
                    $"Chỉ hồ sơ có trạng thái \"{TrangThaiHoSo.DatSoTuyen}\" mới được lập lịch phỏng vấn (hồ sơ này đang là \"{hoSo.TrangThai}\").");
            }

            // 5. Hồ sơ chưa có lịch phỏng vấn đang hiệu lực khác
            bool daCoLich = await _context.LichPhongVans
                .AnyAsync(l => l.MaHoSo == lich.MaHoSo && l.TrangThai == TrangThaiLich.DaLenLich);
            if (daCoLich)
            {
                ModelState.AddModelError(nameof(lich.MaHoSo), "Hồ sơ này đã có một lịch phỏng vấn đang hiệu lực.");
            }

            // 6. Người phỏng vấn phải tồn tại và còn hoạt động
            bool npvHopLe = await _context.NguoiPhongVans
                .AnyAsync(n => n.MaNguoiPhongVan == lich.MaNguoiPhongVan && n.TrangThai);
            if (!npvHopLe)
            {
                ModelState.AddModelError(nameof(lich.MaNguoiPhongVan), "Vui lòng chọn người phỏng vấn hợp lệ đang hoạt động.");
            }

            // 7. Giải thuật kiểm tra trùng khoảng thời gian (Đề mục 9.2: áp dụng trên các lịch chưa hủy)
            // Hai khoảng thời gian [A1, A2] và [B1, B2] trùng nhau khi: A1 < B2 và B1 < A2
            if (lich.ThoiGianKetThuc > lich.ThoiGianBatDau)
            {
                var start = lich.ThoiGianBatDau;
                var end = lich.ThoiGianKetThuc;
                var maThiSinh = hoSo.MaThiSinh;
                var maNpv = lich.MaNguoiPhongVan;

                var lichChuaHuy = _context.LichPhongVans.Where(l => l.TrangThai != TrangThaiLich.DaHuy);

                // Kiểm tra trùng lịch thí sinh (so sánh theo mã thí sinh để bao quát nếu thí sinh có nhiều hồ sơ)
                bool trungThiSinh = await lichChuaHuy.AnyAsync(l =>
                    l.HoSoXetTuyen!.MaThiSinh == maThiSinh &&
                    l.ThoiGianBatDau < end &&
                    start < l.ThoiGianKetThuc);

                if (trungThiSinh)
                {
                    ModelState.AddModelError(string.Empty, "Thí sinh đã có một lịch phỏng vấn khác bị trùng khoảng thời gian này.");
                }

                // Kiểm tra trùng lịch người phỏng vấn
                bool trungNguoiPV = await lichChuaHuy.AnyAsync(l =>
                    l.MaNguoiPhongVan == maNpv &&
                    l.ThoiGianBatDau < end &&
                    start < l.ThoiGianKetThuc);

                if (trungNguoiPV)
                {
                    ModelState.AddModelError(string.Empty, "Người phỏng vấn này đã có lịch làm việc khác bị trùng khoảng thời gian này.");
                }
            }

            return hoSo;
        }

        // Nạp Dropdown hồ sơ và người phỏng vấn
        private async Task NapDropdownAsync(LichPhongVan lich)
        {
            // Chỉ hiển thị hồ sơ "Đạt sơ tuyển" và chưa có lịch đang hiệu lực
            var dsHoSo = await _context.HoSoXetTuyens
                .Where(h => h.TrangThai == TrangThaiHoSo.DatSoTuyen &&
                            !_context.LichPhongVans.Any(l => l.MaHoSo == h.MaHoSo && l.TrangThai == TrangThaiLich.DaLenLich))
                .OrderBy(h => h.MaHoSo)
                .Select(h => new
                {
                    h.MaHoSo,
                    HoTen = h.ThiSinh != null ? h.ThiSinh.HoTen : "Thí sinh",
                    TenNganh = h.NganhTuyenSinh != null ? h.NganhTuyenSinh.TenNganh : "Ngành"
                })
                .ToListAsync();

            ViewBag.DsHoSo = dsHoSo.Select(h => new SelectListItem
            {
                Value = h.MaHoSo.ToString(),
                Text = $"#{h.MaHoSo} - {h.HoTen} - {h.TenNganh}",
                Selected = h.MaHoSo == lich.MaHoSo
            }).ToList();

            ViewBag.DsNguoiPhongVan = await _context.NguoiPhongVans
                .Where(n => n.TrangThai)
                .OrderBy(n => n.HoTen)
                .Select(n => new SelectListItem
                {
                    Value = n.MaNguoiPhongVan.ToString(),
                    Text = n.HoTen,
                    Selected = n.MaNguoiPhongVan == lich.MaNguoiPhongVan
                })
                .ToListAsync();
        }
    }
}
