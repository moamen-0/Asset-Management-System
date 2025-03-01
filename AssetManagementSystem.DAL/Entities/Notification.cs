using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class Notification
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Message { get; set; }
		public string UserId { get; set; }
		public User User { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public bool IsRead { get; set; } = false;
		public string NotificationType { get; set; } // e.g., "Asset", "Transfer", "Disposal"
		public string? RelatedEntityId { get; set; } // e.g., AssetTag, DisposalId, etc.
		public string? ActionUrl { get; set; } // URL to redirect when clicking the notification

	}
}
