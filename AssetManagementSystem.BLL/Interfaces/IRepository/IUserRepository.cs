using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IRepository
{
	public interface IUserRepository
	{
		Task<User> AddUserAsync(User user);
		Task<User> UpdateUserAsync(User user);
		Task<User> DeleteUserAsync(string id);
		Task<IEnumerable<User>> GetAllUsersAsync();
		Task<User> GetUserByIdAsync(string id);
		Task<User> GetUserByEmailAsync(string email);
		Task<User> GetUserByUserNameAsync(string userName);
		Task<User> GetUserByFullNameAsync(string FullName);
		Task<User> GetUserByPhoneNumberAsync(string phoneNumber);
		Task<User> GetUserByDepartmentAsync(int departmentId);
		Task<bool> UpdateUserDepartmentAsync(string userId, int departmentId);
		Task<bool> UserExistsAsync(string id);
		Task<bool> EmailExistsAsync(string email);
		Task<IEnumerable<User>> GetUsersByDepartmentAsync(int departmentId);


	}
}
