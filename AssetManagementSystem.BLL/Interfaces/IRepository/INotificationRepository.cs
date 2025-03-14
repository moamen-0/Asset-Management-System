﻿using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IRepository
{
	public interface INotificationRepository
	{
		Task<IEnumerable<Notification>> GetAllAsync();
		Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
		Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId);
		Task<Notification> GetByIdAsync(int id);
		Task AddAsync(Notification notification);
		Task UpdateAsync(Notification notification);
		Task DeleteAsync(int id);
		Task MarkAsReadAsync(int id);
		Task MarkAllAsReadAsync(string userId);
		Task<int> GetUnreadCountAsync(string userId);
	}
}
