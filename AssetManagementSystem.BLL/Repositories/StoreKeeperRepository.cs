using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.DAL.Data;
using AssetManagementSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Repositories
{
	public class StoreKeeperRepository : IStoreKeeperRepository
	{
		private readonly AssetManagementDbContext _context;

		public StoreKeeperRepository(AssetManagementDbContext context)
		{
			_context = context;
		}

		public async Task<List<StoreKeeper>> GetAllAsync()
		{
			return await _context.StoreKeepers.ToListAsync();
		}

		public async Task<StoreKeeper> GetByAssetTagPrefixAsync(string assetTagPrefix)
		{
			return await _context.StoreKeepers
				.FirstOrDefaultAsync(sk => assetTagPrefix.StartsWith(sk.AssetTypeTag));
		}
	}
}
