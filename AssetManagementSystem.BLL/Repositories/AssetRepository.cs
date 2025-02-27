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
	public class AssetRepository : IAssetRepository
	{
		private readonly AssetManagementDbContext _context;

		public AssetRepository(AssetManagementDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<Asset>> GetAllAsync()
		{
			return await _context.Assets
				.Include(a => a.Facility)
				.Include(a => a.Building)
				.Include(a => a.Floor)
				.Include(a => a.Room)
				.Include(a => a.Department)
				.Include(a => a.User)
				.ToListAsync();
		}

		public async Task<Asset?> GetByIdAsync(string assetTag)
		{
			return await _context.Assets
				.Include(a => a.Facility)
				.Include(a => a.Building)
				.Include(a => a.Floor)
				.Include(a => a.Room)
				.Include(a => a.Department)
				.Include(a => a.User)
				.FirstOrDefaultAsync(a => a.AssetTag == assetTag);
		}

		public async Task AddAsync(Asset asset)
		{
			await _context.Assets.AddAsync(asset);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync(Asset asset)
		{
			_context.Assets.Update(asset);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(string assetTag)
		{
			var asset = await _context.Assets.FindAsync(assetTag);
			if (asset != null)
			{
				_context.Assets.Remove(asset);
				await _context.SaveChangesAsync();
			}
			else
			{
				throw new Exception("Asset not found");
			}
		}

		public async Task<IEnumerable<Facility>> GetAllFacilitiesAsync()
		{
			return await _context.Facilities
				.Include(f => f.Buildings)
				.ThenInclude(b => b.Floors)
				.ThenInclude(f => f.Rooms)
				.ToListAsync();
		}

		public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
		{
			return await _context.Departments.ToListAsync();
		}

		public async Task<IEnumerable<Building>> GetBuildingsByFacilityAsync(int facilityId)
		{
			return await _context.Buildings
				.Where(b => b.FacilityId == facilityId)
				.OrderBy(b => b.Name)
				.ToListAsync();
		}

		public async Task<IEnumerable<Floor>> GetFloorsByBuildingAsync(int buildingId)
		{
			return await _context.Floors
				.Where(f => f.BuildingId == buildingId)
				.OrderBy(f => f.Name)
				.ToListAsync();
		}
		public async Task<IEnumerable<Room>> GetRoomsByFloorAsync(int floorId)
		{
			return await _context.Rooms
				.Where(r => r.FloorId == floorId)
				.OrderBy(r => r.Name)
				.ToListAsync();
		}

		public async Task<IEnumerable<User>> GetUsersAsync()
		{
			return await _context.Users.ToListAsync();

		}

		public async Task AddRange(IEnumerable<Asset> assets)
		{
			await _context.Assets.AddRangeAsync(assets);
			await _context.SaveChangesAsync();
		}
		public async Task BulkTransferAsync(IEnumerable<string> assetTags, int targetDepartmentId, string? targetUserId)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// Get target department
				var targetDepartment = await _context.Departments
					.FirstOrDefaultAsync(d => d.Id == targetDepartmentId);
				if (targetDepartment == null)
				{
					throw new InvalidOperationException($"Department with ID {targetDepartmentId} not found");
				}

				// Get all affected assets in a single query
				var assets = await _context.Assets
					.Include(a => a.Department)
					.Where(a => assetTags.Contains(a.AssetTag))
					.ToListAsync();

				if (!assets.Any())
				{
					throw new InvalidOperationException("No assets found for transfer");
				}

				var transfers = new List<AssetTransfer>();
				var changeLogs = new List<ChangeLog>();
				var currentTime = DateTime.UtcNow;

				foreach (var asset in assets)
				{
					var oldDepartmentName = asset.Department?.Name ?? "None";

					// Create transfer record
					transfers.Add(new AssetTransfer
					{
						AssetTag = asset.AssetTag,
						FromDepartment = oldDepartmentName,
						ToDepartment = targetDepartment.Name,
						TransferDate = currentTime,
						TransferType = "internal"
					});

					// Update asset
					asset.DepartmentId = targetDepartmentId;
					asset.UserId = targetUserId;

					// Create changelog
					changeLogs.Add(new ChangeLog
					{
						EntityName = "Asset",
						EntityId = asset.AssetTag,
						ActionType = "BulkTransfer",
						ChangeDate = currentTime,
						OldValues = $"DepartmentId: {oldDepartmentName}, UserId: {asset.UserId}",
						NewValues = $"DepartmentId: {targetDepartment.Name}, UserId: {targetUserId ?? "None"}",
						UserId = targetUserId
					});
				}

				// Execute all database operations
				await _context.Transfers.AddRangeAsync(transfers);
				await _context.changeLogs.AddRangeAsync(changeLogs);
				_context.Assets.UpdateRange(assets);

				// Save changes and commit transaction
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception($"Bulk transfer failed: {ex.Message}", ex);
			}
		}
		public async Task BulkDisposeAsync(IEnumerable<string> assetTags, string disposalType, decimal saleValue)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var assets = await _context.Assets
					.Where(a => assetTags.Contains(a.AssetTag))
					.ToListAsync();

				var disposals = new List<Disposal>();
				var changeLogs = new List<ChangeLog>();

				foreach (var asset in assets)
				{
					asset.IsDisposed = true;
					asset.Status = "Disposed";

					disposals.Add(new Disposal
					{
						AssetTag = asset.AssetTag,
						DisposalType = disposalType,
						DisposalDate = DateTime.UtcNow,
						SaleValue = saleValue
					});

					changeLogs.Add(new ChangeLog
					{
						EntityName = "Asset",
						EntityId = asset.AssetTag,
						ActionType = "Disposal",
						ChangeDate = DateTime.UtcNow,
						OldValues = "IsDisposed: False",
						NewValues = $"IsDisposed: True, DisposalType: {disposalType}"
					});
				}

				await _context.Disposals.AddRangeAsync(disposals);
				await _context.changeLogs.AddRangeAsync(changeLogs);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}
	}
}
