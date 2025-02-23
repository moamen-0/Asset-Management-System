namespace AssetManagementSystem.PL.Models
{
	public class RoomViewModel
	{
		public string RoomTag { get; set; }
		public string Name { get; set; }
		public int FloorId { get; set; }
		public FloorViewModel Floor { get; set; }
		public int DepartmentId { get; set; }
		public DepartmentViewModel Department { get; set; }
	}
}