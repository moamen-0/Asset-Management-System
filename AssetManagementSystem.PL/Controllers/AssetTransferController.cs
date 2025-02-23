using AssetManagementSystem.BLL.Interfaces;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AssetManagementSystem.PL.Controllers
{
	public class AssetTransferController : Controller
	{
		private readonly IAssetTransferService _assetTransferService;
		private readonly IUnitOfWork _unitOfWork;

		public AssetTransferController(IAssetTransferService assetTransferService, IUnitOfWork unitOfWork)
		{
			_assetTransferService = assetTransferService;
			_unitOfWork = unitOfWork;
		}

		// Action to display the view
		public async Task<IActionResult> Index()
		{
			return View();
		}

		// Action to handle DataTables server-side processing
		[HttpPost]
		public async Task<IActionResult> GetAssetTransfers()
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

			// Fetch all asset transfers with related Asset data
			var assetTransfers = await _assetTransferService.GetAllAssetTransfersAsync();
			var assetTransferList = assetTransfers.ToList();

			// Apply search filter
			if (!string.IsNullOrEmpty(searchValue))
			{
				assetTransferList = assetTransferList.Where(a =>
					a.TransferType.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
					a.FromDepartment.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
					a.ToDepartment.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
					a.AssetTag.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
					(a.Asset != null && a.Asset.AssetDescription.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) // Search AssetDescription
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
						assetTransferList = isAscending ? assetTransferList.OrderBy(a => a.Id).ToList() : assetTransferList.OrderByDescending(a => a.Id).ToList();
						break;
					case 1: // TransferType
						assetTransferList = isAscending ? assetTransferList.OrderBy(a => a.TransferType).ToList() : assetTransferList.OrderByDescending(a => a.TransferType).ToList();
						break;
					case 2: // TransferDate
						assetTransferList = isAscending ? assetTransferList.OrderBy(a => a.TransferDate).ToList() : assetTransferList.OrderByDescending(a => a.TransferDate).ToList();
						break;
					case 3: // FromDepartment
						assetTransferList = isAscending ? assetTransferList.OrderBy(a => a.FromDepartment).ToList() : assetTransferList.OrderByDescending(a => a.FromDepartment).ToList();
						break;
					case 4: // ToDepartment
						assetTransferList = isAscending ? assetTransferList.OrderBy(a => a.ToDepartment).ToList() : assetTransferList.OrderByDescending(a => a.ToDepartment).ToList();
						break;
					case 5: // AssetTag
						assetTransferList = isAscending ? assetTransferList.OrderBy(a => a.AssetTag).ToList() : assetTransferList.OrderByDescending(a => a.AssetTag).ToList();
						break;
					case 6: // AssetDescription
						assetTransferList = isAscending ? assetTransferList.OrderBy(a => a.Asset?.AssetDescription).ToList() : assetTransferList.OrderByDescending(a => a.Asset?.AssetDescription).ToList();
						break;
					default:
						assetTransferList = assetTransferList.OrderBy(a => a.Id).ToList();
						break;
				}
			}

			// Get total records count
			recordsTotal = assetTransferList.Count();

			// Pagination
			var data = assetTransferList.Skip(skip).Take(pageSize).Select(a => new
			{
				id = a.Id,
				transferType = a.TransferType,
				transferDate = a.TransferDate.ToString("yyyy-MM-ddTHH:mm:ss"), // Format the date
				fromDepartment = a.FromDepartment,
				toDepartment = a.ToDepartment,
				assetTag = a.AssetTag,
				assetDescription = a.Asset?.AssetDescription // Include AssetDescription
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

		// Action to get details of a specific asset transfer
		[HttpGet]
		public async Task<IActionResult> Details(int id)
		{
			var assetTransfer = await _assetTransferService.GetAssetTransferByIdAsync(id);
			if (assetTransfer == null)
			{
				return NotFound();
			}
			return View(assetTransfer);
		}

		// Action to display the create view
		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		// Action to handle the creation of a new asset transfer
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(AssetTransfer assetTransfer)
		{
			if (ModelState.IsValid)
			{
				await _assetTransferService.AddAssetTransferAsync(assetTransfer);
				return RedirectToAction(nameof(Index));
			}
			return View(assetTransfer);
		}

		// Action to display the edit view
		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var assetTransfer = await _assetTransferService.GetAssetTransferByIdAsync(id);
			if (assetTransfer == null)
			{
				return NotFound();
			}
			return View(assetTransfer);
		}

		// Action to handle the update of an existing asset transfer
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, AssetTransfer assetTransfer)
		{
			if (id != assetTransfer.Id)
			{
				return BadRequest();
			}

			if (ModelState.IsValid)
			{
				await _assetTransferService.UpdateAssetTransferAsync(assetTransfer);
				return RedirectToAction(nameof(Index));
			}
			return View(assetTransfer);
		}

		// Action to display the delete view
		[HttpGet]
		public async Task<IActionResult> Delete(int id)
		{
			var assetTransfer = await _assetTransferService.GetAssetTransferByIdAsync(id);

			if (assetTransfer == null)
			{
				return NotFound(); // رجّع 404 لو الـ ID غير موجود
			}

			return View(assetTransfer);
		}

		[HttpPost("AssetTransfer/DeleteConfirmed")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			try
			{
				var assetTransfer = await _assetTransferService.GetAssetTransferByIdAsync(id);
				if (assetTransfer == null)
				{
					return NotFound();
				}

				await _assetTransferService.DeleteAssetTransferAsync(id);

				return RedirectToAction(nameof(Index));

			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error deleting asset transfer: {ex.Message}");
				return NotFound();
			}

			

		}
	}
}