using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Services
{
	public class AssetService : IAssetService
	{
		private readonly IAssetRepository _assetRepository;
		private readonly IDisposalRepository _disposalRepository; // إضافة مستودع التكهين

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

		public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
		{
			return await _assetRepository.GetAllDepartmentsAsync();
		}

		public async Task<IEnumerable<Building>> GetBuildingsByFacilityAsync(int facilityId)
		{
			return await _assetRepository.GetBuildingsByFacilityAsync(facilityId);
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
	}
}
