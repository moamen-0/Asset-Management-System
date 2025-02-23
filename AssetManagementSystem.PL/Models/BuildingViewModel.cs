namespace AssetManagementSystem.PL.Models
{
	public class BuildingViewModel
	{
		public int Id { get; set; }
		public string NameAr { get; set; }
		public string Name { get; set; }
		public int FacilityId { get; set; }
		public FacilityViewModel Facility { get; set; }
		public List<FloorViewModel> Floors { get; set; } = new();
	}
}