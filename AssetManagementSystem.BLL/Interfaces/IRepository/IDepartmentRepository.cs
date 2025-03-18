using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IRepository
{
	public interface IDepartmentRepository
	{
		Task<IEnumerable<Department>> GetAllAsync();
		Task<Department?> GetByIdAsync(int id);
		Task AddAsync(Department department);
		Task UpdateAsync(Department department);
		Task DeleteAsync(int id);
		Task<Department> GetByNameAsync(string name);
		Task AddRangeAsync(IEnumerable<Department> departments);
	}
}
