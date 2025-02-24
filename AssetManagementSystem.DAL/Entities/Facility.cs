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

		// One-to-Many relationship with Building
		public ICollection<Building> Buildings { get; set; } = new List<Building>();

		// One-to-Many relationship with Department
		public ICollection<Department> Departments { get; set; } = new List<Department>();

	}
}
