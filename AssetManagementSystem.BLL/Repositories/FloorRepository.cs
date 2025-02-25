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
	public class FloorRepository : IFloorRepository
	{
		private readonly AssetManagementDbContext _context;

		public FloorRepository(AssetManagementDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Floor floor)
		{
			await _context.Floors.AddAsync(floor);
			await _context.SaveChangesAsync();
			
		}

		public async Task DeleteAsync(int id)
		{
			var floor = await GetByIdAsync(id);
			if (floor != null)
			{
				_context.Floors.Remove(floor);
				await _context.SaveChangesAsync();
			}
			else
			{
				throw new Exception("Asset not found");
			}
		}

		public async Task<IEnumerable<Floor>> GetAllAsync()
		{
			return await _context.Floors
				.Include(f => f.Building)
					.ThenInclude(b => b.Facility)
				.Include(f => f.Rooms)
					.ThenInclude(r => r.Department).
					ToListAsync();
		}

		public async Task<Floor?> GetByIdAsync(int id)
		{
			return await _context.Floors
				.Include(f => f.Building)
					.ThenInclude(b => b.Facility)
				.Include(f => f.Rooms)
					.ThenInclude(r => r.Department)
				.FirstOrDefaultAsync(f => f.Id == id);
		}

		public async Task UpdateAsync(Floor floor)
		{
			_context.Floors.Update(floor);
			await _context.SaveChangesAsync();
		}
	}
}
