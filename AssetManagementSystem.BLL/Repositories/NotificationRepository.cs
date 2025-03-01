using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.DAL.Data;
using AssetManagementSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Repositories
{
	public class NotificationRepository : INotificationRepository
	{
		private readonly AssetManagementDbContext _context;

		public NotificationRepository(AssetManagementDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<Notification>> GetAllAsync()
		{
			return await _context.Notifications
				.Include(n => n.User)
				.OrderByDescending(n => n.CreatedAt)
				.ToListAsync();
		}

		public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId)
		{
			return await _context.Notifications
				.Where(n => n.UserId == userId)
				.OrderByDescending(n => n.CreatedAt)
				.ToListAsync();
		}

		public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId)
		{
			return await _context.Notifications
				.Where(n => n.UserId == userId && !n.IsRead)
				.OrderByDescending(n => n.CreatedAt)
				.ToListAsync();
		}

		public async Task<Notification> GetByIdAsync(int id)
		{
			return await _context.Notifications
				.Include(n => n.User)
				.FirstOrDefaultAsync(n => n.Id == id);
		}

		public async Task AddAsync(Notification notification)
		{
			await _context.Notifications.AddAsync(notification);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync(Notification notification)
		{
			_context.Notifications.Update(notification);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var notification = await _context.Notifications.FindAsync(id);
			if (notification != null)
			{
				_context.Notifications.Remove(notification);
				await _context.SaveChangesAsync();
			}
		}

		public async Task MarkAsReadAsync(int id)
		{
			var notification = await _context.Notifications.FindAsync(id);
			if (notification != null)
			{
				notification.IsRead = true;
				await _context.SaveChangesAsync();
			}
		}

		public async Task MarkAllAsReadAsync(string userId)
		{
			var notifications = await _context.Notifications
				.Where(n => n.UserId == userId && !n.IsRead)
				.ToListAsync();

			foreach (var notification in notifications)
			{
				notification.IsRead = true;
			}

			await _context.SaveChangesAsync();
		}

		public async Task<int> GetUnreadCountAsync(string userId)
		{
			return await _context.Notifications
				.CountAsync(n => n.UserId == userId && !n.IsRead);
		}
	}
}