using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AssetManagementSystem.BLL.Interfaces.IService;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ReportsController : Controller
{
	private readonly IAssetService _assetService;

	public ReportsController(IAssetService assetService)
	{
		_assetService = assetService;
	}

	[HttpGet]
	public async Task<IActionResult> ExportAssetsToPdf()
	{
		// Set QuestPDF license type
		QuestPDF.Settings.License = LicenseType.Community;

		try
		{
			// Fetch data from the database
			var assets = await _assetService.GetAllAssetsAsync();

			// Create the PDF document
			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Size(PageSizes.A4);
					page.Margin(20);
					page.DefaultTextStyle(x => x.FontSize(12));

					// Header Section
					page.Header()
						.Row(row =>
						{
							// Logo (if exists)
							var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
							if (System.IO.File.Exists(logoPath))
							{
								row.ConstantItem(100)
.Image(logoPath) // Load logo from path
.FitWidth();
							}

							// Report Title and Date
							row.RelativeItem()
.Column(col =>
{
  col.Item().Text("Asset Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
  col.Item().Text($"Generated on: {System.DateTime.Now:yyyy-MM-dd HH:mm}");
});
						});

					// Content Section
					page.Content()
						.Table(table =>
						{
							// Define table columns
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn();
								columns.RelativeColumn();
								columns.RelativeColumn();
								columns.RelativeColumn();
							});

							// Table Header
							table.Header(header =>
							{
								header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Asset ID").Bold();
								header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Asset Name").Bold();
								header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Department").Bold();
								header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Status").Bold();
							});

							// Table Rows (Dynamic Data)
							foreach (var asset in assets)
							{
								table.Cell().Element(CellStyle).Text(asset.AssetTag);
								table.Cell().Element(CellStyle).Text(asset.AssetDescription);
								table.Cell().Element(CellStyle).Text(asset.Department?.Name ?? "N/A");
								table.Cell().Element(CellStyle).Text(asset.Status);
							}
						});

					// Footer Section
					page.Footer()
						.AlignCenter()
						.Text("© 2025 Your Company Name. All Rights Reserved.")
						.FontSize(10)
						.FontColor(Colors.Grey.Medium);
				});
			});

			// Generate PDF and return as a file
			var stream = new MemoryStream();
			document.GeneratePdf(stream);
			stream.Position = 0; // Reset stream position
			return File(stream, "application/pdf", "Assets.pdf");
		}
		catch (Exception ex)
		{
			// Log the exception (you can use a logging framework like Serilog or NLog)
			Console.WriteLine($"Error generating PDF: {ex.Message}");

			// Return an error response
			return StatusCode(500, "An error occurred while generating the PDF. Please try again later.");
		}
	}


	[HttpGet("export/excel")]
	public async Task<IActionResult> ExportAssetsToExcel()
	{
		try
		{
			// Fetch data from the database
			var assets = await _assetService.GetAllAssetsAsync();

			// Create a new Excel package
			using var package = new ExcelPackage();
			var worksheet = package.Workbook.Worksheets.Add("Assets Report");

			// 🔹 Add logo
			var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
			if (System.IO.File.Exists(logoPath))
			{
				var logo = worksheet.Drawings.AddPicture("Logo", new FileInfo(logoPath));
				logo.SetPosition(0, 0, 0, 0); // Set logo position at the top
				logo.SetSize(120, 60); // Set logo size
			}

			// 🔹 Report title
			worksheet.Cells["A1:D1"].Merge = true;
			worksheet.Cells["A1"].Value = "Asset Report";
			worksheet.Cells["A1"].Style.Font.Size = 18;
			worksheet.Cells["A1"].Style.Font.Bold = true;
			worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
			worksheet.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.Blue);

			// 🔹 Table header
			var headers = new List<string> { "Asset ID", "Asset Name", "Department", "Status" };
			for (int i = 0; i < headers.Count; i++)
			{
				var cell = worksheet.Cells[3, i + 1];
				cell.Value = headers[i];
				cell.Style.Font.Bold = true;
				cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
				cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
				cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
			}

			// 🔹 Fill data
			int row = 4;
			foreach (var asset in assets)
			{
				worksheet.Cells[row, 1].Value = asset.AssetTag;
				worksheet.Cells[row, 2].Value = asset.AssetDescription;
				worksheet.Cells[row, 3].Value = asset.Department?.Name ?? "N/A";
				worksheet.Cells[row, 4].Value = asset.Status;
				row++;
			}

			// 🔹 Auto-fit columns
			worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

			// 🔹 Save file and return for download
			var stream = new MemoryStream();
			package.SaveAs(stream);
			stream.Position = 0; // Reset stream position
			return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Assets.xlsx");
		}
		catch (Exception ex)
		{
			// Log the exception (you can use a logging framework like Serilog or NLog)
			Console.WriteLine($"Error generating Excel file: {ex.Message}");

			// Return an error response
			return StatusCode(500, "An error occurred while generating the Excel file. Please try again later.");
		}
	}
	// Helper method for table cell styling
	static IContainer CellStyle(IContainer container)
	{
		return container.Padding(5);
	}
}
