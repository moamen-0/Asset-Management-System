using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces;
using System.Security.Claims;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.PL.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

public class UserController : Controller
{
	private readonly UserManager<User> _userManager;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IUserService _userService;
	public UserController(UserManager<User> userManager,IUnitOfWork unitOfWork, IUserService userService)
	{
		_userManager = userManager;
		_unitOfWork = unitOfWork;
		_userService = userService;
	}

	[HttpGet]
	[Authorize] // This ensures that only authenticated users can access the Profile action
	public async Task<IActionResult> Profile()
	{
		var user = await _userManager.Users
			.Include(u => u.Department)
			.Include(u => u.Assets)
			.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

		if (user == null)
		{
			return NotFound();
		}

		return View(user);
	}

	// List all users
	public async Task<IActionResult> Index()
	{
		var users = await _unitOfWork.user.GetAllUsersAsync();
		return View(users);
	}

	public async Task<IActionResult> Details(string id)
	{
		var user = await _unitOfWork.user.GetUserByIdAsync(id);
		if (user == null)
		{
			return NotFound();
		}
		return View(user);
	}

	

	// Create new user (GET)
	public IActionResult Create()
	{
		return View();
	}

	// Create new user (POST)
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(User user)
	{
		if (ModelState.IsValid)
		{
			await _unitOfWork.user.AddUserAsync(user);
			return RedirectToAction(nameof(Index));
		}
		return View(user);
	}

	// Edit user (GET)
	public async Task<IActionResult> Edit(string id)
	{
		var user = await _unitOfWork.user.GetUserByIdAsync(id);
		if (user == null) return NotFound();
		return View(user);
	}

	// Edit user (POST)
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(User user)
	{
		if (ModelState.IsValid)
		{
			await _unitOfWork.user.UpdateUserAsync(user);
			return RedirectToAction(nameof(Index));
		}
		return View(user);
	}

	// Delete user (GET)
	public async Task<IActionResult> Delete(string id)
	{
		var user = await _unitOfWork.user.GetUserByIdAsync(id);
		if (user == null) return NotFound();
		return View(user);
	}

	// Delete user (POST)
	[HttpPost, ActionName("Delete")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteConfirmed(string id)
	{
		await _unitOfWork.user.DeleteUserAsync(id);
		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	public async Task<IActionResult> EditProfile()
	{
		try
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await _userService.GetUserByIdAsync(userId);
			if (user == null)
				return NotFound();

			// Get departments and check if the list is not null
			var departments = await _unitOfWork.DepartmentRepository.GetAllAsync() ?? new List<Department>();

			// Add a default "No Department" option
			var departmentList = new List<SelectListItem>
		{
			new SelectListItem { Value = "", Text = "-- No Department --" }
		};

			// Add the rest of the departments
			departmentList.AddRange(departments.Select(d => new SelectListItem
			{
				Value = d.Id.ToString(),
				Text = d.Name
			}));

			ViewBag.Departments = departmentList;

			var model = new EditProfileViewModel
			{
				FullName = user.FullName,
				Email = user.Email,
				DepartmentId = user.DepartmentId
			};

			return View(model);
		}
		catch (Exception ex)
		{
			return RedirectToAction("Error", "Home");
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> EditProfile(EditProfileViewModel model)
	{
		if (!ModelState.IsValid)
		{
			await PopulateDepartmentsDropdown(model.DepartmentId);
			return View(model);
		}

		try
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await _userService.GetUserByIdAsync(userId);
			if (user == null)
				return NotFound();

			user.FullName = model.FullName;
			user.Email = model.Email;
			user.DepartmentId = model.DepartmentId; // This can be null

			var result = await _userService.UpdateUserProfileAsync(user);
			if (result)
			{
				TempData["Success"] = "Profile updated successfully";
				return RedirectToAction("Profile");
			}

			ModelState.AddModelError("", "Failed to update profile");
			await PopulateDepartmentsDropdown(model.DepartmentId);
			return View(model);
		}
		catch (Exception ex)
		{
			ModelState.AddModelError("", "An error occurred while updating the profile");
			await PopulateDepartmentsDropdown(model.DepartmentId);
			return View(model);
		}
	}

	private async Task PopulateDepartmentsDropdown(int? selectedDepartmentId)
	{
		var departments = await _unitOfWork.DepartmentRepository.GetAllAsync() ?? new List<Department>();

		var departmentList = new List<SelectListItem>
	{
		new SelectListItem { Value = "", Text = "-- No Department --" }
	};

		departmentList.AddRange(departments.Select(d => new SelectListItem
		{
			Value = d.Id.ToString(),
			Text = d.Name,
			Selected = d.Id == selectedDepartmentId
		}));

		ViewBag.Departments = departmentList;
	}

	[HttpGet]
	public IActionResult ChangePassword()
	{
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
	{
		if (!ModelState.IsValid)
			return View(model);

		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		var result = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

		if (result)
			return RedirectToAction("Profile");

		ModelState.AddModelError("", "Password change failed");
		return View(model);
	}
}