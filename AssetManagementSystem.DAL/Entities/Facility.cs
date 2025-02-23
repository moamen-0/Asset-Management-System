using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class Facility
	{
		public int Id { get; set; }

		

		[Column(TypeName = "nvarchar(255)")]
		public string Name { get; set; }

		// علاقة One-to-Many مع Building
		public ICollection<Building> Buildings { get; set; } = new List<Building>();
	}
}
