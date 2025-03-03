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
			var user = await _context.Users.FindAsync(id);
			if (user == null) return null;

			_context.Users.Remove(user);
			await _context.SaveChangesAsync();
			return user;
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

		//public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(int departmentId)
		//{
		//	return 
		//}
	}

}
