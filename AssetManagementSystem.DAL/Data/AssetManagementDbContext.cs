using AssetManagementSystem.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Data
{
	public class AssetManagementDbContext : IdentityDbContext<User>
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		public AssetManagementDbContext(DbContextOptions<AssetManagementDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public DbSet<Facility> Facilities { get; set; }
		public DbSet<Building> Buildings { get; set; }
		public DbSet<Floor> Floors { get; set; }
		public DbSet<Room> Rooms { get; set; }
		public DbSet<Department> Departments { get; set; }
		
		public DbSet<Asset> Assets { get; set; }
		public DbSet<AssetTransfer> Transfers { get; set; }
		public DbSet<Disposal> Disposals { get; set; }
		public DbSet<ChangeLog> changeLogs { get; set; }
		public DbSet<DisbursementRequest> DisbursementRequests { get; set; }
		public DbSet<DisbursementItem> DisbursementItems { get; set; }
		public DbSet<StoreKeeper> StoreKeepers { get; set; }
		private string GetCurrentUserId()
		{
			var httpContext = _httpContextAccessor.HttpContext;
			if (httpContext == null || httpContext.User == null)
			{
				return null; // No user is logged in
			}

			// Get the user ID from the claims
			return httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		}
		public override int SaveChanges()
		{
			// Capture the state of entities before saving
			var entries = ChangeTracker.Entries()
				.Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
				.Select(e => new
				{
					Entry = e,
					PreSaveState = e.State // Capture the state before saving
				})
				.ToList();

			// Save changes to the database
			int result = base.SaveChanges();

			// Create change logs using the pre-saved state
			foreach (var entry in entries)
			{
				var _changeLog = CreateChangeLog(entry.Entry, entry.PreSaveState);
				changeLogs.Add(_changeLog);
			}

			// Save the change logs
			base.SaveChanges();

			return result;
		}

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			// Capture the state of entities before saving
			var entries = ChangeTracker.Entries()
				.Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
				.Select(e => new
				{
					Entry = e,
					PreSaveState = e.State // Capture the state before saving
				})
				.ToList();

			// Save changes to the database
			int result = await base.SaveChangesAsync(cancellationToken);

			// Create change logs using the pre-saved state
			foreach (var entry in entries)
			{
				var _changeLog = CreateChangeLog(entry.Entry, entry.PreSaveState);
				changeLogs.Add(_changeLog);
			}

			// Save the change logs
			await base.SaveChangesAsync(cancellationToken);

			return result;
		}

		private void TrackChanges()
		{
			// Capture the state of entities before saving
			var _changeLogs = ChangeTracker.Entries()
				.Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
				.Select(entry => new
				{
					Entry = entry,
					PreSaveState = entry.State // Capture the state before saving
				})
				.Select(entry => CreateChangeLog(entry.Entry, entry.PreSaveState)) // Pass the pre-saved state
				.Where(log => log != null)
				.ToList();

			if (_changeLogs.Any())
			{
				changeLogs.AddRange(_changeLogs);
			}
		}
		private ChangeLog CreateChangeLog(EntityEntry entry, EntityState preSaveState)
		{
			var entityName = entry.Entity.GetType().Name;

			// Determine the key based on the entity type
			string entityId = "0";
			if (entityName == "Asset")
			{
				entityId = entry.Property("AssetTag")?.CurrentValue?.ToString() ?? "Unknown";
			}
			else if (entityName == "Room")
			{
				entityId = entry.Property("RoomTag")?.CurrentValue?.ToString() ?? "Unknown";
			}
			else
			{
				if (entry.Metadata.ClrType == typeof(IdentityUserRole<string>))
				{
					var userIdProp = entry.Property("UserId");
					var roleIdProp = entry.Property("RoleId");

					entityId = $"UserId: {userIdProp?.CurrentValue?.ToString() ?? "Unknown"}, RoleId: {roleIdProp?.CurrentValue?.ToString() ?? "Unknown"}";
				}
				else
				{
					var idProp = entry.Property("Id");
					entityId = idProp?.CurrentValue?.ToString() ?? "0";
				}


				// If the entity is new and hasn't been saved yet, use a temporary GUID
				if (preSaveState == EntityState.Added && entityId == "0")
				{
					entityId = "TEMP-" + Guid.NewGuid().ToString();
				}
			}
			string oldValues = (preSaveState == EntityState.Modified || preSaveState == EntityState.Deleted)
				? JsonConvert.SerializeObject(
					entry.OriginalValues.Properties.ToDictionary(
						p => p.Name,
						p => entry.OriginalValues[p]?.ToString() ?? "NULL"
					),
					new JsonSerializerSettings
					{
						Formatting = Formatting.Indented,
						NullValueHandling = NullValueHandling.Include
					})
				: null;

			string newValues = (preSaveState == EntityState.Added || preSaveState == EntityState.Modified)
				? JsonConvert.SerializeObject(
					entry.CurrentValues.Properties.ToDictionary(
						p => p.Name,
						p => entry.CurrentValues[p]?.ToString() ?? "NULL"
					),
					new JsonSerializerSettings
					{
						Formatting = Formatting.Indented,
						NullValueHandling = NullValueHandling.Include
					})
				: null;

			// Use the pre-saved state to determine the ActionType
			string actionType = preSaveState.ToString();

			// Get the logged-in user's ID
			string userId = GetCurrentUserId();

			return new ChangeLog
			{
				EntityName = entityName,
				EntityId = entityId, // Store AssetTag or RoomTag as the primary key
				ActionType = actionType, // Use the pre-saved state
				OldValues = oldValues,
				NewValues = newValues,
				ChangeDate = DateTime.UtcNow,
				UserId = userId // Assign the logged-in user's ID
			};
		}

		//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		//{
		//	base.OnConfiguring(optionsBuilder);
		//	optionsBuilder.UseSqlServer("DefaultConnection",
		//		sqlOptions => sqlOptions.CommandTimeout(60)); 
		//}


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// تعريف العلاقات بين الجداول
			modelBuilder.Entity<Building>()
				.HasOne(b => b.Facility)
				.WithMany(f => f.Buildings)
				.HasForeignKey(b => b.FacilityId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Floor>()
				.HasOne(f => f.Building)
				.WithMany(b => b.Floors)
				.HasForeignKey(f => f.BuildingId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Room>()
				.HasOne(r => r.Floor)
				.WithMany(f => f.Rooms)
				.HasForeignKey(r => r.FloorId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Room>()
				.HasOne(r => r.Department)
				.WithMany(d => d.Rooms)
				.HasForeignKey(r => r.DepartmentId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Asset>()
				.HasOne(a => a.Facility)
				.WithMany()
				.HasForeignKey(a => a.FacilityId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Asset>()
				.HasOne(a => a.Building)
				.WithMany()
				.HasForeignKey(a => a.BuildingId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Asset>()
				.HasOne(a => a.Floor)
				.WithMany()
				.HasForeignKey(a => a.FloorId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Asset>()
				.HasOne(a => a.Room)
				.WithMany()
				.HasForeignKey(a => a.RoomTag)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Asset>()
				.HasOne(a => a.Department)
				.WithMany()
				.HasForeignKey(a => a.DepartmentId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ChangeLog>()
				.HasOne(cl => cl.User)
				.WithMany()
				.HasForeignKey(cl => cl.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Asset>()
		  .HasOne(a => a.User)  // الأصل لديه مستلم واحد
		  .WithMany(u => u.Assets)  // المستخدم يمكن أن يستلم عدة أصول
		  .HasForeignKey(a => a.UserId)
		  .OnDelete(DeleteBehavior.SetNull); // في حالة حذف المستخدم، لا نحذف الأصول

			modelBuilder.Entity<ChangeLog>()
	  .HasIndex(c => c.ChangeDate);

			modelBuilder.Entity<ChangeLog>()
				.HasIndex(c => new { c.EntityName, c.EntityId });
			modelBuilder.Entity<DisbursementItem>()
	  .HasOne<DisbursementRequest>()
	  .WithMany(d => d.Items)
	  .HasForeignKey(di => di.DisbursementRequestId)
	  .OnDelete(DeleteBehavior.Cascade);

		}
	}
}
