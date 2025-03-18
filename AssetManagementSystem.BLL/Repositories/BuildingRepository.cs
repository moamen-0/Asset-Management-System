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
	public class BuildingRepository : IBuildingRepository
	{
		private readonly AssetManagementDbContext _context;

		public BuildingRepository(AssetManagementDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Building building)
		{
			await _context.Buildings.AddAsync(building);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var buildingToDelete = await GetByIdAsync(id);
			if (buildingToDelete == null)
			{
				throw new Exception("Building not found");
			}
			else
			{
				_context.Buildings.Remove(buildingToDelete);
				await _context.SaveChangesAsync();
			}

			}

		public async Task<IEnumerable<Building>> GetAllAsync()
		{
			return await _context.Buildings
				.Include(b => b.Facility)
				.Include(b => b.Floors)
					.ThenInclude(f => f.Rooms)
				.ToListAsync();
		}

		public async Task<Building?> GetByIdAsync(int id)
		{
			return await _context.Buildings
				.Include(b => b.Facility)
				.Include(b => b.Floors)
				.ThenInclude(f => f.Rooms)
				.FirstOrDefaultAsync(b => b.Id == id);
		}

		public async Task UpdateAsync(Building building)
		{
			var buildingToUpdate = await GetByIdAsync(building.Id);
			if (buildingToUpdate == null)
			{
				throw new Exception("Building not found");
			}

			_context.Entry(buildingToUpdate).State = EntityState.Detached;

			
			_context.Attach(building);
			_context.Entry(building).State = EntityState.Modified;

			await _context.SaveChangesAsync();
		}
		public async Task AddRangeAsync(IEnumerable<Building> buildings)
		{
			await _context.Buildings.AddRangeAsync(buildings);
			await _context.SaveChangesAsync();
		}
		public async Task<Building> GetByNameAndFacilityAsync(string buildingName, int facilityId)
		{
			return await _context.Buildings
				.FirstOrDefaultAsync(b =>
					b.Name.Trim().Equals(buildingName.Trim(), StringComparison.OrdinalIgnoreCase) &&
					b.FacilityId == facilityId);
		}

	}
}
