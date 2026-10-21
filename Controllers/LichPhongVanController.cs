// Họ và tên: Mã Đức Thịnh
// Mã sinh viên: 23103100314
// Nội dung thực hiện: Module 5 - Quản lý lịch phỏng vấn: danh sách, lập lịch (kiểm tra trùng lịch
// thí sinh và người phỏng vấn), hủy lịch, hoàn thành lịch và cập nhật trạng thái hồ sơ.
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Controllers;

public class LichPhongVanController : Controller
{
    private readonly ApplicationDbContext _context;

    // TODO: đối chiếu với Module 1 (đăng nhập). Hai giá trị dưới đây là GIẢ ĐỊNH:
    //  - khóa Session mà Module 1 dùng để lưu vai trò sau khi đăng nhập
    //  - đường dẫn trang đăng nhập
    private const string SessionVaiTro = "VaiTro";
    private const string TrangDangNhapAction = "DangNhap";
    private const string TrangDangNhapController = "TaiKhoan";

    private static readonly string[] HinhThucHopLe = { "Trực tiếp", "Trực tuyến" };

    public LichPhongVanController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Đề mục 5.4: quyền phải được kiểm tra tại Controller, không chỉ ẩn nút trên View.
    // Hàm này chạy trước MỌI action của controller.
    // public override void OnActionExecuting(ActionExecutingContext context)
    // {
    //     var vaiTro = context.HttpContext.Session.GetString(SessionVaiTro);
    //     if (vaiTro != VaiTro.CanBoTuyenSinh && vaiTro != VaiTro.Admin)
    //     {
    //         context.Result = RedirectToAction(TrangDangNhapAction, TrangDangNhapController);
    //         return;
    //     }
    //     base.OnActionExecuting(context);
    // }

    // ------------------------------------------------------------------
    // DANH SÁCH
    // ------------------------------------------------------------------
    public async Task<IActionResult> Index(string? trangThai)
    {
        IQueryable<LichPhongVan> query = _context.LichPhongVans
            .Include(l => l.HoSoXetTuyen).ThenInclude(h => h!.ThiSinh)
            .Include(l => l.HoSoXetTuyen).ThenInclude(h => h!.NganhTuyenSinh)
            .Include(l => l.NguoiPhongVan);

        if (!string.IsNullOrEmpty(trangThai))
            query = query.Where(l => l.TrangThai == trangThai);

        var ds = await query.OrderByDescending(l => l.ThoiGianBatDau).ToListAsync();

        ViewBag.TrangThai = trangThai;
        return View(ds);
    }

