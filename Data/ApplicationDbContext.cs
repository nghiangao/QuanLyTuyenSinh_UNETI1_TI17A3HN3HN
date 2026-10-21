// Họ và tên: Nguyễn Đức Nghĩa
// Mã sinh viên: 23103100307
// Nội dung thực hiện: thêm Migration, tạo cơ sở dữ liệu, tạo bảng, thiết lập quan hệ giữa các bảng, thêm dữ liệu mẫu.

using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;
        public DbSet<Khoa> Khoas { get; set; } = null!;
        public DbSet<NganhTuyenSinh> NganhTuyenSinhs { get; set; } = null!;
        public DbSet<ThiSinh> ThiSinhs { get; set; } = null!;
        public DbSet<HoSoXetTuyen> HoSoXetTuyens { get; set; } = null!;
        public DbSet<NguoiPhongVan> NguoiPhongVans { get; set; } = null!;
        public DbSet<LichPhongVan> LichPhongVans { get; set; } = null!;
        public DbSet<KetQuaXetTuyen> KetQuaXetTuyens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- Module 1 ----
            // TaiKhoan: TenDangNhap unique
            modelBuilder.Entity<TaiKhoan>(entity =>
            {
                entity.HasIndex(e => e.TenDangNhap).IsUnique();
            });

            // Khoa: TenKhoa unique
            modelBuilder.Entity<Khoa>(entity =>
            {
                entity.HasIndex(e => e.TenKhoa).IsUnique();
            });

            // ---- Module 2 ----
            // NganhTuyenSinh: Khoa (1) - (n) NganhTuyenSinh, Restrict delete
            modelBuilder.Entity<NganhTuyenSinh>(entity =>
            {
                entity.Property(e => e.DiemXetTuyenToiThieu)
                      .HasPrecision(5, 2);

                entity.HasOne(e => e.Khoa)
                      .WithMany(k => k.NganhTuyenSinhs)
                      .HasForeignKey(e => e.MaKhoa)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---- Module 3 ----
            // ThiSinh: TaiKhoan (1) - (0..1) ThiSinh
            modelBuilder.Entity<ThiSinh>(entity =>
            {
                entity.Property(e => e.DiemThiTHPT)
                      .HasPrecision(5, 2);

                entity.HasOne(e => e.TaiKhoan)
                      .WithOne(tk => tk.ThiSinh)
                      .HasForeignKey<ThiSinh>(e => e.MaTaiKhoan)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.MaTaiKhoan).IsUnique();
            });

            // HoSoXetTuyen:
            // ThiSinh (1) - (n) HoSoXetTuyen (Restrict delete)
            // NganhTuyenSinh (1) - (n) HoSoXetTuyen (Restrict delete)
            modelBuilder.Entity<HoSoXetTuyen>(entity =>
            {
                entity.HasOne(e => e.ThiSinh)
                      .WithMany(ts => ts.HoSoXetTuyens)
                      .HasForeignKey(e => e.MaThiSinh)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.NganhTuyenSinh)
                      .WithMany(n => n.HoSoXetTuyens)
                      .HasForeignKey(e => e.MaNganh)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---- Module 5 ----
            // LichPhongVan:
            // HoSoXetTuyen (1) - (n) LichPhongVan (Cascade delete)
            // NguoiPhongVan (1) - (n) LichPhongVan (Restrict delete)
            modelBuilder.Entity<LichPhongVan>(entity =>
            {
                entity.HasOne(e => e.HoSoXetTuyen)
                      .WithMany(h => h.LichPhongVans)
                      .HasForeignKey(e => e.MaHoSo)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.NguoiPhongVan)
                      .WithMany(npv => npv.LichPhongVans)
                      .HasForeignKey(e => e.MaNguoiPhongVan)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // KetQuaXetTuyen:
            // HoSoXetTuyen (1) - (0..1) KetQuaXetTuyen
            modelBuilder.Entity<KetQuaXetTuyen>(entity =>
            {
                entity.Property(e => e.DiemDanhGia)
                      .HasPrecision(5, 2);

                entity.HasOne(e => e.HoSoXetTuyen)
                      .WithOne(h => h.KetQuaXetTuyen)
                      .HasForeignKey<KetQuaXetTuyen>(e => e.MaHoSo)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.MaHoSo).IsUnique();
            });
        }
    }

    // Alias TuyenSinhDbContext để tiện sử dụng cả 2 tên
    public class TuyenSinhDbContext : ApplicationDbContext
    {
        public TuyenSinhDbContext(DbContextOptions<TuyenSinhDbContext> options)
            : base(options)
        {
        }
    }
}

