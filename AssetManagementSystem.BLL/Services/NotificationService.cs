// AssetManagementSystem.BLL/Services/NotificationService.cs
using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Services
{
	public class NotificationService : INotificationService
	{
		private readonly INotificationRepository _notificationRepository;

		public NotificationService(INotificationRepository notificationRepository)
		{
			_notificationRepository = notificationRepository;
		}

		public async Task<IEnumerable<Notification>> GetAllAsync()
		{
			return await _notificationRepository.GetAllAsync();
		}

		public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId)
		{
			return await _notificationRepository.GetByUserIdAsync(userId);
		}

		public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId)
		{
			return await _notificationRepository.GetUnreadByUserIdAsync(userId);
		}

		public async Task<Notification> GetByIdAsync(int id)
		{
			return await _notificationRepository.GetByIdAsync(id);
		}

		public async Task CreateNotificationAsync(string userId, string title, string message, string notificationType, string relatedEntityId = null, string actionUrl = null)
		{
			var notification = new Notification
			{
				UserId = userId,
				Title = title,
				Message = message,
				NotificationType = notificationType,
				RelatedEntityId = relatedEntityId,
				ActionUrl = actionUrl,
				CreatedAt = DateTime.UtcNow,
				IsRead = false
			};

			await _notificationRepository.AddAsync(notification);
		}

		public async Task UpdateAsync(Notification notification)
		{
			await _notificationRepository.UpdateAsync(notification);
		}

		public async Task DeleteAsync(int id)
		{
			await _notificationRepository.DeleteAsync(id);
		}

		public async Task MarkAsReadAsync(int id)
		{
			await _notificationRepository.MarkAsReadAsync(id);
		}

		public async Task MarkAllAsReadAsync(string userId)
		{
			await _notificationRepository.MarkAllAsReadAsync(userId);
		}

		public async Task<int> GetUnreadCountAsync(string userId)
		{
			return await _notificationRepository.GetUnreadCountAsync(userId);
		}
	}
}