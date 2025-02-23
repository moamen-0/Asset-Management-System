using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class Floor
	{
		public int Id { get; set; }


		[Column(TypeName = "nvarchar(255)")]
		public string? Name { get; set; }

		// العلاقة مع Building
		public int BuildingId { get; set; }
		public Building Building { get; set; }

		// علاقة One-to-Many مع Room
		public ICollection<Room> Rooms { get; set; } = new List<Room>();
	}
}
