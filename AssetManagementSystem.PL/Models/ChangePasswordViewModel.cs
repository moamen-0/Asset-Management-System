using System.ComponentModel.DataAnnotations;

namespace AssetManagementSystem.PL.Models
{
	public class ChangePasswordViewModel
	{
		[Required]
		[DataType(DataType.Password)]
		public string CurrentPassword { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[Compare("NewPassword")]
		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; }	
	}
}
