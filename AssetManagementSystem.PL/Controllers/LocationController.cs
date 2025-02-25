using AssetManagementSystem.BLL.Interfaces;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.DAL.Utilities;
using AssetManagementSystem.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
		#endregion

	}
}