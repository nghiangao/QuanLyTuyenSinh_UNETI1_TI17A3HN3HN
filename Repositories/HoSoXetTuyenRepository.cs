// Họ và tên: Nguyễn Đức Nghĩa
// Mã sinh viên: 23103100307
// Nội dung thực hiện: Module 4 - Repository Implementation thao tác với DbContext theo kiến trúc N-Tier.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Data;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Repositories
{
    public class HoSoXetTuyenRepository : IHoSoXetTuyenRepository
    {
        private readonly ApplicationDbContext _context;

        public HoSoXetTuyenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HoSoXetTuyen?> GetByIdAsync(int maHoSo)
        {
            return await _context.HoSoXetTuyens.FindAsync(maHoSo);
        }

        public async Task<HoSoXetTuyen?> GetByIdWithDetailsAsync(int maHoSo)
        {
            return await _context.HoSoXetTuyens
                .Include(h => h.ThiSinh)
                    .ThenInclude(ts => ts!.TaiKhoan)
                .Include(h => h.NganhTuyenSinh)
                    .ThenInclude(n => n!.Khoa)
                .Include(h => h.LichPhongVans)
                .Include(h => h.KetQuaXetTuyen)
                .FirstOrDefaultAsync(h => h.MaHoSo == maHoSo);
        }

        public async Task<bool> ExistsAsync(int maHoSo)
        {
            return await _context.HoSoXetTuyens.AnyAsync(h => h.MaHoSo == maHoSo);
        }

        public async Task<int> CountTrungTuyenByNganhAsync(int maNganh)
        {
            return await _context.HoSoXetTuyens
                .CountAsync(h => h.MaNganh == maNganh && h.TrangThai == TrangThaiHoSo.TrungTuyen);
        }

        public Task UpdateAsync(HoSoXetTuyen hoSo)
        {
            _context.HoSoXetTuyens.Update(hoSo);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public IQueryable<HoSoXetTuyen> GetAllQueryable()
        {
            return _context.HoSoXetTuyens.AsQueryable();
        }
    }
}

