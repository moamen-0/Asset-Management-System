using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class Room
	{
		[Key]
		public string RoomTag { get; set; }

		[Required,Column(TypeName = "nvarchar(255)")]
		public required string Name { get; set; }

		

		// العلاقة مع Floor
		public int FloorId { get; set; }
		public Floor Floor { get; set; }


		// العلاقة مع Department
		public int DepartmentId { get; set; }
		public Department Department { get; set; }
	}
}
