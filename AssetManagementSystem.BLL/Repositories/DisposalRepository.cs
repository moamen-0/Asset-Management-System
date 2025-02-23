using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.DAL.Data;
using AssetManagementSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Repositories
{
	public class DisposalRepository : IDisposalRepository
	{
		private readonly AssetManagementDbContext _context;

		public DisposalRepository(AssetManagementDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<Disposal>> GetAllAsync()
		{
			return await _context.Disposals.Include(d => d.Asset).ToListAsync();
		}

		public async Task<Disposal?> GetByIdAsync(int id)
		{
			return await _context.Disposals.Include(d => d.Asset)
										   .FirstOrDefaultAsync(d => d.Id == id);
		}

		public async Task AddAsync(Disposal disposal)
		{
			await _context.Disposals.AddAsync(disposal);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync(Disposal disposal)
		{
			_context.Disposals.Update(disposal);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var disposal = await _context.Disposals.FindAsync(id);
			if (disposal != null)
			{
				_context.Disposals.Remove(disposal);
				await _context.SaveChangesAsync();
			}
		}
	}
}
