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
	public class ReturnDocumentRepository : IReturnDocumentRepository
	{
		private readonly AssetManagementDbContext _context;

		public ReturnDocumentRepository(AssetManagementDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<ReturnDocument>> GetAllAsync()
		{
			return await _context.ReturnDocuments
				.Include(d => d.Items)
				.Include(d => d.Department)
				.ToListAsync();
		}

		public async Task<ReturnDocument> GetByIdAsync(int id)
		{
			return await _context.ReturnDocuments
				.Include(d => d.Items)
				.Include(d => d.Department)
				.FirstOrDefaultAsync(d => d.Id == id);
		}

		public async Task<ReturnDocument> AddAsync(ReturnDocument document)
		{
			await _context.ReturnDocuments.AddAsync(document);
			await _context.SaveChangesAsync();
			return document;
		}

		public async Task UpdateAsync(ReturnDocument document)
		{
			_context.ReturnDocuments.Update(document);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var document = await _context.ReturnDocuments.FindAsync(id);
			if (document != null)
			{
				_context.ReturnDocuments.Remove(document);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<string> GenerateDocumentNumberAsync()
		{
			// Format: "DEC-yyyy" 
			var currentDate = DateTime.Now;
			var month = currentDate.ToString("MMM").ToUpper();
			var year = currentDate.Year;

			return $"{month}-{year}";
		}
	}
}
