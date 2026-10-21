// Họ và tên: Đào Văn Khánh
// Mã sinh viên: 23103100125
// Nội dung thực hiện: Quản lý ngành tuyển sinh (danh sách, chi tiết, thêm, sửa,
// cập nhật trạng thái, xóa có kiểm soát), tìm kiếm, lọc, sắp xếp và phân trang.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.ViewModels;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Controllers;

public class NganhTuyenSinhController : Controller
{
    private readonly ApplicationDbContext _context;

    public NganhTuyenSinhController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =====================================================================
    // CẤU HÌNH CHUNG
    // =====================================================================

    private const int KichThuocTrang = 10;

    // Khóa Session do Module 1 (đăng nhập) lưu. Đổi lại nếu nhóm đặt tên khác.
    private const string SessionMaTaiKhoan = "MaTaiKhoan";
    private const string SessionVaiTro = "VaiTro";

    // Action/Controller đăng nhập của Module 1. Đổi lại nếu nhóm đặt tên khác.
    private const string ActionDangNhap = "DangNhap";
    private const string ControllerDangNhap = "TaiKhoan";

    // Các action chỉ dành cho Admin / Cán bộ tuyển sinh.
    private static readonly HashSet<string> ActionQuanLy = new()
    {
        nameof(Create), nameof(Edit), nameof(ChangeStatus), nameof(Delete)
    };

    // Các giá trị sắp xếp hợp lệ
    private static readonly (string Value, string Text)[] CacKieuSapXep =
    {
        ("ten_asc", "Tên ngành A → Z"),
        ("ten_desc", "Tên ngành Z → A"),
        ("han_asc", "Hạn nộp hồ sơ tăng dần"),
        ("han_desc", "Hạn nộp hồ sơ giảm dần"),
        ("chitieu_asc", "Chỉ tiêu tăng dần"),
        ("chitieu_desc", "Chỉ tiêu giảm dần"),
    };

    // Luồng chuyển trạng thái ngành: không cho chọn trạng thái tùy ý, trái nghiệp vụ.
    // Muốn thay đổi luồng (ví dụ cho mở lại ngành Đã đóng) chỉ cần sửa bảng này.
    private static readonly Dictionary<string, string[]> LuongTrangThai = new()
    {
        [TrangThaiNganh.ChuaMo] = new[] { TrangThaiNganh.DangTuyen },
        [TrangThaiNganh.DangTuyen] = new[] { TrangThaiNganh.TamDung, TrangThaiNganh.DaDong },
        [TrangThaiNganh.TamDung] = new[] { TrangThaiNganh.DangTuyen, TrangThaiNganh.DaDong },
        [TrangThaiNganh.DaDong] = Array.Empty<string>(),
    };

    // =====================================================================
    // PHÂN QUYỀN - kiểm tra tại Controller trước khi xử lý bất kỳ action nào
    // =====================================================================

    private string? VaiTroHienTai => HttpContext.Session.GetString(SessionVaiTro);

    private bool LaQuanLy => VaiTroHienTai is VaiTro.Admin or VaiTro.CanBo;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // 1. Phải đăng nhập (có Session)
        var maTaiKhoan = context.HttpContext.Session.GetInt32(SessionMaTaiKhoan);
        if (maTaiKhoan == null)
        {
            context.Result = RedirectToAction(ActionDangNhap, ControllerDangNhap, new
            {
                returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString
            });
            return;
        }

        // 2. Action quản lý chỉ cho Admin / Cán bộ tuyển sinh
        var tenAction = (context.ActionDescriptor as ControllerActionDescriptor)?.ActionName;
        if (tenAction != null && ActionQuanLy.Contains(tenAction) && !LaQuanLy)
        {
            TempData["Loi"] = "Bạn không có quyền thực hiện chức năng này.";
            context.Result = RedirectToAction(nameof(Index));
            return;
        }

