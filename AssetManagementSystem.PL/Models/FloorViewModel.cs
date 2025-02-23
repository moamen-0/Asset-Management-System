namespace AssetManagementSystem.PL.Models
{
	public class FloorViewModel
	{
		public int Id { get; set; }
		public string? NameAr { get; set; }
		public string? Name { get; set; }
		public int BuildingId { get; set; }
		public BuildingViewModel Building { get; set; }
		public List<RoomViewModel> Rooms { get; set; } = new();
	}
}