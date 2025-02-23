using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class Department
	{
		public int Id { get; set; }

		[Required,Column(TypeName = "nvarchar(255)")]
		public required string Name { get; set; }

	

		// علاقة One-to-Many مع Room
		public ICollection<Room> Rooms { get; set; } = new List<Room>();
	}
}
