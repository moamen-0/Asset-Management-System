using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IService
{
	public interface IAssetService
	{

		Task<IEnumerable<Asset>> GetAllAssetsAsync();
		Task<Asset?> GetAssetByIdAsync(string assetTag);
		Task AddAssetAsync(Asset asset);

		Task AddRange(IEnumerable<Asset> assets);

		Task UpdateAssetAsync(Asset asset);
		Task DeleteAssetAsync(string assetTag);
		Task<bool> DisposeAssetAsync(string assetTag, string disposalType, decimal saleValue);
		Task<IEnumerable<Facility>> GetAllFacilitiesAsync();
		Task<IEnumerable<Department>> GetAllDepartmentsAsync();
		Task<IEnumerable<Building>> GetBuildingsByFacilityAsync(int facilityId);
		Task<IEnumerable<Floor>> GetFloorsByBuildingAsync(int buildingId);
		Task<IEnumerable<Room>> GetRoomsByFloorAsync(int floorId);
		Task<IEnumerable<User>> GetAllUsersAsync();
		Task BulkTransferAssetsAsync(IEnumerable<string> assetTags, int targetDepartmentId, string? targetUserId);
		Task BulkDisposeAssetsAsync(IEnumerable<string> assetTags, string disposalType, decimal saleValue);
		Task<IEnumerable<Asset>> GetAssetsByTags(IEnumerable<string> assetTags);

	}
}
