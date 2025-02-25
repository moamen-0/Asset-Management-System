using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IRepository
{
	public interface IRoomRepository
	{
		Task<IEnumerable<Room>> GetAllAsync();
		Task<Room?> GetByIdAsync(string id);
		Task AddAsync(Room room);
		Task UpdateAsync(Room room);
		Task DeleteAsync(string id);
	}
}
