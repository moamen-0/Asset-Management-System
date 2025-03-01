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
	public class ChangeLogRepository : IChangeLogRepository
	{
		private readonly AssetManagementDbContext _context;

		public ChangeLogRepository(AssetManagementDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(ChangeLog changeLog)
		{
			await _context.changeLogs.AddAsync(changeLog);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var changeLog = await GetByIdAsync(id);
			if (changeLog != null)
			{
				_context.changeLogs.Remove(changeLog);
				await _context.SaveChangesAsync();
			}
			else
			{
				throw new Exception("ChangeLog not found");
			}

			}

		public async Task<IEnumerable<ChangeLog>> GetAllAsync()
		{
			return await _context.changeLogs
						.Include(c => c.User) 
						.ToListAsync();
		}

		public async Task<ChangeLog?> GetByIdAsync(int id)
		{
			return await _context.changeLogs.FindAsync(id);

		}

		public async Task UpdateAsync(ChangeLog changeLog)
		{
			var changeLogToUpdate = await GetByIdAsync(changeLog.Id);
			if (changeLogToUpdate != null)
			{
				_context.changeLogs.Update(changeLog);
				await _context.SaveChangesAsync();
			}
			else
			{
				throw new Exception("ChangeLog not found");
			}
		}

		public async Task<(IEnumerable<ChangeLog> Logs, int TotalCount)> GetPaginatedAsync(
	   int start,
	   int length,
	   string searchValue,
	   string sortColumn,
	   string sortDirection)
		{
			var query = _context.changeLogs
				.Include(c => c.User)
				.AsNoTracking();

			// Get total count
			var totalCount = await query.CountAsync();

			// Apply search
			if (!string.IsNullOrEmpty(searchValue))
			{
				query = query.Where(c =>
					c.EntityName.Contains(searchValue) ||
					c.EntityId.Contains(searchValue) ||
					c.ActionType.Contains(searchValue) ||
					(c.User != null && c.User.FullName.Contains(searchValue)));
			}

			// Apply sorting
			query = sortColumn?.ToLower() switch
			{
				"entityname" => sortDirection == "asc"
					? query.OrderBy(c => c.EntityName)
					: query.OrderByDescending(c => c.EntityName),
				"changedate" => sortDirection == "asc"
					? query.OrderBy(c => c.ChangeDate)
					: query.OrderByDescending(c => c.ChangeDate),
				_ => query.OrderByDescending(c => c.ChangeDate) // default sort
			};

			// Apply pagination
			var logs = await query
				.Skip(start)
				.Take(length)
				.ToListAsync();

			return (logs, totalCount);
		}

		public async Task<ChangeLog> GetByEntityAsync(string entityName, string entityId)
		{
			return await _context.changeLogs
				.Include(c => c.User)
				.Where(c => c.EntityName == entityName && c.EntityId == entityId)
				.OrderByDescending(c => c.ChangeDate)
				.FirstOrDefaultAsync();
		}
	}
}
