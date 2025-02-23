using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
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

		public UserService(
			UserManager<User> userManager,
			IEmailSenderService emailService,
			IUserRepository userRepository)
		{
			_userManager = userManager;
			_emailService = emailService;
			_userRepository = userRepository;
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

			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			user.ResetPasswordToken = token;
			user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(24);

			// Send email with reset link
			var resetLink = $"https://yourwebsite.com/Auth/ResetPassword?email={email}&token={token}";
			await _emailService.SendPasswordResetEmailAsync(email, resetLink);

			return true;
		}

		public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null) return false;

			var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
			return result.Succeeded;
		}

		public async Task<bool> UpdateUserDepartmentAsync(string userId, int departmentId)
		{
			return await _userRepository.UpdateUserDepartmentAsync(userId, departmentId);
		}
	}
}
