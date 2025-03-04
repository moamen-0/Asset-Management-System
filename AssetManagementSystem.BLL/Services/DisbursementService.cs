using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;
using AssetManagementSystem.BLL.Repositories;
using AssetManagementSystem.DAL.Utilities;

namespace AssetManagementSystem.BLL.Services
{
	public class DisbursementService : IDisbursementService
	{
		private readonly IDisbursementRepository _disbursementRepository;
		private readonly IAssetRepository _assetRepository;
		private readonly IStoreKeeperRepository _storeKeeperRepository;
		private readonly IUserRepository _userRepository ;

		public DisbursementService(
			IDisbursementRepository disbursementRepository,
			IAssetRepository assetRepository,
			IStoreKeeperRepository storeKeeperRepository,
			IUserRepository userRepository)
		{
			_disbursementRepository = disbursementRepository;
			_assetRepository = assetRepository;
			_storeKeeperRepository = storeKeeperRepository;
			_userRepository = userRepository;
		}

		public async Task<List<DisbursementRequest>> GetAllRequestsAsync()
		{
			return await _disbursementRepository.GetAllAsync();
		}

		public async Task<DisbursementRequest> GetRequestByIdAsync(int id)
		{
			return await _disbursementRepository.GetByIdAsync(id);
		}

		public async Task<DisbursementRequest> CreateRequestAsync(DisbursementRequest request, List<string> assetTags)
		{
			// Generate request number
			request.RequestNumber = await _disbursementRepository.GenerateRequestNumberAsync();
			request.RequestDate = DateTime.Now;

			// Get assets and create items
			var assets = await GetAssetsByTagsAsync(assetTags);
			request.Items = assets.Select(asset => new DisbursementItem
			{
				AssetTag = asset.AssetTag,
				AssetDescription = asset.AssetDescription,
				Brand = asset.Brand,
				Quantity = 1,
				Unit = "وحدة",
				UnitPrice = 0, // You might want to set a real price
				TotalPrice = 0, // Calculate based on price and quantity
				DisbursementType = "مستديم" // Default value, can be changed
			}).ToList();

			// Set StoreKeeper based on asset tags
			request.StoreKeeper = await GetStoreKeeperForAssetsAsync(assetTags);

			// Save to database
			await _disbursementRepository.AddAsync(request);
			return request;
		}

		public async Task UpdateRequestAsync(DisbursementRequest request, List<string> assetTags)
		{
			// Get the existing request
			var existingRequest = await _disbursementRepository.GetByIdAsync(request.Id);
			if (existingRequest == null)
			{
				throw new ArgumentException($"Request with ID {request.Id} not found");
			}

			// Update basic properties
			existingRequest.Department = request.Department;

			// Get assets and recreate items
			var assets = await GetAssetsByTagsAsync(assetTags);

			// Remove existing items
			existingRequest.Items.Clear();

			// Add new items
			foreach (var asset in assets)
			{
				existingRequest.Items.Add(new DisbursementItem
				{
					AssetTag = asset.AssetTag,
					AssetDescription = asset.AssetDescription,
					Brand = asset.Brand,
					Quantity = 1,
					Unit = "وحدة",
					UnitPrice = 0,
					TotalPrice = 0,
					DisbursementType = "مستديم",
					DisbursementRequestId = existingRequest.Id
				});
			}

			// Update store keeper based on new asset tags
			existingRequest.StoreKeeper = await GetStoreKeeperForAssetsAsync(assetTags);

			// Save changes
			await _disbursementRepository.UpdateAsync(existingRequest);
		}
		public async Task DeleteRequestAsync(int id)
		{
			await _disbursementRepository.DeleteAsync(id);
		}

