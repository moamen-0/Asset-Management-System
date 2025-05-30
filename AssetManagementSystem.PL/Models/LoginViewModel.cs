﻿using System.ComponentModel.DataAnnotations;

namespace AssetManagementSystem.PL.Models
{
	public class LoginViewModel
	{
		[Required, EmailAddress]
		public required string Email { get; set; }

		[Required, DataType(DataType.Password)]
		public required string Password { get; set; }

		[Display(Name = "تذكرني")]
		public bool RememberMe { get; set; }
	}
}
