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

		public async Task<Dictionary<int, ReturnDocument>> CreateReturnDocumentsAsync(List<string> assetTags, string returnReason, string returnCommittee)
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

				// Create new return document
				var returnDocument = new ReturnDocument
				{
					DocumentNumber = await _returnDocumentRepository.GenerateDocumentNumberAsync(),
					ReturnDate = DateTime.Now,
					DepartmentId = departmentId,
					ReturningDepartment = firstAsset.Department.Name,
					ResponsiblePerson = "محمد عبدالكريم الحميد", // Static for now, could be dynamic
					StoreKeeper = "ناصر العمري", // Static
					ReturnCommittee = returnCommittee, // Use the provided committee (supervisors)
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
							Quantity = 1,
							Unit = "الوحدة"
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
					page.Margin(15); // Reduced margin
					page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial")); // Smaller default font

					// Header Section with compact layout
					page.Header().Row(row =>
					{
						// Logo/Right side
						row.ConstantItem(100).Column(col =>
						{
							col.Item().AlignRight().Text("المملكة العربية السعودية")
								.FontSize(12).Bold().FontColor(Colors.Black);
							col.Item().AlignRight().Text("وزارة الصحة")
								.FontSize(11).FontColor(Colors.Black);
							col.Item().AlignRight().Text("مستشفى بريدة المركزي")
								.FontSize(11).FontColor(Colors.Black);
						});

						// Center title
						row.RelativeItem().AlignCenter().Column(col =>
						{
							col.Item().AlignCenter().Text("مستند ارجاع")
								.FontSize(18).Bold().FontColor(Colors.Blue.Medium);
							col.Item().AlignCenter().Text("RETURN DOCUMENT")
								.FontSize(12).FontColor(Colors.Grey.Medium);
							col.Item().AlignCenter().Text($"الجهة المرجعة: {document.ReturningDepartment}")
								.FontSize(10);
						});

						// Document info
						row.ConstantItem(120).Column(col =>
						{
							col.Item().AlignLeft().Text($"رقم المستند: {document.DocumentNumber}")
								.FontSize(10);
							col.Item().AlignLeft().Text($"التاريخ: {document.ReturnDate:yyyy-MM-dd}")
								.FontSize(10);
							col.Item().AlignLeft().Text($"رقم الصفحة: 1/1")
								.FontSize(10);
						});
					});

					// Content Section with compact styling
					page.Content().PaddingVertical(10).Column(col =>
					{
						// Store Name header - made more compact
						col.Item().Background(Colors.Grey.Lighten3).Border(1).BorderColor(Colors.Grey.Medium)
							.Padding(5).AlignCenter().Text("مستودع الرجيع")
							.Bold().FontSize(12);

						// Return Reasons Table with compact styling
						col.Item().PaddingTop(8).Table(table =>
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
								header.Cell().ColumnSpan(4).Element(cell => cell
									.Border(1).BorderColor(Colors.Grey.Medium)
									.Background(Colors.Blue.Lighten4)
									.Padding(4)
									.AlignCenter()
									.Text("أسباب الارجاع").SemiBold().FontSize(11));
							});

							// Reasons with compact checkbox styling
							string[] reasons = { "تالف", "فائض", "عدم الصلاحية", "انتهاء الغرض" };
							foreach (var reason in reasons)
							{
								table.Cell().Element(cell => cell
									.Border(1).BorderColor(Colors.Grey.Medium)
									.Padding(4)
									.Row(row =>
									{
										row.RelativeItem().AlignCenter().Text(reason).FontSize(10);

										// Add a styled checkmark if this is the selected reason
										if (document.ReturnReason == reason)
										{
											row.ConstantItem(20).AlignCenter().Background(Colors.Green.Lighten4)
												.Border(1).BorderColor(Colors.Green.Medium).Padding(1)
												.Text("✓").FontSize(12).Bold().FontColor(Colors.Green.Medium);
										}
										else
										{
											row.ConstantItem(20).AlignCenter()
												.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(1);
										}
									}));
							}
						});

						// Assets Table with compact styling
						col.Item().PaddingTop(8).Table(table =>
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

							// Headers with compact styling
							table.Header(header =>
							{
								string[] headerTitles = { "الرقم", "رقم الصنف", "اسم الصنف", "الوحدة", "الكمية", "للإصلاح", "للبيع", "للإتلاف" };

								foreach (var title in headerTitles)
								{
									header.Cell().Element(cell => cell
										.Border(1).BorderColor(Colors.Grey.Medium)
										.Background(Colors.Blue.Lighten4)
										.Padding(4)
										.AlignCenter()
										.Text(title).SemiBold().FontSize(10));
								}
							});

							// Compact assets rows
							int index = 1;
							foreach (var item in document.Items)
							{
								var rowBackground = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;

								// Number column
								table.Cell().Element(cell => cell
									.Border(1).BorderColor(Colors.Grey.Medium)
									.Background(rowBackground)
									.Padding(3)
									.AlignCenter()
									.Text(index.ToString()).FontSize(9));

								// Asset Tag column
								table.Cell().Element(cell => cell
									.Border(1).BorderColor(Colors.Grey.Medium)
									.Background(rowBackground)
									.Padding(3)
									.AlignCenter()
									.Text(item.AssetTag).FontSize(9));

								// Description column - truncate if too long
								table.Cell().Element(cell => cell
									.Border(1).BorderColor(Colors.Grey.Medium)
									.Background(rowBackground)
									.Padding(3)
									.AlignCenter()
									.Text(TruncateText(item.AssetDescription, 40)).FontSize(9));

								// Unit column
								table.Cell().Element(cell => cell
									.Border(1).BorderColor(Colors.Grey.Medium)
									.Background(rowBackground)
									.Padding(3)
									.AlignCenter()
									.Text(item.Unit).FontSize(9));

								// Quantity column
								table.Cell().Element(cell => cell
									.Border(1).BorderColor(Colors.Grey.Medium)
									.Background(rowBackground)
									.Padding(3)
									.AlignCenter()
									.Text(item.Quantity.ToString()).FontSize(9));

								// Create checkmark cells based on reason
								string reason = document.ReturnReason?.ToLower();

								// للإصلاح (For repair) column
								table.Cell().Element(cell => cell
									.Border(1).BorderColor(Colors.Grey.Medium)
									.Background(rowBackground)
									.Padding(3)
									.AlignCenter()
									.Text(reason == "تالف" ? "✓" : "")
									.FontSize(12).Bold());

								// للبيع (For sale) column
								table.Cell().Element(cell => cell
									.Border(1).BorderColor(Colors.Grey.Medium)
									.Background(rowBackground)
									.Padding(3)
									.AlignCenter()
									.Text(reason == "فائض" ? "✓" : "")
									.FontSize(12).Bold());

								// للإتلاف (For disposal) column
								table.Cell().Element(cell => cell
									.Border(1).BorderColor(Colors.Grey.Medium)
									.Background(rowBackground)
									.Padding(3)
									.AlignCenter()
									.Text((reason == "عدم الصلاحية" || reason == "انتهاء الغرض") ? "✓" : "")
									.FontSize(12).Bold());

								index++;
							}
						});

						// Signatures section with compact styling
						col.Item().PaddingTop(10).Table(table =>
						{
							table.ColumnsDefinition(cols =>
							{
								cols.RelativeColumn();
								cols.RelativeColumn();
								cols.RelativeColumn();
								cols.RelativeColumn();
							});

							// Headers
							string[] signatureHeaders = {
						"المسؤول في الجهة المرجعة",
						"المستلم/امين المستودع",
						"مدير إدارة المستودعات",
						"لجنة فحص الرجيع"
					};

							foreach (var header in signatureHeaders)
							{
								table.Cell().Element(cell => cell
									.Border(1).BorderColor(Colors.Grey.Medium)
									.Background(Colors.Blue.Lighten4)
									.Padding(4)
									.AlignCenter()
									.Text(header).SemiBold().FontSize(9));
							}

							// Names row
							table.Cell().Element(cell => cell
								.Border(1).BorderColor(Colors.Grey.Medium)
								.Padding(4)
								.AlignCenter()
								.Text("الاسم").SemiBold().FontSize(9));

							table.Cell().Element(cell => cell
								.Border(1).BorderColor(Colors.Grey.Medium)
								.Padding(4)
								.AlignCenter()
								.Text(document.ResponsiblePerson).FontSize(9));

							table.Cell().Element(cell => cell
								.Border(1).BorderColor(Colors.Grey.Medium)
								.Padding(4)
								.AlignCenter()
								.Text(document.StoreKeeper).FontSize(9));

							// Committee members in a compact style
							table.Cell().Element(cell => cell
								.Border(1).BorderColor(Colors.Grey.Medium)
								.Padding(4)
								.Column(column =>
								{
									if (!string.IsNullOrEmpty(document.ReturnCommittee))
									{
										var lines = document.ReturnCommittee.Split('\n');
										foreach (var line in lines)
										{
											column.Item().AlignCenter().Text(line).FontSize(9);
										}
									}
								}));

							// Signature row with minimal height
							table.Cell().Element(cell => cell
								.Border(1).BorderColor(Colors.Grey.Medium)
								.Padding(4)
								.AlignCenter()
								.Text("التوقيع").SemiBold().FontSize(9));

							// Empty signature cells
							for (int i = 0; i < 3; i++)
							{
								table.Cell().Element(cell => cell
									.Border(1).BorderColor(Colors.Grey.Medium)
									.MinHeight(25)); // Reduced height
							}

							// Date row
							table.Cell().Element(cell => cell
								.Border(1).BorderColor(Colors.Grey.Medium)
								.Padding(4)
								.AlignCenter()
								.Text("التاريخ").SemiBold().FontSize(9));

							// Empty date cells
							for (int i = 0; i < 3; i++)
							{
								table.Cell().Element(cell => cell
									.Border(1).BorderColor(Colors.Grey.Medium)
									.MinHeight(20)); // Reduced height
							}
						});

						// Hospital Manager section in a compact format
						col.Item().PaddingTop(10).AlignCenter().Row(row =>
						{
							row.RelativeItem(2);
							row.RelativeItem(4).Border(1).BorderColor(Colors.Grey.Medium)
								.Background(Colors.Blue.Lighten5)
								.Padding(5)
								.Column(column =>
								{
									column.Item().AlignCenter().Text("مدير مستشفى بريدة المركزي").SemiBold().FontSize(10);
									column.Item().AlignCenter().Text(document.AuthorityPerson).FontSize(10);
								});
							row.RelativeItem(2);
						});

						// Inventory Control section at the bottom in a compact format
						col.Item().PaddingTop(8).Row(row =>
						{
							row.RelativeItem().AlignRight().Column(column =>
							{
								column.Item().Text("مراقبة المخزون").SemiBold().FontSize(9);
								column.Item().Text("ناصر العميري").FontSize(9);
							});
						});
					});

					// Footer with compact styling
					page.Footer().AlignCenter().Text(text =>
					{
						text.Span("صفحة 1 من 1 | تم الإنشاء في: ")
							.FontSize(8).FontColor(Colors.Grey.Medium);

						text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
							.FontSize(8).FontColor(Colors.Grey.Medium);
					});
				});
			});

			// Generate PDF
			using var stream = new MemoryStream();
			pdfDocument.GeneratePdf(stream);
			return stream.ToArray();
		}

		// Helper method to truncate text if it's too long
		private string TruncateText(string text, int maxLength)
		{
			if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
				return text ?? string.Empty;

			return text.Substring(0, maxLength) + "...";
		}

		// Cell styling helper
		private IContainer CellStyle(IContainer container)
		{
			return container
				.Border(1)
				.BorderColor(Colors.Grey.Medium)
				.Padding(5);
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

		 
	}
}