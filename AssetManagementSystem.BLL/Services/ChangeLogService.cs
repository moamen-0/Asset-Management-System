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
		private readonly IDepartmentRepository _departmentRepository;
		private readonly IBuildingRepository _buildingRepository;
		private readonly IMemoryCache _cache;
		private readonly ILogger<ChangeLogService> _logger;
		private const string CacheKeyPrefix = "ChangeLogs_";

		public ChangeLogService(
	  IChangeLogRepository changeLogRepository,
	  IMemoryCache cache,
	  ILogger<ChangeLogService> logger,
	  IDepartmentRepository departmentRepository,
	  IBuildingRepository buildingRepository)
		{
			_changeLogRepository = changeLogRepository;
			_cache = cache;
			_logger = logger;
			_departmentRepository = departmentRepository;
			_buildingRepository = buildingRepository;
		}

		public async Task AddChangeLogAsync(ChangeLog changeLog)
		{
			// Add facility information to change logs where relevant
			if (changeLog.EntityName == "Department" || changeLog.EntityName == "Building")
			{
				var facilityInfo = await GetFacilityInfo(changeLog.EntityId, changeLog.EntityName);
				changeLog.NewValues = $"Facility: {facilityInfo}, " + changeLog.NewValues;
			}

			await _changeLogRepository.AddAsync(changeLog);
		}
		private async Task<string> GetFacilityInfo(string entityId, string entityType)
		{
			if (entityType == "Department")
			{
				var department = await _departmentRepository.GetByIdAsync(int.Parse(entityId));
				return department?.Facility?.Name ?? "Unknown Facility";
			}
			else if (entityType == "Building")
			{
				var building = await _buildingRepository.GetByIdAsync(int.Parse(entityId));
				return building?.Facility?.Name ?? "Unknown Facility";
			}
			return "Unknown Facility";
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
					entry.Size = 1;
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

		public async Task<ChangeLog> GetChangeLogByEntityAsync(string entityName, string entityId)
		{
			return await _changeLogRepository.GetByEntityAsync(entityName, entityId);
		}
	}
}
