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
        Task<IEnumerable<Room>> SearchAsync(RoomFilter filter);
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

        public async Task<IEnumerable<Room>> SearchAsync(RoomFilter filter)
        {
            var query = _context.Rooms.AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                query = query.Where(r => r.Title.Contains(filter.SearchQuery) || 
                                       r.Description.Contains(filter.SearchQuery) || 
                                       r.Location.Contains(filter.SearchQuery));
            }

            if (!string.IsNullOrEmpty(filter.District))
            {
                query = query.Where(r => r.District == filter.District);
            }

            if (!string.IsNullOrEmpty(filter.Ward))
            {
                query = query.Where(r => r.Ward == filter.Ward);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(r => r.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(r => r.Price <= filter.MaxPrice.Value);
            }

            if (filter.MinArea.HasValue)
            {
                query = query.Where(r => r.Area >= filter.MinArea.Value);
            }

            if (filter.MaxArea.HasValue)
            {
                query = query.Where(r => r.Area <= filter.MaxArea.Value);
            }

            if (filter.Bedrooms.HasValue)
            {
                query = query.Where(r => r.Bedrooms >= filter.Bedrooms.Value);
            }

            if (filter.Bathrooms.HasValue)
            {
                query = query.Where(r => r.Bathrooms >= filter.Bathrooms.Value);
            }

            if (filter.HasKitchen.HasValue && filter.HasKitchen.Value)
            {
                query = query.Where(r => r.HasKitchen == true);
            }

            if (filter.HasWiFi.HasValue && filter.HasWiFi.Value)
            {
                query = query.Where(r => r.HasWiFi == true);
            }

            if (filter.HasAirConditioner.HasValue && filter.HasAirConditioner.Value)
            {
                query = query.Where(r => r.HasAirConditioner == true);
            }

            if (filter.HasWashing.HasValue && filter.HasWashing.Value)
            {
                query = query.Where(r => r.HasWashing == true);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(r => r.Status == filter.Status);
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
