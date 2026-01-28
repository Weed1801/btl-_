using Microsoft.EntityFrameworkCore;
using QuanLyChoThuePhongTro.Data;
using QuanLyChoThuePhongTro.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyChoThuePhongTro.Repositories
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetAllAsync();
        Task<Room?> GetByIdAsync(int id);
        Task<IEnumerable<Room>> SearchAsync(string location, decimal? minPrice, decimal? maxPrice);
        Task<IEnumerable<Room>> GetByOwnerAsync(int ownerId);
        Task AddAsync(Room room);
        Task UpdateAsync(Room room);
        Task DeleteAsync(int id);
        Task SaveAsync();
    }

    public class RoomRepository : IRoomRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Room>> GetAllAsync()
        {
            return await _context.Rooms
                .Include(r => r.Owner)
                .ToListAsync();
        }

        public async Task<Room?> GetByIdAsync(int id)
        {
            return await _context.Rooms
                .Include(r => r.Owner)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Room>> SearchAsync(string location, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Rooms.AsQueryable();

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(r => r.Location.Contains(location) || r.District.Contains(location));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(r => r.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(r => r.Price <= maxPrice.Value);
            }

            return await query.Include(r => r.Owner).ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetByOwnerAsync(int ownerId)
        {
            return await _context.Rooms
                .Where(r => r.OwnerId == ownerId)
                .ToListAsync();
        }

        public async Task AddAsync(Room room)
        {
            _context.Rooms.Add(room);
            await SaveAsync();
        }

        public async Task UpdateAsync(Room room)
        {
            _context.Rooms.Update(room);
            await SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var room = await GetByIdAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await SaveAsync();
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
