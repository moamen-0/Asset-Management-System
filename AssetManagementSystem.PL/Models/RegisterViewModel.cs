using System.ComponentModel.DataAnnotations;

namespace AssetManagementSystem.PL.Models
{
	public class RegisterViewModel
	{
		[Required]
		public required string FullName { get; set; }

		[Required]
		[EmailAddress]
		public required string Email { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		public required string Password { get; set; }

		[DataType(DataType.Password)]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public required string ConfirmPassword { get; set; }

		[Required]
		public required string NationalId { get; set; }

		[Required]
		public required string RecipientFileNumber { get; set; }
	}
}
