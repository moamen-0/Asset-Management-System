using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Services
{
	public class ReturnDocumentService : IReturnDocumentService
	{
		private readonly IReturnDocumentRepository _returnDocumentRepository;
		private readonly IAssetRepository _assetRepository;
		private readonly IStoreKeeperRepository _storeKeeperRepository;
		private readonly ILogger<ReturnDocumentService> _logger;
		private readonly IUserRepository _userRepository;

		public ReturnDocumentService(
			IReturnDocumentRepository returnDocumentRepository,
			IAssetRepository assetRepository,
			IStoreKeeperRepository storeKeeperRepository,
			IUserRepository userRepository,
			ILogger<ReturnDocumentService> logger)
		{
			_returnDocumentRepository = returnDocumentRepository;
			_assetRepository = assetRepository;
			_storeKeeperRepository = storeKeeperRepository;
			_userRepository = userRepository;
			_logger = logger;
		}

		public async Task<IEnumerable<ReturnDocument>> GetAllDocumentsAsync()
		{
			return await _returnDocumentRepository.GetAllAsync();
		}

		public async Task<ReturnDocument> GetDocumentByIdAsync(int id)
		{
			return await _returnDocumentRepository.GetByIdAsync(id);
		}

		public async Task<Dictionary<int, ReturnDocument>> CreateReturnDocumentsAsync(List<string> assetTags, string returnReason)
		{
			// Group assets by department
			var assetsByDepartment = await GroupAssetsByDepartmentAsync(assetTags);
			var createdDocuments = new Dictionary<int, ReturnDocument>();

			foreach (var departmentAssets in assetsByDepartment)
			{
				var departmentId = departmentAssets.Key;
				var departmentAssetTags = departmentAssets.Value;

				// Get department info
				var firstAsset = await _assetRepository.GetByIdAsync(departmentAssetTags.First());
				if (firstAsset == null || firstAsset.Department == null)
				{
					_logger.LogWarning("Department info not found for asset: {AssetTag}", departmentAssetTags.First());
					continue;
				}

				// Determine the committee members based on asset tags
				string committeeMembers = await DetermineCommitteeMembersAsync(departmentAssetTags);

				// Create new return document
				var returnDocument = new ReturnDocument
				{
					DocumentNumber = await _returnDocumentRepository.GenerateDocumentNumberAsync(),
					ReturnDate = DateTime.Now,
					DepartmentId = departmentId,
					ReturningDepartment = firstAsset.Department.Name,
					ResponsiblePerson = "محمد عبدالكريم الحميد", // Static for now, could be dynamic
					StoreKeeper = "ناصر العمري", // Static
					ReturnCommittee = committeeMembers,
					ReturnReason = returnReason,
					Items = new List<ReturnDocumentItem>()
				};

				// Add assets as items
				foreach (var assetTag in departmentAssetTags)
				{
					var asset = await _assetRepository.GetByIdAsync(assetTag);
					if (asset != null)
					{
						returnDocument.Items.Add(new ReturnDocumentItem
						{
							AssetTag = asset.AssetTag,
							AssetDescription = asset.AssetDescription ?? "No Description",
							Quantity = 1
						});
					}
				}

				// Save the document
				var savedDocument = await _returnDocumentRepository.AddAsync(returnDocument);
				createdDocuments.Add(savedDocument.Id, savedDocument);
			}

			return createdDocuments;
		}

		public async Task UpdateReturnDocumentAsync(ReturnDocument document)
		{
			await _returnDocumentRepository.UpdateAsync(document);
		}

		public async Task DeleteReturnDocumentAsync(int id)
		{
			await _returnDocumentRepository.DeleteAsync(id);
		}

		public async Task<byte[]> GeneratePdfAsync(int documentId)
		{
			// Get the return document
			var document = await _returnDocumentRepository.GetByIdAsync(documentId);
			if (document == null)
			{
				throw new ArgumentException("Document not found");
			}

			// Enable QuestPDF license
			QuestPDF.Settings.License = LicenseType.Community;

			// Create PDF document
			var pdfDocument = QuestPDF.Fluent.Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Size(PageSizes.A4.Landscape());
					page.Margin(20);
					page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

					// Header Section
					page.Header().Row(row =>
					{
						// Title - Right Side (Arabic text flows right-to-left)
						row.RelativeItem().Column(col =>
						{
							col.Item().Text("المملكة العربية السعودية")
								.FontSize(14).Bold().FontColor(Colors.Black).AlignRight();
							col.Item().Text("وزارة الصحة")
								.FontSize(14).Bold().FontColor(Colors.Black).AlignRight();
							col.Item().Text("مستشفى بريدة المركزي")
								.FontSize(14).Bold().FontColor(Colors.Black).AlignRight();
							col.Item().Text($"الجهة المرجعة/ {document.ReturningDepartment}")
								.FontSize(12).AlignRight();
						});

						// Report Title - Center
						row.RelativeItem().Column(col =>
						{
							col.Item().Text("مستند ارجاع")
								.FontSize(22).Bold().AlignCenter();
						});

						// Document Info - Left Side
						row.RelativeItem().Column(col =>
						{
							col.Item().Text($"الرقم المتسلسل:")
								.FontSize(12).AlignLeft();
							col.Item().Text($"التاريخ: {document.DocumentNumber}")
								.FontSize(12).AlignLeft();
							col.Item().Text("عدد الصفحات")
								.FontSize(12).AlignLeft();
						});
					});

					// Content Section - All content goes here in a single Content() call
					page.Content().PaddingVertical(10).Column(col =>
					{
						// Store Name
						col.Item().Text("مستودع الرجيع")
							.Bold().FontSize(14).AlignCenter();

						// Return Reasons Table
						col.Item().PaddingTop(10).Table(table =>
						{
							table.ColumnsDefinition(cols =>
							{
								cols.RelativeColumn();
								cols.RelativeColumn();
								cols.RelativeColumn();
								cols.RelativeColumn();
							});

							// Header
							table.Header(header =>
							{
								header.Cell().Text("أسباب الارجاع").AlignCenter().Bold();
								header.Cell().Column(2).Row(1).Element(c => c.Background(Colors.Grey.Lighten3));
							});

							// Reasons
							table.Cell().Text("تالف").AlignCenter();
							table.Cell().Text("فائض").AlignCenter();
							table.Cell().Text("عدم الصلاحية").AlignCenter();
							table.Cell().Text("انتهاء الغرض").AlignCenter();
						});

						// Assets Table
						col.Item().PaddingTop(20).Table(table =>
						{
							table.ColumnsDefinition(cols =>
							{
								cols.ConstantColumn(30); // Number
								cols.RelativeColumn(2); // Asset Tag
								cols.RelativeColumn(3); // Description
								cols.RelativeColumn(1); // Unit
								cols.RelativeColumn(1); // Quantity
								cols.RelativeColumn(1); // للإصلاح
								cols.RelativeColumn(1); // للبيع
								cols.RelativeColumn(1); // للإتلاف
							});

							// Headers
							table.Header(header =>
							{
								header.Cell().Element(CellStyle).Text("الرقم").AlignCenter();
								header.Cell().Element(CellStyle).Text("رقم الصنف").AlignCenter();
								header.Cell().Element(CellStyle).Text("اسم الصنف").AlignCenter();
								header.Cell().Element(CellStyle).Text("الوحدة").AlignCenter();
								header.Cell().Element(CellStyle).Text("الكمية").AlignCenter();
								header.Cell().Element(CellStyle).Text("للإصلاح").AlignCenter();
								header.Cell().Element(CellStyle).Text("للبيع").AlignCenter();
								header.Cell().Element(CellStyle).Text("للإتلاف").AlignCenter();
							});

							// Assets
							int index = 1;
							foreach (var item in document.Items)
							{
								table.Cell().Element(CellStyle).Text(index.ToString()).AlignCenter();
								table.Cell().Element(CellStyle).Text(item.AssetTag).AlignCenter();
								table.Cell().Element(CellStyle).Text(item.AssetDescription).AlignCenter();
								table.Cell().Element(CellStyle).Text(item.Unit).AlignCenter();
								table.Cell().Element(CellStyle).Text(item.Quantity.ToString()).AlignCenter();

								// Add empty cells for checkboxes
								table.Cell().Element(CellStyle).Text("");
								table.Cell().Element(CellStyle).Text("");
								table.Cell().Element(CellStyle).Text("");

								index++;
							}
						});

						// Signatures Section
						col.Item().PaddingTop(40).Table(table =>
						{
							table.ColumnsDefinition(cols =>
							{
								cols.RelativeColumn();
								cols.RelativeColumn();
								cols.RelativeColumn();
								cols.RelativeColumn();
								cols.RelativeColumn();
							});

							// Headers
							table.Cell().Element(CellStyle).Text("المسؤول في الجهة المرجعة").AlignCenter();
							table.Cell().Element(CellStyle).Text("المستلم/امين المستودع").AlignCenter();
							table.Cell().Element(CellStyle).Text("مدير إدارة المستودعات").AlignCenter();
							table.Cell().Element(CellStyle).Text("لجنة فحص الرجيع").AlignCenter();
							table.Cell().Element(CellStyle).Text("صاحب الصلاحية").AlignCenter();

							// Names
							table.Cell().Element(CellStyle).Text("الاسم").AlignCenter();
							table.Cell().Element(CellStyle).Text(document.ResponsiblePerson).AlignCenter();
							table.Cell().Element(CellStyle).Text(document.StoreKeeper).AlignCenter();
							table.Cell().Element(CellStyle).Text(document.WarehouseManager).AlignCenter();
							table.Cell().Element(CellStyle).Text(document.ReturnCommittee).AlignCenter();
							table.Cell().Element(CellStyle).Text(document.AuthorityPerson).AlignCenter();

							// Signatures
							table.Cell().Element(CellStyle).Text("التوقيع").AlignCenter();
							table.Cell().Element(CellStyle).Text("1431343").AlignCenter(); // Sample ID/Signature
							table.Cell().Element(CellStyle).Text("").AlignCenter();
							table.Cell().Element(CellStyle).Text("").AlignCenter();
							table.Cell().Element(CellStyle).Text("").AlignCenter();
							table.Cell().Element(CellStyle).Text("").AlignCenter();

							// Dates
							table.Cell().Element(CellStyle).Text("التاريخ").AlignCenter();
							table.Cell().Element(CellStyle).Text("").AlignCenter();
							table.Cell().Element(CellStyle).Text("").AlignCenter();
							table.Cell().Element(CellStyle).Text("").AlignCenter();
							table.Cell().Element(CellStyle).Text("").AlignCenter();
						});

						// Authority Title
						col.Item().PaddingTop(20).Text($"مدير مستشفى بريدة المركزي\n{document.AuthorityPerson}")
							.AlignCenter().Bold();

						// Inventory Control Signature - Now part of the Content section
						col.Item().PaddingTop(20).Text("مراقبة المخزون").Bold().FontSize(12);
						col.Item().Text("ناصر العميري").Bold().FontSize(12);
					});

					// Footer could be defined here if needed
					// page.Footer()...
				});
			});

			// Generate PDF
			using var stream = new MemoryStream();
			pdfDocument.GeneratePdf(stream);
			return stream.ToArray();
		}

		public async Task<Dictionary<int, List<string>>> GroupAssetsByDepartmentAsync(List<string> assetTags)
		{
			var result = new Dictionary<int, List<string>>();

			foreach (var assetTag in assetTags)
			{
				var asset = await _assetRepository.GetByIdAsync(assetTag);
				if (asset == null)
				{
					_logger.LogWarning("Asset not found: {AssetTag}", assetTag);
					continue;
				}

				if (!result.ContainsKey(asset.DepartmentId))
				{
					result[asset.DepartmentId] = new List<string>();
				}

				result[asset.DepartmentId].Add(assetTag);
			}

			return result;
		}

		private async Task<string> DetermineCommitteeMembersAsync(List<string> assetTags)
		{
			// Get asset types or prefixes to determine the committee members
			// This is based on your example where committee members change based on assets

			// Get the first asset tag's prefix (e.g., "C3-101" from "C3-101-0004973")
			string firstAssetPrefix = GetAssetPrefix(assetTags.First());

			// Get committee members based on asset prefix
			if (firstAssetPrefix.StartsWith("C3-101"))
			{
				return "خالد مهدي الحربي\nمحمد عبدالهادي الحربي";
			}
			else if (firstAssetPrefix.StartsWith("C3-102"))
			{
				return "عبدالكريم السنيدي\nخالد السهيل";
			}
			else
			{
				return "نواف الحربي\nعبدالرحمن العتيبي";
			}
		}

		private string GetAssetPrefix(string assetTag)
		{
			var parts = assetTag.Split('-');
			if (parts.Length >= 2)
			{
				return $"{parts[0]}-{parts[1]}";
			}
			return assetTag;
		}

		// Cell styling helper
		private IContainer CellStyle(IContainer container)
		{
			return container
				.Border(1)
				.BorderColor(Colors.Grey.Medium)
				.Padding(5);
		}
	}
}