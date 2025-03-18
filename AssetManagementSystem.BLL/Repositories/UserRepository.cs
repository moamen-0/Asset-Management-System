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
	public class UserRepository : IUserRepository
	{
		private readonly AssetManagementDbContext _context;

		public UserRepository(AssetManagementDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<User>> GetAllUsersAsync()
		{
			return await _context.Users
				.AsNoTracking() // عدم تتبع البيانات لزيادة السرعة
				.Include(u=>u.Assets)
				.Include(u => u.Department)
				.ToListAsync();
		}

		public async Task<User> GetUserByIdAsync(string id)
		{
			return await _context.Users
				.Include(u => u.Department)
				.Include(u => u.Assets)  // Include the Assets collection
					.ThenInclude(a => a.Department)  // Optionally include nested properties
				.FirstOrDefaultAsync(u => u.Id == id);
		}

		public async Task<User> GetUserByEmailAsync(string email)
		{
			return await _context.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Email == email);
		}

		public async Task<User> GetUserByUserNameAsync(string userName)
		{
			return await _context.Users
				.Include(u => u.Department)
				.Include(u => u.Assets)
				.FirstOrDefaultAsync(u => u.UserName == userName);
		}

		public async Task<User> GetUserByFullNameAsync(string FullName)
		{
			return await _context.Users
				.Include(u => u.Department)
				.Include(u => u.Assets)
				.FirstOrDefaultAsync(u => u.FullName == FullName);
		}

		public async Task<User> GetUserByPhoneNumberAsync(string phoneNumber)
		{
			return await _context.Users
				.Include(u => u.Department)
				.Include(u => u.Assets)
				.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
		}

		public async Task<User> GetUserByDepartmentAsync(int departmentId)
		{
			return await _context.Users
				.Include(u => u.Department)
				.Include(u => u.Assets)
				.FirstOrDefaultAsync(u => u.DepartmentId == departmentId);
		}

		public async Task<User> AddUserAsync(User user)
		{
			await _context.Users.AddAsync(user);
			await _context.SaveChangesAsync();
			return user;
		}

		public async Task<User> UpdateUserAsync(User user)
		{
			var existingUser = await _context.Users.FindAsync(user.Id);
			if (existingUser == null) return null;

			_context.Entry(existingUser).CurrentValues.SetValues(user);
			await _context.SaveChangesAsync();
			return existingUser;
		}

		public async Task<User> DeleteUserAsync(string id)
		{
			// Create a strategy for executing the operations
			var strategy = _context.Database.CreateExecutionStrategy();

			return await strategy.ExecuteAsync(async () =>
			{
				// All transaction work goes inside this lambda
				using var transaction = await _context.Database.BeginTransactionAsync();
				try
				{
					var user = await _context.Users
						.Include(u => u.Assets)
						.Include(u => u.SupervisedAssets)
						.FirstOrDefaultAsync(u => u.Id == id);

					if (user == null) return null;

					// Clear all references in Assets table
					var userAssets = await _context.Assets.Where(a => a.UserId == id).ToListAsync();
					foreach (var asset in userAssets)
					{
						asset.UserId = null;
					}

					// Clear all supervisor references in Assets table
					var supervisedAssets = await _context.Assets.Where(a => a.SupervisorId == id).ToListAsync();
					foreach (var asset in supervisedAssets)
					{
						asset.SupervisorId = null;
					}

					// Update ChangeLogs
					var changelogs = await _context.changeLogs.Where(c => c.UserId == id).ToListAsync();
					foreach (var log in changelogs)
					{
						log.UserId = null;
					}

					// Delete related notifications
					var notifications = await _context.Notifications.Where(n => n.UserId == id).ToListAsync();
					_context.Notifications.RemoveRange(notifications);

					// Save changes to update references
					await _context.SaveChangesAsync();

					// Now remove the user
					_context.Users.Remove(user);
					await _context.SaveChangesAsync();

					await transaction.CommitAsync();
					return user;
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					throw new Exception($"Error deleting user: {ex.Message}", ex);
				}
			});
		}

		public async Task<bool> UserExistsAsync(string id)
		{
			return await _context.Users.AnyAsync(u => u.Id == id);
		}

		public async Task<bool> EmailExistsAsync(string email)
		{
			return await _context.Users.AnyAsync(u => u.Email == email);
		}
		public async Task<bool> UpdateUserDepartmentAsync(string userId, int departmentId)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user == null) return false;

			user.DepartmentId = departmentId;
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(int departmentId)
		{
			return await _context.Users
	   .Where(u => u.DepartmentId == departmentId)
	   .Include(u => u.Department)
	   .ToListAsync();
		}
		public async Task<User> GetFirstUserInRoleAsync(string role)
		{
			// Get the role ID
			var roleId = await _context.Roles
				.Where(r => r.Name == role)
				.Select(r => r.Id)
				.FirstOrDefaultAsync();

			if (string.IsNullOrEmpty(roleId))
				return null;

			// Get the first user ID in that role
			var userId = await _context.UserRoles
				.Where(ur => ur.RoleId == roleId)
				.Select(ur => ur.UserId)
				.FirstOrDefaultAsync();

			if (string.IsNullOrEmpty(userId))
				return null;

			// Return the user
			return await GetUserByIdAsync(userId);
		}

		public async Task<bool> IsUserInRoleAsync(string userId, string role)
		{
			// Get the role ID
			var roleId = await _context.Roles
				.Where(r => r.Name == role)
				.Select(r => r.Id)
				.FirstOrDefaultAsync();

			if (string.IsNullOrEmpty(roleId))
				return false;

			// Check if the user has this role
			return await _context.UserRoles
				.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
		}
		//public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(int departmentId)
		//{
		//	return 
		//}
	}

}
