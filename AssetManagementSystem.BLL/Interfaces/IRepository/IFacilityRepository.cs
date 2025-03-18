using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IRepository
{
	public interface IFacilityRepository
	{
		Task<IEnumerable<Facility>> GetAllAsync();
		Task<Facility?> GetByIdAsync(int id);
		Task AddAsync(Facility facility);
		Task UpdateAsync(Facility facility);
		Task DeleteAsync(int id);
		Task AddRangeAsync(IEnumerable<Facility> facilities);
	}
}
