using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class Building
	{
		public int Id { get; set; }

		

		[Column(TypeName = "nvarchar(255)")]
		public string Name { get; set; }

		// العلاقة مع Facility
		public int FacilityId { get; set; }
		public Facility Facility { get; set; }

		// علاقة One-to-Many مع Floor
		public ICollection<Floor> Floors { get; set; } = new List<Floor>();
	}
}
