// CreateAssetViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AssetManagementSystem.PL.Models
{
	public class CreateAssetViewModel
	{
		[Required]
		public string AssetTag { get; set; }

		public string Cluster { get; set; }

		[Required]
		public int FacilityId { get; set; }

		public int BuildingId { get; set; }

		public int FloorId { get; set; }

		public string RoomTag { get; set; }

		[Required]
		public int DepartmentId { get; set; }

		[Required]
		public string AssetDescription { get; set; }

		public string Brand { get; set; }

		public string Model { get; set; }

		public string UserId { get; set; }

		[Required]
		public string AssetType { get; set; }

		[Required]
		public string Status { get; set; }

		public bool IsDisposed { get; set; }

		public string SerialNumber { get; set; }

		public string Details { get; set; }

		public string AssetClass1 { get; set; }

		public string AssetClass2 { get; set; }

		public string AssetClass3 { get; set; }

		// Dropdown options
		public IEnumerable<SelectListItem> Facilities { get; set; }
		public IEnumerable<SelectListItem> Departments { get; set; }
		public IEnumerable<SelectListItem> AssetTypes { get; set; }
		public IEnumerable<SelectListItem> StatusOptions { get; set; }
	}
}