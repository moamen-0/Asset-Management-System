using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Services
{
	public class AssetTransferService : IAssetTransferService
	{
		private readonly IAssetTransferRepository _assetTransferRepository;

		public AssetTransferService(IAssetTransferRepository assetTransferRepository)
		{
			_assetTransferRepository = assetTransferRepository;
		}

		public async Task AddAssetTransferAsync(AssetTransfer assetTransfer)
		{
			await _assetTransferRepository.AddAsync(assetTransfer);
		}

		public async Task DeleteAssetTransferAsync(int id)
		{
			await _assetTransferRepository.DeleteAsync(id);
		}

		public Task<IEnumerable<AssetTransfer>> GetAllAssetTransfersAsync()
		{
			return _assetTransferRepository.GetAllAsync();
		}

		public Task<AssetTransfer?> GetAssetTransferByIdAsync(int id)
		{
			return _assetTransferRepository.GetByIdAsync(id);
		}

		public Task UpdateAssetTransferAsync(AssetTransfer assetTransfer)
		{
			return _assetTransferRepository.UpdateAsync(assetTransfer);
		}
	}
}
