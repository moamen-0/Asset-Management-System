using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IService
{
	public interface IAssetTransferService
	{
		Task<IEnumerable<AssetTransfer>> GetAllAssetTransfersAsync();
		Task<AssetTransfer?> GetAssetTransferByIdAsync(int id);
		Task AddAssetTransferAsync(AssetTransfer assetTransfer);
		Task UpdateAssetTransferAsync(AssetTransfer assetTransfer);
		Task DeleteAssetTransferAsync(int id);
	}
}
