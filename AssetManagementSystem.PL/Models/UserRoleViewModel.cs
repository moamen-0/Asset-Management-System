namespace AssetManagementSystem.PL.Models
{
	public class UserRoleViewModel
	{
		public string UserId { get; set; }
		public string Email { get; set; }
		public string FullName { get; set; }
		public List<string> CurrentRoles { get; set; } = new List<string>();
		public List<string> AvailableRoles { get; set; } = new List<string>();
	}
}
