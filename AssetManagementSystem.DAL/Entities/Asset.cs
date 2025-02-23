using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class Asset
	{
		[Key]
		[Column(TypeName = "nvarchar(50)")]
		public required string  AssetTag { get; set; }

		public string? Cluster { get; set; }
		public int FacilityId { get; set; }
		public Facility? Facility { get; set; }

		public int BuildingId { get; set; }
		public Building? Building { get; set; }

		public int FloorId { get; set; }
		public Floor? Floor { get; set; }

		[ForeignKey("Room")]
		public string? RoomTag { get; set; }
		public Room? Room { get; set; }

		public int DepartmentId { get; set; }
		public Department? Department { get; set; }

		public string? AssetDescription { get; set; }
		public string? Brand { get; set; }
		public string? Model { get; set; }

		public string? UserId { get; set; }
		public virtual User? User { get; set; }

		public string? AssetType { get; set; } // تالف أو أشياء أخرى
		public string? Status { get; set; }
		public bool IsDisposed { get; set; } = false;
		public string? SerialNumber { get; set; }
		public DateTime InsertDate { get; set; } = DateTime.UtcNow;

		public string? InsertUser { get; set; } =string.Empty;
		public string? Details { get; set; }

		public string? AssetClass1 { get; set; }
		public string? AssetClass2 { get; set; }
		public string? AssetClass3 { get; set; }

	}
}
