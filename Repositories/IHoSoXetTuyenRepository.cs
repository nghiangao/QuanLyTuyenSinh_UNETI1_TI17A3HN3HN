// Họ và tên: Nguyễn Đức Nghĩa
// Mã sinh viên: 23103100307
// Nội dung thực hiện: Module 4 - Repository Interface truy xuất dữ liệu hồ sơ xét tuyển theo kiến trúc N-Tier.

using System.Linq;
using System.Threading.Tasks;
using QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Models;

namespace QuanLyTuyenSinh_UNETI1_TI17A3HN3HN.Repositories
{
    /// <summary>
    /// Repository đóng gói toàn bộ các thao tác truy vấn dữ liệu liên quan đến Hồ sơ xét tuyển.
    /// Giúp tách biệt hoàn toàn tầng Business Logic (Service) khỏi tầng cơ sở dữ liệu (DbContext).
    /// </summary>
    public interface IHoSoXetTuyenRepository
    {
        Task<HoSoXetTuyen?> GetByIdAsync(int maHoSo);

        Task<HoSoXetTuyen?> GetByIdWithDetailsAsync(int maHoSo);

        Task<bool> ExistsAsync(int maHoSo);

        Task<int> CountTrungTuyenByNganhAsync(int maNganh);

        Task UpdateAsync(HoSoXetTuyen hoSo);

        Task SaveChangesAsync();

        IQueryable<HoSoXetTuyen> GetAllQueryable();
    }
}

