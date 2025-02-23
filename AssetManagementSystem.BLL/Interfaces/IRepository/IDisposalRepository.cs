using AssetManagementSystem.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IRepository
{
	public interface IDisposalRepository
	{
		Task<IEnumerable<Disposal>> GetAllAsync();
		Task<Disposal?> GetByIdAsync(int id);
		Task AddAsync(Disposal disposal);
		Task UpdateAsync(Disposal disposal);
		Task DeleteAsync(int id);
	}
}
