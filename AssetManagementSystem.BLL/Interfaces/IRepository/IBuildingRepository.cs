using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IRepository
{
	public interface IBuildingRepository
	{
		Task<IEnumerable<Building>> GetAllAsync();
		Task<Building?> GetByIdAsync(int id);
		Task AddAsync(Building building);
		Task UpdateAsync(Building building);
		Task DeleteAsync(int id);
		Task AddRangeAsync(IEnumerable<Building> buildings);
		Task<Building> GetByNameAndFacilityAsync(string buildingName, int facilityId);

	}
}
