using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.DAL.Utilities;
using AssetManagementSystem.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace AssetManagementSystem.PL.Controllers
{
	[Authorize]
	public class ReturnDocumentController : Controller
	{
		private readonly IReturnDocumentService _returnDocumentService;
		private readonly IAssetService _assetService;
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly ILogger<ReturnDocumentController> _logger;

		public ReturnDocumentController(
			IReturnDocumentService returnDocumentService,
			IAssetService assetService,
			UserManager<User> userManager,
			RoleManager<IdentityRole> roleManager,
			ILogger<ReturnDocumentController> logger)
		{
			_returnDocumentService = returnDocumentService;
			_assetService = assetService;
			_userManager = userManager;
			_roleManager = roleManager;
			_logger = logger;
		}

		// GET: /ReturnDocument
		public async Task<IActionResult> Index()
		{
			var documents = await _returnDocumentService.GetAllDocumentsAsync();
			return View(documents);
		}

		// GET: /ReturnDocument/Create
		public IActionResult Create()
		{
			ViewBag.ReturnReasons = new SelectList(new[]
			{
				"تالف",
				"فائض",
				"عدم الصلاحية",
				"انتهاء الغرض"
			});
			return View(new ReturnDocumentViewModel());
		}

		// POST: /ReturnDocument/Create
	  [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReturnDocumentViewModel model)
        {
            if (!ModelState.IsValid || model.AssetTags == null || !model.AssetTags.Any())
            {
                ViewBag.ReturnReasons = new SelectList(new[]
                {
                    "تالف",
                    "فائض",
                    "عدم الصلاحية",
                    "انتهاء الغرض"
                });
                ModelState.AddModelError("", "Please select at least one asset");
                return View(model);
            }

            try
            {
                // Get supervisors (committee members) for the selected assets
                var returnCommittee = await GetSupervisorsForAssetsAsync(model.AssetTags);
                
                // Group assets by department and create document(s)
                var documents = await _returnDocumentService.CreateReturnDocumentsAsync(
                    model.AssetTags,
                    model.ReturnReason,
                    returnCommittee); // Pass the committee members to the service

                if (documents.Count == 0)
                {
                    ModelState.AddModelError("", "Failed to create return documents. No valid assets found.");
                    return View(model);
                }

                if (documents.Count == 1)
                {
                    // If only one document was created, redirect to its details
                    var documentId = documents.Keys.First();
                    TempData["SuccessMessage"] = "Return document created successfully.";
                    return RedirectToAction(nameof(Details), new { id = documentId });
                }
                else
                {
                    // If multiple documents were created (due to multiple departments), show list
                    TempData["SuccessMessage"] = $"{documents.Count} return documents created successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating return document");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                ViewBag.ReturnReasons = new SelectList(new[]
                {
                    "تالف",
                    "فائض",
                    "عدم الصلاحية",
                    "انتهاء الغرض"
                });
                return View(model);
            }
        }

		// GET: /ReturnDocument/Details/5
		public async Task<IActionResult> Details(int id)
		{
			var document = await _returnDocumentService.GetDocumentByIdAsync(id);
			if (document == null)
			{
				return NotFound();
			}

			return View(document);
		}

		// GET: /ReturnDocument/Delete/5
		public async Task<IActionResult> Delete(int id)
		{
			var document = await _returnDocumentService.GetDocumentByIdAsync(id);
			if (document == null)
			{
				return NotFound();
			}

			return View(document);
		}

		// POST: /ReturnDocument/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			await _returnDocumentService.DeleteReturnDocumentAsync(id);
			TempData["SuccessMessage"] = "Return document deleted successfully.";
			return RedirectToAction(nameof(Index));
		}

		// GET: /ReturnDocument/GetPdf/5
		public async Task<IActionResult> GetPdf(int id)
		{
			try
			{
				var pdfBytes = await _returnDocumentService.GeneratePdfAsync(id);

				// Get document for filename
				var document = await _returnDocumentService.GetDocumentByIdAsync(id);
				string fileName = $"ReturnDocument_{document?.DocumentNumber ?? id.ToString()}.pdf";

				return File(pdfBytes, "application/pdf", fileName);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error generating PDF for return document {Id}", id);
				TempData["ErrorMessage"] = $"Error generating PDF: {ex.Message}";
				return RedirectToAction(nameof(Details), new { id });
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

				// Get all assets
				var allAssets = await _assetService.GetAllAssetsAsync();

				// Find matching assets
				var foundAssets = allAssets
					.Where(a => assetTags.Contains(a.AssetTag))
					.Select(a => new
					{
						assetTag = a.AssetTag,
						description = a.AssetDescription,
						departmentId = a.DepartmentId,
						departmentName = a.Department?.Name ?? "Unknown"
					})
					.ToList();

				// Group by department
				var assetsByDepartment = foundAssets
					.GroupBy(a => a.departmentId)
					.Select(g => new
					{
						departmentId = g.Key,
						departmentName = g.First().departmentName,
						assets = g.ToList(),
						count = g.Count()
					})
					.ToList();

				// Find tags that weren't found
				var foundTags = foundAssets.Select(a => a.assetTag).ToList();
				var notFoundTags = assetTags.Where(tag => !foundTags.Contains(tag)).ToList();
				var committeeMembers = await GetSupervisorsForAssetsAsync(foundTags);

				return Json(new
				{
					success = true,
					foundAssets = foundAssets,
					assetsByDepartment = assetsByDepartment,
					notFoundTags = notFoundTags,
					foundCount = foundAssets.Count,
					notFoundCount = notFoundTags.Count,
					departmentCount = assetsByDepartment.Count,
					returnCommittee = committeeMembers // Add committee members information
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error verifying asset tags");
				return Json(new { success = false, error = ex.Message });
			}
		}
		private async Task<string> GetSupervisorsForAssetsAsync(List<string> assetTags)
		{
			// Get assets with their associated users
			var assets = await _assetService.GetAllAssetsAsync();
			var selectedAssets = assets.Where(a => assetTags.Contains(a.AssetTag)).ToList();

			if (!selectedAssets.Any())
			{
				return "لا توجد أصول محددة";
			}

			// Get all supervisors in the system
			var supervisors = await GetSupervisorsAsync();
			if (!supervisors.Any())
			{
				return "لا يوجد مشرفين معرفين في النظام";
			}

			// Find assignee users for the selected assets
			var assignedUserIds = selectedAssets
				.Where(a => !string.IsNullOrEmpty(a.UserId))
				.Select(a => a.UserId)
				.Distinct()
				.ToList();

			// Default supervisor
			string supervisorList = supervisors.First().FullName;

			// If assets have assigned users, check if any are supervisors
			if (assignedUserIds.Any())
			{
				var assignedUsers = new List<User>();
				foreach (var userId in assignedUserIds)
				{
					var user = await _userManager.FindByIdAsync(userId);
					if (user != null)
					{
						assignedUsers.Add(user);
					}
				}

				var supervisorUsers = assignedUsers
					.Where(u => _userManager.IsInRoleAsync(u, Roles.Supervisor).Result)
					.ToList();

				if (supervisorUsers.Any())
				{
					// Use the first supervisor found if there's only one
					if (supervisorUsers.Count == 1)
					{
						supervisorList = supervisorUsers[0].FullName;
					}
					else
					{
						// If multiple supervisors, concatenate their names
						supervisorList = string.Join("\n", supervisorUsers.Select(u => u.FullName));
					}
				}
			}

			return supervisorList;
		}

		// Helper to get all users with Supervisor role
		private async Task<List<User>> GetSupervisorsAsync()
		{
			var supervisorRole = await _roleManager.FindByNameAsync(Roles.Supervisor);
			if (supervisorRole == null)
				return new List<User>();

			var supervisors = await _userManager.GetUsersInRoleAsync(Roles.Supervisor);
			return supervisors.ToList();
		}
	}
}