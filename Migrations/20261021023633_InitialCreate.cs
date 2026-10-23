// Họ và tên: Nguyễn Đức Nghĩa
// Mã sinh viên: 23103100307
// Nội dung thực hiện: Tạo Migration

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Khoas",
                columns: table => new
                {
                    MaKhoa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmailLienHe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khoas", x => x.MaKhoa);
                });

            migrationBuilder.CreateTable(
                name: "NguoiPhongVans",
                columns: table => new
                {
                    MaNguoiPhongVan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiPhongVans", x => x.MaNguoiPhongVan);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.MaTaiKhoan);
                });

            migrationBuilder.CreateTable(
                name: "NganhTuyenSinhs",
                columns: table => new
                {
                    MaNganh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNganh = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MaKhoa = table.Column<int>(type: "int", nullable: false),
                    ChiTieu = table.Column<int>(type: "int", nullable: false),
                    ToHopXetTuyen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiemXetTuyenToiThieu = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    NgayBatDauNhanHoSo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HanNopHoSo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MoTaNganh = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    YeuCauThiSinh = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NganhTuyenSinhs", x => x.MaNganh);
                    table.ForeignKey(
                        name: "FK_NganhTuyenSinhs_Khoas_MaKhoa",
                        column: x => x.MaKhoa,
                        principalTable: "Khoas",
                        principalColumn: "MaKhoa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThiSinhs",
                columns: table => new
                {
                    MaThiSinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ToHopXetTuyen = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TruongTHPT = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DiemThiTHPT = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ThanhTichNoiBat = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThiSinhs", x => x.MaThiSinh);
                    table.ForeignKey(
                        name: "FK_ThiSinhs_TaiKhoans_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoans",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoSoXetTuyens",
                columns: table => new
                {
                    MaHoSo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaThiSinh = table.Column<int>(type: "int", nullable: false),
                    MaNganh = table.Column<int>(type: "int", nullable: false),
                    NgayNop = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguyenVongXetTuyen = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NgayXuLy = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NhanXetSoTuyen = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoSoXetTuyens", x => x.MaHoSo);
                    table.ForeignKey(
                        name: "FK_HoSoXetTuyens_NganhTuyenSinhs_MaNganh",
                        column: x => x.MaNganh,
                        principalTable: "NganhTuyenSinhs",
                        principalColumn: "MaNganh",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoSoXetTuyens_ThiSinhs_MaThiSinh",
                        column: x => x.MaThiSinh,
                        principalTable: "ThiSinhs",
                        principalColumn: "MaThiSinh",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KetQuaXetTuyens",
                columns: table => new
                {
                    MaKetQuaXetTuyen = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoSo = table.Column<int>(type: "int", nullable: false),
                    DiemDanhGia = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    NhanXet = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    KetQua = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetQuaXetTuyens", x => x.MaKetQuaXetTuyen);
                    table.ForeignKey(
                        name: "FK_KetQuaXetTuyens_HoSoXetTuyens_MaHoSo",
                        column: x => x.MaHoSo,
                        principalTable: "HoSoXetTuyens",
                        principalColumn: "MaHoSo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichPhongVans",
                columns: table => new
                {
                    MaLichPhongVan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoSo = table.Column<int>(type: "int", nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HinhThucPhongVan = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DiaDiemHoacLienKet = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    MaNguoiPhongVan = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichPhongVans", x => x.MaLichPhongVan);
                    table.ForeignKey(
                        name: "FK_LichPhongVans_HoSoXetTuyens_MaHoSo",
                        column: x => x.MaHoSo,
                        principalTable: "HoSoXetTuyens",
                        principalColumn: "MaHoSo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LichPhongVans_NguoiPhongVans_MaNguoiPhongVan",
                        column: x => x.MaNguoiPhongVan,
                        principalTable: "NguoiPhongVans",
                        principalColumn: "MaNguoiPhongVan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HoSoXetTuyens_MaNganh",
                table: "HoSoXetTuyens",
                column: "MaNganh");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoXetTuyens_MaThiSinh",
                table: "HoSoXetTuyens",
                column: "MaThiSinh");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaXetTuyens_MaHoSo",
                table: "KetQuaXetTuyens",
                column: "MaHoSo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Khoas_TenKhoa",
                table: "Khoas",
                column: "TenKhoa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichPhongVans_MaHoSo",
                table: "LichPhongVans",
                column: "MaHoSo");

            migrationBuilder.CreateIndex(
                name: "IX_LichPhongVans_MaNguoiPhongVan",
                table: "LichPhongVans",
                column: "MaNguoiPhongVan");

            migrationBuilder.CreateIndex(
                name: "IX_NganhTuyenSinhs_MaKhoa",
                table: "NganhTuyenSinhs",
                column: "MaKhoa");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_TenDangNhap",
                table: "TaiKhoans",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThiSinhs_MaTaiKhoan",
                table: "ThiSinhs",
                column: "MaTaiKhoan",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KetQuaXetTuyens");

            migrationBuilder.DropTable(
                name: "LichPhongVans");

            migrationBuilder.DropTable(
                name: "HoSoXetTuyens");

            migrationBuilder.DropTable(
                name: "NguoiPhongVans");

            migrationBuilder.DropTable(
                name: "NganhTuyenSinhs");

            migrationBuilder.DropTable(
                name: "ThiSinhs");

            migrationBuilder.DropTable(
                name: "Khoas");

            migrationBuilder.DropTable(
                name: "TaiKhoans");
        }
    }
}
