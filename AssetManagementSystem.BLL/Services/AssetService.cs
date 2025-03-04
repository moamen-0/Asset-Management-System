using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.BLL.Repositories;
using AssetManagementSystem.DAL.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Services
{
	public class AssetService : IAssetService
	{
		private readonly IAssetRepository _assetRepository;
		private readonly IDisposalRepository _disposalRepository; // إضافة مستودع التكهين
		private readonly ILogger<AssetService> _logger;
		private readonly IBuildingRepository _buildingRepository;

		public AssetService(IAssetRepository assetRepository, IDisposalRepository disposalRepository)
		{
			_assetRepository = assetRepository;
			_disposalRepository = disposalRepository;
		}

		public async Task<IEnumerable<Asset>> GetAllAssetsAsync()
		{
			return await _assetRepository.GetAllAsync();
		}

		public async Task<Asset?> GetAssetByIdAsync(string assetTag)
		{
			return await _assetRepository.GetByIdAsync(assetTag);
		}

		public async Task AddAssetAsync(Asset asset)
		{
			await _assetRepository.AddAsync(asset);

		}

		public async Task UpdateAssetAsync(Asset asset)
		{
			await _assetRepository.UpdateAsync(asset);
		}

		public async Task DeleteAssetAsync(string assetTag)
		{
			await _assetRepository.DeleteAsync(assetTag);
		}

		public async Task<bool> DisposeAssetAsync(string assetTag, string disposalType, decimal saleValue)
		{
			var asset = await _assetRepository.GetByIdAsync(assetTag);
			if (asset == null || asset.IsDisposed) return false; // الأصل غير موجود أو مكهّن بالفعل

			asset.IsDisposed = true; // تعيين الأصل كمكهّن

			var disposal = new Disposal
			{
				AssetTag = assetTag,
				DisposalType = disposalType,
				DisposalDate = DateTime.UtcNow,
				SaleValue = saleValue
			};

			await _disposalRepository.AddAsync(disposal); // إضافة التكهين إلى قاعدة البيانات
			await _assetRepository.UpdateAsync(asset); // تحديث حالة الأصل

			return true;
		}

		public async Task<IEnumerable<Facility>> GetAllFacilitiesAsync()
		{
			return await _assetRepository.GetAllFacilitiesAsync();
		}
		public async Task<IEnumerable<Department>> GetDepartmentsByFacilityAsync(int facilityId)
		{
			var facility = (await _assetRepository.GetAllFacilitiesAsync())
				.FirstOrDefault(f => f.Id == facilityId);
			return facility?.Departments ?? Enumerable.Empty<Department>();
		}

		public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
		{
			return await _assetRepository.GetAllDepartmentsAsync();
		}

		public async Task<IEnumerable<Building>> GetBuildingsByFacilityAsync(int facilityId)
		{
			try
			{
				var buildings = await _buildingRepository.GetAllAsync();
				return buildings.Where(b => b.FacilityId == facilityId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in GetBuildingsByFacilityAsync for facilityId {FacilityId}", facilityId);
				return new List<Building>();
			}
		}

		public async Task<IEnumerable<Floor>> GetFloorsByBuildingAsync(int buildingId)
		{
			return await _assetRepository.GetFloorsByBuildingAsync(buildingId);
		}

		public async Task<IEnumerable<Room>> GetRoomsByFloorAsync(int floorId)
		{
			return await _assetRepository.GetRoomsByFloorAsync(floorId);
		}


		public async Task<IEnumerable<User>> GetAllUsersAsync()
		{
		
			return await _assetRepository.GetUsersAsync();


		}

		public async Task AddRange(IEnumerable<Asset> assets)
		{
			await _assetRepository.AddRange(assets);
		}

		public async Task BulkTransferAssetsAsync(IEnumerable<string> assetTags, int targetDepartmentId, string? targetUserId)
		{
			if (assetTags == null || !assetTags.Any())
			{
				throw new ArgumentException("No assets specified for transfer");
			}

			if (targetDepartmentId <= 0)
			{
				throw new ArgumentException("Invalid department ID");
			}

			await _assetRepository.BulkTransferAsync(assetTags, targetDepartmentId, targetUserId);
		}

		public async Task BulkDisposeAssetsAsync(IEnumerable<string> assetTags, string disposalType, decimal saleValue)
		{
			await _assetRepository.BulkDisposeAsync(assetTags, disposalType, saleValue);
		}

		// Add this new method
		public async Task<IEnumerable<Asset>> GetAssetsByTags(IEnumerable<string> assetTags)
		{
			var allAssets = await _assetRepository.GetAllAsync();
			return allAssets.Where(a => assetTags.Contains(a.AssetTag));
		}

		public async Task AssignSupervisorToAssetAsync(string assetTag, string supervisorId)
		{
			var asset = await _assetRepository.GetByIdAsync(assetTag);
			if (asset == null)
				throw new InvalidOperationException($"Asset with tag {assetTag} not found");

			asset.SupervisorId = supervisorId;
			await _assetRepository.UpdateAsync(asset);
		}

		public async Task<IEnumerable<User>> GetSupervisorsAsync()
		{
			// Depending on your implementation, you might want to query users with the Supervisor role
			// For now, we'll just return all users as potential supervisors
			return await _assetRepository.GetUsersAsync();
		}

		public async Task<IEnumerable<Asset>> GetAssetsBySupervisorIdAsync(string supervisorId)
		{
			if (string.IsNullOrEmpty(supervisorId))
				throw new ArgumentException("Supervisor ID cannot be null or empty");

			var allAssets = await _assetRepository.GetAllAsync();
			return allAssets.Where(a => a.SupervisorId == supervisorId).ToList();
		}
	}
}
