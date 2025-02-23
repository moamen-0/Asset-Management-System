using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IRepository
{
	public interface IAssetTransferRepository
	{
		Task<IEnumerable<AssetTransfer>> GetAllAsync();
		Task<AssetTransfer?> GetByIdAsync(int id);
		Task AddAsync(AssetTransfer assetTransfer);
		Task UpdateAsync(AssetTransfer assetTransfer);
		Task DeleteAsync(int id);
	}
}
