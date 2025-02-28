using AssetManagementSystem.BLL.Interfaces;
using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NETCore.MailKit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Services
{
	public class UserService : IUserService
	{
		private readonly UserManager<User> _userManager;
		private readonly IEmailSenderService _emailService;
		private readonly IUserRepository _userRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IConfiguration _configuration;

		public UserService(
			UserManager<User> userManager,
			IEmailSenderService emailService,
			IUserRepository userRepository, IUnitOfWork unitOfWork,
			IConfiguration configuration
			)
		{
			_userManager = userManager;
			_emailService = emailService;
			_userRepository = userRepository;
			_unitOfWork = unitOfWork;
			_configuration = configuration;
		}

		public async Task<User?> GetUserByIdAsync(string id)
		{
			return await _userRepository.GetUserByIdAsync(id);
		}

		public async Task<bool> UpdateUserProfileAsync(User user)
		{
			var existingUser = await _userManager.FindByIdAsync(user.Id);
			if (existingUser == null) return false;

			existingUser.FullName = user.FullName;
			existingUser.Email = user.Email;
			existingUser.UserName = user.Email;

			var result = await _userManager.UpdateAsync(existingUser);
			return result.Succeeded;
		}

		public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null) return false;

			var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
			return result.Succeeded;
		}

		public async Task<bool> InitiatePasswordResetAsync(string email)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null) return false;

			// Generate a token
			var token = await _userManager.GeneratePasswordResetTokenAsync(user);

			// Store the token and expiry in user record
			user.ResetPasswordToken = token;
			user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(24);
			await _userManager.UpdateAsync(user);

			// Create reset link (use absolute URL with the token)
			var resetLink = $"{_configuration["ApplicationUrl"]}/Auth/ResetPassword?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

			// Send email
			await _emailService.SendPasswordResetEmailAsync(email, resetLink);

			return true;
		}

		public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null) return false;

			// Verify token hasn't expired
			if (user.ResetPasswordTokenExpiry < DateTime.UtcNow)
				return false;

			// Reset the password
			var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

			if (result.Succeeded)
			{
				// Clear the token after successful reset
				user.ResetPasswordToken = null;
				user.ResetPasswordTokenExpiry = null;
				await _userManager.UpdateAsync(user);
			}

			return result.Succeeded;
		}

		public async Task<bool> UpdateUserDepartmentAsync(string userId, int departmentId)
    {
			// Verify department exists and belongs to a facility
			var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
        if (department == null || department.FacilityId == 0)
            return false;

        return await _userRepository.UpdateUserDepartmentAsync(userId, departmentId);
    }

    public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(int departmentId)
    {
        return await _userRepository.GetUsersByDepartmentAsync(departmentId);
    }

	
	}
}
