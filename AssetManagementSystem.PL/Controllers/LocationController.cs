using AssetManagementSystem.BLL.Interfaces;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.DAL.Utilities;
using AssetManagementSystem.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace AssetManagementSystem.PL.Controllers
{
	[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
	public class LocationController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IFacilityService _facilityService;

		public LocationController(IUnitOfWork unitOfWork, IFacilityService facilityService)
		{
			_unitOfWork = unitOfWork;
			_facilityService = facilityService;
		}

		// Main view that shows all locations in a hierarchical structure
		public async Task<IActionResult> Index()
		{
			var facilities = await _facilityService.GetAllAsync();
			return View(facilities);
		}

		#region Facility Operations

		[HttpGet]
		public IActionResult CreateFacility()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateFacility(Facility facility)
		{
			if (ModelState.IsValid)
			{
				await _unitOfWork.FacilityRepository.AddAsync(facility);
				return RedirectToAction(nameof(Index));
			}
			return View(facility);
		}

		[HttpGet]
		public async Task<IActionResult> EditFacility(int id)
		{
			var facility = await _unitOfWork.FacilityRepository.GetByIdAsync(id);
			if (facility == null)
				return NotFound();

			return View(facility);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditFacility(Facility facility)
		{
			if (ModelState.IsValid)
			{
				await _unitOfWork.FacilityRepository.UpdateAsync(facility);
				return RedirectToAction(nameof(Index));
			}
			return View(facility);
		}

		[HttpGet]
		public async Task<IActionResult> DeleteFacility(int id)
		{
			var facility = await _unitOfWork.FacilityRepository.GetByIdAsync(id);
			if (facility == null)
				return NotFound();

			return View(facility);
		}

		[HttpPost, ActionName("DeleteFacility")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteFacilityConfirmed(int id)
		{
			await _unitOfWork.FacilityRepository.DeleteAsync(id);
			return RedirectToAction(nameof(Index));
		}
		#endregion

		#region Building Operations

		[HttpGet]
		public async Task<IActionResult> CreateBuilding(int facilityId)
		{
			ViewBag.FacilityId = facilityId;
			var facility = await _unitOfWork.FacilityRepository.GetByIdAsync(facilityId);
			ViewBag.FacilityName = facility?.Name;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateBuilding(Building building)
		{
			ModelState.Remove("Facility");
			if (ModelState.IsValid)
			{
				await _unitOfWork.buildingRepository.AddAsync(building);
				return RedirectToAction(nameof(Index));
			}

			var facility = await _unitOfWork.FacilityRepository.GetByIdAsync(building.FacilityId);
			ViewBag.FacilityName = facility?.Name;
			return View(building);
		}

		[HttpGet]
		public async Task<IActionResult> EditBuilding(int id)
		{
			var building = await _unitOfWork.buildingRepository.GetByIdAsync(id);
			if (building == null)
				return NotFound();

			var facility = await _unitOfWork.FacilityRepository.GetByIdAsync(building.FacilityId);
			ViewBag.FacilityName = facility?.Name;
			return View(building);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditBuilding(Building building)
		{
			ModelState.Remove("Facility");
			if (ModelState.IsValid)
			{
				await _unitOfWork.buildingRepository.UpdateAsync(building);
				return RedirectToAction(nameof(Index));
			}

			var facility = await _unitOfWork.FacilityRepository.GetByIdAsync(building.FacilityId);
			ViewBag.FacilityName = facility?.Name;
			return View(building);
		}

		[HttpGet]
		public async Task<IActionResult> DeleteBuilding(int id)
		{
			var building = await _unitOfWork.buildingRepository.GetByIdAsync(id);
			if (building == null)
				return NotFound();

			return View(building);
		}

		[HttpPost, ActionName("DeleteBuilding")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteBuildingConfirmed(int id)
		{
			await _unitOfWork.buildingRepository.DeleteAsync(id);
			return RedirectToAction(nameof(Index));
		}
		#endregion

		#region Floor Operations

		[HttpGet]
		public async Task<IActionResult> CreateFloor(int buildingId)
		{
			ViewBag.BuildingId = buildingId;
			var building = await _unitOfWork.buildingRepository.GetByIdAsync(buildingId);
			ViewBag.BuildingName = building?.Name;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateFloor(Floor floor)
		{
			ModelState.Remove("Building");
			if (ModelState.IsValid)
			{
				await _unitOfWork.floorRepository.AddAsync(floor);
				return RedirectToAction(nameof(Index));
			}

			var building = await _unitOfWork.buildingRepository.GetByIdAsync(floor.BuildingId);
			ViewBag.BuildingName = building?.Name;
			return View(floor);
		}

		[HttpGet]
		public async Task<IActionResult> EditFloor(int id)
		{
			var floor = await _unitOfWork.floorRepository.GetByIdAsync(id);
			if (floor == null)
				return NotFound();

			var building = await _unitOfWork.buildingRepository.GetByIdAsync(floor.BuildingId);
			ViewBag.BuildingName = building?.Name;
			return View(floor);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditFloor(Floor floor)
		{
			ModelState.Remove("Building");
			if (ModelState.IsValid)
			{
				await _unitOfWork.floorRepository.UpdateAsync(floor);
				return RedirectToAction(nameof(Index));
			}

			var building = await _unitOfWork.buildingRepository.GetByIdAsync(floor.BuildingId);
			ViewBag.BuildingName = building?.Name;
			return View(floor);
		}

		[HttpGet]
		public async Task<IActionResult> DeleteFloor(int id)
		{
			var floor = await _unitOfWork.floorRepository.GetByIdAsync(id);
			if (floor == null)
				return NotFound();

			return View(floor);
		}

		[HttpPost, ActionName("DeleteFloor")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteFloorConfirmed(int id)
		{
			await _unitOfWork.floorRepository.DeleteAsync(id);
			return RedirectToAction(nameof(Index));
		}
		#endregion

		#region Room Operations

		[HttpGet]
		public async Task<IActionResult> CreateRoom(int floorId)
		{
			// Get the floor to display its name
			var floor = await _unitOfWork.floorRepository.GetByIdAsync(floorId);
			if (floor == null)
			{
				return NotFound();
			}

			ViewBag.FloorId = floorId;
			ViewBag.FloorName = floor.Name;

			// Get all departments for the dropdown
			var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();

			// If you can, filter departments by the facility of this floor
			var buildingId = floor.BuildingId;
			var building = await _unitOfWork.buildingRepository.GetByIdAsync(buildingId);
			if (building != null)
			{
				var facilityId = building.FacilityId;
				departments = departments.Where(d => d.FacilityId == facilityId);
			}

			ViewBag.Departments = departments;

			// Initialize with required properties
			return View(new Room
			{
				FloorId = floorId,
				Name = string.Empty // Initialize required Name property
			});
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateRoom(Room room)
		{
			// Remove navigation properties validation
			ModelState.Remove("Floor");
			ModelState.Remove("Department");

			if (ModelState.IsValid)
			{
				await _unitOfWork.RoomRepository.AddAsync(room);
				return RedirectToAction(nameof(Index));
			}

			// If validation fails, we need to repopulate the ViewBag data
			var floor = await _unitOfWork.floorRepository.GetByIdAsync(room.FloorId);
			ViewBag.FloorId = room.FloorId;
			ViewBag.FloorName = floor?.Name ?? "Unknown";

			var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
			if (floor != null)
			{
				var building = await _unitOfWork.buildingRepository.GetByIdAsync(floor.BuildingId);
				if (building != null)
				{
					var facilityId = building.FacilityId;
					departments = departments.Where(d => d.FacilityId == facilityId);
				}
			}

			ViewBag.Departments = departments;

			return View(room);
		}

		[HttpGet]
		public async Task<IActionResult> EditRoom(string id)
		{
			var room = await _unitOfWork.RoomRepository.GetByIdAsync(id);
			if (room == null)
				return NotFound();

			var floor = await _unitOfWork.floorRepository.GetByIdAsync(room.FloorId);
			ViewBag.FloorName = floor?.Name;

			var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
			ViewBag.Departments = departments;

			return View(room);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditRoom(Room room)
		{
			// Remove navigation properties validation
			ModelState.Remove("Floor");
			ModelState.Remove("Department");
			if (ModelState.IsValid)
			{
				await _unitOfWork.RoomRepository.UpdateAsync(room);
				return RedirectToAction(nameof(Index));
			}

			var floor = await _unitOfWork.floorRepository.GetByIdAsync(room.FloorId);
			ViewBag.FloorName = floor?.Name;

			var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
			ViewBag.Departments = departments;

			return View(room);
		}

		[HttpGet]
		public async Task<IActionResult> DeleteRoom(string id)
		{
			var room = await _unitOfWork.RoomRepository.GetByIdAsync(id);
			if (room == null)
				return NotFound();

			return View(room);
		}

		[HttpPost, ActionName("DeleteRoom")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteRoomConfirmed(string id)
		{
			
			var room = await _unitOfWork.RoomRepository.GetByIdAsync(id);
			if (room != null)
			{
				
			 await _unitOfWork.RoomRepository.DeleteAsync(id);
				return RedirectToAction(nameof(Index));
			}
			return NotFound();
		}
		#endregion

		// API methods for AJAX operations
		[HttpGet]
		public async Task<JsonResult> GetBuildingsForFacility(int facilityId)
		{
			var buildings = await _facilityService.GetBuildingsAsync(facilityId);
			return Json(buildings);
		}

		[HttpGet]
		public async Task<JsonResult> GetFloorsForBuilding(int buildingId)
		{
			var floors = await _unitOfWork.floorRepository.GetAllAsync();
			var buildingFloors = floors.Where(f => f.BuildingId == buildingId);
			return Json(buildingFloors);
		}

		[HttpGet]
		public async Task<JsonResult> GetRoomsForFloor(int floorId)
		{
			var rooms = await _unitOfWork.RoomRepository.GetAllAsync();
			var floorRooms = rooms.Where(r => r.FloorId == floorId);
			return Json(floorRooms);
		}

		[HttpGet]
		public async Task<IActionResult> GetFacilityDetails(int id)
		{
			var facility = await _facilityService.GetByIdAsync(id);
			if (facility == null)
				return NotFound();

			return PartialView("_FacilityDetails", facility);
		}

		[HttpGet]
		public async Task<IActionResult> GetBuildingDetails(int id)
		{
			var building = await _unitOfWork.buildingRepository.GetByIdAsync(id);
			if (building == null)
				return NotFound();

			return PartialView("_BuildingDetails", building);
		}

		[HttpGet]
		public async Task<IActionResult> GetFloorDetails(int id)
		{
			var floor = await _unitOfWork.floorRepository.GetByIdAsync(id);
			if (floor == null)
				return NotFound();

			return PartialView("_FloorDetails", floor);
		}
		[HttpGet]
		public async Task<JsonResult> GetDepartmentsByFacility(int facilityId)
		{
			var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
			var facilityDepartments = departments.Where(d => d.FacilityId == facilityId);
			return Json(facilityDepartments);
		}

		#region Department Operations

		[HttpGet]
		public async Task<IActionResult> Departments()
		{
			var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
			return View(departments);
		}

		[HttpGet]
		public async Task<IActionResult> CreateDepartment()
		{
			var facilities = await _unitOfWork.FacilityRepository.GetAllAsync();
			ViewBag.Facilities = facilities;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateDepartment(Department department)
		{
			ModelState.Remove("Facility");
			if (ModelState.IsValid)
			{
				await _unitOfWork.DepartmentRepository.AddAsync(department);
				return RedirectToAction(nameof(Departments));
			}

			var facilities = await _unitOfWork.FacilityRepository.GetAllAsync();
			ViewBag.Facilities = facilities;
			return View(department);
		}

		[HttpGet]
		public async Task<IActionResult> EditDepartment(int id)
		{
			var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(id);
			if (department == null)
				return NotFound();

			var facilities = await _unitOfWork.FacilityRepository.GetAllAsync();
			ViewBag.Facilities = facilities;
			return View(department);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditDepartment(Department department)
		{
			ModelState.Remove("Facility");
			if (ModelState.IsValid)
			{
				await _unitOfWork.DepartmentRepository.UpdateAsync(department);
				return RedirectToAction(nameof(Departments));
			}

			var facilities = await _unitOfWork.FacilityRepository.GetAllAsync();
			ViewBag.Facilities = facilities;
			return View(department);
		}

		[HttpGet]
		public async Task<IActionResult> DeleteDepartment(int id)
		{
			var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(id);
			if (department == null)
				return NotFound();

			return View(department);
		}

		[HttpPost, ActionName("DeleteDepartment")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteDepartmentConfirmed(int id)
		{
			await _unitOfWork.DepartmentRepository.DeleteAsync(id);
			return RedirectToAction(nameof(Departments));
		}

		[HttpGet]
		public async Task<JsonResult> GetDepartmentsForFacility(int facilityId)
		{
			var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
			var facilityDepartments = departments.Where(d => d.FacilityId == facilityId);
			return Json(facilityDepartments);
		}

		[HttpPost]
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
		public async Task<IActionResult> ImportLocations(IFormFile file, string locationType)
		{
			if (file == null || file.Length == 0)
			{
				TempData["Error"] = "Please upload a valid Excel file.";
				return RedirectToAction("Index");
			}

			if (string.IsNullOrEmpty(locationType))
			{
				TempData["Error"] = "Please select a location type to import.";
				return RedirectToAction("Index");
			}

			try
			{
				using (var stream = new MemoryStream())
				{
					file.CopyTo(stream);
					using (var package = new ExcelPackage(stream))
					{
						var worksheet = package.Workbook.Worksheets.FirstOrDefault();
						if (worksheet == null)
						{
							TempData["Error"] = "Invalid Excel file.";
							return RedirectToAction("Index");
						}

						int rowCount = worksheet.Dimension.Rows;
						int successCount = 0;
						int totalCount = rowCount - 1; // Exclude header row

						switch (locationType.ToLower())
						{
							case "facility":
								successCount = await ImportFacilities(worksheet, rowCount);
								break;
							case "department":
								successCount = await ImportDepartments(worksheet, rowCount);
								break;
							case "building":
								successCount = await ImportBuildings(worksheet, rowCount);
								break;
							case "floor":
								successCount = await ImportFloors(worksheet, rowCount);
								break;
							case "room":
								successCount = await ImportRooms(worksheet, rowCount);
								break;
							default:
								TempData["Error"] = "Invalid location type.";
								return RedirectToAction("Index");
						}

						if (successCount == totalCount)
						{
							TempData["Success"] = $"Successfully imported all {successCount} {locationType}(s)!";
						}
						else if (successCount > 0)
						{
							TempData["Warning"] = $"Imported {successCount} out of {totalCount} {locationType}(s). Some entries could not be imported due to missing or invalid references.";
						}
						else
						{
							TempData["Error"] = $"Failed to import any {locationType}s. Please check that referenced entities exist in the system.";
						}
					}
				}
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error importing {locationType}: {ex.Message}");
				TempData["Error"] = $"An error occurred while importing {locationType}s: {ex.Message}";
			}

			return RedirectToAction("Index");
		}

		private async Task<int> ImportFacilities(ExcelWorksheet worksheet, int rowCount)
		{
			int successCount = 0;
			var facilities = new List<Facility>();

			for (int row = 2; row <= rowCount; row++) // Skip header
			{
				var name = worksheet.Cells[row, 1].Value?.ToString()?.Trim();

				if (!string.IsNullOrEmpty(name))
				{
					facilities.Add(new Facility
					{
						Name = name
					});
					successCount++;
				}
			}

			if (facilities.Any())
			{
				await _unitOfWork.FacilityRepository.AddRangeAsync(facilities);
				await _unitOfWork.SaveChangesAsync();
			}

			return successCount;
		}

		private async Task<int> ImportBuildings(ExcelWorksheet worksheet, int rowCount)
		{
			int successCount = 0;
			var buildings = new List<Building>();
			var allFacilities = await _unitOfWork.FacilityRepository.GetAllAsync();
			var facilityDictionary = allFacilities.ToDictionary(f => f.Name.Trim(), f => f.Id, StringComparer.OrdinalIgnoreCase);

			for (int row = 2; row <= rowCount; row++) // Skip header
			{
				var buildingName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
				var facilityName = worksheet.Cells[row, 2].Value?.ToString()?.Trim();

				if (!string.IsNullOrEmpty(buildingName) && !string.IsNullOrEmpty(facilityName))
				{
					// Try to find facility by name
					if (facilityDictionary.TryGetValue(facilityName, out int facilityId))
					{
						buildings.Add(new Building
						{
							Name = buildingName,
							FacilityId = facilityId
						});
						successCount++;
					}
				}
			}

			if (buildings.Any())
			{
				await _unitOfWork.buildingRepository.AddRangeAsync(buildings);
				await _unitOfWork.SaveChangesAsync();
			}

			return successCount;
		}

		private async Task<int> ImportFloors(ExcelWorksheet worksheet, int rowCount)
		{
			int successCount = 0;
			var floors = new List<Floor>();

			// Load all facilities and buildings for lookup
			var allFacilities = await _unitOfWork.FacilityRepository.GetAllAsync();
			var allBuildings = await _unitOfWork.buildingRepository.GetAllAsync();

			// Create dictionaries for fast lookup
			var facilityDict = allFacilities.ToDictionary(f => f.Name.Trim(), f => f, StringComparer.OrdinalIgnoreCase);
			var buildingDict = allBuildings.ToDictionary(
				b => $"{b.Name.Trim()}|{facilityDict.Values.FirstOrDefault(f => f.Id == b.FacilityId)?.Name ?? "unknown"}",
				b => b,
				StringComparer.OrdinalIgnoreCase);

			for (int row = 2; row <= rowCount; row++) // Skip header
			{
				var floorName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
				var buildingName = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
				var facilityName = worksheet.Cells[row, 3].Value?.ToString()?.Trim();

				if (!string.IsNullOrEmpty(floorName) && !string.IsNullOrEmpty(buildingName) && !string.IsNullOrEmpty(facilityName))
				{
					// Look up the building using the combined key of building name and facility name
					var lookupKey = $"{buildingName}|{facilityName}";
					if (buildingDict.TryGetValue(lookupKey, out Building building))
					{
						floors.Add(new Floor
						{
							Name = floorName,
							BuildingId = building.Id
						});
						successCount++;
					}
				}
			}

			if (floors.Any())
			{
				await _unitOfWork.floorRepository.AddRangeAsync(floors);
				await _unitOfWork.SaveChangesAsync();
			}

			return successCount;
		}

		private async Task<int> ImportRooms(ExcelWorksheet worksheet, int rowCount)
		{
			int successCount = 0;
			var rooms = new List<Room>();

			// Load all necessary entities for lookup
			var allFacilities = await _unitOfWork.FacilityRepository.GetAllAsync();
			var allBuildings = await _unitOfWork.buildingRepository.GetAllAsync();
			var allFloors = await _unitOfWork.floorRepository.GetAllAsync();
			var allDepartments = await _unitOfWork.DepartmentRepository.GetAllAsync();

			// Create dictionaries for fast lookup
			var facilityDict = allFacilities.ToDictionary(f => f.Name.Trim(), f => f, StringComparer.OrdinalIgnoreCase);
			var departmentDict = allDepartments.ToDictionary(d => d.Name.Trim(), d => d, StringComparer.OrdinalIgnoreCase);

			// Create a composite key dictionary for buildings: BuildingName|FacilityName -> Building
			var buildingDict = allBuildings.ToDictionary(
				b => $"{b.Name.Trim()}|{facilityDict.Values.FirstOrDefault(f => f.Id == b.FacilityId)?.Name ?? "unknown"}",
				b => b,
				StringComparer.OrdinalIgnoreCase);

			// Create a composite key dictionary for floors: FloorName|BuildingName|FacilityName -> Floor
			var floorDict = allFloors.ToDictionary(
				f => {
					var building = allBuildings.FirstOrDefault(b => b.Id == f.BuildingId);
					var facility = building != null ? facilityDict.Values.FirstOrDefault(fac => fac.Id == building.FacilityId) : null;
					return $"{f.Name.Trim()}|{building?.Name ?? "unknown"}|{facility?.Name ?? "unknown"}";
				},
				f => f,
				StringComparer.OrdinalIgnoreCase);

			for (int row = 2; row <= rowCount; row++) // Skip header
			{
				var roomTag = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
				var roomName = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
				var floorName = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
				var buildingName = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
				var facilityName = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
				var departmentName = worksheet.Cells[row, 6].Value?.ToString()?.Trim();

				if (!string.IsNullOrEmpty(roomTag) && !string.IsNullOrEmpty(roomName) &&
					!string.IsNullOrEmpty(floorName) && !string.IsNullOrEmpty(buildingName) &&
					!string.IsNullOrEmpty(facilityName) && !string.IsNullOrEmpty(departmentName))
				{
					// Look up department
					if (!departmentDict.TryGetValue(departmentName, out Department department))
						continue;

					// Look up floor using composite key
					var floorKey = $"{floorName}|{buildingName}|{facilityName}";
					if (!floorDict.TryGetValue(floorKey, out Floor floor))
						continue;

					rooms.Add(new Room
					{
						RoomTag = roomTag,
						Name = roomName,
						FloorId = floor.Id,
						DepartmentId = department.Id
					});
					successCount++;
				}
			}

			if (rooms.Any())
			{
				await _unitOfWork.RoomRepository.AddRangeAsync(rooms);
				await _unitOfWork.SaveChangesAsync();
			}

			return successCount;
		}
		[HttpGet]
		[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
		public ActionResult DownloadLocationTemplate(string locationType)
		{
			try
			{
				// Create a new Excel package
				using (var package = new ExcelPackage())
				{
					ExcelWorksheet worksheet;

					switch (locationType.ToLower())
					{
						case "facility":
							worksheet = CreateFacilityTemplate(package);
							break;
						case "department":
							worksheet = CreateDepartmentTemplate(package);
							break;
						case "building":
							worksheet = CreateBuildingTemplate(package);
							break;
						case "floor":
							worksheet = CreateFloorTemplate(package);
							break;
						case "room":
							worksheet = CreateRoomTemplate(package);
							break;
						default:
							return BadRequest("Invalid location type.");
					}

					// Auto-fit columns
					worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

					// Prepare the response for download
					var stream = new MemoryStream();
					package.SaveAs(stream);
					stream.Position = 0; // Reset stream position

					// Generate a filename
					string fileName = $"{locationType}Template_{DateTime.Now:yyyyMMdd}.xlsx";

					// Return the Excel file
					return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
				}
			}
			catch (Exception ex)
			{
				// Log the exception
				Console.WriteLine($"Error creating template: {ex.Message}");
				return StatusCode(500, "Error creating template file.");
			}
		}

		private ExcelWorksheet CreateFacilityTemplate(ExcelPackage package)
		{
			var worksheet = package.Workbook.Worksheets.Add("Facilities");

			// Add headers
			worksheet.Cells[1, 1].Value = "Name";

			// Style headers
			using (var range = worksheet.Cells[1, 1, 1, 1])
			{
				range.Style.Font.Bold = true;
				range.Style.Fill.PatternType = ExcelFillStyle.Solid;
				range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
			}

			// Add sample data
			worksheet.Cells[2, 1].Value = "Sample Facility";

			return worksheet;
		}

		private ExcelWorksheet CreateBuildingTemplate(ExcelPackage package)
		{
			var worksheet = package.Workbook.Worksheets.Add("Buildings");

			// Add headers
			worksheet.Cells[1, 1].Value = "Name";
			worksheet.Cells[1, 2].Value = "FacilityName";

			// Style headers
			using (var range = worksheet.Cells[1, 1, 1, 2])
			{
				range.Style.Font.Bold = true;
				range.Style.Fill.PatternType = ExcelFillStyle.Solid;
				range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
			}

			// Add sample data
			worksheet.Cells[2, 1].Value = "Sample Building";
			worksheet.Cells[2, 2].Value = "Sample Facility";

			// Add note about FacilityName
			worksheet.Cells[4, 1].Value = "Note: FacilityName must correspond to an existing Facility in the system.";
			using (var range = worksheet.Cells[4, 1, 4, 2])
			{
				range.Merge = true;
				range.Style.Font.Italic = true;
				range.Style.Font.Color.SetColor(System.Drawing.Color.Red);
			}

			return worksheet;
		}

		private ExcelWorksheet CreateFloorTemplate(ExcelPackage package)
		{
			var worksheet = package.Workbook.Worksheets.Add("Floors");

			// Add headers
			worksheet.Cells[1, 1].Value = "Name";
			worksheet.Cells[1, 2].Value = "BuildingName";
			worksheet.Cells[1, 3].Value = "FacilityName";

			// Style headers
			using (var range = worksheet.Cells[1, 1, 1, 3])
			{
				range.Style.Font.Bold = true;
				range.Style.Fill.PatternType = ExcelFillStyle.Solid;
				range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
			}

			// Add sample data
			worksheet.Cells[2, 1].Value = "First Floor";
			worksheet.Cells[2, 2].Value = "Sample Building";
			worksheet.Cells[2, 3].Value = "Sample Facility";

			// Add note about Building and Facility
			worksheet.Cells[4, 1].Value = "Note: BuildingName and FacilityName must correspond to existing entities in the system.";
			using (var range = worksheet.Cells[4, 1, 4, 3])
			{
				range.Merge = true;
				range.Style.Font.Italic = true;
				range.Style.Font.Color.SetColor(System.Drawing.Color.Red);
			}

			return worksheet;
		}

		private ExcelWorksheet CreateRoomTemplate(ExcelPackage package)
		{
			var worksheet = package.Workbook.Worksheets.Add("Rooms");

			// Add headers
			worksheet.Cells[1, 1].Value = "RoomTag";
			worksheet.Cells[1, 2].Value = "Name";
			worksheet.Cells[1, 3].Value = "FloorName";
			worksheet.Cells[1, 4].Value = "BuildingName";
			worksheet.Cells[1, 5].Value = "FacilityName";
			worksheet.Cells[1, 6].Value = "DepartmentName";

			// Style headers
			using (var range = worksheet.Cells[1, 1, 1, 6])
			{
				range.Style.Font.Bold = true;
				range.Style.Fill.PatternType = ExcelFillStyle.Solid;
				range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
			}

			// Add sample data
			worksheet.Cells[2, 1].Value = "R101";
			worksheet.Cells[2, 2].Value = "Conference Room";
			worksheet.Cells[2, 3].Value = "First Floor";
			worksheet.Cells[2, 4].Value = "Sample Building";
			worksheet.Cells[2, 5].Value = "Sample Facility";
			worksheet.Cells[2, 6].Value = "IT Department";

			// Add notes
			worksheet.Cells[4, 1].Value = "Note: All names must correspond to existing entities in the system.";
			using (var range = worksheet.Cells[4, 1, 4, 6])
			{
				range.Merge = true;
				range.Style.Font.Italic = true;
				range.Style.Font.Color.SetColor(System.Drawing.Color.Red);
			}

			return worksheet;
		}
		private ExcelWorksheet CreateDepartmentTemplate(ExcelPackage package)
		{
			var worksheet = package.Workbook.Worksheets.Add("Departments");

			// Add headers
			worksheet.Cells[1, 1].Value = "Name";
			worksheet.Cells[1, 2].Value = "FacilityName";

			// Style headers
			using (var range = worksheet.Cells[1, 1, 1, 2])
			{
				range.Style.Font.Bold = true;
				range.Style.Fill.PatternType = ExcelFillStyle.Solid;
				range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
			}

			// Add sample data
			worksheet.Cells[2, 1].Value = "IT Department";
			worksheet.Cells[2, 2].Value = "Sample Facility";

			// Add note about FacilityName
			worksheet.Cells[4, 1].Value = "Note: FacilityName must correspond to an existing Facility in the system.";
			using (var range = worksheet.Cells[4, 1, 4, 2])
			{
				range.Merge = true;
				range.Style.Font.Italic = true;
				range.Style.Font.Color.SetColor(System.Drawing.Color.Red);
			}

			return worksheet;
		}
		private async Task<int> ImportDepartments(ExcelWorksheet worksheet, int rowCount)
		{
			int successCount = 0;
			var departments = new List<Department>();
			var allFacilities = await _unitOfWork.FacilityRepository.GetAllAsync();
			var facilityDictionary = allFacilities.ToDictionary(f => f.Name.Trim(), f => f.Id, StringComparer.OrdinalIgnoreCase);

			for (int row = 2; row <= rowCount; row++) // Skip header
			{
				var departmentName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
				var facilityName = worksheet.Cells[row, 2].Value?.ToString()?.Trim();

				if (!string.IsNullOrEmpty(departmentName) && !string.IsNullOrEmpty(facilityName))
				{
					// Try to find facility by name
					if (facilityDictionary.TryGetValue(facilityName, out int facilityId))
					{
						departments.Add(new Department
						{
							Name = departmentName,
							FacilityId = facilityId
						});
						successCount++;
					}
				}
			}

			if (departments.Any())
			{
				await _unitOfWork.DepartmentRepository.AddRangeAsync(departments);
				await _unitOfWork.SaveChangesAsync();
			}

			return successCount;
		}
		#endregion

	}
}