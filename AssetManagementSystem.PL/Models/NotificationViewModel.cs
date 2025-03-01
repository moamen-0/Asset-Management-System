namespace AssetManagementSystem.PL.Models
{
	public class NotificationViewModel
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Message { get; set; }
		public string TimeAgo { get; set; }
		public bool IsRead { get; set; }
		public string NotificationType { get; set; }
		public string ActionUrl { get; set; }
	}
}