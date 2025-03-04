using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class User : IdentityUser
	{
		public string FullName { get; set; }
		public string? NationalId { get; set; }
		public string? RecipientFileNumber { get; set; }
		public int? DepartmentId { get; set; }
		public Department? Department { get; set; }
		public ICollection<Asset>? Assets { get; set; } = null;
		// Assets supervised by this user
		[InverseProperty("Supervisor")]
		public ICollection<Asset>? SupervisedAssets { get; set; } = null;
		public string? ResetPasswordToken { get; set; }
		public DateTime? ResetPasswordTokenExpiry { get; set; }
	}
}
