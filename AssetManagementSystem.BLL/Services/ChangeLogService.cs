using AssetManagementSystem.BLL.Interfaces;
using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.BLL.Repositories;
using AssetManagementSystem.DAL.Data;
using AssetManagementSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Services
{
	public class ChangeLogService : IChangeLogService
	{
		private readonly IChangeLogRepository _changeLogRepository;
		private readonly IMemoryCache _cache;
		private readonly ILogger<ChangeLogService> _logger;
		private const string CacheKeyPrefix = "ChangeLogs_";

		public ChangeLogService(
	  IChangeLogRepository changeLogRepository,
	  IMemoryCache cache,
	  ILogger<ChangeLogService> logger)
		{
			_changeLogRepository = changeLogRepository;
			_cache = cache;
			_logger = logger;
		}

		public async Task AddChangeLogAsync(ChangeLog changeLog)
		{
			await _changeLogRepository.AddAsync(changeLog);
		}

		public async Task DeleteChangeLogAsync(int id)
		{
			await _changeLogRepository.DeleteAsync(id);
		}

		public async Task<IEnumerable<ChangeLog>> GetAllChangeLogsAsync()
		{
			return await _changeLogRepository.GetAllAsync();
		}

		public async Task<ChangeLog?> GetChangeLogByIdAsync(int id)
		{
			return await _changeLogRepository.GetByIdAsync(id);
		}

		public async Task UpdateChangeLogAsync(ChangeLog changeLog)
		{
			await _changeLogRepository.UpdateAsync(changeLog);
		}

		public async Task<(IEnumerable<ChangeLog> Logs, int TotalCount)> GetPaginatedAsync(
	   int start,
	   int length,
	   string searchValue,
	   string sortColumn,
	   string sortDirection)
		{
			try
			{
				string cacheKey = $"{CacheKeyPrefix}{start}_{length}_{searchValue}_{sortColumn}_{sortDirection}";

				return await _cache.GetOrCreateAsync(cacheKey, async entry =>
				{
					entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
					return await _changeLogRepository.GetPaginatedAsync(start, length, searchValue, sortColumn, sortDirection);
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting paginated change logs");
				throw;
			}
		}
	}
}
