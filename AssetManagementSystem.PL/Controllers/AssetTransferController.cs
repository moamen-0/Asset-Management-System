using AssetManagementSystem.BLL.Interfaces;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Colors = QuestPDF.Helpers.Colors;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagementSystem.PL.Controllers
{
	[Authorize]
	public class AssetTransferController : Controller
	{
		private readonly IAssetTransferService _assetTransferService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<User> _userManager;
		private readonly ILogger<AssetTransferController> _logger;

		public AssetTransferController(
			IAssetTransferService assetTransferService,
			IUnitOfWork unitOfWork,
			UserManager<User> userManager,
			ILogger<AssetTransferController> logger)
		{
			_assetTransferService = assetTransferService;
			_unitOfWork = unitOfWork;
			_userManager = userManager;
			_logger = logger;
		}

		// Get current user helper method
		private async Task<User> GetCurrentUserAsync()
		{
			return await _userManager.GetUserAsync(User);
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
			try
			{
				var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
				var start = HttpContext.Request.Form["start"].FirstOrDefault();
				var length = HttpContext.Request.Form["length"].FirstOrDefault();
				var searchValue = HttpContext.Request.Form["search[value]"].FirstOrDefault();
				var sortColumn = HttpContext.Request.Form["order[0][column]"].FirstOrDefault();
				var sortDirection = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
				var startDate = HttpContext.Request.Form["startDate"].FirstOrDefault();
				var endDate = HttpContext.Request.Form["endDate"].FirstOrDefault();
				var departmentId = HttpContext.Request.Form["department"].FirstOrDefault();

				int pageSize = length != null ? Convert.ToInt32(length) : 0;
				int skip = start != null ? Convert.ToInt32(start) : 0;
				int recordsTotal = 0;

				// Fetch all asset transfers with related Asset data
				var assetTransfers = await _assetTransferService.GetAllAssetTransfersAsync();
				var assetTransferList = assetTransfers.ToList();
				if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out DateTime startDateTime))
				{
					assetTransferList = assetTransferList.Where(a => a.TransferDate.Date >= startDateTime.Date).ToList();
				}

				if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out DateTime endDateTime))
				{
					assetTransferList = assetTransferList.Where(a => a.TransferDate.Date <= endDateTime.Date).ToList();
				}
				// Apply search filter
				if (!string.IsNullOrEmpty(searchValue))
				{
					assetTransferList = assetTransferList.Where(a =>
						(a.TransferType != null && a.TransferType.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
						(a.FromDepartment != null && a.FromDepartment.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
						(a.ToDepartment != null && a.ToDepartment.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
						a.AssetTag.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
						(a.Asset != null && a.Asset.AssetDescription != null &&
						 a.Asset.AssetDescription.Contains(searchValue, StringComparison.OrdinalIgnoreCase))
					).ToList();
				}

				// Apply sorting
				if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
				{
					var columnIndex = Convert.ToInt32(sortColumn);
					var isAscending = sortDirection == "asc";

					switch (columnIndex)
					{
						case 1: // Id
							assetTransferList = isAscending ? assetTransferList.OrderBy(a => a.Id).ToList() : assetTransferList.OrderByDescending(a => a.Id).ToList();
							break;
						case 2: // TransferType
							assetTransferList = isAscending ? assetTransferList.OrderBy(a => a.TransferType).ToList() : assetTransferList.OrderByDescending(a => a.TransferType).ToList();
							break;
						case 3: // TransferDate
							assetTransferList = isAscending ? assetTransferList.OrderBy(a => a.TransferDate).ToList() : assetTransferList.OrderByDescending(a => a.TransferDate).ToList();
							break;
						case 4: // FromDepartment
							assetTransferList = isAscending ? assetTransferList.OrderBy(a => a.FromDepartment).ToList() : assetTransferList.OrderByDescending(a => a.FromDepartment).ToList();
							break;
						case 5: // ToDepartment
							assetTransferList = isAscending ? assetTransferList.OrderBy(a => a.ToDepartment).ToList() : assetTransferList.OrderByDescending(a => a.ToDepartment).ToList();
							break;
						case 6: // AssetTag
							assetTransferList = isAscending ? assetTransferList.OrderBy(a => a.AssetTag).ToList() : assetTransferList.OrderByDescending(a => a.AssetTag).ToList();
							break;
						case 7: // AssetDescription
							assetTransferList = isAscending ? assetTransferList.OrderBy(a => a.Asset?.AssetDescription).ToList() : assetTransferList.OrderByDescending(a => a.Asset?.AssetDescription).ToList();
							break;
						default:
							assetTransferList = assetTransferList.OrderByDescending(a => a.TransferDate).ToList(); // Default sort by most recent
							break;
					}
				}
				else
				{
					// Default sort by most recent transfer date if no sorting specified
					assetTransferList = assetTransferList.OrderByDescending(a => a.TransferDate).ToList();
				}

				// Get total records count
				recordsTotal = assetTransferList.Count();

				// Pagination
				var data = assetTransferList.Skip(skip).Take(pageSize).Select(a => new
				{
					id = a.Id,
					transferType = a.TransferType ?? "داخلي", // Default to internal if null
					transferDate = a.TransferDate.ToString("yyyy-MM-dd"), // Format the date
					fromDepartment = a.FromDepartment ?? "غير محدد", // Default if null
					toDepartment = a.ToDepartment ?? "غير محدد", // Default if null
					assetTag = a.AssetTag,
					assetDescription = a.Asset?.AssetDescription ?? "غير محدد", // Include AssetDescription
																				// Add brand and model for more details
					brand = a.Asset?.Brand ?? "غير محدد",
					model = a.Asset?.Model ?? "غير محدد"
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
				_logger.LogError(ex, "Error fetching asset transfers");
				return Json(new
				{
					draw = "1",
					recordsFiltered = 0,
					recordsTotal = 0,
					data = new List<object>(),
					error = "حدث خطأ أثناء جلب البيانات"
				});
			}
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
		public async Task<IActionResult> Create()
		{
			// Get data for dropdowns
			ViewBag.Assets = await _unitOfWork.AssetRepository.GetAllAsync();
			ViewBag.Departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
			ViewBag.Users = await _unitOfWork.user.GetAllUsersAsync();	
			return View();
		}

		// Action to handle the creation of a new asset transfer
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(AssetTransfer assetTransfer)
		{
			try
			{
				if (ModelState.IsValid)
				{
					// Set transfer date to now if not provided
					if (assetTransfer.TransferDate == default)
					{
						assetTransfer.TransferDate = DateTime.Now;
					}

					await _assetTransferService.AddAssetTransferAsync(assetTransfer);

					// Add success message
					TempData["SuccessMessage"] = "تم إنشاء عملية النقل بنجاح";
					return RedirectToAction(nameof(Index));
				}

				// If we got this far, something failed, redisplay form
				ViewBag.Assets = await _unitOfWork.AssetRepository.GetAllAsync();
				ViewBag.Departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
				ViewBag.Users = await _unitOfWork.user.GetAllUsersAsync();
				return View(assetTransfer);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating asset transfer");
				ModelState.AddModelError("", "حدث خطأ أثناء إنشاء عملية النقل");

				ViewBag.Assets = await _unitOfWork.AssetRepository.GetAllAsync();
				ViewBag.Departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
				ViewBag.Users = await _unitOfWork.user.GetAllUsersAsync();
				return View(assetTransfer);
			}
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

			// Get data for dropdowns
			ViewBag.Assets = await _unitOfWork.AssetRepository.GetAllAsync();
			ViewBag.Departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
			ViewBag.Users = await _unitOfWork.user.GetAllUsersAsync();
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
				try
				{
					await _assetTransferService.UpdateAssetTransferAsync(assetTransfer);
					TempData["SuccessMessage"] = "تم تحديث عملية النقل بنجاح";
					return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error updating asset transfer");
					ModelState.AddModelError("", "حدث خطأ أثناء تحديث عملية النقل");
				}
			}

			ViewBag.Assets = await _unitOfWork.AssetRepository.GetAllAsync();
			ViewBag.Departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
			ViewBag.Users = await _unitOfWork.user.GetAllUsersAsync();
			return View(assetTransfer);
		}

		// Action to display the delete view
		[HttpGet]
		public async Task<IActionResult> Delete(int id)
		{
			var assetTransfer = await _assetTransferService.GetAssetTransferByIdAsync(id);

			if (assetTransfer == null)
			{
				return NotFound();
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
				TempData["SuccessMessage"] = "تم حذف عملية النقل بنجاح";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting asset transfer: {Message}", ex.Message);
				TempData["ErrorMessage"] = "حدث خطأ أثناء حذف عملية النقل";
				return RedirectToAction(nameof(Index));
			}
		}

		// Generate PDF for a single transfer
		[HttpGet]
		public async Task<IActionResult> GenerateTransferPdf(int id)
		{
			try
			{
				// Get the asset transfer
				var transfer = await _assetTransferService.GetAssetTransferByIdAsync(id);
				if (transfer == null)
				{
					return NotFound();
				}

				// Get current user
				var currentUser = await GetCurrentUserAsync();
				if (currentUser == null)
				{
					return Unauthorized();
				}

				// Generate PDF document
				var pdfBytes = await GenerateTransferDocumentAsync(transfer, currentUser);

				// Return PDF file
				return File(pdfBytes, "application/pdf", $"AssetTransfer_{transfer.Id}_{DateTime.Now:yyyyMMdd}.pdf");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error generating transfer PDF");
				TempData["ErrorMessage"] = "حدث خطأ أثناء إنشاء ملف PDF";
				return RedirectToAction(nameof(Details), new { id });
			}
		}

		//// Generate PDF for multiple transfers
		//[HttpPost]
		//public async Task<IActionResult> GenerateMultipleTransfersPdf([FromBody] List<int> ids)
		//{
		//	try
		//	{
		//		if (ids == null || !ids.Any())
		//		{
		//			return BadRequest(new { error = "لم يتم تحديد أي عمليات نقل" });
		//		}

		//		// Get current user
		//		var currentUser = await GetCurrentUserAsync();
		//		if (currentUser == null)
		//		{
		//			return Unauthorized();
		//		}

		//		// Fetch all selected transfers
		//		var transfers = new List<AssetTransfer>();
		//		foreach (var id in ids)
		//		{
		//			var transfer = await _assetTransferService.GetAssetTransferByIdAsync(id);
		//			if (transfer != null)
		//			{
		//				transfers.Add(transfer);
		//			}
		//		}

		//		if (!transfers.Any())
		//		{
		//			return NotFound(new { error = "لم يتم العثور على عمليات النقل المحددة" });
		//		}

		//		// Generate combined PDF
		//		var pdfBytes = await GenerateMultipleTransfersDocumentAsync(transfers, currentUser);

		//		// Return PDF file
		//		return File(pdfBytes, "application/pdf", $"AssetTransfers_{DateTime.Now:yyyyMMdd}.pdf");
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError(ex, "Error generating multiple transfers PDF");
		//		return StatusCode(500, new { error = "حدث خطأ أثناء إنشاء ملف PDF المجمع" });
		//	}
		//}

		//// Generate PDF for all transfers in a specific date range
		//[HttpPost]
		//public async Task<IActionResult> GenerateTransfersPdfByDateRange([FromBody] DateRangeRequest request)
		//{
		//	try
		//	{
		//		if (request.StartDate > request.EndDate)
		//		{
		//			return BadRequest(new { error = "تاريخ البداية يجب أن يكون قبل تاريخ النهاية" });
		//		}

		//		// Get current user
		//		var currentUser = await GetCurrentUserAsync();
		//		if (currentUser == null)
		//		{
		//			return Unauthorized();
		//		}

		//		// Fetch all transfers in the date range
		//		var allTransfers = await _assetTransferService.GetAllAssetTransfersAsync();
		//		var transfers = allTransfers
		//			.Where(t => t.TransferDate.Date >= request.StartDate.Date &&
		//						t.TransferDate.Date <= request.EndDate.Date)
		//			.ToList();

		//		if (!transfers.Any())
		//		{
		//			return NotFound(new { error = "لم يتم العثور على عمليات نقل في الفترة المحددة" });
		//		}

		//		// Generate combined PDF
		//		var pdfBytes = await GenerateMultipleTransfersDocumentAsync(transfers, currentUser);

		//		// Return PDF file
		//		return File(pdfBytes, "application/pdf",
		//			$"AssetTransfers_{request.StartDate:yyyyMMdd}_to_{request.EndDate:yyyyMMdd}.pdf");
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError(ex, "Error generating transfers PDF by date range");
		//		return StatusCode(500, new { error = "حدث خطأ أثناء إنشاء ملف PDF حسب النطاق الزمني" });
		//	}
		//}

		// Export to Excel
		[HttpGet]
		public async Task<IActionResult> ExportToExcel()
		{
			try
			{
				var transfers = await _assetTransferService.GetAllAssetTransfersAsync();

				// Export logic would go here
				// (Implementation depends on the Excel library being used)

				return File(new byte[0], "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
					$"AssetTransfers_{DateTime.Now:yyyyMMdd}.xlsx");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error exporting transfers to Excel");
				TempData["ErrorMessage"] = "حدث خطأ أثناء تصدير البيانات إلى Excel";
				return RedirectToAction(nameof(Index));
			}
		}

		private async Task<byte[]> GenerateTransferDocumentAsync(
			AssetTransfer transfer,
			User currentUser)
		{
			// Set QuestPDF license
			QuestPDF.Settings.License = LicenseType.Community;

			// Get the asset details
			var asset = transfer.Asset;

			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Size(PageSizes.A4);
					page.Margin(20);
					page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

					// Header Section - Right to Left for Arabic
					page.Header().Row(row =>
					{
						// Title and Details - Right Side for Arabic
						row.RelativeItem().Column(col =>
						{
							col.Item().Text("المملكة العربية السعودية")
								.FontSize(14).Bold().FontColor(Colors.Black).AlignRight();
							col.Item().Text("وزارة الصحة")
								.FontSize(14).Bold().FontColor(Colors.Black).AlignRight();
							col.Item().Text("مستشفى بريدة المركزي")
								.FontSize(14).Bold().FontColor(Colors.Black).AlignRight();
						});

						// Center Title
						row.RelativeItem().Column(col =>
						{
							col.Item().Text("مستند نقل الأصول")
								.FontSize(18).Bold().FontColor(Colors.Blue.Medium).AlignCenter();
							col.Item().Text($"التاريخ: {DateTime.Now:yyyy/MM/dd}")
								.FontSize(12).AlignCenter();
							col.Item().Text($"رقم المستند: {transfer.Id}")
								.FontSize(12).AlignCenter();
						});

						// Logo - Left Side for Arabic
						var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
						if (System.IO.File.Exists(logoPath))
						{
							row.ConstantItem(100).Image(logoPath).FitWidth();
						}
					});

					// Main Content
					page.Content().Column(col =>
					{
						// Transfer Details Section
						col.Item().Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn();
								columns.RelativeColumn();
							});

							// Table Header
							table.Header(header =>
							{
								header.Cell().Element(CellStyle).Text("بيانات النقل")
									.Bold().FontSize(14).AlignCenter();
								header.Cell().Element(CellStyle).Text("تفاصيل الأصل")
									.Bold().FontSize(14).AlignCenter();
							});

							// Table Row 1
							table.Cell().Element(CellStyle).Column(column =>
							{
								column.Item().Text("نوع النقل:").Bold().AlignRight();
								column.Item().Text(transfer.TransferType ?? "داخلي").AlignRight();

								column.Item().Text("تاريخ النقل:").Bold().AlignRight();
								column.Item().Text(transfer.TransferDate.ToString("yyyy/MM/dd")).AlignRight();

								column.Item().Text("من القسم:").Bold().AlignRight();
								column.Item().Text(transfer.FromDepartment ?? "غير محدد").AlignRight();

								column.Item().Text("إلى القسم:").Bold().AlignRight();
								column.Item().Text(transfer.ToDepartment ?? "غير محدد").AlignRight();
							});

							table.Cell().Element(CellStyle).Column(column =>
							{
								column.Item().Text("رقم الأصل:").Bold().AlignRight();
								column.Item().Text(transfer.AssetTag).AlignRight();

								column.Item().Text("وصف الأصل:").Bold().AlignRight();
								column.Item().Text(asset?.AssetDescription ?? "غير محدد").AlignRight();

								column.Item().Text("الموديل:").Bold().AlignRight();
								column.Item().Text(asset?.Model ?? "غير محدد").AlignRight();

								column.Item().Text("الرقم التسلسلي:").Bold().AlignRight();
								column.Item().Text(asset?.SerialNumber ?? "غير محدد").AlignRight();
							});
						});

						// Reason for Transfer Section
						col.Item().PaddingTop(20).Border(1).BorderColor(Colors.Grey.Medium).Padding(10)
							.Column(column =>
							{
								column.Item().Text("سبب النقل:").Bold().FontSize(14).AlignRight();
								column.Item().Height(60);
							});

						// Approval Section with Signatures
						col.Item().PaddingTop(40).Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn();
								columns.RelativeColumn();
								columns.RelativeColumn();
							});

							table.Header(header =>
							{
								header.Cell().Element(CellStyle).Text("المستلم")
									.Bold().FontSize(14).AlignCenter();
								header.Cell().Element(CellStyle).Text("الناقل")
									.Bold().FontSize(14).AlignCenter();
								header.Cell().Element(CellStyle).Text("المشرف")
									.Bold().FontSize(14).AlignCenter();
							});

							// Signature Spaces
							table.Cell().Element(CellStyle).Column(column =>
							{
								column.Item().Text("الاسم: ").AlignRight();
								column.Item().PaddingTop(20).Text("التوقيع: ").AlignRight();
								column.Item().PaddingTop(10).Text("التاريخ: ").AlignRight();
							});

							table.Cell().Element(CellStyle).Column(column =>
							{
								column.Item().Text($"الاسم: {currentUser.FullName}").AlignRight();
								column.Item().PaddingTop(20).Text("التوقيع: ").AlignRight();
								column.Item().PaddingTop(10).Text($"التاريخ: {DateTime.Now:yyyy/MM/dd}").AlignRight();
							});

							table.Cell().Element(CellStyle).Column(column =>
							{
								column.Item().Text("الاسم: ").AlignRight();
								column.Item().PaddingTop(20).Text("التوقيع: ").AlignRight();
								column.Item().PaddingTop(10).Text("التاريخ: ").AlignRight();
							});
						});

						// Notes Section
						col.Item().PaddingTop(30).Border(1).BorderColor(Colors.Grey.Medium).Padding(10)
							.Column(column =>
							{
								column.Item().Text("ملاحظات:").Bold().FontSize(14).AlignRight();
								column.Item().Height(60);
							});
					});

					// Footer
					page.Footer()
						.AlignCenter()
						.Text(x =>
						{
							x.Span("تم إنشاء هذا المستند بواسطة: ").FontSize(10);
							x.Span(currentUser.FullName).FontSize(10);
							x.Span($" - {DateTime.Now:yyyy/MM/dd HH:mm}").FontSize(10);
						});
				});
			});

			// Generate PDF
			using var stream = new MemoryStream();
			document.GeneratePdf(stream);
			return stream.ToArray();
		}

		private async Task<byte[]> GenerateMultipleTransfersDocumentAsync(
			IEnumerable<AssetTransfer> transfers,
			User currentUser)
		{
			// Set QuestPDF license
			QuestPDF.Settings.License = LicenseType.Community;

			var document = Document.Create(container =>
			{
			container.Page(page =>
			{
			page.Size(PageSizes.A4);
			page.Margin(20);
			page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

			// Header Section
			page.Header().Row(row =>
			{
				// Title and Details - Right Side for Arabic
				row.RelativeItem().Column(col =>
				{
					col.Item().Text("المملكة العربية السعودية")
						.FontSize(14).Bold().FontColor(Colors.Black).AlignRight();
					col.Item().Text("وزارة الصحة")
						.FontSize(14).Bold().FontColor(Colors.Black).AlignRight();
					col.Item().Text("مستشفى بريدة المركزي")
						.FontSize(14).Bold().FontColor(Colors.Black).AlignRight();
				});

				// Center Title
				row.RelativeItem().Column(col =>
				{
					col.Item().Text("تقرير نقل الأصول المتعددة")
						.FontSize(18).Bold().FontColor(Colors.Blue.Medium).AlignCenter();
					col.Item().Text($"التاريخ: {DateTime.Now:yyyy/MM/dd}")
						.FontSize(12).AlignCenter();
					col.Item().Text($"عدد عمليات النقل: {transfers.Count()}")
						.FontSize(12).AlignCenter();
				});

				// Logo - Left Side for Arabic
				var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
				if (System.IO.File.Exists(logoPath))
				{
					row.ConstantItem(100).Image(logoPath).FitWidth();
				}
			});

			// Content - Table of transfers
			page.Content().Column(col =>
			{
			// Summary of transfers
			col.Item().PaddingBottom(10).Text("ملخص عمليات النقل:")
				.Bold().FontSize(14).AlignRight();

			// Transfers table
			col.Item().Table(table =>
			{
				// Define columns
				table.ColumnsDefinition(columns =>
				{
					columns.ConstantColumn(40); // #
					columns.RelativeColumn(2); // Transfer Type
					columns.RelativeColumn(2); // Date 
					columns.RelativeColumn(3); // From Department
					columns.RelativeColumn(3); // To Department
					columns.RelativeColumn(2); // Asset Tag
					columns.RelativeColumn(3); // Asset Description
				});

				// Header row
				table.Header(header =>
				{
					string[] headers = new string[]
					{
									"#", "نوع النقل", "التاريخ", "من القسم",
									"إلى القسم", "رقم الأصل", "وصف الأصل"
					};

					for (int i = 0; i < headers.Length; i++)
					{
						header.Cell().Element(CellStyle).Text(headers[i])
							.Bold().AlignCenter();
					}
				});

				// Data rows
				int index = 1;
				foreach (var transfer in transfers)
				{
					// Row cells
					table.Cell().Element(CellStyle).Text(index.ToString()).AlignCenter();
					table.Cell().Element(CellStyle).Text(transfer.TransferType ?? "داخلي").AlignCenter();
					table.Cell().Element(CellStyle).Text(transfer.TransferDate.ToString("yyyy/MM/dd")).AlignCenter();
					table.Cell().Element(CellStyle).Text(transfer.FromDepartment ?? "غير محدد").AlignCenter();
					table.Cell().Element(CellStyle).Text(transfer.ToDepartment ?? "غير محدد").AlignCenter();
					table.Cell().Element(CellStyle).Text(transfer.AssetTag).AlignCenter();
					table.Cell().Element(CellStyle).Text(transfer.Asset?.AssetDescription ?? "غير محدد").AlignCenter();

					index++;
				}
			});

				// Summary statistics
				col.Item().PaddingTop(20).Table(table =>
				{
					table.ColumnsDefinition(columns =>
					{
						columns.RelativeColumn();
						columns.RelativeColumn();
					});

					// Statistics header
					table.Cell().ColumnSpan(2).Element(cell =>
					{
						cell.Border(1)
							.BorderColor(Colors.Grey.Medium)
							.Background(Colors.Grey.Lighten3)
							.Padding(5)
							.AlignCenter()
							.Text("إحصائيات النقل")
							.Bold()
							.FontSize(14);
					});

					// Count by department
					var departmentStats = transfers
						.GroupBy(t => t.ToDepartment ?? "غير محدد")
						.Select(g => new { Department = g.Key, Count = g.Count() })
						.OrderByDescending(x => x.Count);

					table.Cell().Element(cell =>
					{
						cell.Border(1)
							.BorderColor(Colors.Grey.Medium)
							.Padding(5)
							.Column(column =>
							{
								column.Item().Text("عدد النقل حسب القسم المستلم:")
									.Bold()
									.AlignRight();

								foreach (var stat in departmentStats)
								{
									column.Item().Text($"{stat.Department}: {stat.Count}")
										.AlignRight();
								}
							});
					});

					// Count by date
					var dateStats = transfers
						.GroupBy(t => t.TransferDate.Date)
						.Select(g => new { Date = g.Key, Count = g.Count() })
						.OrderByDescending(x => x.Date);

					table.Cell().Element(cell =>
					{
						cell.Border(1)
							.BorderColor(Colors.Grey.Medium)
							.Padding(5)
							.Column(column =>
							{
								column.Item().Text("عدد النقل حسب التاريخ:")
									.Bold()
									.AlignRight();

								foreach (var stat in dateStats.Take(5)) // Show top 5 dates
								{
									column.Item().Text($"{stat.Date:yyyy/MM/dd}: {stat.Count}")
										.AlignRight();
								}
							});
					});
				});

				// Approval signatures
				col.Item().PaddingTop(50).Table(table =>
				{
					table.ColumnsDefinition(columns =>
					{
						columns.RelativeColumn();
						columns.RelativeColumn();
						columns.RelativeColumn();
					});

					// Signatures header
					table.Cell().ColumnSpan(3).Element(cell =>
					{
						cell.Border(1)
							.BorderColor(Colors.Grey.Medium)
							.Background(Colors.Grey.Lighten3)
							.Padding(5)
							.AlignCenter()
							.Text("التوقيعات")
							.Bold()
							.FontSize(14);
					});

					// Signature blocks
					table.Cell().Element(CellStyle).Column(column =>
					{
						column.Item().Text("مدير القسم")
							.Bold().AlignCenter();
						column.Item().Height(40);
						column.Item().Text("الاسم: __________________")
							.AlignCenter();
						column.Item().Text("التاريخ: ________________")
							.AlignCenter();
					});

					table.Cell().Element(CellStyle).Column(column =>
					{
						column.Item().Text("منشئ التقرير")
							.Bold().AlignCenter();
						column.Item().Height(40);
						column.Item().Text($"الاسم: {currentUser.FullName}")
							.AlignCenter();
						column.Item().Text($"التاريخ: {DateTime.Now:yyyy/MM/dd}")
							.AlignCenter();
					});

					table.Cell().Element(CellStyle).Column(column =>
					{
						column.Item().Text("مدير إدارة الأصول")
							.Bold().AlignCenter();
						column.Item().Height(40);
						column.Item().Text("الاسم: __________________")
							.AlignCenter();
						column.Item().Text("التاريخ: ________________")
							.AlignCenter();
					});
				});
			});

				// Footer
				page.Footer()
					.AlignCenter()
					.Text(text =>
					{
						text.Span("صفحة ").FontSize(10);
						text.CurrentPageNumber().FontSize(10);
						text.Span(" من ").FontSize(10);
						text.TotalPages().FontSize(10);
						text.Span($" - تم إنشاء التقرير بواسطة: {currentUser.FullName} - {DateTime.Now:yyyy/MM/dd HH:mm}")
							.FontSize(10);
					});
			});
			});

			// Generate PDF
			using var stream = new MemoryStream();
			document.GeneratePdf(stream);
			return stream.ToArray();
		}

		// Helper method for cell styling
		private IContainer CellStyle(IContainer container)
		{
			return container
				.Border(1)
				.BorderColor(Colors.Grey.Medium)
				.Padding(5);
		}
		// Generate PDF for all transfers
		[HttpGet]
		public async Task<IActionResult> GenerateAllTransfersPdf()
		{
			try
			{
				// Get current user
				var currentUser = await GetCurrentUserAsync();
				if (currentUser == null)
				{
					return Unauthorized();
				}

				// Get all transfers
				var transfers = await _assetTransferService.GetAllAssetTransfersAsync();

				if (!transfers.Any())
				{
					TempData["ErrorMessage"] = "لا توجد عمليات نقل لتصديرها";
					return RedirectToAction(nameof(Index));
				}

				// Generate PDF
				var pdfBytes = await GenerateMultipleTransfersDocumentAsync(transfers, currentUser);

				// Return PDF
				return File(pdfBytes, "application/pdf", $"AllAssetTransfers_{DateTime.Now:yyyyMMdd}.pdf");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error generating PDF for all transfers");
				TempData["ErrorMessage"] = "حدث خطأ أثناء إنشاء ملف PDF";
				return RedirectToAction(nameof(Index));
			}
		}

		// Generate PDF by date range - Modified to work with GET requests
		[HttpGet]
		public async Task<IActionResult> GenerateTransfersPdfByDateRange(DateTime startDate, DateTime endDate)
		{
			try
			{
				if (startDate > endDate)
				{
					TempData["ErrorMessage"] = "تاريخ البداية يجب أن يكون قبل تاريخ النهاية";
					return RedirectToAction(nameof(Index));
				}

				// Get current user
				var currentUser = await GetCurrentUserAsync();
				if (currentUser == null)
				{
					return Unauthorized();
				}

				// Fetch all transfers in the date range
				var allTransfers = await _assetTransferService.GetAllAssetTransfersAsync();
				var transfers = allTransfers
					.Where(t => t.TransferDate.Date >= startDate.Date &&
								t.TransferDate.Date <= endDate.Date)
					.ToList();

				if (!transfers.Any())
				{
					TempData["ErrorMessage"] = "لم يتم العثور على عمليات نقل في الفترة المحددة";
					return RedirectToAction(nameof(Index));
				}

				// Generate combined PDF
				var pdfBytes = await GenerateMultipleTransfersDocumentAsync(transfers, currentUser);

				// Return PDF file
				return File(pdfBytes, "application/pdf",
					$"AssetTransfers_{startDate:yyyyMMdd}_to_{endDate:yyyyMMdd}.pdf");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error generating transfers PDF by date range");
				TempData["ErrorMessage"] = "حدث خطأ أثناء إنشاء ملف PDF حسب النطاق الزمني";
				return RedirectToAction(nameof(Index));
			}
		}

		// Modified to work with form POST for multiple IDs
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> GenerateMultipleTransfersPdf(int[] ids)
		{
			try
			{
				if (ids == null || !ids.Any())
				{
					TempData["ErrorMessage"] = "لم يتم تحديد أي عمليات نقل";
					return RedirectToAction(nameof(Index));
				}

				// Get current user
				var currentUser = await GetCurrentUserAsync();
				if (currentUser == null)
				{
					return Unauthorized();
				}

				// Fetch all selected transfers
				var transfers = new List<AssetTransfer>();
				foreach (var id in ids)
				{
					var transfer = await _assetTransferService.GetAssetTransferByIdAsync(id);
					if (transfer != null)
					{
						transfers.Add(transfer);
					}
				}

				if (!transfers.Any())
				{
					TempData["ErrorMessage"] = "لم يتم العثور على عمليات النقل المحددة";
					return RedirectToAction(nameof(Index));
				}

				// Generate combined PDF
				var pdfBytes = await GenerateMultipleTransfersDocumentAsync(transfers, currentUser);

				// Return PDF file
				return File(pdfBytes, "application/pdf", $"SelectedTransfers_{DateTime.Now:yyyyMMdd}.pdf");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error generating multiple transfers PDF");
				TempData["ErrorMessage"] = "حدث خطأ أثناء إنشاء ملف PDF المجمع";
				return RedirectToAction(nameof(Index));
			}
		}
	}

	public class DateRangeRequest
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}