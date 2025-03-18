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
using AssetManagementSystem.DAL.Utilities;
using AssetManagementSystem.BLL.Repositories;
using OfficeOpenXml;
using OfficeOpenXml.Style;

[Authorize]
public class UserController : Controller
{
	private readonly UserManager<User> _userManager;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IUserService _userService;
	private readonly RoleManager<IdentityRole> _roleManager;
	private readonly ILogger<UserController> _logger;
	public UserController(UserManager<User> userManager,IUnitOfWork unitOfWork, IUserService userService, RoleManager<IdentityRole> roleManager,ILogger<UserController>logger)
	{
		_userManager = userManager;
		_unitOfWork = unitOfWork;
		_userService = userService;
		_roleManager = roleManager;
		_logger = logger;
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
	[Authorize(Roles = Roles.Admin)]
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

		// Get user roles
		var roles = await _userManager.GetRolesAsync(user);
		ViewBag.UserRoles = roles;

		return View(user);
	}



	// Create new user (GET)
	[Authorize(Roles = Roles.Admin)]
	public async Task<IActionResult> Create()
	{
		// Populate departments dropdown
		await PopulateDepartmentsDropdown(null);

		// Populate roles dropdown using the constants from Roles class
		var roles = new List<string>
	{
		Roles.Admin,
		Roles.Manager,
		Roles.Supervisor,
		Roles.User,
		Roles.DataEntry
	};
		ViewBag.AvailableRoles = new SelectList(roles);

		return View();
	}

	// Create new user (POST)
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(User user, string Password, string Role)
	{
		if (ModelState.IsValid)
		{
			// Create the user with the provided password
			var result = await _userManager.CreateAsync(user, Password);

			if (result.Succeeded)
			{
				// Assign the selected role
				if (!string.IsNullOrEmpty(Role))
				{
					await _userManager.AddToRoleAsync(user, Role);
				}

				return RedirectToAction(nameof(DepartmentUsers));
			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError("", error.Description);
			}
		}

		// If we got this far, something failed, redisplay form
		await PopulateDepartmentsDropdown(user.DepartmentId);

		// Repopulate roles dropdown
		var roles = new List<string>
	{
		Roles.Admin,
		Roles.Manager,
		Roles.Supervisor,
		Roles.User,
		Roles.DataEntry
	};
		ViewBag.AvailableRoles = new SelectList(roles);

		return View(user);
	}
	// Edit user (GET)
	[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
	public async Task<IActionResult> Edit(string id)
	{
		var user = await _unitOfWork.user.GetUserByIdAsync(id);
		if (user == null) return NotFound();

		// Populate departments dropdown
		await PopulateDepartmentsDropdown(user.DepartmentId);

		return View(user);
	}

	// Edit user (POST)
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(User user)
	{
		if (ModelState.IsValid)
		{
			var existingUser = await _unitOfWork.user.GetUserByIdAsync(user.Id);
			if (existingUser == null)
			{
				return NotFound();
			}

			// Update user properties
			existingUser.FullName = user.FullName;
			existingUser.Email = user.Email;
			existingUser.NationalId = user.NationalId;
			existingUser.RecipientFileNumber = user.RecipientFileNumber;
			existingUser.DepartmentId = user.DepartmentId;

			// Save changes
			await _unitOfWork.user.UpdateUserAsync(existingUser);
			return RedirectToAction(nameof(DepartmentUsers));
		}

		// If we got this far, something failed, redisplay form
		await PopulateDepartmentsDropdown(user.DepartmentId);

		return View(user);
	}

	// Delete user (GET)
	[Authorize(Roles = Roles.Admin)]
	public async Task<IActionResult> Delete(string id)
	{
		if (id == null)
		{
			return NotFound();
		}

		var user = await _unitOfWork.user.GetUserByIdAsync(id);
		if (user == null)
		{
			return NotFound();
		}

		return View(user);
	}

	 
	[HttpPost]
	[Route("User/Delete/{id}")]   
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteConfirmed(string id)
	{
		try
		{
			await _unitOfWork.user.DeleteUserAsync(id);
			TempData["SuccessMessage"] = "User deleted successfully";
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
		}

		return RedirectToAction(nameof(DepartmentUsers));
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

			// Populate the department dropdown
			await PopulateDepartmentsDropdown(user.DepartmentId);

			var model = new EditProfileViewModel
			{
				FullName = user.FullName,
				Email = user.Email,
				NationalId = user.NationalId,
				FileNumber = user.RecipientFileNumber,
				DepartmentId = user.DepartmentId
			};

			return View(model);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error in EditProfile (GET): {ex.Message}");
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
			user.DepartmentId = model.DepartmentId ?? null;
			user.NationalId = model.NationalId;
			user.RecipientFileNumber = model.FileNumber;

			var result = await _userService.UpdateUserProfileAsync(user);
			if (result)
			{
				TempData["Success"] = "Profile updated successfully";
				return RedirectToAction("Profile");
			}

			ModelState.AddModelError("", "Failed to update profile");
		}
		catch (DbUpdateException dbEx)
		{
			ModelState.AddModelError("", "Database error occurred while updating profile");
			Console.WriteLine($"Database Error: {dbEx.Message}");
		}
		catch (Exception ex)
		{
			ModelState.AddModelError("", "An unexpected error occurred while updating profile");
			Console.WriteLine($"Unexpected Error: {ex.Message}");
		}

		await PopulateDepartmentsDropdown(model.DepartmentId);
		return View(model);
	}



