using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.DAL.Data;
using AssetManagementSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Repositories
{
	public class RoomRepository : IRoomRepository
	{
		private readonly AssetManagementDbContext _context;

		public RoomRepository(AssetManagementDbContext context)
		{
			_context = context;
		}
		public async Task AddAsync(Room room)
		{
			await _context.Rooms.AddAsync(room);
			await _context.SaveChangesAsync();

		}

		public async Task DeleteAsync(string id)
		{
			_context.Rooms.Remove(await _context.Rooms.FindAsync(id));
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<Room>> GetAllAsync()
		{
			return await _context.Rooms
				.Include(r => r.Floor)
					.ThenInclude(f => f.Building)
						.ThenInclude(b => b.Facility)
				.Include(r => r.Department)
					.ThenInclude(d => d.Facility)
				.ToListAsync();
		}

		public async Task<Room?> GetByIdAsync(string id)
		{
			return await _context.Rooms
				.Include(r => r.Floor)
					.ThenInclude(f => f.Building)
				.Include(r => r.Department)
				.FirstOrDefaultAsync(r => r.RoomTag == id);
		}

		public async Task UpdateAsync(Room room)
		{
			_context.Rooms.Update(room);
			await _context.SaveChangesAsync();
		}
		public async Task AddRangeAsync(IEnumerable<Room> rooms)
		{
			await _context.Rooms.AddRangeAsync(rooms);
			await _context.SaveChangesAsync();
		}
	}
}
