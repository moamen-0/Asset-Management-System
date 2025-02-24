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

	
		// Relationship with Facility
        public int FacilityId { get; set; }
        public Facility Facility { get; set; }

        // One-to-Many relationship with Room
        public ICollection<Room> Rooms { get; set; } = new List<Room>();

        // One-to-Many relationship with Users
        public ICollection<User> Users { get; set; } = new List<User>();
	}
}
