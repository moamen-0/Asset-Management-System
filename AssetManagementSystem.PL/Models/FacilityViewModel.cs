namespace AssetManagementSystem.PL.Models
{
	public class FacilityViewModel
	{

		public int Id { get; set; }
		public string NameAr { get; set; }
		public string Name { get; set; }
		public List<BuildingViewModel> Buildings { get; set; } = new();

	}
}