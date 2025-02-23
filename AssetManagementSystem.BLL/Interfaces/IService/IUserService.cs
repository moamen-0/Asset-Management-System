using AssetManagementSystem.DAL.Data;
using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IService
{
	public interface IUserService
	{
		Task<User?> GetUserByIdAsync(string id);
		Task<bool> UpdateUserProfileAsync(User user);
		Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
		Task<bool> InitiatePasswordResetAsync(string email);
		Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
		Task<bool> UpdateUserDepartmentAsync(string userId, int departmentId);
	}
}