	private async Task PopulateDepartmentsDropdown(int? selectedDepartmentId)
	{
		// Fetch all departments from the database
		var departments = await _unitOfWork.DepartmentRepository.GetAllAsync() ?? new List<Department>();

		// Convert departments to a SelectList
		var departmentList = departments.Select(d => new SelectListItem
		{
			Value = d.Id.ToString(),
			Text = d.Name,
			Selected = selectedDepartmentId.HasValue && d.Id == selectedDepartmentId.Value
		}).ToList();

		// Optional: Add a default "No Department" option at the top
		departmentList.Insert(0, new SelectListItem { Value = "", Text = "--Select Department --" });

		// Pass the list to ViewBag
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

	[Authorize(Roles = Roles.Admin)]
	public async Task<IActionResult> ManageRoles(string userId)
	{
		var user = await _userManager.FindByIdAsync(userId);
		if (user == null)
		{
			return NotFound();
		}

		var model = new UserRoleViewModel
		{
			UserId = user.Id,
			Email = user.Email,
			FullName = user.FullName,
			CurrentRoles = (await _userManager.GetRolesAsync(user)).ToList(),
			AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()
		};

		return View(model);
	}

	[HttpPost]
	[Authorize(Roles = Roles.Admin)]
	public async Task<IActionResult> UpdateRoles(string userId, List<string> selectedRoles)
	{
		var user = await _userManager.FindByIdAsync(userId);
		if (user == null)
		{
			return NotFound();
		}

		var currentRoles = await _userManager.GetRolesAsync(user);

		// Remove existing roles
		await _userManager.RemoveFromRolesAsync(user, currentRoles);

		// Add selected roles
		if (selectedRoles != null && selectedRoles.Any())
		{
			await _userManager.AddToRolesAsync(user, selectedRoles);
		}

		return RedirectToAction(nameof(DepartmentUsers));
	}

	[HttpGet]
	[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
	public async Task<IActionResult> DepartmentUsers(int? departmentId, string? searchTerm, string? roleName, int page = 1)
	{
		// Set up pagination
		int pageSize = 10;
		int skip = (page - 1) * pageSize;

		// Get all departments for the dropdown
		var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
		ViewBag.Departments = new SelectList(departments, "Id", "Name", departmentId);

		// Get all roles for the dropdown
		var roles = await _roleManager.Roles.ToListAsync();
		ViewBag.Roles = new SelectList(roles, "Name", "Name", roleName);

		// Get base query of all users
		var usersQuery = _userManager.Users
			.Include(u => u.Department)
			.AsQueryable();

		// Apply department filter if provided
		if (departmentId.HasValue && departmentId > 0)
		{
			usersQuery = usersQuery.Where(u => u.DepartmentId == departmentId);
			var selectedDepartment = departments.FirstOrDefault(d => d.Id == departmentId);
			ViewBag.SelectedDepartmentId = departmentId;
			ViewBag.SelectedDepartmentName = selectedDepartment?.Name ?? "Unknown Department";
		}

		// Apply search term filter if provided
		if (!string.IsNullOrEmpty(searchTerm))
		{
			searchTerm = searchTerm.Trim().ToLower();
			usersQuery = usersQuery.Where(u =>
				u.FullName.ToLower().Contains(searchTerm) ||
				u.Email.ToLower().Contains(searchTerm) ||
				(u.NationalId != null && u.NationalId.ToLower().Contains(searchTerm)) ||
				(u.RecipientFileNumber != null && u.RecipientFileNumber.ToLower().Contains(searchTerm))
			);
			ViewBag.SearchTerm = searchTerm;
		}

		// Get total count before applying role filter (which requires in-memory processing)
		var totalCount = await usersQuery.CountAsync();

		// Get all users that match the current filters
		var filteredUsers = await usersQuery.ToListAsync();

		// Apply role filter if provided (this must be done in memory since roles are in a separate table)
		// Apply role filter if provided (this must be done in memory since roles are in a separate table)
		if (!string.IsNullOrEmpty(roleName))
		{
			var usersInRole = new List<User>();
			foreach (var user in filteredUsers)
			{
				// Check if user is in the selected role
				if (await _userManager.IsInRoleAsync(user, roleName))
				{
					usersInRole.Add(user);
				}
			}
			filteredUsers = usersInRole;

			// Update total count after role filtering
			totalCount = filteredUsers.Count;

			// Get selected role name for display
			ViewBag.SelectedroleName = roleName;
			ViewBag.SelectedRoleName = roleName;
		}

		// Apply pagination
		var paginatedUsers = filteredUsers
			.Skip(skip)
			.Take(pageSize)
			.ToList();
		// Get roles for each user
		var userRoles = new Dictionary<string, List<string>>();
		foreach (var user in paginatedUsers)
		{
			var _roles = await _userManager.GetRolesAsync(user);
			userRoles[user.Id] = _roles.ToList();
		}

		// Add the roles to ViewBag
		ViewBag.UserRoles = userRoles;
		// Set pagination info for the view
		ViewBag.CurrentPage = page;
		ViewBag.PageSize = pageSize;
		ViewBag.TotalCount = totalCount;
		ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

		return View(paginatedUsers);
	}
	[HttpPost]
	[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
	public async Task<IActionResult> ImportUsers(IFormFile file)
	{
		if (file == null || file.Length == 0)
		{
			TempData["Error"] = "Please upload a valid Excel file.";
			return RedirectToAction("DepartmentUsers");
		}

		List<User> usersToAdd = new List<User>();
		HashSet<string> errors = new HashSet<string>();
		int successCount = 0;

		try
		{
			// Cache all departments to avoid multiple database calls
			var allDepartments = (await _unitOfWork.DepartmentRepository.GetAllAsync()).ToList();

			using (var stream = new MemoryStream())
			{
				file.CopyTo(stream);
				using (var package = new ExcelPackage(stream))
				{
					var worksheet = package.Workbook.Worksheets.FirstOrDefault();
					if (worksheet == null)
					{
						TempData["Error"] = "Invalid Excel file.";
						return RedirectToAction("DepartmentUsers");
					}

					int rowCount = worksheet.Dimension.Rows;

					// Skip header row
					for (int row = 2; row <= rowCount; row++)
					{
						try
						{
							string email = worksheet.Cells[row, 1].Value?.ToString();
							string fullName = worksheet.Cells[row, 2].Value?.ToString();
							string nationalId = worksheet.Cells[row, 3].Value?.ToString();
							string fileNumber = worksheet.Cells[row, 4].Value?.ToString();
							string departmentName = worksheet.Cells[row, 5].Value?.ToString();
							string password = worksheet.Cells[row, 6].Value?.ToString() ?? "Password123!"; // Default password if none provided
							string roles = worksheet.Cells[row, 7].Value?.ToString();

							// Validate required fields
							if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName))
							{
								errors.Add($"Row {row}: Email and Full Name are required.");
								continue;
							}

							// Check if user already exists
							var existingUser = await _userManager.FindByEmailAsync(email);
							if (existingUser != null)
							{
								errors.Add($"Row {row}: User with email {email} already exists.");
								continue;
							}

							// Find department by name if provided
							int? departmentId = null;
							if (!string.IsNullOrEmpty(departmentName))
							{
								var department = allDepartments.FirstOrDefault(d =>
									d.Name.Equals(departmentName, StringComparison.OrdinalIgnoreCase));

								if (department == null)
								{
									errors.Add($"Row {row}: Department '{departmentName}' does not exist.");
									continue;
								}
								departmentId = department.Id;
							}

							// Create new user
							var user = new User
							{
								Email = email,
								UserName = email,
								FullName = fullName,
								NationalId = nationalId,
								RecipientFileNumber = fileNumber,
								DepartmentId = departmentId,
								EmailConfirmed = true
							};

							// Create user in database
							var result = await _userManager.CreateAsync(user, password);

							if (result.Succeeded)
							{
								// Assign roles if provided
								if (!string.IsNullOrEmpty(roles))
								{
									var roleList = roles.Split(',').Select(r => r.Trim());
									foreach (var role in roleList)
									{
										// Check if role exists
										if (await _roleManager.RoleExistsAsync(role))
										{
											await _userManager.AddToRoleAsync(user, role);
										}
										else
										{
											errors.Add($"Row {row}: Role '{role}' does not exist.");
										}
									}
								}
								else
								{
									// Assign default User role
									await _userManager.AddToRoleAsync(user, Roles.User);
								}

								successCount++;
							}
							else
							{
								errors.Add($"Row {row}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
							}
						}
						catch (Exception ex)
						{
							errors.Add($"Row {row}: {ex.Message}");
						}
					}
				}
			}

			if (successCount > 0)
			{
				TempData["Success"] = $"Successfully imported {successCount} users.";
			}

			if (errors.Any())
			{
				TempData["ImportErrors"] = string.Join("<br>", errors);
			}
		}
		catch (Exception ex)
		{
			TempData["Error"] = $"Error importing users: {ex.Message}";
		}

		return RedirectToAction("DepartmentUsers");
	}

	[HttpGet]
	[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
	public async Task<IActionResult> DownloadUserTemplate()
	{
		using (var package = new ExcelPackage())
		{
			var worksheet = package.Workbook.Worksheets.Add("Users");

			// Add headers
			string[] headers =
			{
			"Email (Required)",
			"Full Name (Required)",
			"National ID",
			"File Number",
			"Department Name",
			"Password (Default: Password123!)",
			"Roles (comma separated)"
		};

			for (int i = 0; i < headers.Length; i++)
			{
				worksheet.Cells[1, i + 1].Value = headers[i];
				worksheet.Cells[1, i + 1].Style.Font.Bold = true;
				worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
				worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
			}

			// Try to get available department names for dropdown
			try
			{
				// Get all departments to provide examples in the template
				var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
				var departmentNames = departments.Select(d => d.Name).Take(3).ToList();

				// Add example roles
				var availableRoles = new List<string> {
	Roles.User,
	Roles.Supervisor,
	Roles.Manager,
	Roles.DataEntry,
	Roles.Admin
};

				// Add sample data
				worksheet.Cells[2, 1].Value = "user@example.com";
				worksheet.Cells[2, 2].Value = "Example User";
				worksheet.Cells[2, 3].Value = "1234567890";
				worksheet.Cells[2, 4].Value = "FILE001";
				worksheet.Cells[2, 5].Value = departmentNames.FirstOrDefault() ?? "Department Name";
				worksheet.Cells[2, 6].Value = "Password123!";
				worksheet.Cells[2, 7].Value = Roles.User;

				// Add data validation for department column if we have departments
				if (departmentNames.Any())
				{
					var validation = worksheet.DataValidations.AddListValidation(worksheet.Cells["E2:E1000"].Address);
					foreach (var dept in departmentNames)
					{
						validation.Formula.Values.Add(dept);
					}
					validation.ShowErrorMessage = true;
					validation.ErrorTitle = "Invalid Department";
					validation.Error = "Please select a department from the list";
				}

				// Add data validation for roles
				var roleValidation = worksheet.DataValidations.AddListValidation(worksheet.Cells["G2:G1000"].Address);
				foreach (var role in availableRoles)
				{
					roleValidation.Formula.Values.Add(role);
				}
				roleValidation.AllowBlank = true;
				roleValidation.ShowErrorMessage = true;
				roleValidation.ErrorTitle = "Invalid Role";
				roleValidation.Error = "Please select valid roles (comma separated): " + string.Join(", ", availableRoles);

				// Add an informational note
				worksheet.Cells[3, 1].Value = "Note: Available departments: " + string.Join(", ", departmentNames);
				worksheet.Cells[3, 1].Style.Font.Italic = true;
				worksheet.Cells[3, 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
				worksheet.Cells[3, 1, 3, 7].Merge = true;

				worksheet.Cells[4, 1].Value = "Note: Available roles: " + string.Join(", ", availableRoles);
				worksheet.Cells[4, 1].Style.Font.Italic = true;
				worksheet.Cells[4, 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
				worksheet.Cells[4, 1, 4, 7].Merge = true;
			}
			catch
			{
				// If there's an error accessing departments, just use a placeholder
				worksheet.Cells[2, 5].Value = "Department Name";
			}

			// Auto-fit columns
			worksheet.Cells.AutoFitColumns();

			var stream = new MemoryStream();
			package.SaveAs(stream);
			stream.Position = 0;

			return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "UserImportTemplate.xlsx");
		}
	}
}