        base.OnActionExecuting(context);
    }

    // =====================================================================
    // DANH SÁCH: TÌM KIẾM + LỌC + SẮP XẾP + PHÂN TRANG (kết hợp được với nhau)
    // Truy vấn -> Tìm kiếm -> Lọc -> Sắp xếp -> Phân trang -> Hiển thị
    // =====================================================================

    // GET: /NganhTuyenSinh?tuKhoa=...&maKhoa=...&toHop=...&trangThai=...&diemTu=...&diemDen=...&hanNop=...&sapXep=...&trang=...
    public async Task<IActionResult> Index(
        string? tuKhoa, int? maKhoa, string? toHop, string? trangThai,
        decimal? diemTu, decimal? diemDen, string? hanNop,
        string? sapXep, int trang = 1)
    {
        var vm = new NganhTuyenSinhIndexViewModel
        {
            TuKhoa = tuKhoa?.Trim(),
            MaKhoa = maKhoa,
            ToHop = ChuanHoaMotToHop(toHop),
            TrangThai = trangThai,
            DiemTu = diemTu,
            DiemDen = diemDen,
            HanNop = hanNop,
            SapXep = CacKieuSapXep.Any(s => s.Value == sapXep) ? sapXep! : "ten_asc",
            KichThuocTrang = KichThuocTrang,
            LaQuanLy = LaQuanLy
        };

        // Nếu người dùng nhập khoảng điểm ngược thì đảo lại và báo cho họ biết
        if (vm.DiemTu.HasValue && vm.DiemDen.HasValue && vm.DiemTu > vm.DiemDen)
        {
            (vm.DiemTu, vm.DiemDen) = (vm.DiemDen, vm.DiemTu);
            vm.CanhBao = "Điểm \"từ\" lớn hơn điểm \"đến\" nên hệ thống đã đảo lại hai giá trị.";
        }

        var homNay = DateTime.Today;

        IQueryable<NganhTuyenSinh> query = _context.NganhTuyenSinhs
            .AsNoTracking()
            .Include(n => n.Khoa);

        // Thí sinh không xem các ngành chưa mở
        if (!vm.LaQuanLy)
            query = query.Where(n => n.TrangThai != TrangThaiNganh.ChuaMo);

        // ----- Tìm kiếm: tên ngành hoặc tên khoa -----
        if (!string.IsNullOrEmpty(vm.TuKhoa))
        {
            var tk = vm.TuKhoa;
            query = query.Where(n => n.TenNganh.Contains(tk) || n.Khoa!.TenKhoa.Contains(tk));
        }

        // ----- Lọc -----
        if (vm.MaKhoa.HasValue)
            query = query.Where(n => n.MaKhoa == vm.MaKhoa.Value);

        if (!string.IsNullOrEmpty(vm.ToHop))
        {
            // Tổ hợp của ngành lưu dạng "A00, A01, D01" -> so khớp chính xác từng mã, tránh khớp nhầm một phần
            var th = vm.ToHop;
            query = query.Where(n => ("," + n.ToHopXetTuyen.Replace(" ", "") + ",").Contains("," + th + ","));
        }

        if (!string.IsNullOrEmpty(vm.TrangThai))
            query = query.Where(n => n.TrangThai == vm.TrangThai);

        if (vm.DiemTu.HasValue)
            query = query.Where(n => n.DiemXetTuyenToiThieu >= vm.DiemTu.Value);

        if (vm.DiemDen.HasValue)
            query = query.Where(n => n.DiemXetTuyenToiThieu <= vm.DiemDen.Value);

        // Còn hạn: chưa quá hạn nộp. Hết hạn: đã quá hạn nộp (khớp với NganhTuyenSinh.HetHan)
        if (vm.HanNop == "conhan")
            query = query.Where(n => n.HanNopHoSo >= homNay);
        else if (vm.HanNop == "hethan")
            query = query.Where(n => n.HanNopHoSo < homNay);

        // ----- Sắp xếp (thêm MaNganh để thứ tự ổn định khi phân trang) -----
        IOrderedQueryable<NganhTuyenSinh> sapXepQuery = vm.SapXep switch
        {
            "ten_desc" => query.OrderByDescending(n => n.TenNganh),
            "han_asc" => query.OrderBy(n => n.HanNopHoSo),
            "han_desc" => query.OrderByDescending(n => n.HanNopHoSo),
            "chitieu_asc" => query.OrderBy(n => n.ChiTieu),
            "chitieu_desc" => query.OrderByDescending(n => n.ChiTieu),
            _ => query.OrderBy(n => n.TenNganh)
        };
        sapXepQuery = sapXepQuery.ThenBy(n => n.MaNganh);

        // ----- Phân trang trên truy vấn: Skip/Take (không tải hết dữ liệu về) -----
        vm.TongSoBanGhi = await sapXepQuery.CountAsync();
        vm.TongSoTrang = Math.Max(1, (int)Math.Ceiling(vm.TongSoBanGhi / (double)KichThuocTrang));
        vm.TrangHienTai = Math.Clamp(trang, 1, vm.TongSoTrang);   // xử lý đúng trang đầu / trang cuối

        vm.Items = await sapXepQuery
            .Skip((vm.TrangHienTai - 1) * KichThuocTrang)
            .Take(KichThuocTrang)
            .ToListAsync();

        await NapDanhSachLocAsync(vm);
        return View(vm);
    }

    // =====================================================================
    // CHI TIẾT
    // =====================================================================

    // GET: /NganhTuyenSinh/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var vm = await TaoDetailsViewModelAsync(id.Value);
        if (vm == null) return NotFound();

        return View(vm);
    }

    // =====================================================================
    // THÊM MỚI
    // =====================================================================

    // GET: /NganhTuyenSinh/Create
    public async Task<IActionResult> Create()
    {
        await NapDanhSachKhoaAsync(null);
        return View(new NganhTuyenSinh());   // trạng thái mặc định: Chưa mở
    }

    // POST: /NganhTuyenSinh/Create
    // [Bind] KHÔNG gồm TrangThai: ngành mới luôn ở trạng thái "Chưa mở" (chống overposting).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("TenNganh,MaKhoa,ChiTieu,ToHopXetTuyen,DiemXetTuyenToiThieu,NgayBatDauNhanHoSo,HanNopHoSo,MoTaNganh,YeuCauThiSinh")]
        NganhTuyenSinh nganh)
    {
        nganh.TrangThai = TrangThaiNganh.ChuaMo;
        nganh.TenNganh = nganh.TenNganh?.Trim() ?? string.Empty;

        await KiemTraNghiepVuAsync(nganh, maNganhHienTai: null, maKhoaCu: null);

        if (!ModelState.IsValid)
        {
            await NapDanhSachKhoaAsync(nganh.MaKhoa);
            return View(nganh);
        }

        _context.Add(nganh);
        await _context.SaveChangesAsync();

        TempData["ThanhCong"] = $"Đã thêm ngành \"{nganh.TenNganh}\" (trạng thái: {nganh.TrangThai}).";
        return RedirectToAction(nameof(Details), new { id = nganh.MaNganh });
    }

    // =====================================================================
    // CHỈNH SỬA (không sửa trạng thái ở đây - dùng ChangeStatus)
    // =====================================================================

    // GET: /NganhTuyenSinh/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var nganh = await _context.NganhTuyenSinhs.FindAsync(id);
        if (nganh == null) return NotFound();

        await NapDanhSachKhoaAsync(nganh.MaKhoa);
        return View(nganh);
    }

    // POST: /NganhTuyenSinh/Edit/5
    // Nạp bản ghi từ DB rồi chỉ cập nhật các trường được phép (TryUpdateModelAsync + danh sách trường)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id)
    {
        var nganh = await _context.NganhTuyenSinhs.FindAsync(id);
        if (nganh == null) return NotFound();

        var maKhoaCu = nganh.MaKhoa;

        var capNhatDuoc = await TryUpdateModelAsync(nganh, "",
            n => n.TenNganh, n => n.MaKhoa, n => n.ChiTieu, n => n.ToHopXetTuyen,
            n => n.DiemXetTuyenToiThieu, n => n.NgayBatDauNhanHoSo, n => n.HanNopHoSo,
            n => n.MoTaNganh, n => n.YeuCauThiSinh);

        if (capNhatDuoc)
        {
            nganh.TenNganh = nganh.TenNganh.Trim();
            await KiemTraNghiepVuAsync(nganh, maNganhHienTai: id, maKhoaCu: maKhoaCu);
        }

        if (!capNhatDuoc || !ModelState.IsValid)
        {
            await NapDanhSachKhoaAsync(nganh.MaKhoa);
            return View(nganh);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Không thể lưu thay đổi. Vui lòng thử lại.");
            await NapDanhSachKhoaAsync(nganh.MaKhoa);
            return View(nganh);
        }

        TempData["ThanhCong"] = "Đã cập nhật thông tin ngành tuyển sinh.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // =====================================================================
    // CẬP NHẬT TRẠNG THÁI
    // =====================================================================

    // POST: /NganhTuyenSinh/ChangeStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, string trangThaiMoi, string? returnUrl)
    {
        var nganh = await _context.NganhTuyenSinhs.FindAsync(id);
        if (nganh == null) return NotFound();

        // Chỉ cho chuyển theo luồng hợp lệ
        if (!TrangThaiChoPhep(nganh.TrangThai).Contains(trangThaiMoi))
        {
            TempData["Loi"] = $"Không thể chuyển ngành từ \"{nganh.TrangThai}\" sang \"{trangThaiMoi}\".";
            return QuayLai(returnUrl, id);
        }

        // Không cho chuyển sang Đang tuyển nếu dữ liệu bắt buộc chưa hợp lệ
        if (trangThaiMoi == TrangThaiNganh.DangTuyen)
        {
            var loi = await KiemTraDuDieuKienMoTuyenAsync(nganh);
            if (loi.Count > 0)
            {
                TempData["Loi"] = "Không thể chuyển sang \"Đang tuyển\": " + string.Join("; ", loi) + ".";
                return QuayLai(returnUrl, id);
            }
        }

        var trangThaiCu = nganh.TrangThai;
        nganh.TrangThai = trangThaiMoi;
        await _context.SaveChangesAsync();

        TempData["ThanhCong"] = $"Đã chuyển ngành \"{nganh.TenNganh}\" từ \"{trangThaiCu}\" sang \"{trangThaiMoi}\".";
        return QuayLai(returnUrl, id);
    }

    // =====================================================================
    // XÓA - có kiểm soát để không làm mất dữ liệu lịch sử
    // =====================================================================

    // GET: /NganhTuyenSinh/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var vm = await TaoDetailsViewModelAsync(id.Value);
        if (vm == null) return NotFound();

        return View(vm);
    }

    // POST: /NganhTuyenSinh/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var nganh = await _context.NganhTuyenSinhs.FindAsync(id);
        if (nganh == null) return RedirectToAction(nameof(Index));

        // Ngành đã có hồ sơ xét tuyển thì KHÔNG được xóa (kể cả hồ sơ đã hủy, vì còn là dữ liệu lịch sử)
        var soHoSo = await _context.HoSoXetTuyens.CountAsync(h => h.MaNganh == id);
        if (soHoSo > 0)
        {
            TempData["Loi"] = $"Không thể xóa ngành \"{nganh.TenNganh}\" vì đã có {soHoSo} hồ sơ xét tuyển. " +
                              "Hãy chuyển ngành sang trạng thái \"Đã đóng\" để giữ dữ liệu lịch sử.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            _context.NganhTuyenSinhs.Remove(nganh);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["Loi"] = "Không thể xóa ngành vì còn dữ liệu liên quan.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["ThanhCong"] = $"Đã xóa ngành \"{nganh.TenNganh}\".";
        return RedirectToAction(nameof(Index));
    }

    // =====================================================================
    // HÀM HỖ TRỢ - NGHIỆP VỤ
    // =====================================================================

    /// <summary>
    /// Các quy tắc không viết được bằng Data Annotation (kiểm tra tại Controller):
    /// khoa hợp lệ, không trùng tên ngành trong khoa, không giảm chỉ tiêu dưới số đã trúng tuyển.
    /// </summary>
    private async Task KiemTraNghiepVuAsync(NganhTuyenSinh nganh, int? maNganhHienTai, int? maKhoaCu)
    {
        // Chuẩn hóa tổ hợp: "a00 ,A01;d01" -> "A00, A01, D01"
        nganh.ToHopXetTuyen = ChuanHoaDanhSachToHop(nganh.ToHopXetTuyen);
        if (string.IsNullOrEmpty(nganh.ToHopXetTuyen))
            ModelState.AddModelError(nameof(nganh.ToHopXetTuyen), "Tổ hợp xét tuyển là bắt buộc");

        // Khoa phải tồn tại (select lấy từ DB) và đang hoạt động khi chọn khoa mới
        var khoa = await _context.Khoas.AsNoTracking().FirstOrDefaultAsync(k => k.MaKhoa == nganh.MaKhoa);
        if (khoa == null)
            ModelState.AddModelError(nameof(nganh.MaKhoa), "Vui lòng chọn khoa hợp lệ");
        else if (!khoa.TrangThai && nganh.MaKhoa != maKhoaCu)
            ModelState.AddModelError(nameof(nganh.MaKhoa), "Khoa này đang ngừng hoạt động");

        // Không trùng tên ngành trong cùng một khoa
        if (!string.IsNullOrWhiteSpace(nganh.TenNganh) && khoa != null)
        {
            var ten = nganh.TenNganh;
            var trung = await _context.NganhTuyenSinhs.AnyAsync(n =>
                n.MaKhoa == nganh.MaKhoa && n.TenNganh == ten && n.MaNganh != (maNganhHienTai ?? 0));
            if (trung)
                ModelState.AddModelError(nameof(nganh.TenNganh), "Khoa này đã có ngành cùng tên");
        }

        // Khi sửa: chỉ tiêu không được thấp hơn số hồ sơ đã trúng tuyển
        if (maNganhHienTai.HasValue && nganh.ChiTieu > 0)
        {
            var soTrungTuyen = await _context.HoSoXetTuyens
                .CountAsync(h => h.MaNganh == maNganhHienTai.Value && h.TrangThai == TrangThaiHoSo.TrungTuyen);
            if (nganh.ChiTieu < soTrungTuyen)
                ModelState.AddModelError(nameof(nganh.ChiTieu),
                    $"Chỉ tiêu không được nhỏ hơn số hồ sơ đã trúng tuyển ({soTrungTuyen})");
        }
    }

    /// <summary>Kiểm tra dữ liệu bắt buộc trước khi cho ngành chuyển sang "Đang tuyển".</summary>
    private async Task<List<string>> KiemTraDuDieuKienMoTuyenAsync(NganhTuyenSinh nganh)
    {
        var loi = new List<string>();

        // Data Annotation + IValidatableObject (ngày bắt đầu / hạn nộp)
        var ketQua = new List<ValidationResult>();
        Validator.TryValidateObject(nganh, new ValidationContext(nganh), ketQua, validateAllProperties: true);
        loi.AddRange(ketQua.Select(r => r.ErrorMessage ?? "Dữ liệu không hợp lệ"));

        var khoa = await _context.Khoas.AsNoTracking().FirstOrDefaultAsync(k => k.MaKhoa == nganh.MaKhoa);
        if (khoa == null) loi.Add("Khoa của ngành không tồn tại");
        else if (!khoa.TrangThai) loi.Add("Khoa của ngành đang ngừng hoạt động");

        if (nganh.HanNopHoSo.Date < DateTime.Today)
            loi.Add("Hạn nộp hồ sơ đã qua, hãy sửa lại hạn nộp trước khi mở tuyển sinh");

        return loi;
    }

    private static string[] TrangThaiChoPhep(string trangThaiHienTai) =>
        LuongTrangThai.TryGetValue(trangThaiHienTai, out var ds) ? ds : Array.Empty<string>();

    private async Task<NganhTuyenSinhDetailsViewModel?> TaoDetailsViewModelAsync(int id)
    {
        var nganh = await _context.NganhTuyenSinhs
            .AsNoTracking()
            .Include(n => n.Khoa)
            .FirstOrDefaultAsync(n => n.MaNganh == id);

        if (nganh == null) return null;

        // Thí sinh không được xem ngành chưa mở
        if (!LaQuanLy && nganh.TrangThai == TrangThaiNganh.ChuaMo) return null;

        var vm = new NganhTuyenSinhDetailsViewModel { Nganh = nganh, LaQuanLy = LaQuanLy };

        if (LaQuanLy)
        {
            vm.SoHoSo = await _context.HoSoXetTuyens.CountAsync(h => h.MaNganh == id);
            vm.SoTrungTuyen = await _context.HoSoXetTuyens
                .CountAsync(h => h.MaNganh == id && h.TrangThai == TrangThaiHoSo.TrungTuyen);
            vm.TrangThaiChoPhep = TrangThaiChoPhep(nganh.TrangThai).ToList();
        }

        return vm;
    }

    private IActionResult QuayLai(string? returnUrl, int id) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Details), new { id });

    // =====================================================================
    // HÀM HỖ TRỢ - DỮ LIỆU CHO CÁC Ô SELECT
    // =====================================================================

    private async Task NapDanhSachKhoaAsync(int? maKhoaDangChon)
    {
        // Chỉ hiện khoa đang hoạt động (và khoa hiện tại của ngành khi sửa)
        var khoas = await _context.Khoas
            .AsNoTracking()
            .Where(k => k.TrangThai || k.MaKhoa == maKhoaDangChon)
            .OrderBy(k => k.TenKhoa)
            .ToListAsync();

        ViewBag.DanhSachKhoa = new SelectList(khoas, nameof(Khoa.MaKhoa), nameof(Khoa.TenKhoa), maKhoaDangChon);
    }

    private async Task NapDanhSachLocAsync(NganhTuyenSinhIndexViewModel vm)
    {
        // Khoa
        var khoas = await _context.Khoas.AsNoTracking().OrderBy(k => k.TenKhoa).ToListAsync();
        vm.DanhSachKhoa = khoas
            .Select(k => new SelectListItem(k.TenKhoa, k.MaKhoa.ToString(), vm.MaKhoa == k.MaKhoa))
            .ToList();

        // Tổ hợp xét tuyển: tách từ danh sách "A00, A01, D01" của các ngành (danh mục nhỏ, không phải dữ liệu phân trang)
        var chuoiToHop = await _context.NganhTuyenSinhs.AsNoTracking()
            .Select(n => n.ToHopXetTuyen).ToListAsync();
        vm.DanhSachToHop = chuoiToHop
            .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Select(s => s.ToUpperInvariant())
            .Distinct()
            .OrderBy(s => s)
            .Select(s => new SelectListItem(s, s, vm.ToHop == s))
            .ToList();

        // Trạng thái (thí sinh không thấy "Chưa mở")
        var trangThais = new List<string>
        {
            TrangThaiNganh.ChuaMo, TrangThaiNganh.DangTuyen, TrangThaiNganh.TamDung, TrangThaiNganh.DaDong
        };
        if (!vm.LaQuanLy) trangThais.Remove(TrangThaiNganh.ChuaMo);
        vm.DanhSachTrangThai = trangThais
            .Select(t => new SelectListItem(t, t, vm.TrangThai == t))
            .ToList();

        // Còn hạn / Hết hạn
        vm.DanhSachHanNop = new List<SelectListItem>
        {
            new("Còn hạn nhận hồ sơ", "conhan", vm.HanNop == "conhan"),
            new("Hết hạn nhận hồ sơ", "hethan", vm.HanNop == "hethan"),
        };

        // Sắp xếp
        vm.DanhSachSapXep = CacKieuSapXep
            .Select(s => new SelectListItem(s.Text, s.Value, vm.SapXep == s.Value))
            .ToList();
    }

    // =====================================================================
    // HÀM HỖ TRỢ - CHUẨN HÓA TỔ HỢP
    // =====================================================================

    private static string? ChuanHoaMotToHop(string? toHop) =>
        string.IsNullOrWhiteSpace(toHop) ? null : toHop.Replace(" ", "").ToUpperInvariant();

    private static string ChuanHoaDanhSachToHop(string? dsToHop) =>
        string.Join(", ",
            (dsToHop ?? string.Empty)
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToUpperInvariant())
                .Distinct());
}
