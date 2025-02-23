using AssetManagementSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IRepository
{
	public interface IAssetRepository
	{
		Task<IEnumerable<Asset>> GetAllAsync();
		Task<Asset?> GetByIdAsync(string assetTag);
		Task AddAsync(Asset asset);
		Task UpdateAsync(Asset asset);
		Task DeleteAsync(string assetTag);
		Task<IEnumerable<Facility>> GetAllFacilitiesAsync();
		 Task<IEnumerable<Department>> GetAllDepartmentsAsync();
		Task<IEnumerable<Building>> GetBuildingsByFacilityAsync(int facilityId);
		Task<IEnumerable<Floor>> GetFloorsByBuildingAsync(int buildingId);
		Task<IEnumerable<Room>> GetRoomsByFloorAsync(int floorId);
		Task<IEnumerable<User>> GetUsersAsync();
		Task AddRange(IEnumerable<Asset> assets);
		Task BulkTransferAsync(IEnumerable<string> assetTags, int targetDepartmentId, string? targetUserId);
		Task BulkDisposeAsync(IEnumerable<string> assetTags, string disposalType, decimal saleValue);

	}
}
