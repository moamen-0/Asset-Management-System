using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagementSystem.PL.Controllers
{
	[Authorize]
	public class NotificationController : Controller
	{
		private readonly INotificationService _notificationService;
		private readonly UserManager<User> _userManager;

		public NotificationController(
			INotificationService notificationService,
			UserManager<User> userManager)
		{
			_notificationService = notificationService;
			_userManager = userManager;
		}

		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return RedirectToAction("Login", "Auth");
			}

			var notifications = await _notificationService.GetByUserIdAsync(user.Id);
			var viewModel = notifications.Select(n => new NotificationViewModel
			{
				Id = n.Id,
				Title = n.Title,
				Message = n.Message,
				TimeAgo = GetTimeAgo(n.CreatedAt),
				IsRead = n.IsRead,
				NotificationType = n.NotificationType,
				ActionUrl = n.ActionUrl
			}).ToList();

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> MarkAsRead(int id, string returnUrl = null)
		{
			await _notificationService.MarkAsReadAsync(id);

			if (string.IsNullOrEmpty(returnUrl))
			{
				return RedirectToAction(nameof(Index));
			}

			return Redirect(returnUrl);
		}

		[HttpPost]
		public async Task<IActionResult> MarkAllAsRead()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return RedirectToAction("Login", "Auth");
			}

			await _notificationService.MarkAllAsReadAsync(user.Id);
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			await _notificationService.DeleteAsync(id);
			return RedirectToAction(nameof(Index));
		}

		// Get unread notifications for the navbar dropdown
		[HttpGet]
		public async Task<IActionResult> GetUnreadNotifications()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Json(new { success = false, error = "User not authenticated" });
			}

			var notifications = await _notificationService.GetUnreadByUserIdAsync(user.Id);
			var viewModel = notifications.Select(n => new NotificationViewModel
			{
				Id = n.Id,
				Title = n.Title,
				Message = n.Message,
				TimeAgo = GetTimeAgo(n.CreatedAt),
				IsRead = n.IsRead,
				NotificationType = n.NotificationType,
				ActionUrl = n.ActionUrl
			}).ToList();

			var count = await _notificationService.GetUnreadCountAsync(user.Id);

			return Json(new { success = true, notifications = viewModel, count = count });
		}

		private string GetTimeAgo(DateTime dateTime)
		{
			var span = DateTime.UtcNow - dateTime;

			if (span.Days > 365)
			{
				return $"{span.Days / 365} year{(span.Days / 365 == 1 ? "" : "s")} ago";
			}
			if (span.Days > 30)
			{
				return $"{span.Days / 30} month{(span.Days / 30 == 1 ? "" : "s")} ago";
			}
			if (span.Days > 0)
			{
				return $"{span.Days} day{(span.Days == 1 ? "" : "s")} ago";
			}
			if (span.Hours > 0)
			{
				return $"{span.Hours} hour{(span.Hours == 1 ? "" : "s")} ago";
			}
			if (span.Minutes > 0)
			{
				return $"{span.Minutes} minute{(span.Minutes == 1 ? "" : "s")} ago";
			}
			return "Just now";
		}
	}
}