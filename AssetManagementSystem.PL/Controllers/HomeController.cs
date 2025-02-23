using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AssetManagementSystem.BLL.Interfaces;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementSystem.PL.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<User> _userManager;

		public HomeController(IUnitOfWork unitOfWork, UserManager<User> userManager)
		{
			_unitOfWork = unitOfWork;
			_userManager = userManager;
		}

		[Authorize]
		public async Task<IActionResult> Index()
		{
			// Get the logged-in user's ID
			var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			// Ensure UserManager and UnitOfWork are initialized
			if (_userManager == null)
			{
				throw new Exception("UserManager is not initialized.");
			}
			if (_unitOfWork == null)
			{
				throw new Exception("Unit of Work is not initialized.");
			}

			// Fetch all the required data using the Unit of Work
			var assets = await _unitOfWork.AssetRepository.GetAllAsync() ?? new List<Asset>();
			var assetTransfers = await _unitOfWork.AssetTransferRepository.GetAllAsync() ?? new List<AssetTransfer>();
			var buildings = await _unitOfWork.buildingRepository.GetAllAsync() ?? new List<Building>();
			var facilities = await _unitOfWork.FacilityRepository.GetAllAsync() ?? new List<Facility>();
			var departments = await _unitOfWork.DepartmentRepository.GetAllAsync() ?? new List<Department>();
			var disposals = await _unitOfWork.DisposalRepository.GetAllAsync() ?? new List<Disposal>();
			// Only get recent changelogs (last 10)
			var recentChangelogs = (await _unitOfWork.ChangeLogRepository.GetAllAsync())
				.OrderByDescending(c => c.ChangeDate)
				.Take(10)
				.ToList();
			var rooms = await _unitOfWork.RoomRepository.GetAllAsync() ?? new List<Room>();
			var users = (await _userManager.Users.ToListAsync())?.Where(u => u.Id != loggedInUserId).ToList() ?? new List<User>();


			
			// Create the DashboardViewModel
			var dashboardData = new DashboardViewModel
			{
				Assets = assets,
				AssetTransfers = assetTransfers,
				Buildings = buildings,
				Facilities = facilities,
				Departments = departments,
				Disposals = disposals,
				RecentChangelogs = recentChangelogs,
				Rooms = rooms,
				Users = users,
				AssetCount = assets.Count() // Store asset count
			};

			return View(dashboardData);
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
