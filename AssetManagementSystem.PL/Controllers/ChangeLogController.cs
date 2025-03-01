using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementSystem.PL.Controllers
{
	public class ChangeLogController : Controller
	{
		private readonly IChangeLogService _changeLogService;
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly ILogger<ChangeLogController> _logger;

		public ChangeLogController(
	IChangeLogService changeLogService,
	UserManager<User> userManager,
	SignInManager<User> signInManager,
	ILogger<ChangeLogController> logger)  // Changed this line
		{
			_changeLogService = changeLogService;
			_userManager = userManager;
			_signInManager = signInManager;
			_logger = logger;  // No casting needed
		}
		private async Task<User> GetCurrentUserAsync()
		{
			var userId = _userManager.GetUserId(User); // Get the user ID from the claims
			return await _userManager.FindByIdAsync(userId); // Fetch the user from the database
		}


		public async Task<IActionResult> Index()
		{
			var logs = await _changeLogService.GetAllChangeLogsAsync();
			return View(logs);
		}

		[HttpPost]
		public async Task<IActionResult> GetChangeLogs()
		{
			try
			{
				var draw = int.Parse(Request.Form["draw"].FirstOrDefault() ?? "1");
				var start = int.Parse(Request.Form["start"].FirstOrDefault() ?? "0");
				var length = int.Parse(Request.Form["length"].FirstOrDefault() ?? "10");
				var searchValue = Request.Form["search[value]"].FirstOrDefault();
				var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
				var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
				var sortColumn = Request.Form[$"columns[{sortColumnIndex}][data]"].FirstOrDefault();

				// Use the repository directly to bypass caching
				var logs = await _changeLogService.GetPaginatedAsync(start, length, searchValue, sortColumn, sortDirection);
				var totalCount = logs.TotalCount;

				var data = logs.Logs.Select(l => new
				{
					entityName = l.EntityName,
					entityId = l.EntityId,
					actionType = FormatActionType(l.ActionType),
					hasOldValues = !string.IsNullOrEmpty(l.OldValues),
					hasNewValues = !string.IsNullOrEmpty(l.NewValues),
					oldValuesId = l.Id,
					newValuesId = l.Id,
					changeDate = l.ChangeDate.ToString("yyyy-MM-dd HH:mm:ss"),
					userFullName = l.User?.FullName ?? "System"
				});

				return Json(new
				{
					draw = draw,
					recordsTotal = totalCount,
					recordsFiltered = totalCount,
					data = data
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}

		// Add this new method to get the values when needed
		[HttpGet]
		public async Task<IActionResult> GetChangeLogValues(string id, string entityName, string type)
		{
			try
			{
				// Find all changelogs for this entity
				var changeLogs = await _changeLogService.GetAllChangeLogsAsync();
				var changeLog = changeLogs
					.Where(c => c.EntityName == entityName && c.EntityId == id)
					.OrderByDescending(c => c.ChangeDate)
					.FirstOrDefault();

				if (changeLog == null)
				{
					return NotFound("Change log not found");
				}

				// Return the requested values
				string values = type == "old" ? changeLog.OldValues : changeLog.NewValues;
				return Json(new { values = values });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error fetching change log values");
				return StatusCode(500, ex.Message);
			}
		}

		private string FormatActionType(string actionType)
		{
			return actionType switch
			{
				"Added" => "Created",
				"Modified" => "Updated",
				"Deleted" => "Deleted",
				_ => actionType
			};
		}
		private string FormatJsonData(string jsonData)
		{
			if (string.IsNullOrEmpty(jsonData)) return null;
			try
			{
				// Return the JSON data without replacing quotes
				return jsonData;
			}
			catch
			{
				return jsonData;
			}
		}
	}
}
