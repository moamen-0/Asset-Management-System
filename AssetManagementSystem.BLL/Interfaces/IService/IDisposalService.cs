using AssetManagementSystem.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IService
{
	public interface IDisposalService
	{
		Task<IEnumerable<Disposal>> GetAllDisposalsAsync();
		Task<Disposal?> GetDisposalByIdAsync(int id);
		Task AddDisposalAsync(Disposal disposal);
		Task UpdateDisposalAsync(Disposal disposal);
		Task DeleteDisposalAsync(int id);
	}
}
