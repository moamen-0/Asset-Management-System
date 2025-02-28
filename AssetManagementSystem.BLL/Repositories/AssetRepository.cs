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
			// Create a strategy for executing the operations
			var strategy = _context.Database.CreateExecutionStrategy();

			await strategy.ExecuteAsync(async () =>
			{
				// Start the transaction within the execution strategy
				using var transaction = await _context.Database.BeginTransactionAsync();
				try
				{
					// Get target department
					var targetDepartment = await _context.Departments.FindAsync(targetDepartmentId);
					if (targetDepartment == null)
					{
						throw new InvalidOperationException($"Department with ID {targetDepartmentId} not found");
					}

					// Instead of directly using Contains, get all assets and filter in memory
					var allAssets = await _context.Assets.ToListAsync();
					var assets = allAssets.Where(a => assetTags.Contains(a.AssetTag)).ToList();

					if (!assets.Any())
					{
						throw new InvalidOperationException("No assets found for transfer");
					}

					// Get department names for all relevant departments at once
					var departmentIds = assets.Select(a => a.DepartmentId).Distinct().ToList();
					var departments = await _context.Departments
						.Where(d => departmentIds.Contains(d.Id))
						.ToDictionaryAsync(d => d.Id, d => d.Name);

					var transfers = new List<AssetTransfer>();
					var currentTime = DateTime.UtcNow;

					foreach (var asset in assets)
					{
						// Get the original department name from our cached dictionary
						string oldDepartmentName = "None";
						if (departments.TryGetValue(asset.DepartmentId, out var name))
						{
							oldDepartmentName = name;
						}

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
					}

					// Execute all database operations
					await _context.Transfers.AddRangeAsync(transfers);
					await _context.SaveChangesAsync();

					// Commit the transaction
					await transaction.CommitAsync();
				}
				catch (Exception ex)
				{
					// Rollback on failure
					await transaction.RollbackAsync();
					throw new Exception($"Bulk transfer failed: {ex.Message}", ex);
				}
			});
		}
		public async Task BulkDisposeAsync(IEnumerable<string> assetTags, string disposalType, decimal saleValue)
		{
			// Create a strategy for executing the operations
			var strategy = _context.Database.CreateExecutionStrategy();

			await strategy.ExecuteAsync(async () =>
			{
				// Start the transaction within the execution strategy
				using var transaction = await _context.Database.BeginTransactionAsync();
				try
				{
					// Instead of directly using Contains, get all assets and filter in memory
					var allAssets = await _context.Assets.ToListAsync();
					var assets = allAssets.Where(a => assetTags.Contains(a.AssetTag)).ToList();

					if (!assets.Any())
					{
						throw new InvalidOperationException("No assets found for disposal");
					}

					var disposals = new List<Disposal>();
					var currentTime = DateTime.UtcNow;

					foreach (var asset in assets)
					{
						// Update asset
						asset.IsDisposed = true;
						asset.Status = "Disposed";

						// Create disposal record
						disposals.Add(new Disposal
						{
							AssetTag = asset.AssetTag,
							DisposalType = disposalType,
							DisposalDate = currentTime,
							SaleValue = saleValue
						});
					}

					// Execute all database operations
					await _context.Disposals.AddRangeAsync(disposals);
					await _context.SaveChangesAsync();

					// Commit the transaction
					await transaction.CommitAsync();
				}
				catch (Exception ex)
				{
					// Rollback on failure
					await transaction.RollbackAsync();
					throw new Exception($"Bulk disposal failed: {ex.Message}", ex);
				}
			});
		}
	}
}
