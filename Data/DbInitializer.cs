// Há» vÃ  tÃªn: Nguyá»…n Äá»©c NghÄ©a
// MÃ£ sinh viÃªn: 23103100307
// Ná»™i dung thá»±c hiá»‡n: Táº¡o database, táº¡o báº£ng, thiáº¿t láº­p quan há»‡ giá»¯a cÃ¡c báº£ng, thÃªm dá»¯ liá»‡u máº«u.
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
            // Tá»± Ä‘á»™ng apply Migration
            context.Database.Migrate();

            // Náº¿u Ä‘Ã£ cÃ³ dá»¯ liá»‡u Khoa thÃ¬ bá» qua
            if (context.TaiKhoans.Any())
            {
                return;
            }

            // ==========================================
            // 1. KHOA (05 khoa theo yÃªu cáº§u)
            // ==========================================
            var khoas = new List<Khoa>
            {
                new Khoa { TenKhoa = "CÃ´ng nghá»‡ thÃ´ng tin", MoTa = "ÄÃ o táº¡o cÃ¡c ngÃ nh CNTT mÅ©i nhá»n", EmailLienHe = "cntt@uneti.edu.vn", TrangThai = true },
                new Khoa { TenKhoa = "Äiá»‡n - Äiá»‡n tá»­", MoTa = "ÄÃ o táº¡o ká»¹ sÆ° Ä‘iá»‡n, Ä‘iá»u khiá»ƒn vÃ  tá»± Ä‘á»™ng hÃ³a", EmailLienHe = "diendientu@uneti.edu.vn", TrangThai = true },
                new Khoa { TenKhoa = "CÆ¡ khÃ­", MoTa = "ÄÃ o táº¡o ká»¹ thuáº­t cÆ¡ khÃ­, cÆ¡ Ä‘iá»‡n tá»­, Ã´ tÃ´", EmailLienHe = "cokhi@uneti.edu.vn", TrangThai = true },
                new Khoa { TenKhoa = "Kinh táº¿", MoTa = "ÄÃ o táº¡o quáº£n trá»‹ kinh doanh, káº¿ toÃ¡n, tÃ i chÃ­nh", EmailLienHe = "kinhte@uneti.edu.vn", TrangThai = true },
                new Khoa { TenKhoa = "Ngoáº¡i ngá»¯", MoTa = "ÄÃ o táº¡o ngÃ´n ngá»¯ Anh, Trung Quá»‘c", EmailLienHe = "ngoaignu@uneti.edu.vn", TrangThai = true }
            };
            context.Khoas.AddRange(khoas);
            context.SaveChanges();

            // ==========================================
            // 2. NGÃ€NH TUYá»‚N SINH (15 ngÃ nh: Äang tuyá»ƒn, ÄÃ£ Ä‘Ã³ng, Háº¿t háº¡n, ChÆ°a má»Ÿ)
            // ==========================================
            var today = DateTime.Today;
            var nganhs = new List<NganhTuyenSinh>
            {
                // Khoa CNTT
                new NganhTuyenSinh
                {
                    TenNganh = "CÃ´ng nghá»‡ thÃ´ng tin",
                    MaKhoa = khoas[0].MaKhoa,
                    ChiTieu = 120,
                    ToHopXetTuyen = "A00, A01, D01",
                    DiemXetTuyenToiThieu = 22.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-15),
                    HanNopHoSo = today.AddDays(20),
                    MoTaNganh = "ChÆ°Æ¡ng trÃ¬nh Ä‘Ã o táº¡o ká»¹ sÆ° cÃ´ng nghá»‡ thÃ´ng tin Ä‘á»‹nh hÆ°á»›ng á»©ng dá»¥ng.",
                    YeuCauThiSinh = "Tá»‘t nghiá»‡p THPT, cÃ³ Ä‘am mÃª láº­p trÃ¬nh.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Ká»¹ thuáº­t pháº§n má»m",
                    MaKhoa = khoas[0].MaKhoa,
                    ChiTieu = 80,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 21.5m,
                    NgayBatDauNhanHoSo = today.AddDays(-10),
                    HanNopHoSo = today.AddDays(25),
                    MoTaNganh = "ÄÃ o táº¡o chuyÃªn sÃ¢u vá» kiáº¿n trÃºc pháº§n má»m, quy trÃ¬nh phÃ¡t triá»ƒn Agile/Scrum.",
                    YeuCauThiSinh = "TÆ° duy logic tá»‘t, tiáº¿ng Anh cÆ¡ báº£n.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "An toÃ n thÃ´ng tin",
                    MaKhoa = khoas[0].MaKhoa,
                    ChiTieu = 60,
                    ToHopXetTuyen = "A00, A01, D01",
                    DiemXetTuyenToiThieu = 21.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-12),
                    HanNopHoSo = today.AddDays(18),
                    MoTaNganh = "Báº£o máº­t há»‡ thá»‘ng, phÃ²ng thá»§ khÃ´ng gian máº¡ng.",
                    YeuCauThiSinh = "Äáº¡o Ä‘á»©c tá»‘t, cáº©n tháº­n, ká»· luáº­t.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "TrÃ­ tuá»‡ nhÃ¢n táº¡o",
                    MaKhoa = khoas[0].MaKhoa,
                    ChiTieu = 50,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 23.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-5),
                    HanNopHoSo = today.AddDays(30),
                    MoTaNganh = "NghiÃªn cá»©u thá»‹ giÃ¡c mÃ¡y tÃ­nh, NLP, há»c mÃ¡y hiá»‡n Ä‘áº¡i.",
                    YeuCauThiSinh = "ToÃ¡n há»c tá»‘t, tÆ° duy giáº£i thuáº­t xuáº¥t sáº¯c.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Khoa há»c mÃ¡y tÃ­nh",
                    MaKhoa = khoas[0].MaKhoa,
                    ChiTieu = 70,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 20.5m,
                    NgayBatDauNhanHoSo = today.AddDays(10),
                    HanNopHoSo = today.AddDays(40),
                    MoTaNganh = "Ná»n táº£ng thuáº­t toÃ¡n, tÃ­nh toÃ¡n Ä‘Ã¡m mÃ¢y vÃ  há»‡ thá»‘ng phÃ¢n tÃ¡n.",
                    YeuCauThiSinh = "Äam mÃª khoa há»c cÆ¡ báº£n.",
                    TrangThai = TrangThaiNganh.ChuaMo // ChÆ°a má»Ÿ
                },

                // Khoa Äiá»‡n - Äiá»‡n tá»­
                new NganhTuyenSinh
                {
                    TenNganh = "CÃ´ng nghá»‡ ká»¹ thuáº­t Ä‘iá»‡n, Ä‘iá»‡n tá»­",
                    MaKhoa = khoas[1].MaKhoa,
                    ChiTieu = 100,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 19.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-20),
                    HanNopHoSo = today.AddDays(15),
                    MoTaNganh = "Thiáº¿t káº¿ vi máº¡ch, há»‡ thá»‘ng Ä‘iá»‡n thÃ´ng minh.",
                    YeuCauThiSinh = "Sá»©c khá»e tá»‘t, chá»‹u khÃ³ há»c há»i.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Äiá»u khiá»ƒn vÃ  tá»± Ä‘á»™ng hÃ³a",
                    MaKhoa = khoas[1].MaKhoa,
                    ChiTieu = 90,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 20.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-15),
                    HanNopHoSo = today.AddDays(15),
                    MoTaNganh = "Tá»± Ä‘á»™ng hÃ³a nhÃ  mÃ¡y, PLC, SCADA, Robotics.",
                    YeuCauThiSinh = "Ká»¹ nÄƒng thá»±c hÃ nh tá»‘t.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Ká»¹ thuáº­t viá»…n thÃ´ng",
                    MaKhoa = khoas[1].MaKhoa,
                    ChiTieu = 60,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 18.5m,
                    NgayBatDauNhanHoSo = today.AddDays(-40),
                    HanNopHoSo = today.AddDays(-5), // Háº¿t háº¡n
                    MoTaNganh = "Máº¡ng 5G, truyá»n thÃ´ng khÃ´ng dÃ¢y, IoT.",
                    YeuCauThiSinh = "Tá»‘t nghiá»‡p THPT.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },

                // Khoa CÆ¡ khÃ­
                new NganhTuyenSinh
                {
                    TenNganh = "CÃ´ng nghá»‡ ká»¹ thuáº­t cÆ¡ khÃ­",
                    MaKhoa = khoas[2].MaKhoa,
                    ChiTieu = 80,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 18.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-30),
                    HanNopHoSo = today.AddDays(-2), // Háº¿t háº¡n
                    MoTaNganh = "Gia cÃ´ng CNC, CAD/CAM, cháº¿ táº¡o mÃ¡y hiá»‡n Ä‘áº¡i.",
                    YeuCauThiSinh = "Äá»™ khÃ©o tay, sá»©c khá»e Ä‘áº¡t loáº¡i khÃ¡.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "CÃ´ng nghá»‡ ká»¹ thuáº­t Ã´ tÃ´",
                    MaKhoa = khoas[2].MaKhoa,
                    ChiTieu = 110,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 21.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-10),
                    HanNopHoSo = today.AddDays(20),
                    MoTaNganh = "Ká»¹ thuáº­t Ä‘á»™ng cÆ¡ Ã´ tÃ´, xe Ä‘iá»‡n, cháº©n Ä‘oÃ¡n Ã´ tÃ´ sá»‘.",
                    YeuCauThiSinh = "Sá»©c khá»e tá»‘t, ham thÃ­ch cÃ´ng nghá»‡ Ã´ tÃ´.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Ká»¹ thuáº­t cÆ¡ Ä‘iá»‡n tá»­",
                    MaKhoa = khoas[2].MaKhoa,
                    ChiTieu = 70,
                    ToHopXetTuyen = "A00, A01",
                    DiemXetTuyenToiThieu = 19.5m,
                    NgayBatDauNhanHoSo = today.AddDays(-15),
                    HanNopHoSo = today.AddDays(15),
                    MoTaNganh = "TÃ­ch há»£p cÆ¡ khÃ­, Ä‘iá»‡n tá»­ vÃ  Ä‘iá»u khiá»ƒn sá»‘.",
                    YeuCauThiSinh = "TÆ° duy ká»¹ thuáº­t tá»‘t.",
                    TrangThai = TrangThaiNganh.TamDung // Táº¡m dá»«ng
                },

                // Khoa Kinh táº¿
                new NganhTuyenSinh
                {
                    TenNganh = "Quáº£n trá»‹ kinh doanh",
                    MaKhoa = khoas[3].MaKhoa,
                    ChiTieu = 150,
                    ToHopXetTuyen = "A00, A01, D01",
                    DiemXetTuyenToiThieu = 20.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-20),
                    HanNopHoSo = today.AddDays(10),
                    MoTaNganh = "Khá»Ÿi nghiá»‡p, quáº£n trá»‹ doanh nghiá»‡p, marketing vÃ  nhÃ¢n sá»±.",
                    YeuCauThiSinh = "NÄƒng Ä‘á»™ng, giao tiáº¿p tá»± tin.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "Káº¿ toÃ¡n",
                    MaKhoa = khoas[3].MaKhoa,
                    ChiTieu = 100,
                    ToHopXetTuyen = "A00, A01, D01",
                    DiemXetTuyenToiThieu = 19.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-20),
                    HanNopHoSo = today.AddDays(15),
                    MoTaNganh = "Káº¿ toÃ¡n doanh nghiá»‡p, kiá»ƒm toÃ¡n, thuáº¿ tÃ i chÃ­nh.",
                    YeuCauThiSinh = "Cáº©n tháº­n, tá»‰ má»‰, trung thá»±c.",
                    TrangThai = TrangThaiNganh.DangTuyen
                },
                new NganhTuyenSinh
                {
                    TenNganh = "TÃ i chÃ­nh - NgÃ¢n hÃ ng",
                    MaKhoa = khoas[3].MaKhoa,
                    ChiTieu = 80,
                    ToHopXetTuyen = "A00, A01, D01",
                    DiemXetTuyenToiThieu = 19.5m,
                    NgayBatDauNhanHoSo = today.AddDays(-40),
                    HanNopHoSo = today.AddDays(-10), // Háº¿t háº¡n
                    MoTaNganh = "NgÃ¢n hÃ ng sá»‘, Ä‘áº§u tÆ° tÃ i chÃ­nh, chá»©ng khoÃ¡n.",
                    YeuCauThiSinh = "Nháº¡y bÃ©n vá»›i sá»‘ liá»‡u.",
                    TrangThai = TrangThaiNganh.DaDong // ÄÃ£ Ä‘Ã³ng
                },

                // Khoa Ngoáº¡i ngá»¯
                new NganhTuyenSinh
                {
                    TenNganh = "NgÃ´n ngá»¯ Anh",
                    MaKhoa = khoas[4].MaKhoa,
                    ChiTieu = 100,
                    ToHopXetTuyen = "D01, D07",
                    DiemXetTuyenToiThieu = 20.0m,
                    NgayBatDauNhanHoSo = today.AddDays(-15),
                    HanNopHoSo = today.AddDays(15),
                    MoTaNganh = "Tiáº¿ng Anh thÆ°Æ¡ng máº¡i, biÃªn phiÃªn dá»‹ch chuyÃªn ngÃ nh.",
                    YeuCauThiSinh = "Äiá»ƒm mÃ´n tiáº¿ng Anh THPT Ä‘áº¡t tá»« 7.0 trá»Ÿ lÃªn.",
                    TrangThai = TrangThaiNganh.DangTuyen
                }
            };
            context.NganhTuyenSinhs.AddRange(nganhs);
            context.SaveChanges();

            // ==========================================
            // 3. NGÆ¯á»œI PHá»ŽNG Váº¤N (05 ngÆ°á»i phá»ng váº¥n)
            // ==========================================
            var nguoiPVs = new List<NguoiPhongVan>
            {
                new NguoiPhongVan { HoTen = "TS. Nguyá»…n VÄƒn Nam", Email = "namnv@uneti.edu.vn", TrangThai = true },
                new NguoiPhongVan { HoTen = "ThS. Tráº§n Thá»‹ Mai", Email = "maitt@uneti.edu.vn", TrangThai = true },
                new NguoiPhongVan { HoTen = "PGS.TS. LÃª HoÃ ng Long", Email = "longlh@uneti.edu.vn", TrangThai = true },
                new NguoiPhongVan { HoTen = "TS. Pháº¡m Thu HÃ ", Email = "hapt@uneti.edu.vn", TrangThai = true },
                new NguoiPhongVan { HoTen = "ThS. VÅ© Äá»©c Máº¡nh", Email = "manhvd@uneti.edu.vn", TrangThai = true }
            };
            context.NguoiPhongVans.AddRange(nguoiPVs);
            context.SaveChanges();

            // ==========================================
            // 4. TÃ€I KHOáº¢N (2 Admin, 3 CÃ¡n bá»™, 30 ThÃ­ sinh)
            // ==========================================
            var taiKhoans = new List<TaiKhoan>
            {
                // Admin (02)
                new TaiKhoan { TenDangNhap = "admin1", MatKhau = "123456", HoTen = "Quáº£n trá»‹ viÃªn 1", Email = "admin1@uneti.edu.vn", VaiTro = VaiTro.Admin, TrangThai = true },
                new TaiKhoan { TenDangNhap = "admin2", MatKhau = "123456", HoTen = "Quáº£n trá»‹ viÃªn 2", Email = "admin2@uneti.edu.vn", VaiTro = VaiTro.Admin, TrangThai = true },

                // CÃ¡n bá»™ tuyá»ƒn sinh (03)
                new TaiKhoan { TenDangNhap = "canbo1", MatKhau = "123456", HoTen = "CÃ¡n bá»™ Nguyá»…n VÄƒn A", Email = "canbo1@uneti.edu.vn", VaiTro = VaiTro.CanBoTuyenSinh, TrangThai = true },
                new TaiKhoan { TenDangNhap = "canbo2", MatKhau = "123456", HoTen = "CÃ¡n bá»™ LÃª Thá»‹ B", Email = "canbo2@uneti.edu.vn", VaiTro = VaiTro.CanBoTuyenSinh, TrangThai = true },
                new TaiKhoan { TenDangNhap = "canbo3", MatKhau = "123456", HoTen = "CÃ¡n bá»™ Tráº§n VÄƒn C", Email = "canbo3@uneti.edu.vn", VaiTro = VaiTro.CanBoTuyenSinh, TrangThai = true }
            };

            // 30 TÃ i khoáº£n thÃ­ sinh
            var danhSachTen = new[]
            {
                "Nguyá»…n VÄƒn An", "Tráº§n Thá»‹ BÃ­ch", "LÃª VÄƒn CÆ°á»ng", "Pháº¡m Thá»‹ Dung", "HoÃ ng VÄƒn Em",
                "Äá»— Thá»‹ Giang", "VÅ© VÄƒn HÃ¹ng", "BÃ¹i Thá»‹ Lan", "Äinh VÄƒn Minh", "NgÃ´ Thá»‹ Ngá»c",
                "Phan VÄƒn Oanh", "DÆ°Æ¡ng Thá»‹ PhÆ°Æ¡ng", "Mai VÄƒn QuÃ¢n", "LÃ½ Thá»‹ Sen", "ÄoÃ n VÄƒn Tháº¯ng",
                "Trá»‹nh Thá»‹ UyÃªn", "LÆ°u VÄƒn Viá»‡t", "HÃ  Thá»‹ XuÃ¢n", "Cao VÄƒn YÃªn", "ÄÃ o Thá»‹ Ãnh",
                "Nguyá»…n Tuáº¥n Kiá»‡t", "Tráº§n Báº£o Ngá»c", "LÃª HoÃ ng Yáº¿n", "Pháº¡m Quá»‘c Báº£o", "HoÃ ng Tháº£o My",
                "Äá»— Minh TrÃ­", "VÅ© Háº£i ÄÄƒng", "BÃ¹i KhÃ¡nh Linh", "Äinh Há»¯u PhÆ°á»›c", "NgÃ´ ThÃ¹y Trang"
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
            // 5. THÃ SINH (30 thÃ­ sinh á»©ng vá»›i 30 tÃ i khoáº£n thÃ­ sinh)
            // ==========================================
            var thiSinhTaiKhoans = taiKhoans.Where(t => t.VaiTro == VaiTro.ThiSinh).ToList();
            var thiSinhs = new List<ThiSinh>();

            var toHopList = new[] { "A00", "A01", "D01", "A00", "A01" };
            var truongList = new[] { "THPT ChuyÃªn HÃ  Ná»™i - Amsterdam", "THPT Chu VÄƒn An", "THPT Kim LiÃªn", "THPT ThÄƒng Long", "THPT Tráº§n PhÃº" };

            for (int i = 0; i < thiSinhTaiKhoans.Count; i++)
            {
                var tk = thiSinhTaiKhoans[i];
                decimal diem = 19.5m + (i % 10) * 0.9m; // Dao Ä‘á»™ng 19.5 -> 27.6
                if (diem > 29.5m) diem = 28.5m;

                thiSinhs.Add(new ThiSinh
                {
                    MaTaiKhoan = tk.MaTaiKhoan,
                    HoTen = tk.HoTen,
                    NgaySinh = new DateTime(2007, (i % 12) + 1, (i % 27) + 1),
                    GioiTinh = (i % 2 == 0) ? GioiTinhConst.Nam : GioiTinhConst.Nu,
                    SoDienThoai = $"098{i:D7}",
                    Email = tk.Email,
                    DiaChi = $"Sá»‘ {i + 12}, ÄÆ°á»ng Giáº£i PhÃ³ng, HÃ  Ná»™i",
                    ToHopXetTuyen = toHopList[i % toHopList.Length],
                    TruongTHPT = truongList[i % truongList.Length],
                    DiemThiTHPT = diem,
                    ThanhTichNoiBat = (i % 3 == 0) ? "Äáº¡t giáº£i Há»c sinh giá»i cáº¥p tá»‰nh mÃ´n ToÃ¡n" : "Há»c sinh giá»i 3 nÄƒm THPT",
                    TrangThai = true
                });
            }
            context.ThiSinhs.AddRange(thiSinhs);
            context.SaveChanges();

            // ==========================================
            // 6. Há»’ SÆ  XÃ‰T TUYá»‚N (45 há»“ sÆ¡ tráº£i Ä‘á»§ tráº¡ng thÃ¡i)
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
                var ng = nganhs[i % 8]; // Chá»n trong cÃ¡c ngÃ nh Äang tuyá»ƒn
                var status = trangThais[i % trangThais.Length];

                var hs = new HoSoXetTuyen
                {
                    MaThiSinh = ts.MaThiSinh,
                    MaNganh = ng.MaNganh,
                    NgayNop = today.AddDays(-14 + (i % 10)),
                    NguyenVongXetTuyen = $"ÄÄƒng kÃ½ nguyá»‡n vá»ng vÃ o ngÃ nh {ng.TenNganh} Ä‘á»ƒ trá»Ÿ thÃ nh ká»¹ sÆ° giá»i.",
                    GhiChu = "ÄÃ£ ná»™p Ä‘áº§y Ä‘á»§ há»c báº¡ vÃ  giáº¥y chá»©ng nháº­n tá»‘t nghiá»‡p táº¡m thá»i.",
                    TrangThai = status,
                    NgayXuLy = (status != TrangThaiHoSo.ChoDuyet) ? today.AddDays(-5 + (i % 4)) : null,
                    NhanXetSoTuyen = (status != TrangThaiHoSo.ChoDuyet)
                        ? (status == TrangThaiHoSo.KhongDatSoTuyen ? "Äiá»ƒm xÃ©t tuyá»ƒn chÆ°a Ä‘áº¡t ngÆ°á»¡ng yÃªu cáº§u" : "Há»“ sÆ¡ Ä‘áº§y Ä‘á»§, Ä‘áº¡t yÃªu cáº§u sÆ¡ tuyá»ƒn")
                        : null
                };
                hoSos.Add(hs);
            }
            context.HoSoXetTuyens.AddRange(hoSos);
            context.SaveChanges();

            // ==========================================
            // 7. Lá»ŠCH PHá»ŽNG Váº¤N (18 lá»‹ch phá»ng váº¥n)
            // ==========================================
            // Láº­p lá»‹ch cho cÃ¡c há»“ sÆ¡ á»Ÿ tráº¡ng thÃ¡i Äáº¡t sÆ¡ tuyá»ƒn, Chá» phá»ng váº¥n, ÄÃ£ phá»ng váº¥n, TrÃºng tuyá»ƒn, KhÃ´ng trÃºng tuyá»ƒn
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

                // Náº¿u há»“ sÆ¡ ÄÃ£ phá»ng váº¥n, TrÃºng tuyá»ƒn, KhÃ´ng trÃºng tuyá»ƒn => Lá»‹ch ÄÃ£ hoÃ n thÃ nh (quÃ¡ khá»©)
                // Náº¿u Chá» phá»ng váº¥n => Lá»‹ch ÄÃ£ lÃªn lá»‹ch (tÆ°Æ¡ng lai)
                bool isPast = (hs.TrangThai == TrangThaiHoSo.DaPhongVan || hs.TrangThai == TrangThaiHoSo.TrungTuyen || hs.TrangThai == TrangThaiHoSo.KhongTrungTuyen);
                DateTime start = isPast ? today.AddDays(-3).AddHours(8 + (i % 8)) : today.AddDays(2 + (i % 5)).AddHours(8 + (i % 8));

                string statusLich = isPast ? TrangThaiLich.DaHoanThanh : (i == hsCanLich.Count - 1 ? TrangThaiLich.DaHuy : TrangThaiLich.DaLenLich);

                lichPVs.Add(new LichPhongVan
                {
                    MaHoSo = hs.MaHoSo,
                    ThoiGianBatDau = start,
                    ThoiGianKetThuc = start.AddMinutes(45),
                    HinhThucPhongVan = (i % 2 == 0) ? "Trá»±c tiáº¿p" : "Trá»±c tuyáº¿n",
                    DiaDiemHoacLienKet = (i % 2 == 0) ? $"PhÃ²ng PV {101 + i % 5}, TÃ²a nhÃ  HA8" : "https://meet.google.com/xyz-abcd-efg",
                    MaNguoiPhongVan = npv.MaNguoiPhongVan,
                    GhiChu = "Phá»ng váº¥n Ä‘Ã¡nh giÃ¡ kiáº¿n thá»©c cÆ¡ sá»Ÿ vÃ  nÄƒng lá»±c tÆ° duy.",
                    TrangThai = statusLich
                });
            }
            context.LichPhongVans.AddRange(lichPVs);
            context.SaveChanges();

            // ==========================================
            // 8. Káº¾T QUáº¢ XÃ‰T TUYá»‚N (Ghi nháº­n cho cÃ¡c há»“ sÆ¡ Ä‘Ã£ phá»ng váº¥n)
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
                    NhanXet = isPass ? "ThÃ­ sinh cÃ³ tÆ° duy pháº£n biá»‡n tá»‘t, ká»¹ nÄƒng giao tiáº¿p tá»‘t, tá»± tin." : "Kiáº¿n thá»©c ná»n táº£ng chÆ°a vá»¯ng, cáº§n rÃ¨n luyá»‡n thÃªm ká»¹ nÄƒng má»m.",
                    KetQua = isPass ? KetQuaTuyen.TrungTuyen : KetQuaTuyen.KhongTrungTuyen,
                    NgayCapNhat = today.AddDays(-1)
                });
            }
            context.KetQuaXetTuyens.AddRange(ketQuas);
            context.SaveChanges();
        }
    }
}