    // ------------------------------------------------------------------
    // LẬP LỊCH
    // ------------------------------------------------------------------
    public async Task<IActionResult> Create(int? maHoSo)
    {
        var batDau = DateTime.Today.AddDays(1).AddHours(8);
        var lich = new LichPhongVan
        {
            MaHoSo = maHoSo ?? 0,
            ThoiGianBatDau = batDau,
            ThoiGianKetThuc = batDau.AddMinutes(45)
        };

        await NapDropdownAsync(lich);
        return View(lich);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        // Bind liệt kê rõ trường được nhận từ form: TrangThai KHÔNG có trong danh sách,
        // nên người dùng không thể tự gửi trạng thái trái nghiệp vụ (đề mục 14).
        [Bind("MaHoSo,ThoiGianBatDau,ThoiGianKetThuc,HinhThucPhongVan,DiaDiemHoacLienKet,MaNguoiPhongVan,GhiChu")]
        LichPhongVan lich)
    {
        lich.TrangThai = TrangThaiLich.DaLenLich;   // do hệ thống gán

        var hoSo = await KiemTraNghiepVuAsync(lich);

        if (!ModelState.IsValid || hoSo == null)
        {
            await NapDropdownAsync(lich);
            return View(lich);
        }

        // Lập lịch thành công: hồ sơ Đạt sơ tuyển -> Chờ phỏng vấn (luồng ở đề mục 8.4).
        hoSo.TrangThai = TrangThaiHoSo.ChoPhongVan;
        _context.LichPhongVans.Add(lich);

        // Một lần SaveChanges nên lịch và hồ sơ được lưu cùng nhau (cùng thành công hoặc cùng thất bại).
        await _context.SaveChangesAsync();

        TempData["ThongBao"] = "Đã lập lịch phỏng vấn thành công.";
        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------------
    // HỦY LỊCH
    // ------------------------------------------------------------------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(int id)
    {
        var lich = await _context.LichPhongVans
            .Include(l => l.HoSoXetTuyen)
            .FirstOrDefaultAsync(l => l.MaLichPhongVan == id);

        if (lich == null) return NotFound();

        if (lich.TrangThai != TrangThaiLich.DaLenLich)
        {
            TempData["Loi"] = $"Chỉ hủy được lịch đang ở trạng thái \"{TrangThaiLich.DaLenLich}\".";
            return RedirectToAction(nameof(Index));
        }

        lich.TrangThai = TrangThaiLich.DaHuy;

        // Hồ sơ đang chờ phỏng vấn thì quay về Đạt sơ tuyển để có thể lập lịch khác.
        if (lich.HoSoXetTuyen != null && lich.HoSoXetTuyen.TrangThai == TrangThaiHoSo.ChoPhongVan)
            lich.HoSoXetTuyen.TrangThai = TrangThaiHoSo.DatSoTuyen;

        await _context.SaveChangesAsync();

        TempData["ThongBao"] = "Đã hủy lịch phỏng vấn.";
        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------------
    // HOÀN THÀNH LỊCH (đề mục 9.3)
    // ------------------------------------------------------------------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> HoanThanh(int id)
    {
        var lich = await _context.LichPhongVans
            .Include(l => l.HoSoXetTuyen)
            .FirstOrDefaultAsync(l => l.MaLichPhongVan == id);

        if (lich == null) return NotFound();

        if (lich.TrangThai == TrangThaiLich.DaHuy)
        {
            TempData["Loi"] = "Lịch đã bị hủy, không thể đánh dấu Đã phỏng vấn.";
            return RedirectToAction(nameof(Index));
        }

        if (lich.TrangThai == TrangThaiLich.DaHoanThanh)
        {
            TempData["Loi"] = "Lịch này đã hoàn thành trước đó.";
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
        hoSo!.TrangThai = TrangThaiHoSo.DaPhongVan;

        await _context.SaveChangesAsync();

        TempData["ThongBao"] = "Đã hoàn thành phỏng vấn. Hồ sơ chuyển sang \"Đã phỏng vấn\".";
        return RedirectToAction(nameof(Index));
    }

    // ==================================================================
    // HÀM HỖ TRỢ
    // ==================================================================

    // Kiểm tra các quy tắc ở đề mục 9.2. Lỗi được thêm vào ModelState để hiện trên View.
    // Trả về hồ sơ nếu tìm thấy (để dùng tiếp khi lưu), ngược lại trả về null.
    private async Task<HoSoXetTuyen?> KiemTraNghiepVuAsync(LichPhongVan lich)
    {
        // 1. Hình thức và địa điểm/liên kết
        if (!HinhThucHopLe.Contains(lich.HinhThucPhongVan))
            ModelState.AddModelError(nameof(lich.HinhThucPhongVan), "Hình thức phỏng vấn không hợp lệ.");

        if (string.IsNullOrWhiteSpace(lich.DiaDiemHoacLienKet))
            ModelState.AddModelError(nameof(lich.DiaDiemHoacLienKet),
                "Vui lòng nhập địa điểm (phỏng vấn trực tiếp) hoặc liên kết (phỏng vấn trực tuyến).");

        // 2. Thời gian. Lỗi "kết thúc phải sau bắt đầu" đã được hàm Validate của Entity báo,
        //    nên ở đây chỉ kiểm tra thêm điều kiện "không nằm trong quá khứ".
        if (lich.ThoiGianBatDau < DateTime.Now)
            ModelState.AddModelError(nameof(lich.ThoiGianBatDau), "Thời gian phỏng vấn không được nằm trong quá khứ.");

        // 3. Hồ sơ phải tồn tại và đang Đạt sơ tuyển
        var hoSo = await _context.HoSoXetTuyens.FirstOrDefaultAsync(h => h.MaHoSo == lich.MaHoSo);
        if (hoSo == null)
        {
            ModelState.AddModelError(nameof(lich.MaHoSo), "Vui lòng chọn hồ sơ.");
            return null;
        }

        if (hoSo.TrangThai != TrangThaiHoSo.DatSoTuyen)
            ModelState.AddModelError(nameof(lich.MaHoSo),
                $"Chỉ hồ sơ \"{TrangThaiHoSo.DatSoTuyen}\" mới được lập lịch phỏng vấn (hồ sơ này đang \"{hoSo.TrangThai}\").");

        // 4. Hồ sơ chưa có lịch đang hiệu lực khác
        bool daCoLichConHieuLuc = await _context.LichPhongVans
            .AnyAsync(l => l.MaHoSo == lich.MaHoSo && l.TrangThai == TrangThaiLich.DaLenLich);
        if (daCoLichConHieuLuc)
            ModelState.AddModelError(nameof(lich.MaHoSo), "Hồ sơ này đã có một lịch phỏng vấn đang hiệu lực.");

        // 5. Người phỏng vấn phải tồn tại và còn hoạt động
        bool npvHopLe = await _context.NguoiPhongVans
            .AnyAsync(n => n.MaNguoiPhongVan == lich.MaNguoiPhongVan && n.TrangThai);
        if (!npvHopLe)
            ModelState.AddModelError(nameof(lich.MaNguoiPhongVan), "Vui lòng chọn người phỏng vấn hợp lệ.");

        // 6. Kiểm tra trùng khoảng thời gian (chỉ khi khoảng thời gian hợp lệ)
        if (lich.ThoiGianKetThuc > lich.ThoiGianBatDau)
        {
            var batDau = lich.ThoiGianBatDau;
            var ketThuc = lich.ThoiGianKetThuc;
            var maThiSinh = hoSo.MaThiSinh;
            var maNguoiPV = lich.MaNguoiPhongVan;

            // Chỉ xét các lịch CHƯA HỦY (đề mục 9.2).
            // Hai khoảng [A1,A2] và [B1,B2] trùng nhau khi A1 < B2 và B1 < A2.
            var lichChuaHuy = _context.LichPhongVans.Where(l => l.TrangThai != TrangThaiLich.DaHuy);

            // Trùng lịch của THÍ SINH (một thí sinh có thể có nhiều hồ sơ nên so theo mã thí sinh)
            bool trungThiSinh = await lichChuaHuy.AnyAsync(l =>
                l.HoSoXetTuyen!.MaThiSinh == maThiSinh &&
                l.ThoiGianBatDau < ketThuc &&
                batDau < l.ThoiGianKetThuc);
            if (trungThiSinh)
                ModelState.AddModelError(string.Empty, "Thí sinh đã có lịch phỏng vấn khác trùng khoảng thời gian này.");

            // Trùng lịch của NGƯỜI PHỎNG VẤN
            bool trungNguoiPV = await lichChuaHuy.AnyAsync(l =>
                l.MaNguoiPhongVan == maNguoiPV &&
                l.ThoiGianBatDau < ketThuc &&
                batDau < l.ThoiGianKetThuc);
            if (trungNguoiPV)
                ModelState.AddModelError(string.Empty, "Người phỏng vấn đã có lịch khác trùng khoảng thời gian này.");
        }

        return hoSo;
    }

    // Nạp dữ liệu cho hai dropdown trên form lập lịch.
    private async Task NapDropdownAsync(LichPhongVan lich)
    {
        // Chỉ hồ sơ Đạt sơ tuyển và chưa có lịch đang hiệu lực
        var hoSos = await _context.HoSoXetTuyens
            .Where(h => h.TrangThai == TrangThaiHoSo.DatSoTuyen &&
                        !_context.LichPhongVans.Any(l => l.MaHoSo == h.MaHoSo && l.TrangThai == TrangThaiLich.DaLenLich))
            .OrderBy(h => h.MaHoSo)
            .Select(h => new { h.MaHoSo, HoTen = h.ThiSinh!.HoTen, Nganh = h.NganhTuyenSinh!.TenNganh })
            .ToListAsync();

        ViewBag.DsHoSo = hoSos
            .Select(h => new SelectListItem
            {
                Value = h.MaHoSo.ToString(),
                Text = $"#{h.MaHoSo} - {h.HoTen} - {h.Nganh}",
                Selected = h.MaHoSo == lich.MaHoSo
            })
            .ToList();

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