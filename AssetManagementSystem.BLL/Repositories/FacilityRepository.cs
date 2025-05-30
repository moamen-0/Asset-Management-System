﻿using AssetManagementSystem.BLL.Interfaces.IRepository;
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
	public class FacilityRepository : IFacilityRepository
	{
		private readonly AssetManagementDbContext _context;

		public FacilityRepository(AssetManagementDbContext context)
		{
			_context = context;
		}
		public async Task AddAsync(Facility facility)
		{
			await _context.AddAsync(facility);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var facility = await GetByIdAsync(id);
			if (facility != null)
			{
				_context.Facilities.Remove(facility);
				await _context.SaveChangesAsync();
			}
			else
			{
				throw new Exception("Facility not found");
			}
		}

		public async Task<IEnumerable<Facility>> GetAllAsync()
		{
			return await _context.Facilities
				.Include(f => f.Buildings)
					.ThenInclude(b => b.Floors)
						.ThenInclude(f => f.Rooms)
				.Include(f => f.Departments) // Add Departments include
				.ToListAsync();
		}

		public async Task<Facility?> GetByIdAsync(int id)
		{
			return await _context.Facilities
		.Include(f => f.Buildings)
			.ThenInclude(b => b.Floors)
			.ThenInclude(f => f.Rooms)
		.Include(f => f.Departments)
		.FirstOrDefaultAsync(f => f.Id == id);
		}

		public async Task UpdateAsync(Facility facility)
		{
			_context.Update(facility);
			await _context.SaveChangesAsync();
		}
		public async Task AddRangeAsync(IEnumerable<Facility> facilities)
		{
			await _context.Facilities.AddRangeAsync(facilities);
			await _context.SaveChangesAsync();
		}
		public async Task<Facility> GetByNameAsync(string name)
		{
			return await _context.Facilities
				.FirstOrDefaultAsync(f => f.Name.Trim().Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
		}
	}
}
