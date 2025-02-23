using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagementSystem.PL.Controllers
{
	public class DisposalController : Controller
	{
		private readonly IDisposalService _disposalService;

		public DisposalController(IDisposalService disposalService)
		{
			_disposalService = disposalService;
		}

		// Action to display the view
		public async Task<IActionResult> Index()
		{
			return View();
		}

		// Action to handle DataTables server-side processing
		[HttpPost]
		public async Task<IActionResult> GetDisposals()
		{
			var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
			var start = HttpContext.Request.Form["start"].FirstOrDefault();
			var length = HttpContext.Request.Form["length"].FirstOrDefault();
			var searchValue = HttpContext.Request.Form["search[value]"].FirstOrDefault();
			var sortColumn = HttpContext.Request.Form["order[0][column]"].FirstOrDefault();
			var sortDirection = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();

			int pageSize = length != null ? Convert.ToInt32(length) : 0;
			int skip = start != null ? Convert.ToInt32(start) : 0;
			int recordsTotal = 0;

			// Fetch all disposals with related Asset data
			var disposals = await _disposalService.GetAllDisposalsAsync();
			var disposalList = disposals.ToList();

			// Apply search filter
			if (!string.IsNullOrEmpty(searchValue))
			{
				disposalList = disposalList.Where(d =>
					d.AssetTag.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
					d.DisposalType.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
					d.DisposalDate.ToString("yyyy-MM-dd").Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
					d.SaleValue.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
					(d.Asset != null && d.Asset.AssetDescription.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) // Search AssetDescription
				).ToList();
			}

			// Apply sorting
			if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
			{
				var columnIndex = Convert.ToInt32(sortColumn);
				var isAscending = sortDirection == "asc";

				switch (columnIndex)
				{
					case 0: // Id
						disposalList = isAscending ? disposalList.OrderBy(d => d.Id).ToList() : disposalList.OrderByDescending(d => d.Id).ToList();
						break;
					case 1: // AssetTag
						disposalList = isAscending ? disposalList.OrderBy(d => d.AssetTag).ToList() : disposalList.OrderByDescending(d => d.AssetTag).ToList();
						break;
					case 2: // DisposalType
						disposalList = isAscending ? disposalList.OrderBy(d => d.DisposalType).ToList() : disposalList.OrderByDescending(d => d.DisposalType).ToList();
						break;
					case 3: // DisposalDate
						disposalList = isAscending ? disposalList.OrderBy(d => d.DisposalDate).ToList() : disposalList.OrderByDescending(d => d.DisposalDate).ToList();
						break;
					case 4: // SaleValue
						disposalList = isAscending ? disposalList.OrderBy(d => d.SaleValue).ToList() : disposalList.OrderByDescending(d => d.SaleValue).ToList();
						break;
					case 5: // AssetDescription
						disposalList = isAscending ? disposalList.OrderBy(d => d.Asset?.AssetDescription).ToList() : disposalList.OrderByDescending(d => d.Asset?.AssetDescription).ToList();
						break;
					default:
						disposalList = disposalList.OrderBy(d => d.Id).ToList();
						break;
				}
			}

			// Get total records count
			recordsTotal = disposalList.Count();

			// Pagination
			var data = disposalList.Skip(skip).Take(pageSize).Select(d => new
			{
				id = d.Id,
				assetTag = d.AssetTag,
				disposalType = d.DisposalType,
				disposalDate = d.DisposalDate.ToString("yyyy-MM-ddTHH:mm:ss"), // Format the date
				saleValue = d.SaleValue,
				assetDescription = d.Asset?.AssetDescription // Include AssetDescription
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


		// 🟢 عرض نموذج إنشاء عملية التخلص
		public IActionResult Create()
		{
			return View();
		}

		// 🟢 تنفيذ عملية إنشاء سجل جديد
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Disposal disposal)
		{
			if (ModelState.IsValid)
			{
				await _disposalService.AddDisposalAsync(disposal);
				return RedirectToAction(nameof(Index));
			}
			return View(disposal);
		}

		// 🟢 عرض نموذج التعديل
		public async Task<IActionResult> Edit(int id)
		{
			var disposal = await _disposalService.GetDisposalByIdAsync(id);
			if (disposal == null)
			{
				return NotFound();
			}
			return View(disposal);
		}

		// 🟢 تنفيذ عملية التعديل
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Disposal disposal)
		{
			if (id != disposal.Id)
			{
				return BadRequest();
			}

			if (ModelState.IsValid)
			{
				await _disposalService.UpdateDisposalAsync(disposal);
				return RedirectToAction(nameof(Index));
			}
			return View(disposal);
		}

		//  عرض التأكيد قبل الحذف
		public async Task<IActionResult> Delete(int id)
		{
			var disposal = await _disposalService.GetDisposalByIdAsync(id);
			if (disposal == null)
			{
				return NotFound();
			}
			return View(disposal);
		}

		//  تنفيذ الحذف
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			await _disposalService.DeleteDisposalAsync(id);
			return RedirectToAction(nameof(Index));
		}


	}
}