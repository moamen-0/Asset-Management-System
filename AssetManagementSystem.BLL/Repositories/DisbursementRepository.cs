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
	public class DisbursementRepository : IDisbursementRepository
	{
		private readonly AssetManagementDbContext _context;

		public DisbursementRepository(AssetManagementDbContext context)
		{
			_context = context;
		}

		public async Task<List<DisbursementRequest>> GetAllAsync()
		{
			return await _context.DisbursementRequests
				.Include(d => d.Items)
				.ToListAsync();
		}

		public async Task<DisbursementRequest> GetByIdAsync(int id)
		{
			return await _context.DisbursementRequests
				.Include(d => d.Items)
				.FirstOrDefaultAsync(d => d.Id == id);
		}

		public async Task<DisbursementRequest> AddAsync(DisbursementRequest request)
		{
			await _context.DisbursementRequests.AddAsync(request);
			await _context.SaveChangesAsync();
			return request;
		}

		public async Task UpdateAsync(DisbursementRequest request)
		{
			_context.DisbursementRequests.Update(request);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var request = await _context.DisbursementRequests.FindAsync(id);
			if (request != null)
			{
				_context.DisbursementRequests.Remove(request);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<string> GenerateRequestNumberAsync()
		{
			// Format: yy/sequential number
			var currentYear = DateTime.Now.Year.ToString().Substring(2);
			var lastRequest = await _context.DisbursementRequests
				.OrderByDescending(d => d.Id)
				.FirstOrDefaultAsync();

			int nextNumber = 1;
			if (lastRequest != null && !string.IsNullOrEmpty(lastRequest.RequestNumber))
			{
				var parts = lastRequest.RequestNumber.Split('/');
				if (parts.Length > 1 && int.TryParse(parts[1], out int lastNumber))
				{
					nextNumber = lastNumber + 1;
				}
			}

			return $"{currentYear}/{nextNumber:D8}";
		}
	}
}