		public async Task<byte[]> GeneratePdfAsync(int requestId)
		{
			var request = await _disbursementRepository.GetByIdAsync(requestId);
			if (request == null)
			{
				throw new ArgumentException("Request not found");
			}

			// Enable QuestPDF license
			QuestPDF.Settings.License = LicenseType.Community;

			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Size(PageSizes.A4.Landscape());
					page.Margin(30);
					page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

					// Header Section
					page.Header().AlignCenter().Column(column =>
					{
						column.Spacing(5);
						column.Item().Row(row =>
						{
							row.RelativeItem().Text("المملكة العربية السعودية")
								.FontSize(14).Bold().FontColor(Colors.Black);
							row.RelativeItem().Text("وزارة الصحة")
								.FontSize(14).Bold().FontColor(Colors.Black).AlignRight();
						});

						column.Item().Text("طلب صرف مواد")
							.FontSize(18).Bold().FontColor(Colors.Red.Medium).AlignCenter();

						column.Item().Row(row =>
						{
							row.RelativeItem().Text($"التاريخ: {request.RequestDate:MM/dd/yyyy}")
								.FontSize(12).AlignLeft();
							row.RelativeItem().Text("مستشفى بريدة المركزي")
								.FontSize(12).AlignRight();
						});

						column.Item().Row(row =>
						{
							row.RelativeItem().Text($"رقم الطلب: {request.RequestNumber}")
								.FontSize(12).AlignLeft();
							row.RelativeItem().Text($"الجهة الطالبة: {request.Department}")
								.FontSize(12).AlignRight();
						});
					});

					// Table of Items
					page.Content().Table(table =>
					{
						table.ColumnsDefinition(columns =>
						{
							columns.ConstantColumn(30);
							columns.RelativeColumn(2);
							columns.RelativeColumn(3);
							columns.RelativeColumn(2);
							columns.RelativeColumn(1);
							columns.RelativeColumn(1);
							columns.RelativeColumn(1);
							columns.RelativeColumn(1);
							columns.RelativeColumn(1);
							columns.RelativeColumn(1);
						});

						// Table Header with Styling and Borders
						table.Header(header =>
						{
							string[] headers = { "الرقم", "رقم الصنف", "وصف الصنف", "نوع الصرف", "الوحدة",
						"الكمية المطلوبة", "الكمية المصروفة", "سعر الوحدة", "قيمة الكمية", "الماركة" };

							for (int i = 0; i < headers.Length; i++)
							{
								header.Cell().Row(1).Column((uint)(i + 1))
									.Element(cell =>
									{
										cell.Background(Colors.Grey.Lighten2)
											.Border(1).BorderColor(Colors.Black)  // إضافة الحدود
											.Padding(5)
											.AlignCenter()
											.Text(headers[i])
											.Bold().FontSize(12);
									});
							}
						});

						// Table Content with Borders and Alternating Row Colors
						for (int i = 0; i < request.Items.Count; i++)
						{
							var item = request.Items[i];
							bool isEvenRow = i % 2 == 0;
							var rowBackground = isEvenRow ? Colors.White : Colors.Grey.Lighten4; // Alternate row colors

							for (int j = 0; j < 10; j++)
							{
								table.Cell().Row((uint)(i + 2)).Column((uint)(j + 1))
									.Element(cell =>
									{
										cell.Background(rowBackground)
											.Border(1).BorderColor(Colors.Black)  // إضافة الحدود
											.Padding(5)
											.AlignCenter()
											.Text(GetCellText(j, item))
											.FontSize(11);
									});
							}
						}
					});

					// Footer - Signatures
					page.Footer().PaddingTop(20).Column(column =>
					{
						column.Spacing(10);
						column.Item().AlignCenter().Text("التوقيعات").Bold().FontSize(14);

						column.Item().Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn();
								columns.RelativeColumn();
								columns.RelativeColumn();
								columns.RelativeColumn();
							});

							string[] signatures = { "مراقبة المخزون", "إدارة المستودعات", "أمين/مأمور المستودع", "من الجهة الطالبة" };
							for (int i = 0; i < signatures.Length; i++)
							{
								table.Cell().Row(1).Column((uint)(i + 1))
									.Element(cell =>
									{
										cell.Background(Colors.Grey.Lighten2)
											.Border(1).BorderColor(Colors.Black)
											.Padding(5)
											.AlignCenter()
											.Text(signatures[i])
											.Bold().FontSize(12);
									});
							}

							table.Cell().Row(2).Column(1).Text("");
							table.Cell().Row(2).Column(2).Text(request.WarehouseManager);
							table.Cell().Row(2).Column(3).Text(request.StoreKeeper);
							table.Cell().Row(2).Column(4).Text(request.Requester);
						});
					});
				});
			});

			// Generate PDF and return as byte array
			using (var stream = new MemoryStream())
			{
				document.GeneratePdf(stream);
				return stream.ToArray();
			}
		}

		// Helper function to get text for table cells
		private string GetCellText(int columnIndex, DisbursementItem item)
		{
			return columnIndex switch
			{
				0 => (columnIndex + 1).ToString(),
				1 => item.AssetTag,
				2 => item.AssetDescription,
				3 => item.DisbursementType,
				4 => item.Unit,
				5 => item.Quantity.ToString(),
				6 => "",
				7 => item.UnitPrice != null ? item.UnitPrice.Value.ToString("C") : "N/A",
				8 => item.TotalPrice != null ? item.TotalPrice.Value.ToString("C") : "N/A",
				9 => item.Brand,
				_ => ""
			};
		}

		public async Task<string> GetStoreKeeperForAssetsAsync(List<string> assetTags)
		{
			if (assetTags == null || !assetTags.Any())
			{
				return string.Empty;
			}

			// Get the actual assets
			var assets = await GetAssetsByTagsAsync(assetTags);
			if (!assets.Any())
			{
				return "غير معروف";
			}

			// Collect all non-null user IDs from assets
			var assignedUserIds = assets
				.Where(a => !string.IsNullOrEmpty(a.UserId))
				.Select(a => a.UserId)
				.Distinct()
				.ToList();

			// If no users assigned to assets, return default message
			if (!assignedUserIds.Any())
			{
				// Get the first available supervisor as fallback
				var fallbackSupervisor = await _userRepository.GetFirstUserInRoleAsync(Roles.Supervisor);
				return fallbackSupervisor?.FullName ?? "غير محدد";
			}

			// Get users that are supervisors
			var supervisors = new List<User>();
			foreach (var userId in assignedUserIds)
			{
				var user = await _userRepository.GetUserByIdAsync(userId);
				if (user != null && await _userRepository.IsUserInRoleAsync(user.Id, Roles.Supervisor))
				{
					supervisors.Add(user);
				}
			}

			// Return supervisor information
			if (supervisors.Count == 0)
			{
				// No supervisors found among assigned users
				var fallbackSupervisor = await _userRepository.GetFirstUserInRoleAsync(Roles.Supervisor);
				return fallbackSupervisor?.FullName ?? "غير محدد";
			}
			else if (supervisors.Count == 1)
			{
				// One supervisor found
				return supervisors[0].FullName;
			}
			else
			{
				// Multiple supervisors found
				return string.Join(", ", supervisors.Select(s => s.FullName));
			}
		}

		// Helper method to get assets by their tags
		private async Task<List<Asset>> GetAssetsByTagsAsync(List<string> assetTags)
		{
			if (assetTags == null || !assetTags.Any())
			{
				return new List<Asset>();
			}

			var assets = new List<Asset>();
			foreach (var tag in assetTags)
			{
				var asset = await _assetRepository.GetByIdAsync(tag);
				if (asset != null)
				{
					assets.Add(asset);
				}
			}

			return assets;
		}
	}
}