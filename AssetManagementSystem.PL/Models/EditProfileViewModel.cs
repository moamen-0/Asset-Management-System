using Microsoft.AspNetCore.Mvc.Rendering;
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

		public string NationalId { get; set; }

		public string FileNumber { get; set; }

		public int? DepartmentId { get; set; } // Nullable

		public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
	}

}
