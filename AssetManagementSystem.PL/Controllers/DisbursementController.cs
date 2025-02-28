using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.PL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AssetManagementSystem.PL.Controllers
{
	public class DisbursementController : Controller
	{
		private readonly IDisbursementService _disbursementService;
		private readonly IAssetService _assetService;
		private readonly UserManager<User> _userManager;

		public DisbursementController(
			IDisbursementService disbursementService,
			IAssetService assetService,
			UserManager<User> userManager)
		{
			_disbursementService = disbursementService;
			_assetService = assetService;
			_userManager = userManager;
		}

		// GET: /Disbursement
		public async Task<IActionResult> Index()
		{
			var requests = await _disbursementService.GetAllRequestsAsync();
			return View(requests);
		}

		// GET: /Disbursement/Create
		public async Task<IActionResult> Create()
		{
			ViewBag.DepartmentList = await GetDepartmentsAsync();
			return View();
		}

		// POST: /Disbursement/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(DisbursementViewModel model)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.DepartmentList = await GetDepartmentsAsync();
				return View(model);
			}

			try
			{
				// Get current user
				var currentUser = await _userManager.GetUserAsync(User);
				if (currentUser == null)
				{
					return RedirectToAction("Login", "Auth");
				}

				// Create the request
				var request = new DisbursementRequest
				{
					Department = model.Department,
					Requester = currentUser.FullName,
					RequesterContact = currentUser.PhoneNumber ?? "N/A",
					WarehouseManager = "ناصر العدل", // Default or get from settings
					AuthorityName = "مدير مستشفى بريدة المركزي",
					AuthorityPerson = "حمود بن صالح الزيد",
					Status = "Pending"
				};

				// Create with asset tags
				await _disbursementService.CreateRequestAsync(request, model.AssetTags);

				// Redirect to details with success message
				TempData["SuccessMessage"] = "تم إنشاء طلب الصرف بنجاح.";
				return RedirectToAction(nameof(Details), new { id = request.Id });
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"حدث خطأ: {ex.Message}");
				ViewBag.DepartmentList = await GetDepartmentsAsync();
				return View(model);
			}
		}

		// GET: /Disbursement/Edit/5
		public async Task<IActionResult> Edit(int id)
		{
			var request = await _disbursementService.GetRequestByIdAsync(id);
			if (request == null)
			{
				return NotFound();
			}

			var model = new DisbursementViewModel
			{
				Id = request.Id,
				Department = request.Department,
				AssetTags = request.Items.Select(i => i.AssetTag).ToList()
			};

			ViewBag.DepartmentList = await GetDepartmentsAsync();
			return View(model);
		}

		// POST: /Disbursement/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, DisbursementViewModel model)
		{
			if (id != model.Id)
			{
				return BadRequest();
			}

			if (!ModelState.IsValid)
			{
				ViewBag.DepartmentList = await GetDepartmentsAsync();
				return View(model);
			}

			try
			{
				// Get the existing request
				var request = await _disbursementService.GetRequestByIdAsync(id);
				if (request == null)
				{
					return NotFound();
				}

				// Update basic properties
				request.Department = model.Department;

				// Update with new asset tags
				await _disbursementService.UpdateRequestAsync(request, model.AssetTags);

				TempData["SuccessMessage"] = "تم تحديث طلب الصرف بنجاح.";
				return RedirectToAction(nameof(Details), new { id = request.Id });
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"حدث خطأ: {ex.Message}");
				ViewBag.DepartmentList = await GetDepartmentsAsync();
				return View(model);
			}
		}

		// GET: /Disbursement/Details/5
		public async Task<IActionResult> Details(int id)
		{
			var request = await _disbursementService.GetRequestByIdAsync(id);
			if (request == null)
			{
				return NotFound();
			}

			return View(request);
		}

		// GET: /Disbursement/Delete/5
		public async Task<IActionResult> Delete(int id)
		{
			var request = await _disbursementService.GetRequestByIdAsync(id);
			if (request == null)
			{
				return NotFound();
			}

			return View(request);
		}

		// POST: /Disbursement/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			await _disbursementService.DeleteRequestAsync(id);
			TempData["SuccessMessage"] = "تم حذف طلب الصرف بنجاح.";
			return RedirectToAction(nameof(Index));
		}

		// GET: /Disbursement/GetPdf/5
		public async Task<IActionResult> GetPdf(int id)
		{
			try
			{
				var pdfBytes = await _disbursementService.GeneratePdfAsync(id);

				// Get request to use in filename
				var request = await _disbursementService.GetRequestByIdAsync(id);
				string fileName = $"DisbursementRequest_{request?.RequestNumber ?? id.ToString()}.pdf";

				return File(pdfBytes, "application/pdf", fileName);
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = $"فشل إنشاء ملف PDF: {ex.Message}";
				return RedirectToAction(nameof(Details), new { id });
			}
		}

		// Helper for asset selection
		[HttpGet]
		public async Task<IActionResult> GetAssetsByFilter(string filter)
		{
			try
			{
				var assets = await _assetService.GetAllAssetsAsync();
				var filteredAssets = assets.Where(a =>
					string.IsNullOrEmpty(filter) ||
					a.AssetTag.Contains(filter) ||
					a.AssetDescription.Contains(filter) ||
					a.Brand.Contains(filter)
				).Take(20).Select(a => new
				{
					assetTag = a.AssetTag,
					description = a.AssetDescription,
					brand = a.Brand
				}).ToList();

				return Json(new { success = true, data = filteredAssets });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		// Get store keeper for selected assets
		[HttpPost]
		public async Task<IActionResult> GetStoreKeeper([FromBody] List<string> assetTags)
		{
			try
			{
				if (assetTags == null || !assetTags.Any())
				{
					return Json(new { success = false, error = "No asset tags provided" });
				}

				string storeKeeperName = await _disbursementService.GetStoreKeeperForAssetsAsync(assetTags);
				return Json(new { success = true, storeKeeper = storeKeeperName });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}
		private async Task<SelectList> GetDepartmentsAsync()
		{
			var departments = new List<string>
		{
			"العيادات الخارجية",
			"الادارة",
			"المختبر",
			"الطوارئ",
			"العناية المركزة",
			"أقسام التنويم",
			"قسم الأشعة",
			"الصيدلية",
			"المستودع الطبي",
			"قسم المشتريات"
		};

			return new SelectList(departments);
		}
		[HttpPost]
		public async Task<IActionResult> GetAssetsByTags([FromBody] List<string> assetTags)
		{
			try
			{
				if (assetTags == null || !assetTags.Any())
				{
					return Json(new { success = false, error = "No asset tags provided" });
				}

				var assets = await _assetService.GetAllAssetsAsync();
				var filteredAssets = assets
					.Where(a => assetTags.Contains(a.AssetTag))
					.Select(a => new
					{
						assetTag = a.AssetTag,
						description = a.AssetDescription,
						brand = a.Brand
					})
					.ToList();

				return Json(new { success = true, data = filteredAssets });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}
		[HttpPost]
		public async Task<IActionResult> VerifyAssetTags([FromBody] List<string> assetTags)
		{
			try
			{
				if (assetTags == null || !assetTags.Any())
				{
					return Json(new { success = false, error = "لم يتم تقديم رموز للمواد" });
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
						brand = a.Brand
					})
					.ToList();

				// Find tags that weren't found
				var foundTags = foundAssets.Select(a => a.assetTag).ToList();
				var notFoundTags = assetTags.Where(tag => !foundTags.Contains(tag)).ToList();

				return Json(new
				{
					success = true,
					foundAssets = foundAssets,
					notFoundTags = notFoundTags,
					foundCount = foundAssets.Count,
					notFoundCount = notFoundTags.Count
				});
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}
	}
}
