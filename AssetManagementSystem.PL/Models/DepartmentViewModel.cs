namespace AssetManagementSystem.PL.Models
{
	public class DepartmentViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public List<RoomViewModel> Rooms { get; set; } = new();
	}
}