using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IRepository
{
	public interface IStoreKeeperRepository
	{
		Task<List<StoreKeeper>> GetAllAsync();
		Task<StoreKeeper> GetByAssetTagPrefixAsync(string assetTagPrefix);
	}
}
