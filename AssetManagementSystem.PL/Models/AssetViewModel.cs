using System.ComponentModel.DataAnnotations;

namespace AssetManagementSystem.PL.Models
{
	public class AssetViewModel
	{
		[Required]
		public string AssetTag { get; set; }

		public string? Cluster { get; set; }

		[Required]
		public int FacilityId { get; set; }

		[Required]
		public int BuildingId { get; set; }

		[Required]
		public int FloorId { get; set; }

		[Required]
		public string RoomTag { get; set; }

		[Required]
		public int DepartmentId { get; set; }

		public string? AssetDescription { get; set; }
		public string? Brand { get; set; }
		public string? Model { get; set; }

		public int? UserId { get; set; } // Now a simple input field

		public string? AssetType { get; set; }
		public string? Status { get; set; }
		public string? SerialNumber { get; set; }

		public bool IsDisposed { get; set; }
	}
}
