﻿using System.ComponentModel.DataAnnotations;

namespace AssetManagementSystem.PL.Models
{
	public class ResetPasswordViewModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		public string Token { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[Compare("NewPassword")]
		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; }
	}
}
