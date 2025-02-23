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
	public class AssetTransferRepository : IAssetTransferRepository
	{
		private readonly AssetManagementDbContext _context;

		public AssetTransferRepository(AssetManagementDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(AssetTransfer assetTransfer)
		{
			await _context.Transfers.AddAsync(assetTransfer);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var assetTransfer = await GetByIdAsync(id);
			if (assetTransfer != null)
			{
				_context.Transfers.Remove(assetTransfer);
				await _context.SaveChangesAsync();
			}
			else
			{
				throw new Exception("Asset Transfer not found");
			}
		}

		public async Task<IEnumerable<AssetTransfer>> GetAllAsync()
		{
			return await _context.Transfers
				.Include(a => a.Asset)
				.ToListAsync();

		}

		public async Task<AssetTransfer?> GetByIdAsync(int id)
		{
			return await _context.Transfers
				.Include(a => a.Asset)
				  .FirstOrDefaultAsync(a => a.Id == id);
		}

		public async Task UpdateAsync(AssetTransfer assetTransfer)
		{
			var existingEntity = _context.Transfers.Local.FirstOrDefault(e => e.Id == assetTransfer.Id);
			if (existingEntity != null)
			{
				_context.Entry(existingEntity).State = EntityState.Detached;
			}
			_context.Transfers.Update(assetTransfer);
			await _context.SaveChangesAsync();
		}
	}
}
