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

		public async Task DeleteAsync(int id)
		{
			await _context.Rooms.FindAsync(id);
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<Room>> GetAllAsync()
		{
			return await _context.Rooms
				.Include(r => r.Floor)
				.Include(r => r.Department)
				.ToListAsync();
		}

		public async Task<Room?> GetByIdAsync(string id)
		{
			return await _context.Rooms
				.Include(r => r.Floor)
				.Include(r => r.Department)
				.FirstOrDefaultAsync(r => r.RoomTag == id);
		}

		public async Task UpdateAsync(Room room)
		{
			_context.Rooms.Update(room);
			await _context.SaveChangesAsync();
		}
	}
}
