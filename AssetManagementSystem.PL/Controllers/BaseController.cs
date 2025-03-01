
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AssetManagementSystem.PL.Controllers
{
	public abstract class BaseController : Controller
	{
		protected readonly INotificationService _notificationService;
		protected readonly UserManager<User> _userManager;

		public BaseController(INotificationService notificationService, UserManager<User> userManager)
		{
			_notificationService = notificationService;
			_userManager = userManager;
		}

		protected async Task CreateNotificationForUser(
			string userId,
			string title,
			string message,
			string notificationType,
			string relatedEntityId = null,
			string actionUrl = null)
		{
			await _notificationService.CreateNotificationAsync(
				userId, title, message, notificationType, relatedEntityId, actionUrl);
		}

		protected async Task CreateNotificationForAdmin(
			string title,
			string message,
			string notificationType,
			string relatedEntityId = null,
			string actionUrl = null)
		{
			var admins = await _userManager.GetUsersInRoleAsync("Admin");
			foreach (var admin in admins)
			{
				await _notificationService.CreateNotificationAsync(
					admin.Id, title, message, notificationType, relatedEntityId, actionUrl);
			}
		}

		protected async Task CreateNotificationForRole(
			string role,
			string title,
			string message,
			string notificationType,
			string relatedEntityId = null,
			string actionUrl = null)
		{
			var users = await _userManager.GetUsersInRoleAsync(role);
			foreach (var user in users)
			{
				await _notificationService.CreateNotificationAsync(
					user.Id, title, message, notificationType, relatedEntityId, actionUrl);
			}
		}
	}
}