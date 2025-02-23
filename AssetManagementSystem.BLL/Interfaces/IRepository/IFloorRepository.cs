using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IRepository
{
	public interface IFloorRepository
	{
		Task<IEnumerable<Floor>> GetAllAsync();
		Task<Floor?> GetByIdAsync(int id);
		Task AddAsync(Floor floor);
		Task UpdateAsync(Floor floor);
		Task DeleteAsync(int id);
	}
}
