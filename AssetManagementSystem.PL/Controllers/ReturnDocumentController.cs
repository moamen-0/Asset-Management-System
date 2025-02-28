using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
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
		private readonly ILogger<ReturnDocumentController> _logger;

		public ReturnDocumentController(
			IReturnDocumentService returnDocumentService,
			IAssetService assetService,
			UserManager<User> userManager,
			ILogger<ReturnDocumentController> logger)
		{
			_returnDocumentService = returnDocumentService;
			_assetService = assetService;
			_userManager = userManager;
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
				// Group assets by department and create document(s)
				var documents = await _returnDocumentService.CreateReturnDocumentsAsync(
					model.AssetTags,
					model.ReturnReason);

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

				return Json(new
				{
					success = true,
					foundAssets = foundAssets,
					assetsByDepartment = assetsByDepartment,
					notFoundTags = notFoundTags,
					foundCount = foundAssets.Count,
					notFoundCount = notFoundTags.Count,
					departmentCount = assetsByDepartment.Count
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error verifying asset tags");
				return Json(new { success = false, error = ex.Message });
			}
		}
	}
}