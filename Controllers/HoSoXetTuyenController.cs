//Họ và tên: Nguyễn Đức Nghĩa
//Mã sinh viên: 23103100307
//Nội dung thực hiện: Controller quản lý tiếp nhận hồ sơ, sơ tuyển, luồng trạng thái và kiểm soát chỉ tiêu theo kiến trúc N-Tier (Task M4.1, M4.3, M4.4, M4.5, M4.6).

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Services;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.ViewModels;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Controllers
{
    /// <summary>
    /// Module 4: Tiếp nhận hồ sơ - Sơ tuyển - Quản lý trạng thái - Kiểm soát chỉ tiêu.
    /// Quyền truy cập: Chỉ Admin và Cán bộ tuyển sinh.
    /// </summary>
    [Authorize(Roles = VaiTro.Admin + "," + VaiTro.CanBoTuyenSinh)]
    public class HoSoXetTuyenController : Controller
    {
        private readonly IHoSoXetTuyenService _hoSoService;
        private readonly ILogger<HoSoXetTuyenController> _logger;

        public HoSoXetTuyenController(
            IHoSoXetTuyenService hoSoService,
            ILogger<HoSoXetTuyenController> logger)
        {
            _hoSoService = hoSoService;
            _logger = logger;
        }

        /// <summary>
        /// M4.1: Danh sách hồ sơ cần xử lý với đầy đủ tìm kiếm, bộ lọc (khoa, trạng thái, ngày nộp), sắp xếp và phân trang LINQ.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(HoSoXetTuyenIndexViewModel filter)
        {
            var model = await _hoSoService.GetDanhSachHoSoAsync(filter);
            return View(model);
        }

        /// <summary>
        /// M4.1 & M4.6: Xem chi tiết hồ sơ xét tuyển, thông tin đối chiếu và cảnh báo chỉ tiêu ngành.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _hoSoService.GetChiTietHoSoAsync(id);
            if (model == null)
            {
                return NotFound($"Không tìm thấy hồ sơ xét tuyển có mã #{id}.");
            }

            return View(model);
        }

        /// <summary>
        /// M4.3 & M4.4: Màn sơ tuyển hồ sơ xét tuyển.
        /// Hiển thị thông tin thí sinh, ngành tuyển sinh, bảng so sánh tổ hợp/điểm số tham khảo và 4 điều kiện kiểm tra.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SoTuyen(int id)
        {
            var model = await _hoSoService.GetThongTinSoTuyenAsync(id);
            if (model == null)
            {
                return NotFound($"Không tìm thấy hồ sơ xét tuyển có mã #{id}.");
            }

            return View(model);
        }

        /// <summary>
        /// M4.4 & M4.5: Xử lý duyệt / từ chối sơ tuyển.
        /// Kiểm tra 4 điều kiện nghiệp vụ, cập nhật trạng thái (Đạt/Không đạt), NgayXuLy và NhanXetSoTuyen.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoTuyen(int id, SoTuyenViewModel formModel)
        {
            if (id != formModel.MaHoSo)
            {
                return BadRequest("Dữ liệu gửi lên không khớp với mã hồ sơ đang xử lý.");
            }

            if (!ModelState.IsValid)
            {
                // Nạp lại thông tin hiển thị tham khảo
                var reloadModel = await _hoSoService.GetThongTinSoTuyenAsync(id);
                if (reloadModel == null) return NotFound();

                reloadModel.KetQuaSoTuyen = formModel.KetQuaSoTuyen;
                reloadModel.NhanXetSoTuyen = formModel.NhanXetSoTuyen;
                return View(reloadModel);
            }

            var (thanhCong, thongBao) = await _hoSoService.XuLySoTuyenAsync(id, formModel.KetQuaSoTuyen, formModel.NhanXetSoTuyen);
            if (!thanhCong)
            {
                ModelState.AddModelError(string.Empty, thongBao);

                var reloadModel = await _hoSoService.GetThongTinSoTuyenAsync(id);
                if (reloadModel == null) return NotFound();

                reloadModel.KetQuaSoTuyen = formModel.KetQuaSoTuyen;
                reloadModel.NhanXetSoTuyen = formModel.NhanXetSoTuyen;
                return View(reloadModel);
            }

            TempData["SuccessMessage"] = thongBao;
            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// M4.6: Đóng ngành tuyển sinh khi đã đủ hoặc vượt chỉ tiêu trúng tuyển.
        /// Bảo toàn toàn bộ hồ sơ còn lại, không xóa dữ liệu.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DongNganh(int maNganh, int maHoSo)
        {
            var (thanhCong, thongBao) = await _hoSoService.DongNganhTuyenSinhAsync(maNganh);
            if (thanhCong)
            {
                TempData["SuccessMessage"] = thongBao;
            }
            else
            {
                TempData["ErrorMessage"] = thongBao;
            }

            return RedirectToAction(nameof(Details), new { id = maHoSo });
        }
    }
}

