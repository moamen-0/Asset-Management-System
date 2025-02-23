using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.PL.Models;
using AssetManagementSystem.BLL.Interfaces.IService;

namespace AssetManagementSystem.Controllers
{
	public class AuthController : Controller
	{
		private readonly SignInManager<User> _signInManager;
		private readonly UserManager<User> _userManager;
		private readonly IUserService _userService;

		public AuthController(SignInManager<User> signInManager, UserManager<User> userManager, IUserService userService)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_userService = userService;
		}

		public IActionResult Login() => View();

		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.FindByEmailAsync(model.Email);

			if (user != null)
			{
				
				var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: false);
				if (result.Succeeded)
				{
					
					return RedirectToAction("Index", "Home");
				}
			}

			ModelState.AddModelError("", "Invalid Login Attempt");
			return View(model);
		}


		public IActionResult Register() => View();

		[HttpPost]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (!ModelState.IsValid)
			{
				// Log or inspect ModelState errors
				foreach (var state in ModelState)
				{
					foreach (var error in state.Value.Errors)
					{
						// Log or inspect the error
						Console.WriteLine($"{state.Key}: {error.ErrorMessage}");
					}
				}
				return View(model);
			}

			var newUser = new User
			{
				FullName = model.FullName,
				Email = model.Email,
				UserName = model.Email,
				NationalId = model.NationalId,
				RecipientFileNumber = model.RecipientFileNumber
			};

			var result = await _userManager.CreateAsync(newUser, model.Password);

			if (result.Succeeded)
			{
				return RedirectToAction("Login");
			}

			foreach (var error in result.Errors)
			{
				// Log or inspect the error
				Console.WriteLine($"{error.Code}: {error.Description}");
				ModelState.AddModelError("", error.Description);
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Login");
		}


		[HttpGet]
		public IActionResult ForgotPassword()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> ForgotPassword(string email)
		{
			if (string.IsNullOrEmpty(email))
				return BadRequest("Email is required");

			await _userService.InitiatePasswordResetAsync(email);

			// Always return success to prevent email enumeration
			return RedirectToAction("ForgotPasswordConfirmation");
		}

		[HttpGet]
		public IActionResult ResetPassword(string email, string token)
		{
			var model = new ResetPasswordViewModel { Email = email, Token = token };
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var result = await _userService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);
			if (result)
				return RedirectToAction("ResetPasswordConfirmation");

			ModelState.AddModelError("", "Invalid reset attempt");
			return View(model);
		}
	}
}
