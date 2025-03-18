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
	public class DepartmentRepository : IDepartmentRepository
	{
		private readonly AssetManagementDbContext _context;

		public DepartmentRepository(AssetManagementDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Department department)
		{
			await _context.Departments.AddAsync(department);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var department = _context.Departments.Find(id);
			if (department != null)
			{
				_context.Departments.Remove(department);
				await _context.SaveChangesAsync();
			}

			else
			{
				throw new Exception("Department not found");
			}

		}

		public async Task<IEnumerable<Department>> GetAllAsync()
		{
			return await _context.Departments
				.Include(d => d.Facility)
				.Include(d => d.Rooms)
				.Include(d => d.Users)
				.ToListAsync();
		}

		public async Task<Department?> GetByIdAsync(int id)
		{
			return await _context.Departments
				.Include(d => d.Facility)
				.Include(d => d.Rooms)
				.Include(d => d.Users)
				.FirstOrDefaultAsync(d => d.Id == id);
		}
		public async Task UpdateAsync(Department department)
		{
			if (department == null)
			{
				throw new Exception("Department not found");
			}
			_context.Departments.Update(department);
			await _context.SaveChangesAsync();
		}
		public async Task<Department> GetByNameAsync(string name)
		{
			return await _context.Departments
				.FirstOrDefaultAsync(d => d.Name.Trim().Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
		}
		public async Task AddRangeAsync(IEnumerable<Department> departments)
		{
			await _context.Departments.AddRangeAsync(departments);
			await _context.SaveChangesAsync();
		}
	}
}
