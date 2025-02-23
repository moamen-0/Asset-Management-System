using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Services
{
	public class DisposalService : IDisposalService
	{
		private readonly IDisposalRepository _disposalRepository;

		public DisposalService(IDisposalRepository disposalRepository)
		{
			_disposalRepository = disposalRepository;
		}

		public async Task<IEnumerable<Disposal>> GetAllDisposalsAsync()
		{
			return await _disposalRepository.GetAllAsync();
		}

		public async Task<Disposal?> GetDisposalByIdAsync(int id)
		{
			return await _disposalRepository.GetByIdAsync(id);
		}

		public async Task AddDisposalAsync(Disposal disposal)
		{
			await _disposalRepository.AddAsync(disposal);
		}

		public async Task UpdateDisposalAsync(Disposal disposal)
		{
			await _disposalRepository.UpdateAsync(disposal);
		}

		public async Task DeleteDisposalAsync(int id)
		{
			await _disposalRepository.DeleteAsync(id);
		}
	}
}
