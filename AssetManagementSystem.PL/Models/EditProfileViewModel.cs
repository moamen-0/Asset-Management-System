using System.ComponentModel.DataAnnotations;

namespace AssetManagementSystem.PL.Models
{
	public class EditProfileViewModel
	{
		[Required]
		public string FullName { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		public int? DepartmentId { get; set; }
	}
}
