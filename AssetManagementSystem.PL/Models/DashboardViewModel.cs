using AssetManagementSystem.DAL.Entities;

namespace AssetManagementSystem.PL.Models
{
	public class DashboardViewModel
	{
		public IEnumerable<Asset?>? Assets { get; set; }
		public IEnumerable<AssetTransfer?>? AssetTransfers { get; set; }
		public IEnumerable<Building?>? Buildings { get; set; }
		public IEnumerable<Facility?>? Facilities { get; set; }
		public IEnumerable<Department?>? Departments { get; set; }
		public IEnumerable<Disposal?>? Disposals { get; set; }
		public IEnumerable<ChangeLog?>? RecentChangelogs { get; set; }
		public IEnumerable<Room?>? Rooms { get; set; }
		public IEnumerable<User?>? Users { get; set; } // All users except the logged-in user
		public int AssetCount { get; set; }
	}
}
