using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.DAL.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementSystem.PL.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = Roles.Admin)]
	public class RoleController : Controller
	{
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly UserManager<User> _userManager;

		public RoleController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
		{
			_roleManager = roleManager;
			_userManager = userManager;
		}

		public async Task<IActionResult> Index()
		{
			var roles = _roleManager.Roles.ToList();
			return View(roles);
		}

		public async Task<IActionResult> AssignRoleToUser(string userId, string roleName)
		{
			var user = await _userManager.FindByIdAsync(userId);
			await _userManager.AddToRoleAsync(user, roleName);
			return RedirectToAction("Index", "User");
		}
	}
}
