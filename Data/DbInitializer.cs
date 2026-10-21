// Họ và tên: Nguyễn Đức Nghĩa
// Mã sinh viên: 23103100307
// Nội dung thực hiện: Tạo database, tạo bảng, thiết lập quan hệ giữa các bảng, thêm dữ liệu mẫu.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Tự động apply Migration
            context.Database.Migrate();

            // Nếu đã có dữ liệu Khoa thì bỏ qua
            if (context.Khoas.Any())
            {
                return;
            }

            // ==========================================
            // 1. KHOA (05 khoa theo yêu cầu)
            // ==========================================
            var khoas = new List<Khoa>
            {
                new Khoa { TenKhoa = "Công nghệ thông tin", MoTa = "Đào tạo các ngành CNTT mũi nhọn", EmailLienHe = "cntt@uneti.edu.vn", TrangThai = true },
                new Khoa { TenKhoa = "Điện - Điện tử", MoTa = "Đào tạo kỹ sư điện, điều khiển và tự động hóa", EmailLienHe = "diendientu@uneti.edu.vn", TrangThai = true },
                new Khoa { TenKhoa = "Cơ khí", MoTa = "Đào tạo kỹ thuật cơ khí, cơ điện tử, ô tô", EmailLienHe = "cokhi@uneti.edu.vn", TrangThai = true },
                new Khoa { TenKhoa = "Kinh tế", MoTa = "Đào tạo quản trị kinh doanh, kế toán, tài chính", EmailLienHe = "kinhte@uneti.edu.vn", TrangThai = true },
                new Khoa { TenKhoa = "Ngoại ngữ", MoTa = "Đào tạo ngôn ngữ Anh, Trung Quốc", EmailLienHe = "ngoaignu@uneti.edu.vn", TrangThai = true }
            };
            context.Khoas.AddRange(khoas);
            context.SaveChanges();

            // ==========================================
            // 2. NGÀNH TUYỂN SINH (15 ngành: Đang tuyển, Đã đóng, Hết hạn, Chưa mở)
            // ==========================================
            var today = DateTime.Today;
            var nganhs = new List<NganhTuyenSinh>
            {
                // Khoa CNTT
                new NganhTuyenSinh
                {
                    TenNganh = "Công nghệ thông tin",
                    MaKhoa = khoas[0].MaKhoa,
                    ChiTieu = 120,
                    ToHopXetTuyen = "A00, A01, D01",
                    DiemXetTuyenToiThieu = 22.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-15),
                    HanNopHoSo = today.AddDays(20),
                    MoTaNganh = "Chương trình đào tạo kỹ sư công nghệ thông tin định hướng ứng dụng.",
                    YeuCauThiSinh = "Tốt nghiệp THPT, có đam mê lập trình.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Kỹ thuật phần mềm",
                    MaKhoa = khoas[0].MaKhoa,
                    ChiTieu = 80,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 21.5m,
                    NgayBatDauNhanHoSo = today.AddDays(-10),
                    HanNopHoSo = today.AddDays(25),
                    MoTaNganh = "Đào tạo chuyên sâu về kiến trúc phần mềm, quy trình phát triển Agile/Scrum.",
                    YeuCauThiSinh = "Tư duy logic tốt, tiếng Anh cơ bản.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "An toàn thông tin",
                    MaKhoa = khoas[0].MaKhoa,
                    ChiTieu = 60,
                    ToHopXetTuyen = "A00, A01, D01",
                    DiemXetTuyenToiThieu = 21.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-12),
                    HanNopHoSo = today.AddDays(18),
                    MoTaNganh = "Bảo mật hệ thống, phòng thủ không gian mạng.",
                    YeuCauThiSinh = "Đạo đức tốt, cẩn thận, kỷ luật.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Trí tuệ nhân tạo",
                    MaKhoa = khoas[0].MaKhoa,
                    ChiTieu = 50,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 23.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-5),
                    HanNopHoSo = today.AddDays(30),
                    MoTaNganh = "Nghiên cứu thị giác máy tính, NLP, học máy hiện đại.",
                    YeuCauThiSinh = "Toán học tốt, tư duy giải thuật xuất sắc.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Khoa học máy tính",
                    MaKhoa = khoas[0].MaKhoa,
                    ChiTieu = 70,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 20.5m,
                    NgayBatDauNhanHoSo = today.AddDays(10),
                    HanNopHoSo = today.AddDays(40),
                    MoTaNganh = "Nền tảng thuật toán, tính toán đám mây và hệ thống phân tán.",
                    YeuCauThiSinh = "Đam mê khoa học cơ bản.",
                    TrangThai = TrangThaiNganh.ChuaMo // Chưa mở
                },

                // Khoa Điện - Điện tử
                new NganhTuyenSinh
                {
                    TenNganh = "Công nghệ kỹ thuật điện, điện tử",
                    MaKhoa = khoas[1].MaKhoa,
                    ChiTieu = 100,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 19.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-20),
                    HanNopHoSo = today.AddDays(15),
                    MoTaNganh = "Thiết kế vi mạch, hệ thống điện thông minh.",
                    YeuCauThiSinh = "Sức khỏe tốt, chịu khó học hỏi.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Điều khiển và tự động hóa",
                    MaKhoa = khoas[1].MaKhoa,
                    ChiTieu = 90,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 20.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-15),
                    HanNopHoSo = today.AddDays(15),
                    MoTaNganh = "Tự động hóa nhà máy, PLC, SCADA, Robotics.",
                    YeuCauThiSinh = "Kỹ năng thực hành tốt.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Kỹ thuật viễn thông",
                    MaKhoa = khoas[1].MaKhoa,
                    ChiTieu = 60,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 18.5m,
                    NgayBatDauNhanHoSo = today.AddDays(-40),
                    HanNopHoSo = today.AddDays(-5), // Hết hạn
                    MoTaNganh = "Mạng 5G, truyền thông không dây, IoT.",
                    YeuCauThiSinh = "Tốt nghiệp THPT.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },

                // Khoa Cơ khí
                new NganhTuyenSinh
                {
                    TenNganh = "Công nghệ kỹ thuật cơ khí",
                    MaKhoa = khoas[2].MaKhoa,
                    ChiTieu = 80,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 18.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-30),
                    HanNopHoSo = today.AddDays(-2), // Hết hạn
                    MoTaNganh = "Gia công CNC, CAD/CAM, chế tạo máy hiện đại.",
                    YeuCauThiSinh = "Độ khéo tay, sức khỏe đạt loại khá.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Công nghệ kỹ thuật ô tô",
                    MaKhoa = khoas[2].MaKhoa,
                    ChiTieu = 110,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 21.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-10),
                    HanNopHoSo = today.AddDays(20),
                    MoTaNganh = "Kỹ thuật động cơ ô tô, xe điện, chẩn đoán ô tô số.",
                    YeuCauThiSinh = "Sức khỏe tốt, ham thích công nghệ ô tô.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Kỹ thuật cơ điện tử",
                    MaKhoa = khoas[2].MaKhoa,
                    ChiTieu = 70,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 19.5m,
                    NgayBatDauNhanHoSo = today.AddDays(-15),
                    HanNopHoSo = today.AddDays(15),
                    MoTaNganh = "Tích hợp cơ khí, điện tử và điều khiển số.",
                    YeuCauThiSinh = "Tư duy kỹ thuật tốt.",
                    TrangThai = TrangThaiNganh.TamDung // Tạm dừng
                },

                // Khoa Kinh tế
                new NganhTuyenSinh
                {
                    TenNganh = "Quản trị kinh doanh",
                    MaKhoa = khoas[3].MaKhoa,
                    ChiTieu = 150,
                    ToHopXetTuyen = "A00, A01, D01",
                    DiemXetTuyenToiThieu = 20.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-20),
                    HanNopHoSo = today.AddDays(10),
                    MoTaNganh = "Khởi nghiệp, quản trị doanh nghiệp, marketing và nhân sự.",
                    YeuCauThiSinh = "Năng động, giao tiếp tự tin.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Kế toán",
                    MaKhoa = khoas[3].MaKhoa,
                    ChiTieu = 100,
                    ToHopXetTuyen = "A00, A01, D01",
                    DiemXetTuyenToiThieu = 19.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-20),
                    HanNopHoSo = today.AddDays(15),
                    MoTaNganh = "Kế toán doanh nghiệp, kiểm toán, thuế tài chính.",
                    YeuCauThiSinh = "Cẩn thận, tỉ mỉ, trung thực.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Tài chính - Ngân hàng",
                    MaKhoa = khoas[3].MaKhoa,
                    ChiTieu = 80,
                    ToHopXetTuyen = "A00, A01, D01",
                    DiemXetTuyenToiThieu = 19.5m,
                    NgayBatDauNhanHoSo = today.AddDays(-40),
                    HanNopHoSo = today.AddDays(-10), // Hết hạn
                    MoTaNganh = "Ngân hàng số, đầu tư tài chính, chứng khoán.",
                    YeuCauThiSinh = "Nhạy bén với số liệu.",
                    TrangThai = TrangThaiNganh.DaDong // Đã đóng
                },

                // Khoa Ngoại ngữ
                new NganhTuyenSinh
                {
                    TenNganh = "Ngôn ngữ Anh",
                    MaKhoa = khoas[4].MaKhoa,
                    ChiTieu = 100,
                    ToHopXetTuyen = "D01, D07",
                    DiemXetTuyenToiThieu = 20.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-15),
                    HanNopHoSo = today.AddDays(15),
                    MoTaNganh = "Tiếng Anh thương mại, biên phiên dịch chuyên ngành.",
                    YeuCauThiSinh = "Điểm môn tiếng Anh THPT đạt từ 7.0 trở lên.",
                    TrangThai = TrangThaiNganh.DangTuyen
                }
            };
            context.NganhTuyenSinhs.AddRange(nganhs);
            context.SaveChanges();

            // ==========================================
            // 3. NGƯỜI PHỎNG VẤN (05 người phỏng vấn)
            // ==========================================
            var nguoiPVs = new List<NguoiPhongVan>
            {
                new NguoiPhongVan { HoTen = "TS. Nguyễn Văn Nam", Email = "namnv@uneti.edu.vn", TrangThai = true },
                new NguoiPhongVan { HoTen = "ThS. Trần Thị Mai", Email = "maitt@uneti.edu.vn", TrangThai = true },
                new NguoiPhongVan { HoTen = "PGS.TS. Lê Hoàng Long", Email = "longlh@uneti.edu.vn", TrangThai = true },
                new NguoiPhongVan { HoTen = "TS. Phạm Thu Hà", Email = "hapt@uneti.edu.vn", TrangThai = true },
                new NguoiPhongVan { HoTen = "ThS. Vũ Đức Mạnh", Email = "manhvd@uneti.edu.vn", TrangThai = true }
            };
            context.NguoiPhongVans.AddRange(nguoiPVs);
            context.SaveChanges();

            // ==========================================
            // 4. TÀI KHOẢN (2 Admin, 3 Cán bộ, 30 Thí sinh)
            // ==========================================
            var taiKhoans = new List<TaiKhoan>
            {
                // Admin (02)
                new TaiKhoan { TenDangNhap = "admin1", MatKhau = "123456", HoTen = "Quản trị viên 1", Email = "admin1@uneti.edu.vn", VaiTro = VaiTro.Admin, TrangThai = true },
                new TaiKhoan { TenDangNhap = "admin2", MatKhau = "123456", HoTen = "Quản trị viên 2", Email = "admin2@uneti.edu.vn", VaiTro = VaiTro.Admin, TrangThai = true },

                // Cán bộ tuyển sinh (03)
                new TaiKhoan { TenDangNhap = "canbo1", MatKhau = "123456", HoTen = "Cán bộ Nguyễn Văn A", Email = "canbo1@uneti.edu.vn", VaiTro = VaiTro.CanBoTuyenSinh, TrangThai = true },
                new TaiKhoan { TenDangNhap = "canbo2", MatKhau = "123456", HoTen = "Cán bộ Lê Thị B", Email = "canbo2@uneti.edu.vn", VaiTro = VaiTro.CanBoTuyenSinh, TrangThai = true },
                new TaiKhoan { TenDangNhap = "canbo3", MatKhau = "123456", HoTen = "Cán bộ Trần Văn C", Email = "canbo3@uneti.edu.vn", VaiTro = VaiTro.CanBoTuyenSinh, TrangThai = true }
            };

            // 30 Tài khoản thí sinh
            var danhSachTen = new[]
            {
                "Nguyễn Văn An", "Trần Thị Bích", "Lê Văn Cường", "Phạm Thị Dung", "Hoàng Văn Em",
                "Đỗ Thị Giang", "Vũ Văn Hùng", "Bùi Thị Lan", "Đinh Văn Minh", "Ngô Thị Ngọc",
                "Phan Văn Oanh", "Dương Thị Phương", "Mai Văn Quân", "Lý Thị Sen", "Đoàn Văn Thắng",
                "Trịnh Thị Uyên", "Lưu Văn Việt", "Hà Thị Xuân", "Cao Văn Yên", "Đào Thị Ánh",
                "Nguyễn Tuấn Kiệt", "Trần Bảo Ngọc", "Lê Hoàng Yến", "Phạm Quốc Bảo", "Hoàng Thảo My",
                "Đỗ Minh Trí", "Vũ Hải Đăng", "Bùi Khánh Linh", "Đinh Hữu Phước", "Ngô Thùy Trang"
            };

            for (int i = 1; i <= 30; i++)
            {
                string uname = $"thisinh{i:D2}";
                taiKhoans.Add(new TaiKhoan
                {
                    TenDangNhap = uname,
                    MatKhau = "123456",
                    HoTen = danhSachTen[i - 1],
                    Email = $"{uname}@gmail.com",
                    VaiTro = VaiTro.ThiSinh,
                    TrangThai = true
                });
            }

            context.TaiKhoans.AddRange(taiKhoans);
            context.SaveChanges();

            // ==========================================
            // 5. THÍ SINH (30 thí sinh ứng với 30 tài khoản thí sinh)
            // ==========================================
            var thiSinhTaiKhoans = taiKhoans.Where(t => t.VaiTro == VaiTro.ThiSinh).ToList();
            var thiSinhs = new List<ThiSinh>();

            var toHopList = new[] { "A00", "A01", "D01", "A00", "A01" };
            var truongList = new[] { "THPT Chuyên Hà Nội - Amsterdam", "THPT Chu Văn An", "THPT Kim Liên", "THPT Thăng Long", "THPT Trần Phú" };

            for (int i = 0; i < thiSinhTaiKhoans.Count; i++)
            {
                var tk = thiSinhTaiKhoans[i];
                decimal diem = 19.5m + (i % 10) * 0.9m; // Dao động 19.5 -> 27.6
                if (diem > 29.5m) diem = 28.5m;

                thiSinhs.Add(new ThiSinh
                {
                    MaTaiKhoan = tk.MaTaiKhoan,
                    HoTen = tk.HoTen,
                    NgaySinh = new DateTime(2007, (i % 12) + 1, (i % 27) + 1),
                    GioiTinh = (i % 2 == 0) ? GioiTinhConst.Nam : GioiTinhConst.Nu,
                    SoDienThoai = $"098{i:D7}",
                    Email = tk.Email,
                    DiaChi = $"Số {i + 12}, Đường Giải Phóng, Hà Nội",
                    ToHopXetTuyen = toHopList[i % toHopList.Length],
                    TruongTHPT = truongList[i % truongList.Length],
                    DiemThiTHPT = diem,
                    ThanhTichNoiBat = (i % 3 == 0) ? "Đạt giải Học sinh giỏi cấp tỉnh môn Toán" : "Học sinh giỏi 3 năm THPT",
                    TrangThai = true
                });
            }
            context.ThiSinhs.AddRange(thiSinhs);
            context.SaveChanges();

            // ==========================================
            // 6. HỒ SƠ XÉT TUYỂN (45 hồ sơ trải đủ trạng thái)
            // ==========================================
            var hoSos = new List<HoSoXetTuyen>();
            var trangThais = new[]
            {
                TrangThaiHoSo.ChoDuyet,
                TrangThaiHoSo.DatSoTuyen,
                TrangThaiHoSo.KhongDatSoTuyen,
                TrangThaiHoSo.ChoPhongVan,
                TrangThaiHoSo.DaPhongVan,
                TrangThaiHoSo.TrungTuyen,
                TrangThaiHoSo.KhongTrungTuyen,
                TrangThaiHoSo.DaHuy
            };

            for (int i = 0; i < 45; i++)
            {
                var ts = thiSinhs[i % thiSinhs.Count];
                var ng = nganhs[i % 8]; // Chọn trong các ngành Đang tuyển
                var status = trangThais[i % trangThais.Length];

                var hs = new HoSoXetTuyen
                {
                    MaThiSinh = ts.MaThiSinh,
                    MaNganh = ng.MaNganh,
                    NgayNop = today.AddDays(-14 + (i % 10)),
                    NguyenVongXetTuyen = $"Đăng ký nguyện vọng vào ngành {ng.TenNganh} để trở thành kỹ sư giỏi.",
                    GhiChu = "Đã nộp đầy đủ học bạ và giấy chứng nhận tốt nghiệp tạm thời.",
                    TrangThai = status,
                    NgayXuLy = (status != TrangThaiHoSo.ChoDuyet) ? today.AddDays(-5 + (i % 4)) : null,
                    NhanXetSoTuyen = (status != TrangThaiHoSo.ChoDuyet)
                        ? (status == TrangThaiHoSo.KhongDatSoTuyen ? "Điểm xét tuyển chưa đạt ngưỡng yêu cầu" : "Hồ sơ đầy đủ, đạt yêu cầu sơ tuyển")
                        : null
                };
                hoSos.Add(hs);
            }
            context.HoSoXetTuyens.AddRange(hoSos);
            context.SaveChanges();

            // ==========================================
            // 7. LỊCH PHỎNG VẤN (18 lịch phỏng vấn)
            // ==========================================
            // Lập lịch cho các hồ sơ ở trạng thái Đạt sơ tuyển, Chờ phỏng vấn, Đã phỏng vấn, Trúng tuyển, Không trúng tuyển
            var hsCanLich = hoSos.Where(h =>
                h.TrangThai == TrangThaiHoSo.DatSoTuyen ||
                h.TrangThai == TrangThaiHoSo.ChoPhongVan ||
                h.TrangThai == TrangThaiHoSo.DaPhongVan ||
                h.TrangThai == TrangThaiHoSo.TrungTuyen ||
                h.TrangThai == TrangThaiHoSo.KhongTrungTuyen
            ).Take(18).ToList();

            var lichPVs = new List<LichPhongVan>();
            for (int i = 0; i < hsCanLich.Count; i++)
            {
                var hs = hsCanLich[i];
                var npv = nguoiPVs[i % nguoiPVs.Count];

                // Nếu hồ sơ Đã phỏng vấn, Trúng tuyển, Không trúng tuyển => Lịch Đã hoàn thành (quá khứ)
                // Nếu Chờ phỏng vấn => Lịch Đã lên lịch (tương lai)
                bool isPast = (hs.TrangThai == TrangThaiHoSo.DaPhongVan || hs.TrangThai == TrangThaiHoSo.TrungTuyen || hs.TrangThai == TrangThaiHoSo.KhongTrungTuyen);
                DateTime start = isPast ? today.AddDays(-3).AddHours(8 + (i % 8)) : today.AddDays(2 + (i % 5)).AddHours(8 + (i % 8));

                string statusLich = isPast ? TrangThaiLich.DaHoanThanh : (i == hsCanLich.Count - 1 ? TrangThaiLich.DaHuy : TrangThaiLich.DaLenLich);

                lichPVs.Add(new LichPhongVan
                {
                    MaHoSo = hs.MaHoSo,
                    ThoiGianBatDau = start,
                    ThoiGianKetThuc = start.AddMinutes(45),
                    HinhThucPhongVan = (i % 2 == 0) ? "Trực tiếp" : "Trực tuyến",
                    DiaDiemHoacLienKet = (i % 2 == 0) ? $"Phòng PV {101 + i % 5}, Tòa nhà HA8" : "https://meet.google.com/xyz-abcd-efg",
                    MaNguoiPhongVan = npv.MaNguoiPhongVan,
                    GhiChu = "Phỏng vấn đánh giá kiến thức cơ sở và năng lực tư duy.",
                    TrangThai = statusLich
                });
            }
            context.LichPhongVans.AddRange(lichPVs);
            context.SaveChanges();

            // ==========================================
            // 8. KẾT QUẢ XÉT TUYỂN (Ghi nhận cho các hồ sơ đã phỏng vấn)
            // ==========================================
            var hsCoKetQua = hoSos.Where(h =>
                h.TrangThai == TrangThaiHoSo.DaPhongVan ||
                h.TrangThai == TrangThaiHoSo.TrungTuyen ||
                h.TrangThai == TrangThaiHoSo.KhongTrungTuyen
            ).ToList();

            var ketQuas = new List<KetQuaXetTuyen>();
            for (int i = 0; i < hsCoKetQua.Count; i++)
            {
                var hs = hsCoKetQua[i];
                bool isPass = hs.TrangThai == TrangThaiHoSo.TrungTuyen || (hs.TrangThai == TrangThaiHoSo.DaPhongVan && i % 2 == 0);

                decimal diemDG = isPass ? 78.5m + (i % 5) * 4.0m : 55.0m + (i % 4) * 3.0m;
                if (diemDG > 98.0m) diemDG = 95.0m;

                ketQuas.Add(new KetQuaXetTuyen
                {
                    MaHoSo = hs.MaHoSo,
                    DiemDanhGia = diemDG,
                    NhanXet = isPass ? "Thí sinh có tư duy phản biện tốt, kỹ năng giao tiếp tốt, tự tin." : "Kiến thức nền tảng chưa vững, cần rèn luyện thêm kỹ năng mềm.",
                    KetQua = isPass ? KetQuaTuyen.TrungTuyen : KetQuaTuyen.KhongTrungTuyen,
                    NgayCapNhat = today.AddDays(-1)
                });
            }
            context.KetQuaXetTuyens.AddRange(ketQuas);
            context.SaveChanges();
        }
    }
}

