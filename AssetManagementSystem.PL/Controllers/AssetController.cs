using AssetManagementSystem.BLL;
using AssetManagementSystem.BLL.Interfaces;
using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.DAL.Utilities;
using AssetManagementSystem.PL.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.PL.Controllers
{
	[Authorize]
	public class AssetController : BaseController
	{
		private readonly IAssetService _assetService;
		private readonly IFacilityService _facilityService;
		private readonly ILogger<AssetController> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IAssetRepository _assetRepository;

	
		private readonly SignInManager<User> _signInManager;

		public AssetController(
		   IAssetService assetService,
		   IUnitOfWork unitOfWork,
		   IAssetRepository assetRepository,
		   UserManager<User> userManager,
		   SignInManager<User> signInManager,
		   IFacilityService facilityService,
		   ILogger<AssetController> logger,
		   INotificationService notificationService)
		   : base(notificationService, userManager)
		{
			_assetService = assetService;
			_unitOfWork = unitOfWork;
			_assetRepository = assetRepository;
			_signInManager = signInManager;
			_facilityService = facilityService;
			_logger = logger;
		}
		public async Task<IActionResult> Index()
		{
			var assets = await _assetService.GetAllAssetsAsync();
			return View(assets);
		}
		private async Task<User> GetCurrentUserAsync()
		{
			var userId = _userManager.GetUserId(User); // Get the user ID from the claims
			return await _userManager.FindByIdAsync(userId); // Fetch the user from the database
		}

		[HttpPost]
		public async Task<IActionResult> GetAssets()
		{
			try
			{
				var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
				var start = HttpContext.Request.Form["start"].FirstOrDefault();
				var length = HttpContext.Request.Form["length"].FirstOrDefault();
				var searchValue = HttpContext.Request.Form["search[value]"].FirstOrDefault();
				var sortColumn = HttpContext.Request.Form["order[0][column]"].FirstOrDefault();
				var sortDirection = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();

				List<string> assetTagsFilter = null;
				if (HttpContext.Session.TryGetValue("AssetTagsFilter", out byte[] filterData))
				{
					string filterJson = Encoding.UTF8.GetString(filterData);
					assetTagsFilter = System.Text.Json.JsonSerializer.Deserialize<List<string>>(filterJson);
				}

				int pageSize = length != null ? Convert.ToInt32(length) : 0;
				int skip = start != null ? Convert.ToInt32(start) : 0;
				int recordsTotal = 0;


				// Get all assets
				var assets = await _assetService.GetAllAssetsAsync();
				var assetList = assets.ToList();

				// Apply the asset tags filter if it exists
				if (assetTagsFilter != null && assetTagsFilter.Any())
				{
					assetList = assetList.Where(a => assetTagsFilter.Contains(a.AssetTag)).ToList();
				}

				// Apply search filter
				if (!string.IsNullOrEmpty(searchValue))
				{
					assetList = assetList.Where(a =>
						a.AssetTag.Contains(searchValue) ||
						(a.Cluster != null && a.Cluster.Contains(searchValue)) ||
						(a.Facility != null && a.Facility.Name.Contains(searchValue)) ||
						(a.Building != null && a.Building.Name.Contains(searchValue)) ||
						(a.Floor != null && a.Floor.Name.Contains(searchValue)) ||
						(a.Room != null && a.Room.Name.Contains(searchValue)) ||
						(a.Department != null && a.Department.Name.Contains(searchValue)) ||
						(a.AssetDescription != null && a.AssetDescription.Contains(searchValue)) ||
						(a.Brand != null && a.Brand.Contains(searchValue)) ||
						(a.Model != null && a.Model.Contains(searchValue)) ||
						(a.User != null && a.User.FullName.Contains(searchValue)) ||
						(a.AssetType != null && a.AssetType.Contains(searchValue)) ||
						(a.Status != null && a.Status.Contains(searchValue)) ||
						(a.SerialNumber != null && a.SerialNumber.Contains(searchValue)) ||
						(a.InsertUser != null && a.InsertUser.Contains(searchValue)) ||
						(a.Details != null && a.Details.Contains(searchValue)) ||
						(a.AssetClass1 != null && a.AssetClass1.Contains(searchValue)) ||
						(a.AssetClass2 != null && a.AssetClass2.Contains(searchValue)) ||
						(a.AssetClass3 != null && a.AssetClass3.Contains(searchValue))
					).ToList();
				}

				// Apply sorting
				if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
				{
					var columnIndex = Convert.ToInt32(sortColumn);
					var isAscending = sortDirection == "asc";

					switch (columnIndex)
					{
						case 0: // AssetTag
							assetList = isAscending ? assetList.OrderBy(a => a.AssetTag).ToList() : assetList.OrderByDescending(a => a.AssetTag).ToList();
							break;
						case 1: // Cluster
							assetList = isAscending ? assetList.OrderBy(a => a.Cluster).ToList() : assetList.OrderByDescending(a => a.Cluster).ToList();
							break;
						case 2: // Facility
							assetList = isAscending ? assetList.OrderBy(a => a.Facility?.Name).ToList() : assetList.OrderByDescending(a => a.Facility?.Name).ToList();
							break;
						case 3: // Building
							assetList = isAscending ? assetList.OrderBy(a => a.Building?.Name).ToList() : assetList.OrderByDescending(a => a.Building?.Name).ToList();
							break;
						case 4: // Floor
							assetList = isAscending ? assetList.OrderBy(a => a.Floor?.Name).ToList() : assetList.OrderByDescending(a => a.Floor?.Name).ToList();
							break;
						case 5: // Room
							assetList = isAscending ? assetList.OrderBy(a => a.Room?.Name).ToList() : assetList.OrderByDescending(a => a.Room?.Name).ToList();
							break;
						case 6: // Department
							assetList = isAscending ? assetList.OrderBy(a => a.Department?.Name).ToList() : assetList.OrderByDescending(a => a.Department?.Name).ToList();
							break;
						case 7: // AssetDescription
							assetList = isAscending ? assetList.OrderBy(a => a.AssetDescription).ToList() : assetList.OrderByDescending(a => a.AssetDescription).ToList();
							break;
						case 8: // Brand
							assetList = isAscending ? assetList.OrderBy(a => a.Brand).ToList() : assetList.OrderByDescending(a => a.Brand).ToList();
							break;
						case 9: // Model
							assetList = isAscending ? assetList.OrderBy(a => a.Model).ToList() : assetList.OrderByDescending(a => a.Model).ToList();
							break;
						case 10: // User
							assetList = isAscending ? assetList.OrderBy(a => a.User?.FullName).ToList() : assetList.OrderByDescending(a => a.User?.FullName).ToList();
							break;
						case 11: // AssetType
							assetList = isAscending ? assetList.OrderBy(a => a.AssetType).ToList() : assetList.OrderByDescending(a => a.AssetType).ToList();
							break;
						case 12: // Status
							assetList = isAscending ? assetList.OrderBy(a => a.Status).ToList() : assetList.OrderByDescending(a => a.Status).ToList();
							break;
						case 13: // IsDisposed
							assetList = isAscending ? assetList.OrderBy(a => a.IsDisposed).ToList() : assetList.OrderByDescending(a => a.IsDisposed).ToList();
							break;
						case 14: // SerialNumber
							assetList = isAscending ? assetList.OrderBy(a => a.SerialNumber).ToList() : assetList.OrderByDescending(a => a.SerialNumber).ToList();
							break;
						case 15: // InsertDate
							assetList = isAscending ? assetList.OrderBy(a => a.InsertDate).ToList() : assetList.OrderByDescending(a => a.InsertDate).ToList();
							break;
						case 16: // InsertUser
							assetList = isAscending ? assetList.OrderBy(a => a.InsertUser).ToList() : assetList.OrderByDescending(a => a.InsertUser).ToList();
							break;
						case 17: // Details
							assetList = isAscending ? assetList.OrderBy(a => a.Details).ToList() : assetList.OrderByDescending(a => a.Details).ToList();
							break;
						case 18: // AssetClass1
							assetList = isAscending ? assetList.OrderBy(a => a.AssetClass1).ToList() : assetList.OrderByDescending(a => a.AssetClass1).ToList();
							break;
						case 19: // AssetClass2
							assetList = isAscending ? assetList.OrderBy(a => a.AssetClass2).ToList() : assetList.OrderByDescending(a => a.AssetClass2).ToList();
							break;
						case 20: // AssetClass3
							assetList = isAscending ? assetList.OrderBy(a => a.AssetClass3).ToList() : assetList.OrderByDescending(a => a.AssetClass3).ToList();
							break;
						default:
							assetList = assetList.OrderBy(a => a.AssetTag).ToList();
							break;
					}
				}

				// Get total records count
				recordsTotal = assetList.Count();

				// Pagination
				var data = assetList.Skip(skip).Take(pageSize).Select(a => new
				{
					assetTag = a.AssetTag,
					cluster = a.Cluster,
					facility = a.Facility != null ? new { name = a.Facility.Name } : null,
					building = a.Building != null ? new { name = a.Building.Name } : null,
					floor = a.Floor != null ? new { name = a.Floor.Name } : null,
					room = a.Room != null ? new { name = a.Room.Name } : null,
					department = a.Department != null ? new { name = a.Department.Name } : null,
					assetDescription = a.AssetDescription,
					brand = a.Brand,
					model = a.Model,
					user = a.User != null ? new { fullName = a.User.FullName ?? "N/A" } : new { fullName = "N/A" },
					supervisor = a.Supervisor != null ? new { fullName = a.Supervisor.FullName ?? "N/A" } : new { fullName = "N/A" }, // Add supervisor info
					assetType = a.AssetType,
					status = a.Status,
					isDisposed = a.IsDisposed,
					serialNumber = a.SerialNumber,
					insertDate = a.InsertDate.ToString("yyyy-MM-ddTHH:mm:ss"), // Format the date
					insertUser = a.InsertUser,
					details = a.Details,
					assetClass1 = a.AssetClass1,
					assetClass2 = a.AssetClass2,
					assetClass3 = a.AssetClass3
				}).ToList();

				// Return JSON data
				return Json(new
				{
					draw = draw,
					recordsFiltered = recordsTotal,
					recordsTotal = recordsTotal,
					data = data
				});
			}
			catch (Exception ex)
			{
				// Log the exception
				return StatusCode(500, new { error = ex.Message });
			}
		}
		public async Task<IActionResult> Details(string id)
		{
			var asset = await _assetService.GetAssetByIdAsync(id);
			if (asset == null)
				return NotFound();
			return View(asset);
		}

		// GET: Asset/Create
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.DataEntry}")]
		public async Task<IActionResult> Create()
		{
			try
			{
				// Load all dropdown data needed for the form
				await PopulateDropdownsAsync();
				// Fetch all data needed for dropdowns
				var facilities = await _facilityService.GetAllAsync();
				var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
				var buildings = await _unitOfWork.buildingRepository.GetAllAsync();
				var floors = await _unitOfWork.floorRepository.GetAllAsync();
				var rooms = await _unitOfWork.RoomRepository.GetAllAsync();

				// Add data to ViewBag
				ViewBag.Facilities = facilities;
				ViewBag.Departments = departments;
				ViewBag.Buildings = buildings;
				ViewBag.Floors = floors;
				ViewBag.Rooms = rooms;
				ViewBag.Supervisors = await GetSupervisorsForDropdownAsync();
				return View();
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = $"Error preparing create form: {ex.Message}";
				return RedirectToAction(nameof(Index));
			}
		}

		// POST: Asset/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.DataEntry}")]
		public async Task<IActionResult> Create(Asset asset)
		{
			try
			{
				// Remove navigation properties to avoid validation errors
				ModelState.Remove("Facility");
				ModelState.Remove("Building");
				ModelState.Remove("Floor");
				ModelState.Remove("Room");
				ModelState.Remove("Department");
				ModelState.Remove("User");
				ModelState.Remove("Supervisor");

				if (!ModelState.IsValid)
				{
					await PopulateDropdownsAsync();
					ViewBag.Supervisors = await GetSupervisorsForDropdownAsync();
					return View(asset);
				}

				// Get the current logged-in user
				var currentUser = await GetCurrentUserAsync();
				if (currentUser == null)
				{
					TempData["ErrorMessage"] = "User authentication required";
					return RedirectToAction("Login", "Auth");
				}

				// Set creation metadata
				asset.InsertDate = DateTime.UtcNow;
				asset.InsertUser = currentUser.UserName;

				// Perform validation before saving
				if (string.IsNullOrEmpty(asset.AssetTag))
				{
					ModelState.AddModelError("AssetTag", "Asset Tag is required");
					await PopulateDropdownsAsync();
					ViewBag.Supervisors = await GetSupervisorsForDropdownAsync();
					return View(asset);
				}

				// Check if asset with same tag already exists
				var existingAsset = await _assetService.GetAssetByIdAsync(asset.AssetTag);
				if (existingAsset != null)
				{
					ModelState.AddModelError("AssetTag", "An asset with this tag already exists");
					await PopulateDropdownsAsync();
					ViewBag.Supervisors = await GetSupervisorsForDropdownAsync();
					return View(asset);
				}

				// Save the asset
				await _assetService.AddAssetAsync(asset);
				// Create notification for admins and managers
				await CreateNotificationForRole(Roles.Admin,
					"New Asset Created",
					$"Asset {asset.AssetTag} was created by {currentUser.FullName}",
					"Asset",
					asset.AssetTag,
					Url.Action("Details", "Asset", new { id = asset.AssetTag }));

				// Create notification for managers as well
				await CreateNotificationForRole(Roles.Manager,
					"New Asset Created",
					$"Asset {asset.AssetTag} was created by {currentUser.FullName}",
					"Asset",
					asset.AssetTag,
					Url.Action("Details", "Asset", new { id = asset.AssetTag }));

				TempData["SuccessMessage"] = $"Asset {asset.AssetTag} created successfully";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"An error occurred: {ex.Message}");
				await PopulateDropdownsAsync();
				ViewBag.Supervisors = await GetSupervisorsForDropdownAsync();
				return View(asset);
			}
		}

		private async Task PopulateDropdownsAsync()
		{
			// Load all users for the User dropdown
			var users = await _assetService.GetAllUsersAsync();
			ViewBag.Users = new SelectList(users, "Id", "FullName");

			// Define status options
			ViewBag.StatusOptions = new SelectList(new[]
			{
				"متاح",
				"مكهن",
				"صيانة",
				"تالف"
			});

			// Define asset types
			ViewBag.AssetTypes = new SelectList(new[]
			{
				"Hardware",
				"Software",
				"Furniture",
				"Equipment",
				"Vehicle",
				"Building",
				"Land",
				"Intangible"
			});

			
		}
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Supervisor}")]
		public async Task<IActionResult> Edit(string id)
		{
			var asset = await _assetService.GetAssetByIdAsync(id);
			if (asset == null)
				return NotFound();
			ViewBag.Supervisors = await GetSupervisorsForDropdownAsync();
			return View(asset);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(string id, Asset asset)
		{
			if (id != asset.AssetTag)
				return BadRequest();

			if (!ModelState.IsValid)
			{
				ViewBag.Supervisors = await GetSupervisorsForDropdownAsync();
				return View(asset);
			}

			var currentUser = await GetCurrentUserAsync();
			if (currentUser == null)
			{
				return RedirectToAction("Login", "Auth"); // Redirect to login if no user is logged in
			}

			asset.InsertDate = DateTime.UtcNow; // Add an UpdateDate field to track when the asset was last updated
			asset.InsertUser = currentUser.UserName; // Assign the logged-in user's username

			await _assetService.UpdateAssetAsync(asset);

			// Create notification for asset edit
			await CreateNotificationForRole(Roles.Admin,
				"Asset Updated",
				$"Asset {asset.AssetTag} was updated by {currentUser.FullName}",
				"Asset",
				asset.AssetTag,
				Url.Action("Details", "Asset", new { id = asset.AssetTag }));

			// Also notify managers
			await CreateNotificationForRole(Roles.Manager,
				"Asset Updated",
				$"Asset {asset.AssetTag} was updated by {currentUser.FullName}",
				"Asset",
				asset.AssetTag,
				Url.Action("Details", "Asset", new { id = asset.AssetTag }));

			// Notify the original owner/user of the asset if applicable
			if (!string.IsNullOrEmpty(asset.UserId))
			{
				await CreateNotificationForUser(
					asset.UserId,
					"Asset Updated",
					$"Asset {asset.AssetTag} ({asset.AssetDescription}) assigned to you has been updated",
					"AssetUpdate",
					asset.AssetTag,
					Url.Action("Details", "Asset", new { id = asset.AssetTag }));
			}

			return RedirectToAction(nameof(Index));
		}


		[HttpGet("Asset/Delete/{id}")]
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
		public async Task<IActionResult> Delete(string id)
		{
			var asset = await _assetService.GetAssetByIdAsync(id);
			if (asset == null)
				return NotFound();
			return View(asset);
		}

		[HttpPost("Asset/DeleteConfirmed")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(string id)
		{
			var asset = await _unitOfWork.AssetRepository.GetByIdAsync(id);
			if (asset == null)
			{
				return NotFound();
			}

			var currentUser = await GetCurrentUserAsync();
			if (currentUser == null)
			{
				return RedirectToAction("Login", "Auth"); // Redirect to login if no user is logged in
			}
			// Store asset details before deletion for use in notifications
			string assetDescription = asset.AssetDescription ?? "No description";
			string assetUserId = asset.UserId;
			asset.InsertDate = DateTime.UtcNow; // Add a DeleteDate field to track when the asset was deleted
			asset.InsertUser = currentUser.UserName; // Assign the logged-in user's username

			await _unitOfWork.AssetRepository.DeleteAsync(id);
			await _unitOfWork.SaveChangesAsync();
			// Notify admins about asset deletion
			await CreateNotificationForRole(Roles.Admin,
				"Asset Deleted",
				$"Asset {id} ({assetDescription}) was deleted by {currentUser.FullName}",
				"AssetDeletion",
				id);

			// Notify managers about asset deletion
			await CreateNotificationForRole(Roles.Manager,
				"Asset Deleted",
				$"Asset {id} ({assetDescription}) was deleted by {currentUser.FullName}",
				"AssetDeletion",
				id);

			// Notify the original owner/user of the asset if applicable
			if (!string.IsNullOrEmpty(assetUserId))
			{
				await CreateNotificationForUser(
					assetUserId,
					"Asset Deleted",
					$"Asset {id} ({assetDescription}) that was assigned to you has been deleted",
					"AssetDeletion",
					id);
			}
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.DataEntry}")]
		public async Task<IActionResult> Dispose(string id)
		{
			var asset = await _assetService.GetAssetByIdAsync(id);
			if (asset == null)
				return NotFound();
			return View(asset);
		}

		[HttpPost]
		public async Task<IActionResult> Dispose(string id, string disposalType, decimal saleValue)
		{
			var success = await _assetService.DisposeAssetAsync(id, disposalType, saleValue);
			if (!success)
				return BadRequest("Failed to dispose asset");

			return RedirectToAction(nameof(Index));
		}


		[HttpGet]
		public async Task<JsonResult> GetAllFacilitiesAsync()
		{
			try
			{
				var facilities = await _assetService.GetAllFacilitiesAsync();
				return Json(facilities.Select(f => new { id = f.Id, name = f.Name }).ToList());
			}
			catch (Exception ex)
			{
				// Log the exception
				return Json(new { error = ex.Message });
			}
		}

		[HttpGet]
		public async Task<JsonResult> GetAllDepartmentsAsync()
		{
			try
			{
				var departments = await _assetService.GetAllDepartmentsAsync();
				var result = departments.Select(d => new { id = d.Id, name = d.Name }).ToList();
				return Json(result);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetAllDepartmentsAsync: {ex.Message}");
				return Json(new List<object>());
			}
		}

		


	

		[HttpPost]
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.DataEntry}")]
		public  IActionResult UpdateAsset([FromBody] Asset asset)
		{
			 _assetService.UpdateAssetAsync(asset);
			return Ok();
		}

		[HttpPost]
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.DataEntry}")]
		public IActionResult AddAsset([FromBody] Asset asset)
		{
			_assetService.AddAssetAsync(asset);
			return Ok();
		}
		[HttpPost]
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.DataEntry}")]
		public async Task<IActionResult> ImportAssets(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				TempData["Error"] = "الرجاء تحميل ملف Excel صالح.";
				return RedirectToAction("Index");
			}

			var successCount = 0;
			var errorCount = 0;
			var errors = new List<string>();
			var currentUser = await GetCurrentUserAsync();

			try
			{
				// Set EPPlus license
				ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

				using (var stream = new MemoryStream())
				{
					await file.CopyToAsync(stream);
					using (var package = new ExcelPackage(stream))
					{
						var worksheet = package.Workbook.Worksheets.FirstOrDefault();
						if (worksheet == null)
						{
							TempData["Error"] = "ملف Excel غير صالح.";
							return RedirectToAction("Index");
						}

						int rowCount = worksheet.Dimension.Rows;
						if (rowCount <= 1) // Only header row
						{
							TempData["Error"] = "ملف Excel فارغ. لا توجد بيانات للاستيراد.";
							return RedirectToAction("Index");
						}

						// Get all required entities for name lookup
						var facilities = await _assetService.GetAllFacilitiesAsync();
						var departments = await _assetService.GetAllDepartmentsAsync();
						var buildings = await _unitOfWork.buildingRepository.GetAllAsync();
						var floors = await _unitOfWork.floorRepository.GetAllAsync();
						var rooms = await _unitOfWork.RoomRepository.GetAllAsync();
						var users = await _assetService.GetAllUsersAsync();

						// Dictionary for name lookups (case insensitive)
						var facilityDict = facilities.ToDictionary(f => f.Name.ToLower(), f => f);
						var departmentDict = departments.ToDictionary(d => d.Name.ToLower(), d => d);
						var buildingDict = buildings.ToDictionary(b => b.Name.ToLower(), b => b);
						var floorDict = floors.ToDictionary(f => f.Name?.ToLower() ?? "", f => f);
						var roomDict = rooms.ToDictionary(r => r.Name.ToLower(), r => r);
						var userDict = users.ToDictionary(u => (u.FullName ?? "").ToLower(), u => u);

						var assets = new List<Asset>();
						var existingAssetTags = new HashSet<string>((await _assetService.GetAllAssetsAsync()).Select(a => a.AssetTag));

						for (int row = 2; row <= rowCount; row++) // Skip header row
						{
							try
							{
								string assetTag = worksheet.Cells[row, 1].Value?.ToString()?.Trim();

								// Skip empty rows
								if (string.IsNullOrWhiteSpace(assetTag))
									continue;

								// Check if asset tag already exists
								if (existingAssetTags.Contains(assetTag))
								{
									errors.Add($"سطر {row}: رمز الأصل '{assetTag}' موجود مسبقاً في النظام");
									errorCount++;
									continue;
								}

								// Extract values from the row
								string facilityName = (worksheet.Cells[row, 3].Value?.ToString() ?? "").Trim();
								string buildingName = (worksheet.Cells[row, 4].Value?.ToString() ?? "").Trim();
								string floorName = (worksheet.Cells[row, 5].Value?.ToString() ?? "").Trim();
								string roomName = (worksheet.Cells[row, 6].Value?.ToString() ?? "").Trim();
								string departmentName = (worksheet.Cells[row, 7].Value?.ToString() ?? "").Trim();
								string userName = (worksheet.Cells[row, 15].Value?.ToString() ?? "").Trim();

								// Lookups by name
								if (string.IsNullOrEmpty(facilityName))
								{
									errors.Add($"سطر {row}: اسم المنشأة مطلوب");
									errorCount++;
									continue;
								}

								if (string.IsNullOrEmpty(departmentName))
								{
									errors.Add($"سطر {row}: اسم القسم مطلوب");
									errorCount++;
									continue;
								}

								// Try to find entities by name (case insensitive)
								if (!facilityDict.TryGetValue(facilityName.ToLower(), out var facility))
								{
									errors.Add($"سطر {row}: المنشأة '{facilityName}' غير موجودة في النظام");
									errorCount++;
									continue;
								}

								if (!departmentDict.TryGetValue(departmentName.ToLower(), out var department))
								{
									errors.Add($"سطر {row}: القسم '{departmentName}' غير موجود في النظام");
									errorCount++;
									continue;
								}

								// Optional lookups
								Building building = null;
								Floor floor = null;
								Room room = null;
								User user = null;

								if (!string.IsNullOrEmpty(buildingName))
									buildingDict.TryGetValue(buildingName.ToLower(), out building);

								if (!string.IsNullOrEmpty(floorName))
									floorDict.TryGetValue(floorName.ToLower(), out floor);

								if (!string.IsNullOrEmpty(roomName))
									roomDict.TryGetValue(roomName.ToLower(), out room);

								if (!string.IsNullOrEmpty(userName))
									userDict.TryGetValue(userName.ToLower(), out user);

								// Create a new asset
								var asset = new Asset
								{
									AssetTag = assetTag,
									Cluster = worksheet.Cells[row, 2].Value?.ToString(),
									FacilityId = facility.Id,
									BuildingId = building?.Id ?? facility.Buildings.FirstOrDefault()?.Id ?? 0,
									FloorId = floor?.Id ?? (building != null ? building.Floors.FirstOrDefault()?.Id ?? 0 : 0),
									RoomTag = room?.RoomTag,
									DepartmentId = department.Id,
									AssetDescription = worksheet.Cells[row, 8].Value?.ToString(),
									Brand = worksheet.Cells[row, 9].Value?.ToString(),
									Model = worksheet.Cells[row, 10].Value?.ToString(),
									AssetType = worksheet.Cells[row, 11].Value?.ToString() ?? "Hardware",
									Status = worksheet.Cells[row, 12].Value?.ToString() ?? "متاح",
									IsDisposed = (worksheet.Cells[row, 13].Value?.ToString() ?? "").ToLower() == "true",
									SerialNumber = worksheet.Cells[row, 14].Value?.ToString(),
									UserId = user?.Id,
									InsertDate = DateTime.UtcNow,
									InsertUser = currentUser?.FullName ?? worksheet.Cells[row, 16].Value?.ToString() ?? "System",
									Details = worksheet.Cells[row, 17].Value?.ToString(),
									AssetClass1 = worksheet.Cells[row, 18].Value?.ToString(),
									AssetClass2 = worksheet.Cells[row, 19].Value?.ToString(),
									AssetClass3 = worksheet.Cells[row, 20].Value?.ToString(),
								};

								// Add to import list
								assets.Add(asset);
								successCount++;
							}
							catch (Exception ex)
							{
								errors.Add($"سطر {row}: {ex.Message}");
								errorCount++;
								_logger.LogError(ex, "Error processing import row {Row}", row);
							}
						}

						// Save assets to database if we have any valid ones
						if (assets.Any())
						{
							try
							{
								await _assetService.AddRange(assets);

								// Create change logs
								foreach (var asset in assets)
								{
									await _unitOfWork.ChangeLogRepository.AddAsync(new ChangeLog
									{
										EntityName = "Asset",
										EntityId = asset.AssetTag,
										ActionType = "Added",
										NewValues = JsonConvert.SerializeObject(new
										{
											asset.AssetTag,
											asset.AssetDescription,
											asset.FacilityId,
											asset.DepartmentId,
											asset.Status
										}),
										ChangeDate = DateTime.UtcNow,
										UserId = currentUser?.Id
									});
								}

								await _unitOfWork.SaveChangesAsync();
							}
							catch (Exception ex)
							{
								_logger.LogError(ex, "Error saving imported assets to database");
								TempData["Error"] = $"حدث خطأ أثناء حفظ الأصول المستوردة: {ex.Message}";
								return RedirectToAction("Index");
							}
						}
					}
				}

				// Create notification about the import
				if (currentUser != null)
				{
					await CreateNotificationForRole(Roles.Admin,
						"استيراد أصول جديدة",
						$"قام {currentUser.FullName} باستيراد {successCount} أصول جديدة",
						"BulkImport",
						null,
						Url.Action("Index", "Asset"));

					await CreateNotificationForRole(Roles.Manager,
						"استيراد أصول جديدة",
						$"قام {currentUser.FullName} باستيراد {successCount} أصول جديدة",
						"BulkImport",
						null,
						Url.Action("Index", "Asset"));
				}

				// Show results
				if (errorCount > 0)
				{
					string errorSummary = string.Join("<br>", errors.Take(Math.Min(5, errors.Count)));
					if (errors.Count > 5)
						errorSummary += "<br>...والمزيد";

					if (successCount > 0)
					{
						TempData["Warning"] = $"تم استيراد {successCount} أصول بنجاح، مع {errorCount} أخطاء:<br>{errorSummary}";
					}
					else
					{
						TempData["Error"] = $"فشل الاستيراد. {errorCount} أخطاء:<br>{errorSummary}";
					}
				}
				else if (successCount > 0)
				{
					TempData["Success"] = $"تم استيراد {successCount} أصول بنجاح!";
				}
				else
				{
					TempData["Warning"] = "لم يتم استيراد أي أصول. تأكد من أن الملف يحتوي على بيانات صالحة.";
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error importing assets");
				TempData["Error"] = $"حدث خطأ أثناء استيراد الأصول: {ex.Message}";
			}

			return RedirectToAction("Index");
		}
		[HttpGet]
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.DataEntry}")]
		public async Task<ActionResult> ExportAssets()
		{
			try
			{
				// Fetch data from the repository
				var assets = await _assetRepository.GetAllAsync();

				// Create a new Excel package
				using (var package = new ExcelPackage())
				{
					var worksheet = package.Workbook.Worksheets.Add("Assets");

					// Define headers
					string[] headers = {
		"Asset Tag", "Cluster", "Facility ID", "Building ID", "Floor ID",
		"Room Tag", "Department ID", "Description", "Brand", "Model",
		"User ID", "Asset Type", "Status", "Is Disposed", "Serial Number",
		"Insert Date", "Insert User", "Details", "Asset Class 1", "Asset Class 2", "Asset Class 3"
		};

					// Set headers and style
					for (int i = 0; i < headers.Length; i++)
					{
						worksheet.Cells[1, i + 1].Value = headers[i];
						worksheet.Cells[1, i + 1].Style.Font.Bold = true;
						worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
						worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
						worksheet.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					}

					// Insert data
					int row = 2;
					foreach (var asset in assets)
					{
						worksheet.Cells[row, 1].Value = asset.AssetTag;
						worksheet.Cells[row, 2].Value = asset.Cluster;
						worksheet.Cells[row, 3].Value = asset.FacilityId;
						worksheet.Cells[row, 4].Value = asset.BuildingId;
						worksheet.Cells[row, 5].Value = asset.FloorId;
						worksheet.Cells[row, 6].Value = asset.RoomTag;
						worksheet.Cells[row, 7].Value = asset.DepartmentId;
						worksheet.Cells[row, 8].Value = asset.AssetDescription;
						worksheet.Cells[row, 9].Value = asset.Brand;
						worksheet.Cells[row, 10].Value = asset.Model;
						worksheet.Cells[row, 11].Value = asset.UserId;
						worksheet.Cells[row, 12].Value = asset.AssetType;
						worksheet.Cells[row, 13].Value = asset.Status;
						worksheet.Cells[row, 14].Value = asset.IsDisposed ? "Yes" : "No"; // Use "Yes" or "No" for better readability
						worksheet.Cells[row, 15].Value = asset.SerialNumber;
						worksheet.Cells[row, 16].Value = asset.InsertDate.ToString("yyyy-MM-dd");
						worksheet.Cells[row, 17].Value = asset.InsertUser;
						worksheet.Cells[row, 18].Value = asset.Details;
						worksheet.Cells[row, 19].Value = asset.AssetClass1;
						worksheet.Cells[row, 20].Value = asset.AssetClass2;
						worksheet.Cells[row, 21].Value = asset.AssetClass3;

						row++;
					}

					// Auto-fit columns
					worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

					// Prepare the response for download
					var stream = new MemoryStream();
					package.SaveAs(stream);
					stream.Position = 0; // Reset stream position

					// Generate a unique file name
					string excelName = $"Assets_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

					// Return the Excel file
					return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
				}
			}
			catch (Exception ex)
			{
				// Log the exception (you can use a logging framework like Serilog or NLog)
				Console.WriteLine($"Error exporting assets to Excel: {ex.Message}");

				// Return an error response
				TempData["Error"] = "An error occurred while exporting assets. Please try again later.";
				return RedirectToAction("Index");
			}
		}

		[HttpGet]
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.DataEntry}")]
		public async Task<IActionResult> ExportAssetsToPdf()
		{
			// Set QuestPDF license type
			QuestPDF.Settings.License = LicenseType.Community;

			try
			{
				// Fetch data from the database
				var assets = await _assetService.GetAllAssetsAsync();

				// Create the PDF document
				var document = Document.Create(container =>
				{
					container.Page(page =>
					{
						page.Size(PageSizes.A4);
						page.Margin(20);
						page.DefaultTextStyle(x => x.FontSize(12));

						// Header Section
						page.Header()
							.Row(row =>
							{
								// Logo (if exists)
								var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
								if (System.IO.File.Exists(logoPath))
								{
									row.ConstantItem(100)
	.Image(logoPath) // Load logo from path
	.FitWidth();
								}

								// Report Title and Date
								row.RelativeItem()
	.Column(col =>
	{
		col.Item().Text("Asset Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
		col.Item().Text($"Generated on: {System.DateTime.Now:yyyy-MM-dd HH:mm}");
	});
							});

						// Content Section
						page.Content()
							.Table(table =>
							{
								// Define table columns
								table.ColumnsDefinition(columns =>
								{
									columns.RelativeColumn();
									columns.RelativeColumn();
									columns.RelativeColumn();
									columns.RelativeColumn();
								});

								// Table Header
								table.Header(header =>
								{
									header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Asset ID").Bold();
									header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Asset Name").Bold();
									header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Department").Bold();
									header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Status").Bold();
								});

								// Table Rows (Dynamic Data)
								foreach (var asset in assets)
								{
									table.Cell().Element(CellStyle).Text(asset.AssetTag);
									table.Cell().Element(CellStyle).Text(asset.AssetDescription);
									table.Cell().Element(CellStyle).Text(asset.Department?.Name ?? "N/A");
									table.Cell().Element(CellStyle).Text(asset.Status);
								}
							});

						// Footer Section
						page.Footer()
							.AlignCenter()
							.Text("© 2025 Your Company Name. All Rights Reserved.")
							.FontSize(10)
							.FontColor(Colors.Grey.Medium);
					});
				});

				// Generate PDF and return as a file
				var stream = new MemoryStream();
				document.GeneratePdf(stream);
				stream.Position = 0; // Reset stream position
				return File(stream, "application/pdf", "Assets.pdf");
			}
			catch (Exception ex)
			{
				// Log the exception (you can use a logging framework like Serilog or NLog)
				Console.WriteLine($"Error generating PDF: {ex.Message}");

				// Return an error response
				return StatusCode(500, "An error occurred while generating the PDF. Please try again later.");
			}
		}


		[HttpGet]
		public async Task<IActionResult> GetDepartments()
		{
			try
			{
				var departments = await _assetService.GetAllDepartmentsAsync();
				var result = departments.Select(f => new { id = f.Id, name = f.Name }).ToList();
				return Json(result);
			}
			catch (Exception ex)
			{
				return Json(new { error = ex.Message });
			}
		}


		//static IContainer CellStyle(IContainer container)
		//{
		//	return container.Padding(5);
		//}

		[HttpPost]
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.DataEntry}")]
		public async Task<IActionResult> BulkTransfer([FromBody] BulkOperationRequest request)
		{
			try
			{
				if (request?.AssetTags == null || !request.AssetTags.Any())
				{
					return BadRequest(new { error = "No assets selected for transfer" });
				}

				if (!request.TargetDepartmentId.HasValue)
				{
					return BadRequest(new { error = "Target department is required" });
				}

				// Get current user
				var currentUser = await GetCurrentUserAsync();
				if (currentUser == null)
				{
					return Unauthorized(new { error = "User authentication required" });
				}

				// Verify department exists
				var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.TargetDepartmentId.Value);
				if (department == null)
				{
					return BadRequest(new { error = "Selected department not found" });
				}

				// Perform the bulk transfer
				await _assetService.BulkTransferAssetsAsync(
					request.AssetTags,
					request.TargetDepartmentId.Value,
					request.TargetUserId
				);

				return Ok(new
				{
					success = true,
					message = $"Successfully transferred {request.AssetTags.Count} assets"
				});
			}
			catch (Exception ex)
			{
				// Log the exception details
				Console.WriteLine($"Bulk transfer error: {ex.Message}");
				Console.WriteLine($"Stack trace: {ex.StackTrace}");

				return StatusCode(500, new
				{
					error = "Transfer operation failed",
					details = ex.Message
				});
			}
		}

		[HttpPost]
		public async Task<IActionResult> BulkDispose([FromBody] BulkOperationRequest request)
		{
			try
			{
				var currentUser = await GetCurrentUserAsync();
				if (currentUser == null)
				{
					return Unauthorized();
				}

				// Validate request
				if (request.AssetTags == null || !request.AssetTags.Any())
				{
					return BadRequest("No assets selected for disposal");
				}

				// Get the assets being disposed
				var assets = await _assetService.GetAssetsByTags(request.AssetTags);

				// Check if all assets are from same department
				var departments = assets.Select(a => a.DepartmentId).Distinct();
				if (departments.Count() > 1)
				{
					return BadRequest("Assets must be from the same department");
				}

				// Perform bulk disposal
				await _assetService.BulkDisposeAssetsAsync(
					request.AssetTags,
					request.DisposalType,
					(decimal)request.SaleValue
				);

				// Log the change
				await _unitOfWork.ChangeLogRepository.AddAsync(new ChangeLog
				{
					EntityName = "Asset",
					ActionType = "BulkDisposal",
					ChangeDate = DateTime.UtcNow,
					UserId = currentUser.Id,
					NewValues = $"DisposalType: {request.DisposalType}, SaleValue: {request.SaleValue}",
					EntityId = string.Join(",", request.AssetTags)
				});

				// Generate disposal document
				var pdfBytes = await GenerateDisposalDocumentAsync(assets, request, currentUser);

				// Return PDF file
				return File(pdfBytes, "application/pdf", $"Disposal_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = ex.Message });
			}
		}

		private async Task<byte[]> GenerateDisposalDocumentAsync(
		IEnumerable<Asset> assets,
		BulkOperationRequest request,
		User currentUser)
		{
			// تعيين ترخيص QuestPDF
			QuestPDF.Settings.License = LicenseType.Community;

			var department = assets.FirstOrDefault()?.Department;
			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					// إعدادات الصفحة
					page.Size(PageSizes.A4);
					page.Margin(20);
					page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

					// الرأس - ترويسة المستند
					page.Header().Row(row =>
					{
						// شعار الموقع
						var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
						if (System.IO.File.Exists(logoPath))
						{
							row.ConstantItem(100).Image(logoPath).FitWidth();
						}

// عنوان المستند - في المنتصف
						row.RelativeItem().Column(col =>
						{
							col.Item().Text("مستند تكهين أصول")
								.FontSize(24)
								.Bold()
								.FontColor(Colors.Red.Medium)
								.AlignRight();

							col.Item().PaddingTop(5).Text($"التاريخ: {DateTime.Now.ToString("yyyy/MM/dd")}")
								.FontSize(12)
								.AlignRight();

							col.Item().Text($" DISP-{DateTime.Now.ToString("yyyyMMdd")}-{Guid.NewGuid().ToString().Substring(0, 4)}:رقم المستند")
								.FontSize(12)
								.AlignRight();
						});

						// معلومات المستند - على اليمين (النص العربي من اليمين لليسار)
						row.RelativeItem().Column(col =>
						{
							col.Item().Text("المملكة العربية السعودية")
								.FontSize(16)
								.Bold()
								.FontColor(Colors.Black)
								.AlignRight();

							col.Item().Text("وزارة الصحة")
								.FontSize(14)
								.Bold()
								.FontColor(Colors.Black)
								.AlignRight();

							col.Item().Text(" ")
								.FontSize(14)
								.Bold()
								 
								.AlignRight();

							col.Item().Text($" {department?.Name ?? "غير محدد"}:القسم")
								.FontSize(12)
								.AlignRight();
						});

						
					});

					// المحتوى الرئيسي
					page.Content().PaddingVertical(10).Column(col =>
					{
						// تفاصيل التكهين
						col.Item().Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.ConstantColumn(150);
								columns.RelativeColumn();
							});

							// إضافة تفاصيل التكهين
							AddArabicTableRow(table, request.DisposalType, ":نوع التكهين");
							AddArabicTableRow(table, request.SaleValue?.ToString("N2") + " ريال" ?? "غير محدد" , ":القيمة المالية");
							AddArabicTableRow(table, currentUser.FullName ,":تم التكهين بواسطة" );
							AddArabicTableRow(table,DateTime.Now.ToString("yyyy/MM/dd HH:mm"),":تاريخ التكهين");
							 
						});

						// عنوان جدول الأصول
						col.Item().PaddingTop(15).Background(Colors.Grey.Lighten4).Padding(5)
							.Text("قائمة الأصول المكهنة")
							.FontSize(14)
							.Bold()
							.FontColor(Colors.Blue.Medium)
							.AlignCenter();

						// جدول الأصول
						col.Item().PaddingTop(5).Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.ConstantColumn(30);  // رقم
								columns.RelativeColumn(2);   // رمز الأصل
								columns.RelativeColumn(3);   // الوصف
								
								columns.RelativeColumn(2);   // الموقع
								
							});

							// ترويسة الجدول
							table.Header(header =>
							{
								header.Cell().Element(CellStyle).Text("#").AlignCenter();
								header.Cell().Element(CellStyle).Text("رمز الأصل").AlignCenter();
								header.Cell().Element(CellStyle).Text("الوصف").AlignCenter();
								 
								header.Cell().Element(CellStyle).Text("الموقع").AlignCenter();
							 
							});

							// صفوف الأصول
							int index = 1;
							foreach (var asset in assets)
							{
								table.Cell().Element(CellStyle).Text(index.ToString()).AlignCenter();
								table.Cell().Element(CellStyle).Text(asset.AssetTag).AlignCenter();
								table.Cell().Element(CellStyle).Text(asset.AssetDescription ?? "بدون وصف").AlignCenter();
								 
								table.Cell().Element(CellStyle).Text(GetArabicAssetLocation(asset)).AlignCenter();
								 

								index++;
							}

							// إجماليات
							table.Footer(footer => {
								footer.Cell().Element(FooterCellStyle).Text(assets.Count().ToString()).Bold().AlignCenter().FontSize(12);
								footer.Cell().ColumnSpan(3).Element(FooterCellStyle).Text("عدد الأصول المكهنة").AlignRight().FontSize(12);
							});
						});

						// تفاصيل الموافقة
						col.Item().PaddingTop(15).Background(Colors.Grey.Lighten4).Padding(5)
							.Text("موافقات التكهين")
							.FontSize(14)
							.Bold()
							.FontColor(Colors.Blue.Medium)
							.AlignCenter();

						// قسم التوقيعات
						col.Item().PaddingTop(10).Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn();
								columns.RelativeColumn();
								columns.RelativeColumn();
							});

							// ترويسة التوقيعات
							table.Cell().Padding(5).Border(1).BorderColor(Colors.Grey.Medium)
								.Text("الموظف المسؤول عن التكهين").AlignCenter().Bold();

							table.Cell().Padding(5).Border(1).BorderColor(Colors.Grey.Medium)
								.Text("اسم التجمع الصحي").AlignCenter().Bold();

							table.Cell().Padding(5).Border(1).BorderColor(Colors.Grey.Medium)
								.Text("اسم المرفق الصحي").AlignCenter().Bold();

							// مكان التوقيع
							table.Cell().Padding(20).Border(1).BorderColor(Colors.Grey.Medium).Text("").AlignCenter();
							table.Cell().Padding(20).Border(1).BorderColor(Colors.Grey.Medium).Text("").AlignCenter();
							table.Cell().Padding(20).Border(1).BorderColor(Colors.Grey.Medium).Text("").AlignCenter();

							// الأسماء
							table.Cell().Padding(5).Border(1).BorderColor(Colors.Grey.Medium)
								.Text(currentUser.FullName).AlignCenter();

							table.Cell().Padding(5).Border(1).BorderColor(Colors.Grey.Medium)
								.Text("------------").AlignCenter();

							table.Cell().Padding(5).Border(1).BorderColor(Colors.Grey.Medium)
								.Text("------------").AlignCenter();
						});

						// الختم الرسمي
						col.Item().PaddingTop(30).AlignRight().Width(200).Height(100).Placeholder("الختم الرسمي");

						
					});

					// التذييل
					page.Footer()
						.AlignCenter()
						.Text(x =>
						{
							x.Span("تم إنشاء هذا المستند بواسطة نظام إدارة الأصول - ").FontSize(9);
							x.Span(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")).FontSize(9);
							x.Span(" - صفحة ").FontSize(9);
							x.CurrentPageNumber().FontSize(9);
							x.Span(" من ").FontSize(9);
							x.TotalPages().FontSize(9);
						});
				});
			});

			// إنشاء ملف PDF
			using var stream = new MemoryStream();
			document.GeneratePdf(stream);
			return stream.ToArray();
		}

		private void AddArabicTableRow(TableDescriptor table, string label, string value)
		{
			table.Cell().Padding(5).Text(label).Bold().AlignRight();
			table.Cell().Padding(5).Text(value).Bold().AlignRight();
		}

		private string GetArabicAssetLocation(Asset asset)
		{
			var locations = new[]
			{
		asset.Facility?.Name,
		asset.Building?.Name,
		asset.Floor?.Name,
		asset.Room?.Name
	};

			return string.Join(" / ", locations.Where(l => !string.IsNullOrEmpty(l)));
		}

		private IContainer CellStyle(IContainer container)
		{
			return container
				.Border(1)
				.BorderColor(Colors.Grey.Medium)
				.Background(Colors.Grey.Lighten3)
				.Padding(5);
		}

		private IContainer FooterCellStyle(IContainer container)
		{
			return container
				.Border(1)
				.BorderColor(Colors.Grey.Medium)
				.Background(Colors.Blue.Lighten4)
				.Padding(5);
		}

		private void AddTableRow(TableDescriptor table, string label, string value)
		{
			table.Cell().Padding(5).Text(label).Bold();
			table.Cell().Padding(5).Text(value);
		}

		private string GetAssetLocation(Asset asset)
		{
			var locations = new[]
			{
		asset.Facility?.Name,
		asset.Building?.Name,
		asset.Floor?.Name,
		asset.Room?.Name
	};

			return string.Join(" / ", locations.Where(l => !string.IsNullOrEmpty(l)));
		}

		
		[HttpGet]
		public async Task<IActionResult> GetUsers()
		{
			try
			{
				// Use the asset service to get users
				var users = await _assetService.GetAllUsersAsync();

				// Transform the users into a format suitable for the dropdown
				var userList = users
					.Where(u => u != null) // Filter out any null users
					.Select(u => new
					{
						id = u.Id,
						name = BuildUserDisplayName(u)
					})
					.OrderBy(u => u.name)
					.ToList();

				return Json(new
				{
					success = true,
					data = userList,
					count = userList.Count
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error loading users: {ex.Message}");
				return StatusCode(500, new
				{
					success = false,
					error = "Failed to load users",
					details = ex.Message
				});
			}
		}
		


		[HttpGet("GetFacilities")]
		public async Task<JsonResult> GetFacilities()
		{
			try
			{
				var facilities = await _assetService.GetAllFacilitiesAsync();
				var result = facilities.Select(f => new { id = f.Id, name = f.Name }).ToList();
				return Json(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving all facilities");
				return Json(new { error = ex.Message });
			}
		}
		// Helper method to build a consistent user display name
		private string BuildUserDisplayName(User user)
		{
			var name = !string.IsNullOrEmpty(user.FullName) ? user.FullName : "No Name";
			var email = !string.IsNullOrEmpty(user.Email) ? user.Email : "No Email";
			//var department = user.Department?.Name ?? "No Department";

			return $"{name} ({email})";
		}
		[HttpGet]
		[Route("Asset/GetBuildingsByFacilityAsync/{facilityId}")]
		public async Task<JsonResult> GetBuildingsByFacilityAsync(int facilityId)
		{
			try
			{
				var buildings = await _assetService.GetBuildingsByFacilityAsync(facilityId);
				var result = buildings.Select(b => new { id = b.Id, name = b.Name }).ToList();
				return Json(result);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetBuildingsByFacilityAsync: {ex.Message}");
				return Json(new List<object>());
			}
		}

		// GET: /Asset/GetDepartmentsByFacilityAsync/{id}
		[HttpGet]
		[Route("Asset/GetDepartmentsByFacilityAsync/{facilityId}")]
		public async Task<JsonResult> GetDepartmentsByFacilityAsync(int facilityId)
		{
			try
			{
				var departments = await _facilityService.GetDepartmentsAsync(facilityId);
				var result = departments.Select(d => new { id = d.Id, name = d.Name }).ToList();
				return Json(result);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetDepartmentsByFacilityAsync: {ex.Message}");
				return Json(new List<object>());
			}
		}

		// GET: /Asset/GetFloorsByBuildingAsync/{id}
		[HttpGet]
		[Route("Asset/GetFloorsByBuildingAsync/{buildingId}")]
		public async Task<JsonResult> GetFloorsByBuildingAsync(int buildingId)
		{
			try
			{
				var floors = await _assetService.GetFloorsByBuildingAsync(buildingId);
				var result = floors.Select(f => new { id = f.Id, name = f.Name }).ToList();
				return Json(result);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetFloorsByBuildingAsync: {ex.Message}");
				return Json(new List<object>());
			}
		}

		// GET: /Asset/GetRoomsByFloorAsync/{id}
		[HttpGet]
		[Route("Asset/GetRoomsByFloorAsync/{floorId}")]
		public async Task<JsonResult> GetRoomsByFloorAsync(int floorId)
		{
			try
			{
				var rooms = await _assetService.GetRoomsByFloorAsync(floorId);
				var result = rooms.Select(r => new { roomTag = r.RoomTag, name = r.Name }).ToList();
				return Json(result);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetRoomsByFloorAsync: {ex.Message}");
				return Json(new List<object>());
			}
		}
		[HttpPost]
		public async Task<IActionResult> FilteredAssets(List<string> assetTags)
		{
			if (assetTags == null || !assetTags.Any())
			{
				return RedirectToAction("Index");
			}

			// Remove duplicates from the asset tags
			assetTags = assetTags.Distinct().ToList();

			// Store the deduplicated filter in ViewBag
			ViewBag.FilteredTags = assetTags;

			// Get filtered assets
			var assets = await _assetService.GetAllAssetsAsync();
			var filteredAssets = assets.Where(a => assetTags.Contains(a.AssetTag)).ToList();

			// Return the dedicated view with the filtered assets
			return View(filteredAssets);
		}
		[HttpPost]
		public async Task<IActionResult> FilterByTags([FromBody] List<string> assetTags)
		{
			if (assetTags == null || !assetTags.Any())
			{
				return BadRequest(new { success = false, error = "No asset tags provided" });
			}

			try
			{
				// Store the filter in session
				HttpContext.Session.SetString("AssetTagsFilter",
					System.Text.Json.JsonSerializer.Serialize(assetTags));

				// Count the matching assets
				var assets = await _assetService.GetAllAssetsAsync();
				var filteredCount = assets.Count(a => assetTags.Contains(a.AssetTag));

				return Ok(new
				{
					success = true,
					count = filteredCount,
					message = $"Filter applied with {filteredCount} matching assets"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error filtering assets by tags");
				return StatusCode(500, new { success = false, error = "Internal server error" });
			}
		}

		[HttpPost]
		public IActionResult ClearFilter()
		{
			try
			{
				// Remove the filter from session
				HttpContext.Session.Remove("AssetTagsFilter");
				return Ok(new { success = true });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error clearing asset filter");
				return StatusCode(500, new { success = false, error = "Internal server error" });
			}
		}
		[HttpPost]
		public async Task<IActionResult> GetFilteredAssets([FromBody] List<string> assetTags)
		{
			if (assetTags == null || !assetTags.Any())
			{
				return BadRequest(new { success = false, error = "No asset tags provided" });
			}

			try
			{
				// Remove duplicates
				assetTags = assetTags.Distinct().ToList();

				// Get filtered assets
				var assets = await _assetService.GetAllAssetsAsync();
				var filteredAssets = assets.Where(a => assetTags.Contains(a.AssetTag)).ToList();

				// Format the response data
				var responseData = filteredAssets.Select(a => new {
					assetTag = a.AssetTag,
					assetDescription = a.AssetDescription,
					department = a.Department != null ? new { name = a.Department.Name } : null,
					brand = a.Brand,
					model = a.Model,
					status = a.Status,
					isDisposed = a.IsDisposed,
					serialNumber = a.SerialNumber
				}).ToList();

				return Json(new
				{
					success = true,
					data = responseData,
					message = $"Found {filteredAssets.Count} matching assets"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting filtered assets");
				return StatusCode(500, new { success = false, error = "Internal server error" });
			}
		}


		[HttpPost]
		public async Task<IActionResult> CreateReturnDocument(List<string> assetTags)
		{
			if (assetTags == null || !assetTags.Any())
			{
				return BadRequest("No assets selected");
			}

			// Store selected asset tags in TempData to pass to the Return Document creation page
			TempData["SelectedAssetTags"] = System.Text.Json.JsonSerializer.Serialize(assetTags);

			// Redirect to the Return Document creation page
			return RedirectToAction("Create", "ReturnDocument");
		}

		[HttpGet]
		public async Task<IActionResult> GetStats()
		{
			try
			{
				var assets = await _assetService.GetAllAssetsAsync();

				var stats = new
				{
					success = true,
					totalCount = assets.Count(),
					activeCount = assets.Count(a => a.Status == "Available" || a.Status == "متاح"),
					maintenanceCount = assets.Count(a => a.Status == "صيانة"),
					disposedCount = assets.Count(a => a.IsDisposed || a.Status == "مكهن")
				};

				return Json(stats);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		private async Task<SelectList> GetSupervisorsForDropdownAsync()
		{
			var users = await _userManager.GetUsersInRoleAsync(Roles.Supervisor);
			return new SelectList(users, "Id", "FullName");
		}
		[HttpPost]
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
		public async Task<IActionResult> BulkAssignSupervisor([FromBody] BulkSupervisorAssignRequest request)
		{
			try
			{
				if (request?.AssetTags == null || !request.AssetTags.Any() || string.IsNullOrEmpty(request.SupervisorId))
				{
					return BadRequest(new { error = "Invalid request parameters" });
				}

				// Verify supervisor exists and has Supervisor role
				var supervisor = await _userManager.FindByIdAsync(request.SupervisorId);
				if (supervisor == null)
				{
					return BadRequest(new { error = "Selected supervisor not found" });
				}

				if (!await _userManager.IsInRoleAsync(supervisor, Roles.Supervisor))
				{
					return BadRequest(new { error = "Selected user is not a supervisor" });
				}

				// Assign supervisor to each asset
				foreach (var assetTag in request.AssetTags)
				{
					await _assetService.AssignSupervisorToAssetAsync(assetTag, request.SupervisorId);
				}

				// Get current user for notifications
				var currentUser = await GetCurrentUserAsync();

				// Notify the supervisor
				await CreateNotificationForUser(
					request.SupervisorId,
					"Bulk Supervision Assignment",
					$"You have been assigned as supervisor for {request.AssetTags.Count} assets",
					"BulkAssetSupervision",
					null,
					Url.Action("SupervisedAssets", "Asset"));

				return Ok(new
				{
					success = true,
					message = $"Successfully assigned supervisor to {request.AssetTags.Count} assets"
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = ex.Message });
			}
		}
		[HttpGet]
		public async Task<IActionResult> GetSupervisors()
		{
			try
			{
				
				var supervisors = await _userManager.GetUsersInRoleAsync(Roles.Supervisor);

				var result = supervisors.Select(s => new
				{
					id = s.Id,
					name = s.FullName
				}).ToList();

				return Json(new { success = true, data = result });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "خطأ في الحصول على المشرفين");
				return Json(new { success = false, error = "حدث خطأ في استرجاع قائمة المشرفين" });
			}
		}
		[HttpGet]
		public async Task<IActionResult> GetUsersByDepartment(int departmentId)
		{
			try
			{
				var users = await _unitOfWork.user.GetUsersByDepartmentAsync(departmentId);

				var result = users.Select(u => new {
					id = u.Id,
					name = BuildUserDisplayName(u)
				}).ToList();

				return Json(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error loading users by department");
				return StatusCode(500, new { error = "Failed to load users" });
			}
		}
		[HttpPost]
		public async Task<IActionResult> VerifyAssetTags([FromBody] List<string> assetTags)
		{
			try
			{
				if (assetTags == null || !assetTags.Any())
				{
					return Json(new { success = false, error = "No asset tags provided" });
				}

				// Get all assets from the database
				var allAssets = await _assetService.GetAllAssetsAsync();

				// Filter to find matching assets
				var foundAssets = allAssets.Where(a => assetTags.Contains(a.AssetTag)).ToList();

				// Get the list of found and not found tags
				var foundTags = foundAssets.Select(a => a.AssetTag).ToList();
				var notFoundTags = assetTags.Except(foundTags, StringComparer.OrdinalIgnoreCase).ToList();

				return Json(new
				{
					success = true,
					foundTags = foundTags,
					notFoundTags = notFoundTags,
					totalFound = foundTags.Count,
					totalNotFound = notFoundTags.Count
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error verifying asset tags");
				return Json(new { success = false, error = "An error occurred while verifying asset tags" });
			}
		}
		[HttpGet]
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.DataEntry}")]
		public IActionResult DownloadTemplate()
		{
			// Set EPPlus license
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add("Assets Template");

				// Define column headers with names instead of IDs for location entities
				string[] headers = {
			"Asset Tag", "Cluster", "Facility", "Building", "Floor",
			"Room", "Department", "Asset Description", "Brand", "Model",
			"Asset Type", "Status", "Is Disposed", "Serial Number",
			"اسم المستلم", "Details", "Asset Class 1", "Asset Class 2", "Asset Class 3"
		};

				// Set headers with styling
				for (int i = 0; i < headers.Length; i++)
				{
					var cell = worksheet.Cells[1, i + 1];
					cell.Value = headers[i];
					cell.Style.Font.Bold = true;
					cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
					cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
					cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				}

				worksheet.Cells[2, 1].Value = "AST001"; // Example Asset Tag
				worksheet.Cells[2, 2].Value = "ClusterA"; // Example Cluster
				worksheet.Cells[2, 3].Value = "المستشفى الرئيسي"; // Example Facility (Arabic)
				worksheet.Cells[2, 4].Value = "المبنى الإداري"; // Example Building (Arabic)
				worksheet.Cells[2, 5].Value = "الطابق الأول"; // Example Floor (Arabic)
				worksheet.Cells[2, 6].Value = "غرفة 101"; // Example Room (Arabic)
				worksheet.Cells[2, 7].Value = "قسم تقنية المعلومات"; // Example Department (Arabic)
				worksheet.Cells[2, 8].Value = "جهاز كمبيوتر"; // Example Asset Description (Arabic)
				worksheet.Cells[2, 9].Value = "ديل"; // Example Brand (Arabic)
				worksheet.Cells[2, 10].Value = "لاتيتيود 7400"; // Example Model
				worksheet.Cells[2, 11].Value = "أجهزة"; // Example Asset Type (Arabic)
				worksheet.Cells[2, 12].Value = "متاح"; // Example Status (Arabic)
				worksheet.Cells[2, 13].Value = "FALSE"; // Example Is Disposed
				worksheet.Cells[2, 14].Value = "SN12345"; // Example Serial Number
				worksheet.Cells[2, 15].Value = "محمد أحمد"; // اسم المستلم example
				worksheet.Cells[2, 16].Value = "تفاصيل الأصل"; // Example Details (Arabic)

				// Add info row under example explaining this is sample data
				worksheet.Cells[3, 1].Value = "ملاحظة:";
				worksheet.Cells[3, 2].Value = "البيانات أعلاه هي مجرد أمثلة. يرجى استبدالها بمعلوماتك الفعلية.";
				worksheet.Cells[3, 2, 3, 10].Merge = true;
				worksheet.Cells[3, 2].Style.Font.Bold = true;
				worksheet.Cells[3, 2].Style.Font.Color.SetColor(System.Drawing.Color.Red);

				// Add important notes
				worksheet.Cells[5, 1].Value = "ملاحظات هامة:";
				worksheet.Cells[5, 1].Style.Font.Bold = true;

				worksheet.Cells[6, 1].Value = "1.";
				worksheet.Cells[6, 2].Value = "استخدم أسماء المنشآت والمباني والطوابق والغرف والأقسام بدلاً من معرفاتها.";
				worksheet.Cells[6, 2, 6, 10].Merge = true;

				worksheet.Cells[7, 1].Value = "2.";
				worksheet.Cells[7, 2].Value = "يجب أن تكون أسماء المنشآت والأقسام موجودة بالفعل في النظام.";
				worksheet.Cells[7, 2, 7, 10].Merge = true;

				worksheet.Cells[8, 1].Value = "3.";
				worksheet.Cells[8, 2].Value = "خانة Asset Tag مطلوبة ويجب أن تكون فريدة لكل أصل.";
				worksheet.Cells[8, 2, 8, 10].Merge = true;

				worksheet.Cells[9, 1].Value = "4.";
				worksheet.Cells[9, 2].Value = "اسم المستلم يجب أن يكون موجود في النظام.";
				worksheet.Cells[9, 2, 9, 10].Merge = true;

				// Add helper data tabs - create lists of valid Facilities, Departments, etc.
				try
				{
					// Add Facilities reference sheet
					var facilitiesSheet = package.Workbook.Worksheets.Add("Facilities");
					facilitiesSheet.Cells[1, 1].Value = "Facility Name";
					facilitiesSheet.Cells[1, 1].Style.Font.Bold = true;
					facilitiesSheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
					facilitiesSheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

					// Add Departments reference sheet
					var departmentsSheet = package.Workbook.Worksheets.Add("Departments");
					departmentsSheet.Cells[1, 1].Value = "Department Name";
					departmentsSheet.Cells[1, 2].Value = "Facility";
					departmentsSheet.Cells[1, 1].Style.Font.Bold = true;
					departmentsSheet.Cells[1, 2].Style.Font.Bold = true;
					departmentsSheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
					departmentsSheet.Cells[1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
					departmentsSheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
					departmentsSheet.Cells[1, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

					// Add valid statuses and asset types
					var referencesSheet = package.Workbook.Worksheets.Add("References");
					referencesSheet.Cells[1, 1].Value = "Valid Status";
					referencesSheet.Cells[1, 2].Value = "Valid Asset Types";
					referencesSheet.Cells[1, 1].Style.Font.Bold = true;
					referencesSheet.Cells[1, 2].Style.Font.Bold = true;
					referencesSheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
					referencesSheet.Cells[1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
					referencesSheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
					referencesSheet.Cells[1, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

					string[] statuses = { "متاح", "قيد الاستخدام", "صيانة", "مكهن" };
					for (int i = 0; i < statuses.Length; i++)
					{
						referencesSheet.Cells[i + 2, 1].Value = statuses[i];
					}

					string[] assetTypes = { "أجهزة", "برمجيات", "أثاث", "معدات", "مركبات", "مباني" };
					for (int i = 0; i < assetTypes.Length; i++)
					{
						referencesSheet.Cells[i + 2, 2].Value = assetTypes[i];
					}
				}
				catch (Exception ex)
				{
					// Handle error, but don't stop the template download
					_logger.LogError(ex, "Error adding reference sheets to template");
				}

				// Apply column formatting
				worksheet.Column(1).Width = 15; // Asset Tag
				worksheet.Column(8).Width = 30; // Asset Description
				worksheet.Column(12).Width = 15; // Status
				worksheet.Column(15).Width = 20; // اسم المستلم
				worksheet.Column(16).Width = 30; // Details


				// Auto-size remaining columns
				for (int col = 1; col <= headers.Length; col++)
				{
					if (col != 1 && col != 8 && col != 13 && col != 17)
						worksheet.Column(col).AutoFit();
				}

				// Freeze the header row
				worksheet.View.FreezePanes(2, 1);

				// Prepare the stream
				var stream = new MemoryStream();
				package.SaveAs(stream);
				stream.Position = 0;

				// Return the Excel file
				string excelName = $"Assets_Import_Template_{DateTime.Now:yyyyMMdd}.xlsx";
				return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
			}
		}
	}
public class BulkSupervisorAssignRequest
		{
			public List<string> AssetTags { get; set; }
			public string SupervisorId { get; set; }
		}
	public class BulkOperationRequest
	{
        public List<string> AssetTags { get; set; }
		public int? TargetDepartmentId { get; set; }
		public string? TargetUserId { get; set; }
		public string? DisposalType { get; set; }
		public decimal? SaleValue { get; set; }
	}

}
